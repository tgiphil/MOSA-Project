// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mosa.Compiler.Framework;

/// <summary>
/// Helper for monitoring lock contention across the compiler pipeline.
/// </summary>
internal static class LockMonitor
{
	private const long MonitoringThresholdTicks = TimeSpan.TicksPerSecond * 0; // 0 seconds
	private const long WaitWarningThresholdMs = 15; // Warn if lock wait > 15ms
	private const long ReportIntervalTicks = TimeSpan.TicksPerSecond * 5; // Report every 5 seconds

	private static readonly Stopwatch GlobalTimer = Stopwatch.StartNew();

	private static long lastReportTicks;
	private static long contentionCount;
	private static readonly Dictionary<string, LockStats> lockStats = new();
	private static readonly object statsLock = new();

	private struct LockStats
	{
		public long Count;
		public long TotalWaitMs;
		public long PeakWaitMs;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool ShouldMonitor()
	{
		return GlobalTimer.ElapsedTicks >= MonitoringThresholdTicks;
	}

	public static void RecordLockWait(string lockName, Stopwatch lockTimer, Compiler compiler)
	{
		if (!ShouldMonitor())
			return;

		var waitMs = lockTimer.ElapsedMilliseconds;

		if (waitMs < WaitWarningThresholdMs)
			return;

		bool shouldReport = false;
		LockStats currentStats;

		lock (statsLock)
		{
			if (!lockStats.TryGetValue(lockName, out var stats))
			{
				stats = new LockStats();
			}

			stats.Count++;
			stats.TotalWaitMs += waitMs;
			if (waitMs > stats.PeakWaitMs)
				stats.PeakWaitMs = waitMs;

			lockStats[lockName] = stats;
			currentStats = stats;

			Interlocked.Increment(ref contentionCount);

			var currentTicks = GlobalTimer.ElapsedTicks;
			var lastReport = Interlocked.Read(ref lastReportTicks);
			if (currentTicks - lastReport >= ReportIntervalTicks)
			{
				if (Interlocked.CompareExchange(ref lastReportTicks, currentTicks, lastReport) == lastReport)
				{
					shouldReport = true;
				}
			}
		}

		if (shouldReport)
		{
			ReportContention(lockName, waitMs, currentStats, compiler);
		}
	}

	private static void ReportContention(string lockName, long currentWaitMs, LockStats stats, Compiler compiler)
	{
		if (compiler == null)
			return;

		var elapsedSeconds = (GlobalTimer.ElapsedTicks - MonitoringThresholdTicks) / (double)Stopwatch.Frequency;
		var avgWaitMs = stats.Count > 0 ? stats.TotalWaitMs / (double)stats.Count : 0;
		var rate = elapsedSeconds > 0 ? stats.Count / elapsedSeconds : 0;

		compiler.PostEvent(
			CompilerEvent.Debug,
			$"[Lock Contention] {lockName} | Current: {currentWaitMs}ms | Peak: {stats.PeakWaitMs}ms | " +
			$"Avg: {avgWaitMs:F1}ms | Count: {stats.Count} ({rate:F1}/s) | " +
			$"Time since 30s: {elapsedSeconds:F1}s"
		);
	}

	public static string GetSummary()
	{
		if (contentionCount == 0)
			return "";

		lock (statsLock)
		{
			var summary = "Lock Contention Summary:\n";
			foreach (var kvp in lockStats.OrderByDescending(x => x.Value.TotalWaitMs))
			{
				var avgWaitMs = kvp.Value.Count > 0 ? kvp.Value.TotalWaitMs / (double)kvp.Value.Count : 0;
				summary += $"  {kvp.Key}: Count={kvp.Value.Count}, Avg={avgWaitMs:F2}ms, Peak={kvp.Value.PeakWaitMs}ms\n";
			}
			return summary;
		}
	}
}
