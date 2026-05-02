// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Mosa.Utility.Configuration;

namespace Mosa.Utility.UnitTestBisector.Supervisor;

internal sealed class ProcessSupervisor
{
	private const int PollIntervalMs = 2000;
	private const int RestartDelayMs = 3000;
	private const long MinimumFreeMemoryForLimitMB = 8L * 1024L;

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
		var startupFreeMemoryMB = GetStartupAvailablePhysicalMemoryInMB();
		var maxMemoryMB = GetStartupMemoryLimitInMB(maxMemoryPercent, startupFreeMemoryMB);
		var maxRestarts = GetValidatedMaxRestarts();

		stopwatch.Start();
		OutputStatus("Supervisor started");
		OutputStatus($"Target: {targetPath}");
		OutputStatus($"Target Arguments: {targetArguments}");
		OutputStatus($"Working Directory: {workingDirectory}");
		OutputStatus($"Poll Interval: {PollIntervalMs} ms");
		OutputStatus($"Restart Delay: {RestartDelayMs} ms");
		if (maxMemoryMB <= 0)
			OutputStatus($"Memory Limit: disabled (startup free physical memory {startupFreeMemoryMB} MB is below {MinimumFreeMemoryForLimitMB} MB)");
		else
			OutputStatus($"Memory Limit: {maxMemoryPercent}% of startup free physical memory ({startupFreeMemoryMB} MB -> {maxMemoryMB} MB)");
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
		settings.LoadAppLocations();

		var discoveredTargetPath = settings.BisectorPersistentApp;
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

	private static long GetStartupAvailablePhysicalMemoryInMB()
	{
		var freeBytes = GetAvailablePhysicalMemoryBytes();
		if (freeBytes <= 0)
			throw new InvalidOperationException("Unable to determine available physical memory at startup.");

		return freeBytes / (1024 * 1024);
	}

	private static long GetAvailablePhysicalMemoryBytes()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var memoryStatus = new MemoryStatusEx();
			if (!GlobalMemoryStatusEx(memoryStatus))
				throw new InvalidOperationException("Unable to query physical memory using GlobalMemoryStatusEx.");

			return (long)memoryStatus.AvailPhys;
		}

		var info = GC.GetGCMemoryInfo();
		if (info.TotalAvailableMemoryBytes <= 0)
			throw new InvalidOperationException("Unable to determine available memory at startup.");

		return info.TotalAvailableMemoryBytes;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private sealed class MemoryStatusEx
	{
		public uint Length = (uint)Marshal.SizeOf<MemoryStatusEx>();
		public uint MemoryLoad;
		public ulong TotalPhys;
		public ulong AvailPhys;
		public ulong TotalPageFile;
		public ulong AvailPageFile;
		public ulong TotalVirtual;
		public ulong AvailVirtual;
		public ulong AvailExtendedVirtual;
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx lpBuffer);

	private static long GetStartupMemoryLimitInMB(int percent, long startupFreeMemoryMB)
	{
		if (startupFreeMemoryMB < MinimumFreeMemoryForLimitMB)
			return 0;

		var limitMB = startupFreeMemoryMB * percent / 100;
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

	private static bool IsMemoryExceeded(Process process, long maxMemoryMB, out long memoryMB)
	{
		memoryMB = 0;
		if (maxMemoryMB <= 0)
			return false;

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
