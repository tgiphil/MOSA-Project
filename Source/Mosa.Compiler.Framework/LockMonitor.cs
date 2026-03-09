// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;

namespace Mosa.Compiler.Framework;

/// <summary>
/// Lock Monitor
/// </summary>
public sealed class LockMonitor
{
	public delegate void PostEventDelegate(CompilerEvent compilerEvent, string message);

	private const long LockWaitWarningThresholdMs = 15;
	private const long LockReportIntervalTicks = TimeSpan.TicksPerSecond * 2;

	private struct LockStats
	{
		public long Count;
		public long TotalWaitMs;
		public long PeakWaitMs;
	}

	private readonly Stopwatch lockMonitorTimer = Stopwatch.StartNew();
	private long lockMonitorLastReportTicks;
	private long lockMonitorContentionCount;
	private readonly Dictionary<string, LockStats> lockMonitorStats = new();
	private readonly PostEventDelegate postEvent;

	private readonly object _lock = new();

	public LockMonitor(PostEventDelegate postEvent)
	{
		this.postEvent = postEvent;
	}

	public void RecordLockWait(string lockName, Stopwatch lockTimer)
	{
		var waitMs = lockTimer.ElapsedMilliseconds;

		if (waitMs < LockWaitWarningThresholdMs)
			return;

		bool shouldReport = false;
		LockStats currentStats;

		lock (_lock)
		{
			if (!lockMonitorStats.TryGetValue(lockName, out var stats))
			{
				stats = new LockStats();
			}

			stats.Count++;
			stats.TotalWaitMs += waitMs;
			if (waitMs > stats.PeakWaitMs)
				stats.PeakWaitMs = waitMs;

			lockMonitorStats[lockName] = stats;
			currentStats = stats;

			Interlocked.Increment(ref lockMonitorContentionCount);

			var currentTicks = lockMonitorTimer.ElapsedTicks;
			var lastReport = Interlocked.Read(ref lockMonitorLastReportTicks);
			if (currentTicks - lastReport >= LockReportIntervalTicks)
			{
				if (Interlocked.CompareExchange(ref lockMonitorLastReportTicks, currentTicks, lastReport) == lastReport)
				{
					shouldReport = true;
				}
			}
		}

		if (shouldReport)
		{
			ReportLockContention(lockName, waitMs, currentStats);
		}
	}

	public string GetLockContentionSummary()
	{
		if (lockMonitorContentionCount == 0)
			return string.Empty;

		lock (_lock)
		{
			var summary = "Lock Contention Summary:\n";
			foreach (var kvp in lockMonitorStats.OrderByDescending(x => x.Value.TotalWaitMs))
			{
				var avgWaitMs = kvp.Value.Count > 0 ? kvp.Value.TotalWaitMs / (double)kvp.Value.Count : 0;
				summary += $"  {kvp.Key}: Count={kvp.Value.Count}, Avg={avgWaitMs:F2}ms, Peak={kvp.Value.PeakWaitMs}ms\n";
			}
			return summary;
		}
	}

	private void ReportLockContention(string lockName, long currentWaitMs, LockStats stats)
	{
		var elapsedSeconds = lockMonitorTimer.ElapsedTicks / (double)Stopwatch.Frequency;
		var avgWaitMs = stats.Count > 0 ? stats.TotalWaitMs / (double)stats.Count : 0;
		var rate = elapsedSeconds > 0 ? stats.Count / elapsedSeconds : 0;

		postEvent(
			CompilerEvent.Debug,
			$"[Lock Contention] {lockName} | Current: {currentWaitMs}ms | Peak: {stats.PeakWaitMs}ms | " +
			$"Avg: {avgWaitMs:F1}ms | Count: {stats.Count} ({rate:F1}/s) | " +
			$"Elapsed: {elapsedSeconds:F1}s"
		);
	}
}
