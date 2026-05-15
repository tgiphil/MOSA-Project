// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;
using System.Text.Json;
using Mosa.Utility.Configuration;

namespace Mosa.Utility.UnitTestBisector.Supervisor;

internal sealed partial class ProcessSupervisor
{
	private const int RestartDelayMs = 1000;
	private const int WorkerContinueExitCode = 2;

	private const string OptionBisectWorkerIteration = "-bisect-worker-iteration";
	private const string OptionBisectState = "-bisect-state";

	private static readonly HashSet<string> BisectOptions = new(StringComparer.Ordinal)
	{
		"-bisect-worker-iteration",
		"-bisect-state",
		"-bisect-max-restarts",
		"-bisect-reset"
	};

	private readonly Stopwatch Stopwatch = new();
	private readonly MosaSettings MosaSettings = new();

	private string[] args;
	private int restartCount;

	public ProcessSupervisor()
	{
	}

	public int Start(string[] args)
	{
		this.args = args;
		Stopwatch.Start();

		MosaSettings.SetDefaultSettings();
		MosaSettings.LoadAppLocations();
		SetDefaultSettings();
		MosaSettings.LoadArguments(args);
		MosaSettings.NormalizeSettings();
		MosaSettings.ResolveDefaults();
		MosaSettings.ResolveFileAndPathSettings();
		MosaSettings.AddStandardPlugs();
		MosaSettings.ExpandSearchPaths();

		var targetArguments = BuildTargetArguments(MosaSettings.BisectorStateFile);
		var supervisorIteration = 0;

		if (MosaSettings.BisectorResetState && File.Exists(MosaSettings.BisectorStateFile))
			File.Delete(MosaSettings.BisectorStateFile);

		OutputStatus("Supervisor started");
		OutputStatus($"Target Arguments: {targetArguments}");

		while (true)
		{
			supervisorIteration++;
			OutputStatus($"Supervisor Iteration: {supervisorIteration}");

			var before = ReadStateSnapshot(MosaSettings.BisectorStateFile, $"Supervisor iteration {supervisorIteration} pre-launch state");
			if (before.Found && before.Completed)
			{
				OutputStatus("State indicates completed=true before launch. Exiting supervisor.");
				if (before.LastExitCode != 0)
					OutputStatus($"WARNING: completed state has non-zero last exit code {before.LastExitCode} ({before.LastExitKind}). Treating as complete.");

				return 0;
			}

			using var process = StartTarget(MosaSettings.BisectorApp, targetArguments);
			process.WaitForExit();
			var exitCode = process.ExitCode;

			var after = ReadStateSnapshot(MosaSettings.BisectorStateFile, $"Supervisor iteration {supervisorIteration} post-exit state");

			if (after.Found && after.Completed)
			{
				if (exitCode != 0)
					OutputStatus($"WARNING: target exited with non-zero code {exitCode} but state reports completed=true. Treating as complete.");

				OutputStatus("completed-from-state");
				return 0;
			}

			if (exitCode == WorkerContinueExitCode)
			{
				OutputStatus("continue-iteration");
				continue;
			}

			if (exitCode == 0)
			{
				OutputStatus("Target exited successfully.");
				return 0;
			}

			if (after.Found && string.Equals(after.LastExitKind, "Failure", StringComparison.Ordinal))
			{
				OutputStatus($"abnormal-exit-code: {exitCode}");
				OutputStatus("State indicates a terminal failure (non-retriable). Exiting supervisor.");
				return exitCode;
			}

			var verifiedExitCode = !after.Found || after.LastExitCode == 0 || after.LastExitCode == exitCode;
			if (verifiedExitCode)
			{
				OutputStatus($"abnormal-exit-code: {exitCode}");
			}
			else
			{
				OutputStatus($"WARNING: abnormal exit code mismatch. Process={exitCode}, State={after.LastExitCode} ({after.LastExitKind})");
				OutputStatus($"abnormal-exit-code: {exitCode}");
			}

			restartCount++;
			OutputStatus($"abnormal-exit-retry (restart #{restartCount})");

			if (MosaSettings.BisectorMaxRestarts > 0 && restartCount > MosaSettings.BisectorMaxRestarts)
			{
				OutputStatus("Maximum restart count reached. Exiting supervisor.");
				return exitCode;
			}

			Thread.Sleep(RestartDelayMs);
		}
	}

