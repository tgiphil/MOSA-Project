// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;
using Mosa.Utility.Configuration;

namespace Mosa.Utility.UnitTestBisector.Supervisor;

internal sealed class ProcessSupervisor
{
	private const int RestartDelayMs = 1000;

	private readonly Stopwatch stopwatch = new();
	private readonly MosaSettings settings;
	private readonly string[] args;
	private int restartCount;

	public ProcessSupervisor(MosaSettings settings, string[] args)
	{
		this.settings = settings;
		this.args = args ?? Array.Empty<string>();
	}

	public int Run()
	{
		var targetPath = ResolveAndValidateTargetPath();
		var targetArguments = BuildTargetArguments();
		var workingDirectory = ResolveAndValidateWorkingDirectory(targetPath);
		var maxRestarts = GetValidatedMaxRestarts();

		stopwatch.Start();
		OutputStatus("Supervisor started");
		OutputStatus($"Target: {targetPath}");
		OutputStatus($"Target Arguments: {targetArguments}");
		OutputStatus($"Working Directory: {workingDirectory}");
		OutputStatus($"Max Restarts: {(maxRestarts <= 0 ? "unlimited" : maxRestarts)}");

		while (true)
		{
			using var process = StartTarget(targetPath, targetArguments, workingDirectory);
			process.WaitForExit();

			OutputStatus($"Target exited with code {process.ExitCode}");

			if (process.ExitCode == 0)
				return 0;

			restartCount++;
			if (maxRestarts > 0 && restartCount > maxRestarts)
			{
				OutputStatus("Maximum restart count reached. Exiting supervisor.");
				return process.ExitCode;
			}

			Thread.Sleep(RestartDelayMs);
			OutputStatus($"Restarting target (restart #{restartCount})...");
		}
	}

	private string ResolveAndValidateTargetPath()
	{
		settings.LoadAppLocations();

		var discoveredTargetPath = settings.BisectorApp;
		if (string.IsNullOrWhiteSpace(discoveredTargetPath))
			throw new InvalidOperationException("Unable to locate persistent bisector target from app locations.");

		var resolvedDiscoveredTargetPath = Path.IsPathRooted(discoveredTargetPath)
			? discoveredTargetPath
			: Path.GetFullPath(discoveredTargetPath);

		if (!File.Exists(resolvedDiscoveredTargetPath))
			throw new InvalidOperationException($"Discovered target does not exist: {resolvedDiscoveredTargetPath}");

		return resolvedDiscoveredTargetPath;
	}

	private string BuildTargetArguments()
	{
		var forwarded = new List<string>();
		var hasWorkerIteration = false;

		for (var i = 0; i < args.Length; i++)
		{
			var arg = args[i];

			if (string.Equals(arg, "-bisect-worker-iteration", StringComparison.OrdinalIgnoreCase))
				hasWorkerIteration = true;

			if (IsSupervisorOption(arg, out var takesValue))
			{
				if (takesValue && i + 1 < args.Length)
					i++;
				continue;
			}

			forwarded.Add(QuoteIfNeeded(arg));
		}

		if (!hasWorkerIteration)
			forwarded.Add("-bisect-worker-iteration");

		return string.Join(" ", forwarded);
	}

	private static bool IsSupervisorOption(string arg, out bool takesValue)
	{
		takesValue = true;

		if (string.Equals(arg, "-bisect-working-dir", StringComparison.OrdinalIgnoreCase))
			return true;

		if (string.Equals(arg, "-bisect-max-restarts", StringComparison.OrdinalIgnoreCase))
			return true;

		return false;
	}

	private static string QuoteIfNeeded(string arg)
	{
		if (arg.Contains(' '))
			return $"\"{arg.Replace("\"", "\\\"")}" + "\"";

		return arg;
	}

	private string ResolveAndValidateWorkingDirectory(string targetPath)
	{
		var workingDirectory = settings.BisectorWorkingDirectory;
		if (string.IsNullOrWhiteSpace(workingDirectory))
			workingDirectory = Environment.CurrentDirectory;
		else if (!Path.IsPathRooted(workingDirectory))
			workingDirectory = Path.GetFullPath(workingDirectory);

		if (string.IsNullOrWhiteSpace(workingDirectory))
			workingDirectory = Path.GetDirectoryName(targetPath) ?? Environment.CurrentDirectory;

		if (!Directory.Exists(workingDirectory))
			throw new InvalidOperationException($"Working directory does not exist: {workingDirectory}");

		return workingDirectory;
	}

	private int GetValidatedMaxRestarts()
	{
		var value = settings.BisectorMaxRestarts;
		if (value < 0)
			throw new InvalidOperationException("Invalid value for -bisect-max-restarts. Minimum is 0.");

		return value;
	}

	private Process StartTarget(string targetPath, string targetArguments, string workingDirectory)
	{
		var startInfo = new ProcessStartInfo
		{
			FileName = targetPath,
			Arguments = targetArguments,
			WorkingDirectory = workingDirectory,
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
		Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds:00.00} | [Supervisor] {status}");
	}
}
