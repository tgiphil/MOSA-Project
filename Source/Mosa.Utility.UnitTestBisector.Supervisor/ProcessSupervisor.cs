// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;
using Mosa.Utility.Configuration;

namespace Mosa.Utility.UnitTestBisector.Supervisor;

internal sealed class ProcessSupervisor
{
	private const int PollIntervalMs = 2000;
	private const int RestartDelayMs = 3000;

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
		var maxMemoryPercent = GetValidatedMaxMemoryPercent();
		var maxMemoryMB = GetStartupMemoryLimitInMB(maxMemoryPercent);
		var maxRestarts = GetValidatedMaxRestarts();

		stopwatch.Start();
		OutputStatus("Supervisor started");
		OutputStatus($"Target: {targetPath}");
		OutputStatus($"Target Arguments: {targetArguments}");
		OutputStatus($"Working Directory: {workingDirectory}");
		OutputStatus($"Poll Interval: {PollIntervalMs} ms");
		OutputStatus($"Restart Delay: {RestartDelayMs} ms");
		OutputStatus($"Memory Limit: {maxMemoryPercent}% of startup free memory ({maxMemoryMB} MB)");
		OutputStatus($"Max Restarts: {(maxRestarts <= 0 ? "unlimited" : maxRestarts)}");

		while (true)
		{
			using var process = StartTarget(targetPath, targetArguments, workingDirectory);

			while (true)
			{
				if (process.HasExited)
				{
					OutputStatus($"Target exited with code {process.ExitCode}");
					break;
				}

				if (IsMemoryExceeded(process, maxMemoryMB, out var memoryMB))
				{
					OutputStatus($"Memory limit exceeded: {memoryMB} MB > {maxMemoryMB} MB. Terminating target.");
					TryTerminate(process);
					break;
				}

				Thread.Sleep(PollIntervalMs);
			}

			restartCount++;
			if (maxRestarts > 0 && restartCount > maxRestarts)
			{
				OutputStatus("Maximum restart count reached. Exiting supervisor.");
				return 0;
			}

			Thread.Sleep(RestartDelayMs);

			OutputStatus($"Restarting target (restart #{restartCount})...");
		}
	}

	private string ResolveAndValidateTargetPath()
	{
		var configuredTargetPath = settings.BisectorSupervisorTargetPath;

		if (!string.IsNullOrWhiteSpace(configuredTargetPath))
		{
			var resolvedConfiguredTargetPath = Path.IsPathRooted(configuredTargetPath)
				? configuredTargetPath
				: Path.GetFullPath(configuredTargetPath);

			if (File.Exists(resolvedConfiguredTargetPath))
				return resolvedConfiguredTargetPath;
		}

		settings.LoadAppLocations();

		var discoveredTargetPath = settings.BisectorPersistentApp;
		if (!string.IsNullOrWhiteSpace(discoveredTargetPath))
		{
			var resolvedDiscoveredTargetPath = Path.IsPathRooted(discoveredTargetPath)
				? discoveredTargetPath
				: Path.GetFullPath(discoveredTargetPath);

			if (File.Exists(resolvedDiscoveredTargetPath))
				return resolvedDiscoveredTargetPath;
		}

		if (!string.IsNullOrWhiteSpace(configuredTargetPath))
		{
			var resolvedConfiguredTargetPath = Path.IsPathRooted(configuredTargetPath)
				? configuredTargetPath
				: Path.GetFullPath(configuredTargetPath);

			throw new InvalidOperationException($"Target does not exist: {resolvedConfiguredTargetPath}");
		}

		throw new InvalidOperationException("Missing target path. Use -bisect-target.");
	}

	private string BuildTargetArguments()
	{
		var forwarded = new List<string>();

		for (var i = 0; i < args.Length; i++)
		{
			var arg = args[i];

			if (IsSupervisorOption(arg, out var takesValue))
			{
				if (takesValue && i + 1 < args.Length)
					i++;
				continue;
			}

			forwarded.Add(QuoteIfNeeded(arg));
		}

		return string.Join(" ", forwarded);
	}

	private static bool IsSupervisorOption(string arg, out bool takesValue)
	{
		takesValue = true;

		if (string.Equals(arg, "-bisect-target", StringComparison.OrdinalIgnoreCase))
			return true;

		if (string.Equals(arg, "-bisect-working-dir", StringComparison.OrdinalIgnoreCase))
			return true;

		if (string.Equals(arg, "-bisect-max-restarts", StringComparison.OrdinalIgnoreCase))
			return true;

		if (string.Equals(arg, "-bisect-max-memory-percent", StringComparison.OrdinalIgnoreCase))
			return true;

		return false;
	}

	private static string QuoteIfNeeded(string arg)
	{
		if (arg.Contains(' '))
			return $"\"{arg.Replace("\"", "\\\"")}\"";

		return arg;
	}

	private string ResolveAndValidateWorkingDirectory(string targetPath)
	{
		var workingDirectory = settings.BisectorSupervisorWorkingDirectory;
		if (string.IsNullOrWhiteSpace(workingDirectory))
			workingDirectory = Path.GetDirectoryName(targetPath) ?? Environment.CurrentDirectory;
		else if (!Path.IsPathRooted(workingDirectory))
			workingDirectory = Path.GetFullPath(workingDirectory);

		if (!Directory.Exists(workingDirectory))
			throw new InvalidOperationException($"Working directory does not exist: {workingDirectory}");

		return workingDirectory;
	}

	private int GetValidatedMaxMemoryPercent()
	{
		var value = settings.BisectorSupervisorMaxMemoryPercent;
		if (value <= 0 || value > 100)
			throw new InvalidOperationException("Invalid value for -bisect-max-memory-percent. Range is 1-100.");

		return value;
	}

	private int GetValidatedMaxRestarts()
	{
		var value = settings.BisectorSupervisorMaxRestarts;
		if (value < 0)
			throw new InvalidOperationException("Invalid value for -bisect-max-restarts. Minimum is 0.");

		return value;
	}

	private static long GetStartupMemoryLimitInMB(int percent)
	{
		var freeBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
		if (freeBytes <= 0)
			throw new InvalidOperationException("Unable to determine available memory at startup.");

		var limitBytes = freeBytes * percent / 100;
		var limitMB = limitBytes / (1024 * 1024);

		return Math.Max(1, limitMB);
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

		OutputStatus($"Target started (PID: {process.Id})");
		return process;
	}

	private static bool IsMemoryExceeded(Process process, long maxMemoryMB, out long memoryMB)
	{
		memoryMB = 0;

		try
		{
			process.Refresh();
			memoryMB = process.WorkingSet64 / (1024 * 1024);
			return memoryMB > maxMemoryMB;
		}
		catch
		{
			return false;
		}
	}

	private static void TryTerminate(Process process)
	{
		try
		{
			if (!process.HasExited)
			{
				process.Kill(entireProcessTree: true);
				process.WaitForExit(5000);
			}
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
