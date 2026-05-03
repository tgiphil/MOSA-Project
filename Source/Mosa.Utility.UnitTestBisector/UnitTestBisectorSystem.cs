// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;
using System.Text.Json;
using Mosa.Compiler.Framework;
using Mosa.Compiler.Framework.Linker;
using Mosa.Compiler.Framework.Stages;
using Mosa.Compiler.MosaTypeSystem;
using Mosa.Utility.Configuration;
using Mosa.Utility.UnitTests;

namespace Mosa.Utility.UnitTestBisector;

public sealed class UnitTestBisectorSystem
{
	private const int WorkerContinueExitCode = 2;

	private readonly Stopwatch stopwatch = new();
	private readonly MosaSettings mosaSettings = new();
	private readonly object transformDiscoveryLock = new();
	private readonly JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };

	private bool hasCompilationFailure;
	private string lastCompilationFailure;

	private List<UnitTestInfo> discoveredUnitTests = [];
	private HashSet<string> selectedTestMethodNames = [];
	private bool observeFilterOnly;
	private Type selectedStageType;
	private string selectedStageName;
	private HashSet<string> observedTransformNames = [];
	private Dictionary<string, int> observedTransformCounts = new(StringComparer.Ordinal);
	private HashSet<string> bisectorDisabledTransformNames = [];
	private HashSet<string> effectiveDisabledTransformNames = [];
	private HashSet<string> forcedDisabledTransformNames = [];
	private Bisector<string> bisector;

	public int Start(string[] args)
	{
		try
		{
			mosaSettings.LoadArguments(args);
			mosaSettings.UnitTestFailFast = true;
			hasCompilationFailure = false;
			lastCompilationFailure = null;

			stopwatch.Start();

			var plan = ParsePlan(mosaSettings.BisectorPlan);
			var isBisectorPlan = IsBisectorPlan(plan);
			var order = isBisectorPlan ? OrderKind.Unspecified : ParseOrder(mosaSettings.BisectorOrder);
			var stateFile = GetFullStateFilePath();
			if (mosaSettings.BisectorResetState && File.Exists(stateFile))
			{
				File.Delete(stateFile);
				OutputStatusBisector($"Deleted state file: {stateFile}");
			}

			LoadForcedDisabledTransforms();

			selectedStageType = ResolveStageType(mosaSettings.BisectorStage);
			selectedStageName = selectedStageType.Name;
			OutputStatusBisector($"Stage: {selectedStageType.FullName} ({selectedStageName})");
			OutputStatusBisector($"Plan: {plan}");
			if (!isBisectorPlan)
			{
				OutputStatusBisector($"Order: {order}");
			}
			OutputStatusBisector($"State File: {stateFile}");

			OutputStatus("Discovering Unit Tests...");
			discoveredUnitTests = Discovery.DiscoverUnitTests(mosaSettings.UnitTestFilter);
			selectedTestMethodNames = discoveredUnitTests
				.Select(x => x.FullMethodName)
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.ToHashSet(StringComparer.Ordinal);
			observeFilterOnly = !string.IsNullOrWhiteSpace(mosaSettings.UnitTestFilter) && selectedTestMethodNames.Count != 0;
			OutputStatus($"Found Tests: {discoveredUnitTests.Count} in {stopwatch.ElapsedMilliseconds / 1000.0:F2} secs");
			OutputStatusBisector($"Observe Filter Methods Only: {(observeFilterOnly ? "ON" : "OFF")}");
			if (observeFilterOnly)
				OutputStatusBisector($"Observed Methods In Scope: {selectedTestMethodNames.Count}");

			if (discoveredUnitTests.Count == 0)
			{
				OutputStatus("ERROR: No tests matched the filter.");
				return 1;
			}

			if (isBisectorPlan)
				return ExecuteBisectorPlan(plan, stateFile);

			var state = LoadOrCreateState(stateFile, plan);
			EnsureStateCompatibility(state, plan, order);

			if (state.ObservedTransforms.Count == 0)
			{
				OutputStatusBisector("Running transform discovery iteration...");
				observedTransformCounts.Clear();
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

				var filteredCounts = new Dictionary<string, int>(StringComparer.Ordinal);
				foreach (var name in observed)
				{
					filteredCounts[name] = observedTransformCounts.TryGetValue(name, out var count) ? count : 0;
				}

				state.Order = order;
				state.ObservedTransformCounts = filteredCounts;
				state.RandomSeed = ResolveRandomSeed(state.RandomSeed);
				state.ObservedTransforms = BuildIterationSequence(observed, filteredCounts, order, state.RandomSeed);
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

				state.ObservedTransformCounts ??= new Dictionary<string, int>(StringComparer.Ordinal);
				state.Order = state.Order == OrderKind.Unspecified ? order : state.Order;
				state.RandomSeed = ResolveRandomSeed(state.RandomSeed);

				OutputStatusBisector($"Loaded transforms from state: {state.ObservedTransforms.Count}");
			}

			return plan == PlanKind.RandomCombo
				? ExecuteRandomComboPlan(stateFile, state)
				: ExecuteDeterministicPlan(stateFile, state, plan);
		}
		catch (Exception ex)
		{
			OutputStatus($"Exception: {ex.Message}");
			OutputStatus($"Exception: {ex.StackTrace}");
			return 1;
		}
	}

	private static bool IsBisectorPlan(PlanKind plan)
	{
		return plan is PlanKind.FailureInducing or PlanKind.Masking;
	}

	private int ExecuteBisectorPlan(PlanKind plan, string stateFile)
	{
		if (!IsBisectorPlan(plan))
			throw new InvalidOperationException($"Plan '{plan}' is not a bisector plan.");

		var state = new BisectorState
		{
			Plan = plan,
			StageTypeName = selectedStageType.FullName,
			StageName = selectedStageName,
			UnitTestFilter = mosaSettings.UnitTestFilter,
			DisabledTransformsFile = mosaSettings.BisectorDisabledTransformsFile,
			Order = OrderKind.Unspecified,
		};

		OutputStatusBisector("Running transform discovery iteration...");
		observedTransformCounts.Clear();
		bisectorDisabledTransformNames = [];
		RebuildEffectiveDisabledSet();

		var discoveryResult = ExecuteIteration();
		state.BaselineCompleted = true;
		state.BaselinePassed = discoveryResult.Passed;
		OutputStatusBisector($"Discovery Iteration: {(discoveryResult.Passed ? "PASS" : "FAIL")}");

		if (hasCompilationFailure)
		{
			SaveState(stateFile, state);
			WriteFailureReviewFile(stateFile, plan, state);
			return 1;
		}

		var observed = observedTransformNames
			.Where(name => !forcedDisabledTransformNames.Contains(name))
			.OrderBy(name => name)
			.ToList();

		if (observed.Count == 0)
		{
			SaveState(stateFile, state);
			OutputStatusBisector("ERROR: No observed transforms were captured for the selected stage.");
			return 1;
		}

		OutputStatusBisector($"Observed Transforms: {observed.Count}");
		ReportForcedDisabledNotObserved();

		state.ObservedTransforms = observed;
		state.ObservedTransformCounts = observed.ToDictionary(
			name => name,
			name => observedTransformCounts.TryGetValue(name, out var count) ? count : 0,
			StringComparer.Ordinal);
		SaveState(stateFile, state);

		if (plan == PlanKind.FailureInducing)
		{
			RunBisectorSession("Failure-Inducing", invertOutcome: false, discoveryResult);
			state.Completed = !hasCompilationFailure;
			SaveState(stateFile, state);
			WriteFailureReviewFile(stateFile, plan, state);
			return hasCompilationFailure ? 1 : 0;
		}

		if (!discoveryResult.Passed)
		{
			OutputStatusBisector("Masking baseline is FAIL. Falling back to failure-inducing bisector to narrow failing transforms.");
			RunBisectorSession("Failure-Inducing", invertOutcome: false, discoveryResult);
			state.Completed = !hasCompilationFailure;
			SaveState(stateFile, state);
			WriteFailureReviewFile(stateFile, plan, state);
			return hasCompilationFailure ? 1 : 0;
		}

		OutputStatusBisector("Running masking pre-check (all transforms disabled)...");
		bisectorDisabledTransformNames = [.. observed];
		RebuildEffectiveDisabledSet();
		var maskingPreCheckResult = ExecuteIteration();
		bisectorDisabledTransformNames = [];
		RebuildEffectiveDisabledSet();

		if (hasCompilationFailure)
		{
			SaveState(stateFile, state);
			WriteFailureReviewFile(stateFile, plan, state);
			return 1;
		}

		OutputStatusBisector($"Masking Pre-Check -> Actual: {(maskingPreCheckResult.Passed ? "PASS" : "FAIL")}");

		if (maskingPreCheckResult.Passed)
		{
			state.Completed = true;
			SaveState(stateFile, state);
			WriteFailureReviewFile(stateFile, plan, state);
			OutputStatusBisector("Masking pre-check passed (all-disabled still passes). No masking transforms identified.");
			return 0;
		}

		RunBisectorSession("Masking", invertOutcome: true, discoveryResult);
		state.Completed = !hasCompilationFailure;
		SaveState(stateFile, state);
		WriteFailureReviewFile(stateFile, plan, state);
		return hasCompilationFailure ? 1 : 0;
	}

	private void RunBisectorSession(string sessionName, bool invertOutcome, IterationResult discoveryResult)
	{
		bisectorDisabledTransformNames = [];
		RebuildEffectiveDisabledSet();
		bisector = new Bisector<string>(observedTransformNames.Where(name => !forcedDisabledTransformNames.Contains(name)), enablePairwise: mosaSettings.BisectorPairwise);
		var reportedBadItems = new HashSet<string>(StringComparer.Ordinal);

		bisectorDisabledTransformNames = [.. bisector.GetNextDisabledItems()];
		RebuildEffectiveDisabledSet();
		var mappedBaseline = MapOutcome(discoveryResult.Passed, invertOutcome);
		bisector.AcceptResult(mappedBaseline);
		OutputStatusBisector($"{sessionName} Baseline -> Actual: {(discoveryResult.Passed ? "PASS" : "FAIL")}, Mapped: {(mappedBaseline ? "PASS" : "FAIL")}");
		PrintNewlyConfirmedBadItems(sessionName, bisector, reportedBadItems);

		while (!bisector.IsComplete)
		{
			bisectorDisabledTransformNames = [.. bisector.GetNextDisabledItems()];
			RebuildEffectiveDisabledSet();
			PrintIterationHeader(sessionName, bisector.GetStatus());
			PrintDisabledTransforms();

			var iterationResult = ExecuteIteration();
			var mappedResult = MapOutcome(iterationResult.Passed, invertOutcome);

			bisector.AcceptResult(mappedResult);
			OutputStatusBisector($"Iteration Result -> Actual: {(iterationResult.Passed ? "PASS" : "FAIL")}, Mapped: {(mappedResult ? "PASS" : "FAIL")}");
			PrintNewlyConfirmedBadItems(sessionName, bisector, reportedBadItems);
			PrintStatus(bisector.GetStatus());

			if (hasCompilationFailure)
				return;
		}

		OutputStatusBisector($"{sessionName} bisector complete.");
		PrintFinalReport(sessionName, bisector);

		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	private void PrintIterationHeader(string sessionName, Bisector<string>.BisectorStatus status)
	{
		OutputStatusBisector($"{sessionName} Iteration: {status.Iteration + 1}");
		OutputStatusBisector($"Level: {status.Level}");
		OutputStatusBisector($"Phase: {status.Phase}");
		OutputStatusBisector($"Stage: {selectedStageType.FullName} ({selectedStageName})");
	}

	private static bool MapOutcome(bool passed, bool invertOutcome)
	{
		return invertOutcome ? !passed : passed;
	}

	private void PrintNewlyConfirmedBadItems(string sessionName, Bisector<string> sessionBisector, HashSet<string> reportedBadItems)
	{
		foreach (var transform in sessionBisector.ConfirmedBadItems.OrderBy(t => t))
		{
			if (reportedBadItems.Add(transform))
				OutputStatusBisector($"{sessionName} Known Bad Item: {transform}");
		}
	}

	private void PrintStatus(Bisector<string>.BisectorStatus status)
	{
		OutputStatusBisector($"Status.Iteration: {status.Iteration}");
		OutputStatusBisector($"Status.TotalItems: {status.TotalItemCount}");
		OutputStatusBisector($"Status.Suspects: {status.SuspectItemCount}");
		OutputStatusBisector($"Status.BadItems: {status.ConfirmedBadItemCount}");
		OutputStatusBisector($"Status.BadPairs: {status.ConfirmedBadPairCount}");
		OutputStatusBisector($"Status.PairwiseCompleted: {status.PairwiseTestsCompleted}");
		OutputStatusBisector($"Status.PairwiseRemaining: {status.PairwiseTestsRemaining}");
	}

	private void PrintFinalReport(string sessionName, Bisector<string> sessionBisector)
	{
		OutputStatusBisector($"{sessionName} Final Stage: {selectedStageType.FullName} ({selectedStageName})");
		OutputStatusBisector("Forced Disabled Items:");
		foreach (var transform in forcedDisabledTransformNames.OrderBy(t => t))
		{
			OutputStatusBisector($"  {transform}");
		}

		OutputStatusBisector("Confirmed Bad Items:");
		foreach (var transform in sessionBisector.ConfirmedBadItems.OrderBy(t => t))
		{
			OutputStatusBisector($"  {transform}");
		}

		OutputStatusBisector("Confirmed Bad Pairs:");
		foreach (var pair in sessionBisector.ConfirmedBadPairs.OrderBy(p => p.Item1).ThenBy(p => p.Item2))
		{
			OutputStatusBisector($"  {pair.Item1} + {pair.Item2}");
		}

		OutputStatusBisector("Remaining Suspects:");
		foreach (var transform in sessionBisector.RemainingSuspectItems.OrderBy(t => t))
		{
			OutputStatusBisector($"  {transform}");
		}
	}

	private static PlanKind ParsePlan(string plan)
	{
		if (string.Equals(plan, "enable-one", StringComparison.OrdinalIgnoreCase))
			return PlanKind.EnableOne;

		if (string.Equals(plan, "disable-one", StringComparison.OrdinalIgnoreCase))
			return PlanKind.DisableOne;

		if (string.Equals(plan, "random-combo", StringComparison.OrdinalIgnoreCase))
			return PlanKind.RandomCombo;

		if (string.Equals(plan, "failure-inducing", StringComparison.OrdinalIgnoreCase))
			return PlanKind.FailureInducing;

		if (string.Equals(plan, "masking", StringComparison.OrdinalIgnoreCase))
			return PlanKind.Masking;

		throw new InvalidOperationException($"Unknown plan '{plan}'. Valid values: disable-one, enable-one, random-combo, failure-inducing, masking.");
	}

	private static OrderKind ParseOrder(string order)
	{
		if (string.IsNullOrWhiteSpace(order) || string.Equals(order, "original", StringComparison.OrdinalIgnoreCase))
			return OrderKind.Original;

		if (string.Equals(order, "count", StringComparison.OrdinalIgnoreCase) || string.Equals(order, "count-ascending", StringComparison.OrdinalIgnoreCase))
			return OrderKind.CountAscending;

		if (string.Equals(order, "random", StringComparison.OrdinalIgnoreCase))
			return OrderKind.Random;

		throw new InvalidOperationException($"Invalid order value '{order}'. Valid values: original, count, random.");
	}

	private string GetFullStateFilePath()
	{
		var stateFile = mosaSettings.BisectorStateFile;
		if (string.IsNullOrWhiteSpace(stateFile))
			stateFile = "unit-test-bisector-state.json";

		if (!Path.IsPathRooted(stateFile))
			stateFile = Path.GetFullPath(stateFile);

		return stateFile;
	}

	private BisectorState LoadOrCreateState(string stateFile, PlanKind plan)
	{
		if (!File.Exists(stateFile))
		{
			return new BisectorState
			{
				Plan = plan,
				StageTypeName = selectedStageType.FullName,
				StageName = selectedStageName,
				UnitTestFilter = mosaSettings.UnitTestFilter,
				DisabledTransformsFile = mosaSettings.BisectorDisabledTransformsFile,
			};
		}

		var content = File.ReadAllText(stateFile);
		var state = JsonSerializer.Deserialize<BisectorState>(content);
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

	private void SaveState(string stateFile, BisectorState state)
	{
		var directory = Path.GetDirectoryName(stateFile);
		if (!string.IsNullOrEmpty(directory))
			Directory.CreateDirectory(directory);

		var json = JsonSerializer.Serialize(state, jsonSerializerOptions);
		File.WriteAllText(stateFile, json);
	}

	private void EnsureStateCompatibility(BisectorState state, PlanKind plan, OrderKind order)
	{
		if (!string.Equals(state.StageTypeName, selectedStageType.FullName, StringComparison.Ordinal))
			throw new InvalidOperationException("State file stage type does not match current -bisect-stage.");

		if (state.Plan != plan)
			throw new InvalidOperationException("State file plan does not match current -plan.");

		if (!string.Equals(state.UnitTestFilter, mosaSettings.UnitTestFilter, StringComparison.Ordinal))
			throw new InvalidOperationException("State file UnitTest filter does not match current -filter.");

		if (!string.Equals(state.DisabledTransformsFile, mosaSettings.BisectorDisabledTransformsFile, StringComparison.Ordinal))
			throw new InvalidOperationException("State file disabled transforms file does not match current -bisect-disabled-file.");

		if (state.Order == OrderKind.Unspecified)
			state.Order = order;

		if (state.Order != order)
			throw new InvalidOperationException("State file order does not match current -bisect-order.");
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
				CaptureCompilationFailure(unitTestEngine.CompilationFailure);
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

	private void NotifyTransformObserved(string stageName, string transformName, string methodFullName)
	{
		if (!string.Equals(stageName, selectedStageName, StringComparison.Ordinal))
			return;

		if (observeFilterOnly && !selectedTestMethodNames.Contains(methodFullName))
			return;

		lock (transformDiscoveryLock)
		{
			var observed = observedTransformNames.Add(transformName);
			observedTransformCounts[transformName] = observedTransformCounts.TryGetValue(transformName, out var count) ? count + 1 : 1;

			if (observed && bisector != null && !forcedDisabledTransformNames.Contains(transformName))
				bisector.ObserveItem(transformName);
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
		OutputStatusBisector($"Session Disabled: {bisectorDisabledTransformNames.Count}");
		OutputStatusBisector($"Effective Disabled: {effectiveDisabledTransformNames.Count}");
	}

	private void PrintFinalReport(PlanKind plan, BisectorState state)
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
		Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds:00.00} | [Bisector] {status}");
	}

	private void OutputStatus(string status)
	{
		Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds:00.00} | {status}");
	}

	private void CaptureCompilationFailure(string failure)
	{
		if (string.IsNullOrWhiteSpace(failure))
			return;

		hasCompilationFailure = true;
		if (string.Equals(lastCompilationFailure, failure, StringComparison.Ordinal))
			return;

		lastCompilationFailure = failure;
		OutputStatusBisector("Compilation failure details:");
		foreach (var line in failure.Split([Environment.NewLine], StringSplitOptions.None))
		{
			if (!string.IsNullOrWhiteSpace(line))
				OutputStatusBisector($"  {line}");
		}
	}

	private void WriteFailureReviewFile(string stateFile, PlanKind plan, BisectorState state)
	{
		var reviewFile = stateFile + ".failures.txt";
		var lines = new List<string>
		{
			$"Unit Test Bisector Failure Review",
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
		RandomCombo,
		FailureInducing,
		Masking,
	}

	private enum OrderKind
	{
		Unspecified = 0,
		Original = 1,
		CountAscending = 2,
		Random = 3,
	}

	private sealed class BisectorState
	{
		public string StageTypeName { get; set; }

		public string StageName { get; set; }

		public PlanKind Plan { get; set; }

		public string UnitTestFilter { get; set; }

		public string DisabledTransformsFile { get; set; }

		public List<string> ObservedTransforms { get; set; } = [];

		public Dictionary<string, int> ObservedTransformCounts { get; set; } = new(StringComparer.Ordinal);

		public bool BaselineCompleted { get; set; }

		public bool BaselinePassed { get; set; }

		public int NextIndex { get; set; }

		public OrderKind Order { get; set; }

		public int RandomSeed { get; set; }

		public List<PlanResult> Results { get; set; } = [];

		public bool Completed { get; set; }
	}

	private sealed class PlanResult
	{
		public string Transform { get; set; }

		public bool Passed { get; set; }

		public List<string> DisabledTransforms { get; set; } = [];
	}

	private int ExecuteDeterministicPlan(string stateFile, BisectorState state, PlanKind plan)
	{
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

			if (hasCompilationFailure)
			{
				WriteFailureReviewFile(stateFile, plan, state);
				return 1;
			}
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

			if (hasCompilationFailure)
				return 1;

			if (mosaSettings.BisectorWorkerIteration && state.NextIndex < state.ObservedTransforms.Count)
				return WorkerContinueExitCode;
		}

		state.Completed = true;
		SaveState(stateFile, state);
		WriteFailureReviewFile(stateFile, plan, state);

		PrintFinalReport(plan, state);
		return 0;
	}

	private int ExecuteRandomComboPlan(string stateFile, BisectorState state)
	{
		if (!state.BaselineCompleted)
		{
			bisectorDisabledTransformNames = [];
			RebuildEffectiveDisabledSet();

			OutputStatusBisector("Running baseline iteration...");
			PrintDisabledTransforms();

			var baselineResult = ExecuteIteration();
			state.BaselineCompleted = true;
			state.BaselinePassed = baselineResult.Passed;
			SaveState(stateFile, state);
			OutputStatusBisector($"Baseline Result: {(baselineResult.Passed ? "PASS" : "FAIL")}{Environment.NewLine}");

			if (hasCompilationFailure)
			{
				WriteFailureReviewFile(stateFile, PlanKind.RandomCombo, state);
				return 1;
			}

			if (mosaSettings.BisectorWorkerIteration)
				return WorkerContinueExitCode;
		}

		var iterationsThisRun = mosaSettings.BisectorWorkerIteration ? 1 : Math.Max(1, mosaSettings.BisectorIterations);

		for (var i = 0; i < iterationsThisRun; i++)
		{
			bisectorDisabledTransformNames = BuildRandomDisabledSet(state.ObservedTransforms, state.RandomSeed, state.NextIndex);
			RebuildEffectiveDisabledSet();
			var disabledSnapshot = effectiveDisabledTransformNames.OrderBy(x => x).ToList();

			OutputStatusBisector($"Random Iteration {state.NextIndex + 1}");
			PrintDisabledTransforms();

			var iterationResult = ExecuteIteration();
			state.Results.Add(new PlanResult
			{
				Transform = $"random-{state.NextIndex + 1}",
				Passed = iterationResult.Passed,
				DisabledTransforms = disabledSnapshot,
			});

			state.NextIndex++;
			SaveState(stateFile, state);
			WriteFailureReviewFile(stateFile, PlanKind.RandomCombo, state);

			OutputStatusBisector($"Iteration Result: {(iterationResult.Passed ? "PASS" : "FAIL")}");

			if (hasCompilationFailure)
				return 1;
		}

		return mosaSettings.BisectorWorkerIteration ? WorkerContinueExitCode : 0;
	}

	private static List<string> BuildIterationSequence(List<string> observed, Dictionary<string, int> counts, OrderKind order, int randomSeed)
	{
		if (order == OrderKind.CountAscending)
			return observed.OrderBy(name => counts.TryGetValue(name, out var count) ? count : 0).ThenBy(name => name).ToList();

		if (order == OrderKind.Random)
			return Shuffle(observed, randomSeed);

		return observed;
	}

	private int ResolveRandomSeed(int existingSeed)
	{
		if (existingSeed != 0)
			return existingSeed;

		if (mosaSettings.BisectorRandomSeed != 0)
			return mosaSettings.BisectorRandomSeed;

		return Random.Shared.Next(1, int.MaxValue);
	}

	private static List<string> Shuffle(List<string> items, int seed)
	{
		var copy = items.ToList();
		var random = new Random(seed);

		for (var i = copy.Count - 1; i > 0; i--)
		{
			var j = random.Next(i + 1);
			(copy[i], copy[j]) = (copy[j], copy[i]);
		}

		return copy;
	}

	private static HashSet<string> BuildRandomDisabledSet(List<string> transforms, int seed, int index)
	{
		var random = new Random(seed + (index * 7919));
		var disabled = new HashSet<string>(StringComparer.Ordinal);

		foreach (var transform in transforms)
		{
			if (random.Next(2) == 0)
				disabled.Add(transform);
		}

		return disabled;
	}
}
