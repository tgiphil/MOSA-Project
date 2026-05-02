// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;
using System.Text.Json;
using Mosa.Compiler.Framework;
using Mosa.Compiler.Framework.Linker;
using Mosa.Compiler.Framework.Stages;
using Mosa.Compiler.MosaTypeSystem;
using Mosa.Utility.Configuration;
using Mosa.Utility.UnitTests;

namespace Mosa.Utility.UnitTestBisector.Persistent;

public sealed class PersistentUnitTestBisectorSystem
{
    private readonly Stopwatch stopwatch = new();
    private readonly MosaSettings mosaSettings = new();
    private readonly object transformDiscoveryLock = new();
    private readonly JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };

    private List<UnitTestInfo> discoveredUnitTests = [];
    private Type selectedStageType;
    private string selectedStageName;
    private HashSet<string> observedTransformNames = [];
    private HashSet<string> bisectorDisabledTransformNames = [];
    private HashSet<string> effectiveDisabledTransformNames = [];
    private HashSet<string> forcedDisabledTransformNames = [];

    public int Start(string[] args)
    {
        try
        {
            mosaSettings.LoadArguments(args);
            mosaSettings.UnitTestFailFast = true;

            stopwatch.Start();

            var plan = ParsePlan(mosaSettings.BisectorPersistentPlan);
            var stateFile = GetFullStateFilePath();
            if (mosaSettings.BisectorPersistentResetState && File.Exists(stateFile))
            {
                File.Delete(stateFile);
                OutputStatusBisector($"Deleted state file: {stateFile}");
            }

            LoadForcedDisabledTransforms();

            selectedStageType = ResolveStageType(mosaSettings.BisectorStage);
            selectedStageName = selectedStageType.Name;
            OutputStatusBisector($"Stage: {selectedStageType.FullName} ({selectedStageName})");
            OutputStatusBisector($"Plan: {plan}");
            OutputStatusBisector($"State File: {stateFile}");

            OutputStatus("Discovering Unit Tests...");
            discoveredUnitTests = Discovery.DiscoverUnitTests(mosaSettings.UnitTestFilter);
            OutputStatus($"Found Tests: {discoveredUnitTests.Count} in {stopwatch.ElapsedMilliseconds / 1000.0:F2} secs");

            if (discoveredUnitTests.Count == 0)
            {
                OutputStatus("ERROR: No tests matched the filter.");
                return 1;
            }

            var state = LoadOrCreateState(stateFile, plan);
            EnsureStateCompatibility(state, plan);

            if (state.ObservedTransforms.Count == 0)
            {
                OutputStatusBisector("Running transform discovery iteration...");
                bisectorDisabledTransformNames = [];
                RebuildEffectiveDisabledSet();

                var discoveryResult = ExecuteIteration();
                OutputStatusBisector($"Discovery Iteration: {(discoveryResult.Passed ? "PASS" : "FAIL")}");

                var observed = observedTransformNames
                    .Where(name => !forcedDisabledTransformNames.Contains(name))
                    .OrderBy(name => name)
                    .ToList();

                if (observed.Count == 0)
                {
                    OutputStatusBisector("ERROR: No observed transforms were captured for the selected stage.");
                    return 1;
                }

                state.ObservedTransforms = observed;
                SaveState(stateFile, state);

                OutputStatusBisector($"Discovered transforms for plan: {state.ObservedTransforms.Count}");
                ReportForcedDisabledNotObserved();
            }
            else
            {
                foreach (var transform in state.ObservedTransforms)
                {
                    observedTransformNames.Add(transform);
                }

                OutputStatusBisector($"Loaded transforms from state: {state.ObservedTransforms.Count}");
            }

            if (!state.BaselineCompleted)
            {
                bisectorDisabledTransformNames = BuildDisabledSetForBaseline(plan, state.ObservedTransforms);
                RebuildEffectiveDisabledSet();

                OutputStatusBisector("Running baseline iteration...");
                PrintDisabledTransforms();

                var baselineResult = ExecuteIteration();
                state.BaselineCompleted = true;
                state.BaselinePassed = baselineResult.Passed;
                SaveState(stateFile, state);

                OutputStatusBisector($"Baseline Result: {(baselineResult.Passed ? "PASS" : "FAIL")}");
            }
            else
            {
                OutputStatusBisector($"Resuming after baseline. Baseline Result: {(state.BaselinePassed ? "PASS" : "FAIL")}");
            }

            while (state.NextIndex < state.ObservedTransforms.Count)
            {
                var transform = state.ObservedTransforms[state.NextIndex];
                bisectorDisabledTransformNames = BuildDisabledSetForTransform(plan, state.ObservedTransforms, transform);
                RebuildEffectiveDisabledSet();
                var disabledSnapshot = effectiveDisabledTransformNames.OrderBy(x => x).ToList();

                OutputStatusBisector($"Iteration {state.NextIndex + 1}/{state.ObservedTransforms.Count}");
                OutputStatusBisector($"Transform: {transform}");
                PrintDisabledTransforms();

                var iterationResult = ExecuteIteration();
                state.Results.Add(new PlanResult
                {
                    Transform = transform,
                    Passed = iterationResult.Passed,
                    DisabledTransforms = disabledSnapshot,
                });
                state.NextIndex++;
                SaveState(stateFile, state);
                WriteFailureReviewFile(stateFile, plan, state);

                OutputStatusBisector($"Iteration Result: {(iterationResult.Passed ? "PASS" : "FAIL")}");
                if (!iterationResult.Passed)
                {
                    OutputStatusBisector($"Failure state captured for review: transform={transform}, disabled={disabledSnapshot.Count}");
                }
            }

            state.Completed = true;
            SaveState(stateFile, state);
            WriteFailureReviewFile(stateFile, plan, state);

            PrintFinalReport(plan, state);
            return 0;
        }
        catch (Exception ex)
        {
            OutputStatus($"Exception: {ex.Message}");
            OutputStatus($"Exception: {ex.StackTrace}");
            return 1;
        }
    }

    private static PlanKind ParsePlan(string plan)
    {
        if (string.Equals(plan, "enable-one", StringComparison.OrdinalIgnoreCase))
            return PlanKind.EnableOne;

        if (string.Equals(plan, "disable-one", StringComparison.OrdinalIgnoreCase))
            return PlanKind.DisableOne;

        throw new InvalidOperationException($"Unknown plan '{plan}'. Valid values: enable-one, disable-one.");
    }

    private string GetFullStateFilePath()
    {
        var stateFile = mosaSettings.BisectorPersistentStateFile;
        if (string.IsNullOrWhiteSpace(stateFile))
            stateFile = "unit-test-bisector-persistent-state.json";

        if (!Path.IsPathRooted(stateFile))
            stateFile = Path.GetFullPath(stateFile);

        return stateFile;
    }

    private PersistentState LoadOrCreateState(string stateFile, PlanKind plan)
    {
        if (!File.Exists(stateFile))
        {
            return new PersistentState
            {
                Plan = plan,
                StageTypeName = selectedStageType.FullName,
                StageName = selectedStageName,
                UnitTestFilter = mosaSettings.UnitTestFilter,
                DisabledTransformsFile = mosaSettings.BisectorDisabledTransformsFile,
            };
        }

        var content = File.ReadAllText(stateFile);
        var state = JsonSerializer.Deserialize<PersistentState>(content);
        if (state == null)
            throw new InvalidOperationException($"Unable to deserialize state file: {stateFile}");

        state.Results ??= [];
        state.ObservedTransforms ??= [];

        if (state.NextIndex < 0)
            state.NextIndex = 0;

        if (state.NextIndex > state.Results.Count)
            state.NextIndex = state.Results.Count;

        if (state.Results.Count > state.NextIndex)
            state.Results = state.Results.Take(state.NextIndex).ToList();

        return state;
    }

    private void SaveState(string stateFile, PersistentState state)
    {
        var directory = Path.GetDirectoryName(stateFile);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(state, jsonSerializerOptions);
        File.WriteAllText(stateFile, json);
    }

    private void EnsureStateCompatibility(PersistentState state, PlanKind plan)
    {
        if (!string.Equals(state.StageTypeName, selectedStageType.FullName, StringComparison.Ordinal))
            throw new InvalidOperationException("State file stage type does not match current -bisect-stage.");

        if (state.Plan != plan)
            throw new InvalidOperationException("State file plan does not match current -plan.");

        if (!string.Equals(state.UnitTestFilter, mosaSettings.UnitTestFilter, StringComparison.Ordinal))
            throw new InvalidOperationException("State file UnitTest filter does not match current -filter.");

        if (!string.Equals(state.DisabledTransformsFile, mosaSettings.BisectorDisabledTransformsFile, StringComparison.Ordinal))
            throw new InvalidOperationException("State file disabled transforms file does not match current -bisect-disabled-file.");
    }

    private HashSet<string> BuildDisabledSetForBaseline(PlanKind plan, List<string> transforms)
    {
        if (plan == PlanKind.EnableOne)
            return [.. transforms];

        return [];
    }

    private HashSet<string> BuildDisabledSetForTransform(PlanKind plan, List<string> transforms, string transform)
    {
        if (plan == PlanKind.DisableOne)
            return [transform];

        var disabled = new HashSet<string>(transforms, StringComparer.Ordinal);
        disabled.Remove(transform);
        return disabled;
    }

    private IterationResult ExecuteIteration()
    {
        using var assertCapture = new AssertCaptureScope();

        try
        {
            using var unitTestEngine = new UnitTestEngine(mosaSettings, OutputStatus, CreateCompilerHooks);
            if (unitTestEngine.IsAborted)
            {
                OutputStatusBisector("Iteration compiler run aborted. Treating as FAIL.");
                return new IterationResult(false);
            }

            var unitTests = PrepareUnitTests(discoveredUnitTests, unitTestEngine.TypeSystem, unitTestEngine.Linker);

            unitTestEngine.QueueUnitTests(unitTests);
            unitTestEngine.WaitUntilComplete();
            unitTestEngine.Terminate();

            var passed = true;
            foreach (var unitTest in unitTests)
            {
                if (unitTest.Status is UnitTestStatus.Failed or UnitTestStatus.FailedByCrash or UnitTestStatus.Pending)
                {
                    passed = false;
                    break;
                }
            }

            return new IterationResult(passed);
        }
        catch (AssertFailureException ex)
        {
            OutputStatusBisector($"Debug.Assert captured and treated as FAIL: {ex.Message}");
            return new IterationResult(false);
        }
        catch (Exception ex)
        {
            OutputStatusBisector($"Iteration exception treated as FAIL: {ex.Message}");
            OutputStatusBisector($"Iteration exception stack: {ex.StackTrace}");
            return new IterationResult(false);
        }
        finally
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private CompilerHooks CreateCompilerHooks()
    {
        return new CompilerHooks
        {
            NotifyTransformObserved = NotifyTransformObserved,
            IsTransformDisabled = IsTransformDisabled,
        };
    }

    private void NotifyTransformObserved(string stageName, string transformName)
    {
        if (!string.Equals(stageName, selectedStageName, StringComparison.Ordinal))
            return;

        lock (transformDiscoveryLock)
        {
            observedTransformNames.Add(transformName);
        }
    }

    private bool IsTransformDisabled(string stageName, string transformName)
    {
        if (!string.Equals(stageName, selectedStageName, StringComparison.Ordinal))
            return false;

        return effectiveDisabledTransformNames.Contains(transformName);
    }

    private void LoadForcedDisabledTransforms()
    {
        forcedDisabledTransformNames = [];

        var filename = mosaSettings.BisectorDisabledTransformsFile;
        if (string.IsNullOrWhiteSpace(filename))
            return;

        if (!File.Exists(filename))
            throw new InvalidOperationException($"Disabled transforms file does not exist: {filename}");

        foreach (var line in File.ReadLines(filename))
        {
            var text = line.Trim();

            if (text.Length == 0)
                continue;

            if (text.StartsWith("#", StringComparison.Ordinal) || text.StartsWith("//", StringComparison.Ordinal))
                continue;

            forcedDisabledTransformNames.Add(text);
        }

        OutputStatusBisector($"Forced Disabled Transforms File: {filename}");
        OutputStatusBisector($"Forced Disabled Transforms Loaded: {forcedDisabledTransformNames.Count}");
    }

    private void RebuildEffectiveDisabledSet()
    {
        effectiveDisabledTransformNames = [.. forcedDisabledTransformNames];

        foreach (var transformName in bisectorDisabledTransformNames)
            effectiveDisabledTransformNames.Add(transformName);
    }

    private void ReportForcedDisabledNotObserved()
    {
        if (forcedDisabledTransformNames.Count == 0)
            return;

        var notObserved = forcedDisabledTransformNames.Where(name => !observedTransformNames.Contains(name)).OrderBy(name => name).ToList();
        if (notObserved.Count == 0)
            return;

        OutputStatusBisector($"WARNING: {notObserved.Count} forced-disabled transforms were not observed in selected stage:");
        foreach (var name in notObserved)
        {
            OutputStatusBisector($"  {name}");
        }
    }

    private Type ResolveStageType(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new InvalidOperationException("A stage type name is required. Use -bisect-stage.");

        var stageTypes = typeof(OptimizationStage).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(BaseTransformStage).IsAssignableFrom(t))
            .ToList();

        var fullNameMatches = stageTypes.Where(t => string.Equals(t.FullName, stageName, StringComparison.Ordinal)).ToList();
        if (fullNameMatches.Count == 1)
            return fullNameMatches[0];
        if (fullNameMatches.Count > 1)
            throw new InvalidOperationException($"Stage name '{stageName}' is ambiguous.");

        var shortNameMatches = stageTypes.Where(t => string.Equals(t.Name, stageName, StringComparison.Ordinal)).ToList();
        if (shortNameMatches.Count == 1)
            return shortNameMatches[0];
        if (shortNameMatches.Count > 1)
            throw new InvalidOperationException($"Stage name '{stageName}' is ambiguous. Use the full type name.");

        throw new InvalidOperationException($"Unable to resolve stage '{stageName}'.");
    }

    private List<UnitTest> PrepareUnitTests(List<UnitTestInfo> tests, TypeSystem typeSystem, MosaLinker linker)
    {
        var unitTests = new List<UnitTest>(tests.Count);
        var id = 0;

        foreach (var unitTestInfo in tests)
        {
            var linkerMethodInfo = Linker.GetMethodInfo(typeSystem, linker, unitTestInfo);
            var unitTest = new UnitTest(unitTestInfo, linkerMethodInfo)
            {
                SerializedUnitTest = UnitTestSystem.SerializeUnitTestMessage(new UnitTest(unitTestInfo, linkerMethodInfo)),
                UnitTestID = ++id,
            };

            unitTests.Add(unitTest);
        }

        return unitTests;
    }

    private void PrintDisabledTransforms()
    {
        OutputStatusBisector($"Forced Disabled: {forcedDisabledTransformNames.Count}");
        OutputStatusBisector($"Plan Disabled: {bisectorDisabledTransformNames.Count}");
        OutputStatusBisector($"Effective Disabled: {effectiveDisabledTransformNames.Count}");
    }

    private void PrintFinalReport(PlanKind plan, PersistentState state)
    {
        OutputStatusBisector($"Plan complete: {plan}");
        OutputStatusBisector($"Final Stage: {selectedStageType.FullName} ({selectedStageName})");
        OutputStatusBisector($"Baseline: {(state.BaselinePassed ? "PASS" : "FAIL")}");
        OutputStatusBisector($"Iterations: {state.Results.Count}");

        var passed = state.Results.Count(r => r.Passed);
        var failed = state.Results.Count - passed;
        OutputStatusBisector($"Passed Iterations: {passed}");
        OutputStatusBisector($"Failed Iterations: {failed}");

        OutputStatusBisector("Failed Transforms:");
        foreach (var result in state.Results.Where(r => !r.Passed).OrderBy(r => r.Transform))
        {
            OutputStatusBisector($"  {result.Transform}");
        }
    }

    private void OutputStatusBisector(string status)
    {
        Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds:00.00} | [PersistentBisector] {status}");
    }

    private void OutputStatus(string status)
    {
        Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds:00.00} | {status}");
    }

    private void WriteFailureReviewFile(string stateFile, PlanKind plan, PersistentState state)
    {
        var reviewFile = stateFile + ".failures.txt";
        var lines = new List<string>
        {
            $"Persistent Unit Test Bisector Failure Review",
            $"Stage: {selectedStageType.FullName} ({selectedStageName})",
            $"Plan: {plan}",
            $"State File: {stateFile}",
            $"Baseline: {(state.BaselinePassed ? "PASS" : "FAIL")}",
            $"Completed: {state.Completed}",
            $"Progress: {state.NextIndex}/{state.ObservedTransforms.Count}",
            $"Total Iterations: {state.Results.Count}",
            string.Empty,
        };

        var failedResults = state.Results.Where(r => !r.Passed).OrderBy(r => r.Transform).ToList();
        lines.Add($"Failed Iterations: {failedResults.Count}");
        lines.Add(string.Empty);

        if (failedResults.Count == 0)
        {
            lines.Add("No failing iterations recorded.");
        }
        else
        {
            foreach (var result in failedResults)
            {
                lines.Add($"Transform: {result.Transform}");
                lines.Add($"  Result: FAIL");
                lines.Add("  Disabled Transforms:");

                foreach (var disabled in result.DisabledTransforms.OrderBy(x => x))
                {
                    lines.Add($"    {disabled}");
                }

                lines.Add(string.Empty);
            }
        }

        File.WriteAllLines(reviewFile, lines);
    }

    private readonly record struct IterationResult(bool Passed);

    private sealed class AssertCaptureScope : IDisposable
    {
        private readonly List<(DefaultTraceListener Listener, bool AssertUiEnabled)> defaultListeners = new();
        private readonly TraceListener listener = new AssertExceptionTraceListener();

        public AssertCaptureScope()
        {
            foreach (TraceListener traceListener in Trace.Listeners)
            {
                if (traceListener is DefaultTraceListener defaultTraceListener)
                {
                    defaultListeners.Add((defaultTraceListener, defaultTraceListener.AssertUiEnabled));
                    defaultTraceListener.AssertUiEnabled = false;
                }
            }

            Trace.Listeners.Add(listener);
        }

        public void Dispose()
        {
            Trace.Listeners.Remove(listener);

            foreach (var (listener, assertUiEnabled) in defaultListeners)
            {
                listener.AssertUiEnabled = assertUiEnabled;
            }
        }
    }

    private sealed class AssertExceptionTraceListener : TraceListener
    {
        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
        }

        public override void Fail(string message, string detailMessage)
        {
            throw new AssertFailureException(message, detailMessage);
        }
    }

    private sealed class AssertFailureException : Exception
    {
        public AssertFailureException(string message, string detailMessage)
            : base(string.IsNullOrWhiteSpace(detailMessage) ? message : $"{message} {detailMessage}")
        {
        }
    }

    private enum PlanKind
    {
        DisableOne,
        EnableOne,
    }

    private sealed class PersistentState
    {
        public string StageTypeName { get; set; }
        public string StageName { get; set; }
        public PlanKind Plan { get; set; }
        public string UnitTestFilter { get; set; }
        public string DisabledTransformsFile { get; set; }
        public List<string> ObservedTransforms { get; set; } = [];
        public bool BaselineCompleted { get; set; }
        public bool BaselinePassed { get; set; }
        public int NextIndex { get; set; }
        public List<PlanResult> Results { get; set; } = [];
        public bool Completed { get; set; }
    }

    private sealed class PlanResult
    {
        public string Transform { get; set; }
        public bool Passed { get; set; }
        public List<string> DisabledTransforms { get; set; } = [];
    }
}