	private void SetDefaultSettings()
	{
		if (string.IsNullOrEmpty(MosaSettings.BisectorStateFile))
			MosaSettings.BisectorStateFile = "%DEFAULT%";

		MosaSettings.BisectorWorkerIteration = true;
	}

	private string BuildTargetArguments(string stateFile)
	{
		var forwarded = new List<string>();

		for (var i = 0; i < args.Length; i++)
		{
			var arg = args[i];

			if (BisectOptions.Contains(arg))
				continue;

			forwarded.Add(QuoteIfNeeded(arg));
		}

		if (MosaSettings.BisectorWorkerIteration)
			forwarded.Add(OptionBisectWorkerIteration);

		forwarded.Add(OptionBisectState);
		forwarded.Add(QuoteIfNeeded(stateFile));

		return string.Join(" ", forwarded);
	}

	private static string QuoteIfNeeded(string arg)
	{
		if (arg.Contains(' '))
			return $"\"{arg.Replace("\"", "\\\"")}" + "\"";

		return arg;
	}

	private StateSnapshot ReadStateSnapshot(string stateFile, string context)
	{
		if (!File.Exists(stateFile))
			return new StateSnapshot(false, false, 0, 0, 0, 0, "Unknown", 0);

		try
		{
			var content = File.ReadAllText(stateFile);
			using var jsonDocument = JsonDocument.Parse(content);
			var root = jsonDocument.RootElement;

			var completed = ReadBoolean(root, "Completed");
			var iterationNumber = ReadInt32(root, "IterationNumber");
			var totalIterations = ReadInt32(root, "TotalIterationCount");
			var passCount = ReadInt32(root, "PassCount");
			var nextIndex = ReadInt32(root, "NextIndex");
			var lastExitKind = ReadString(root, "LastExitKind") ?? "Unknown";
			var lastExitCode = ReadInt32(root, "LastExitCode");

			return new StateSnapshot(true, completed, iterationNumber, totalIterations, passCount, nextIndex, lastExitKind, lastExitCode);
		}
		catch (Exception ex)
		{
			OutputStatus($"WARNING: failed to read state file: {ex.Message}");
			return new StateSnapshot(false, false, 0, 0, 0, 0, "Unknown", 0);
		}
	}

	private static bool ReadBoolean(JsonElement root, string propertyName)
	{
		if (!root.TryGetProperty(propertyName, out var value))
			return false;

		if (value.ValueKind == JsonValueKind.True)
			return true;

		if (value.ValueKind == JsonValueKind.False)
			return false;

		return false;
	}

	private static string ReadString(JsonElement root, string propertyName)
	{
		if (!root.TryGetProperty(propertyName, out var value))
			return null;

		if (value.ValueKind == JsonValueKind.String)
			return value.GetString();

		return null;
	}

	private static int ReadInt32(JsonElement root, string propertyName)
	{
		if (!root.TryGetProperty(propertyName, out var value))
			return 0;

		if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
			return number;

		return 0;
	}

	private Process StartTarget(string targetPath, string targetArguments)
	{
		var startInfo = new ProcessStartInfo
		{
			FileName = targetPath,
			Arguments = targetArguments,
			UseShellExecute = false,
		};

		var process = Process.Start(startInfo);
		if (process == null)
			throw new InvalidOperationException($"Failed to start target: {targetPath}");

		TryEnableAllProcessors(process);

		OutputStatus($"Target started (PID: {process.Id})");
		return process;
	}

	private static void TryEnableAllProcessors(Process process)
	{
		try
		{
			var maxBits = IntPtr.Size * 8;
			var processorCount = Math.Max(1, Math.Min(Environment.ProcessorCount, maxBits));

			long mask;
			if (processorCount >= 63)
				mask = long.MaxValue;
			else
				mask = (1L << processorCount) - 1;

			process.ProcessorAffinity = (IntPtr)mask;
		}
		catch
		{
		}
	}

	private void OutputStatus(string status)
	{
		Console.WriteLine($"{Stopwatch.Elapsed.TotalSeconds:00.00} | [Supervisor] {status}");
	}
}
