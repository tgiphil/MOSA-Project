// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Collections;
using System.Diagnostics;

namespace Mosa.Compiler.Framework;

/// <summary>
/// Lock Monitor
/// </summary>
public sealed class LockMonitor
{
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
	private readonly Compiler compiler;

	private readonly object _lock = new();

	public LockMonitor(Compiler compiler)
	{
		this.compiler = compiler;
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
			ReportLockContention(lockName, currentStats);
		}
	}

	public void GetLockContentionSummary(long waitThresholdMs)
	{
		var snapshot = lockMonitorStats
			.Where(x => x.Value.TotalWaitMs > waitThresholdMs)
			.OrderByDescending(x => x.Value.TotalWaitMs)
			.ToList();

		if (snapshot.Count == 0)
			return;

		compiler.PostEvent(CompilerEvent.Debug, "Lock Contention Summary:");

		foreach (var kvp in snapshot)
		{
			var stats = kvp.Value;
			var lockName = kvp.Key;

			ReportLockContention(lockName, stats);
		}
	}

	private void ReportLockContention(string lockName, LockStats stats)
	{
		var avgWaitMs = stats.Count > 0 ? stats.TotalWaitMs / (double)stats.Count : 0;

		compiler.PostEvent(
			CompilerEvent.Debug,
			$"[Lock Contention] Count: {stats.Count} | Peak: {stats.PeakWaitMs}ms | Avg: {avgWaitMs:F1}ms | Wait: {stats.TotalWaitMs}ms -> {lockName}");
	}
}
