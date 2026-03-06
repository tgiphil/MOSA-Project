// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.MosaTypeSystem;
using System.Diagnostics;

namespace Mosa.Compiler.Framework;

/// <summary>
/// Schedules compilation of types/methods.
/// </summary>
public sealed class MethodScheduler
{
	#region Data Members

	public Compiler Compiler;

	private readonly HashSet<MethodData> workingSet = new HashSet<MethodData>();

	private readonly PriorityQueue<MethodData, int> queue = new PriorityQueue<MethodData, int>();

	private readonly HashSet<MethodData> queueSet = new HashSet<MethodData>();

	private int totalMethods;
	private int totalQueued;

	// Queue profiling metrics
	private int peakQueueSize;
	private long queueEmptyCount;
	private long totalDequeueOperations;
	private long totalEnqueueOperations;
	private readonly Stopwatch queueProfileTimer = Stopwatch.StartNew();
	private long lastQueueReportTicks;
	private long lastReportedDequeueCount;
	private long lastReportedEnqueueCount;
	private const long QueueReportIntervalTicks = TimeSpan.TicksPerSecond * 2; // Report every 2 seconds
	private const int QueueReportIntervalOperations = 200; // Report every 200 completed work items

	// CPU monitoring
	private readonly Process currentProcess = Process.GetCurrentProcess();
	private TimeSpan lastCpuTime;
	private long lastCpuCheckTicks;
	private readonly int processorCount = Environment.ProcessorCount;

	// Reference to pipeline pool for tracking active workers
	private PipelinePool pipelinePool;

	#endregion Data Members

	#region Properties

	public int PassCount { get; }

	/// <summary>
	/// Gets the total methods.
	/// </summary>
	/// <value>
	/// The total methods.
	/// </value>
	public int TotalMethods => totalMethods;

	/// <summary>
	/// Gets the queued methods.
	/// </summary>
	/// <value>
	/// The queued methods.
	/// </value>
	public int TotalQueuedMethods => totalQueued;

	/// <summary>
	/// Gets the peak queue size observed.
	/// </summary>
	public int PeakQueueSize => peakQueueSize;

	/// <summary>
	/// Gets the number of times the queue became empty.
	/// </summary>
	public long QueueEmptyCount => queueEmptyCount;

	/// <summary>
	/// Gets the total number of dequeue operations.
	/// </summary>
	public long TotalDequeueOperations => totalDequeueOperations;

	/// <summary>
	/// Gets the total number of enqueue operations.
	/// </summary>
	public long TotalEnqueueOperations => totalEnqueueOperations;

	#endregion Properties

	public MethodScheduler(Compiler compiler)
	{
		Compiler = compiler;
		PassCount = 0;
		lastQueueReportTicks = queueProfileTimer.ElapsedTicks;
		lastReportedDequeueCount = 0;
		lastReportedEnqueueCount = 0;
		lastCpuTime = currentProcess.TotalProcessorTime;
		lastCpuCheckTicks = queueProfileTimer.ElapsedTicks;
	}

	/// <summary>
	/// Sets the pipeline pool reference for tracking active workers.
	/// </summary>
	internal void SetPipelinePool(PipelinePool pool)
	{
		pipelinePool = pool;
	}

	public void ScheduleAll(TypeSystem typeSystem)
	{
		foreach (var type in typeSystem.AllTypes)
		{
			Schedule(type);
		}
	}

	public bool IsCompilable(MosaType type)
	{
		if (type.IsModule)
			return false;

		if (type.IsInterface)
			return false;

		if (type.HasOpenGenericParams || type.IsPointer)
			return false;

		return true;
	}

	public bool IsCompilable(MosaMethod method)
	{
		if (method.IsAbstract && !method.HasImplementation)
			return false;

		if (method.HasOpenGenericParams)
			return false;

		if (method.IsCompilerGenerated)
			return false;

		return true;
	}

	public void Schedule(MosaType type)
	{
		if (!IsCompilable(type))
			return;

		foreach (var method in type.Methods)
		{
			Schedule(method);
		}
	}

	public void Schedule(MosaMethod method)
	{
		if (!IsCompilable(method))
			return;

		AddToQueue(method);
	}

	public void AddToQueue(MosaMethod method)
	{
		var methodData = Compiler.GetMethodData(method);
		Add(methodData);
	}

	public void Add(MethodData methodData)
	{
		int queueSize;

		lock (queue)
		{
			AddInsideLock(methodData);
			queueSize = totalQueued;
		}

		UpdateQueueMetrics(queueSize);
		SignalEnqueued();
	}

	public void Add(HashSet<MosaMethod> methods)
	{
		int queueSize;

		lock (queue)
		{
			foreach (var method in methods)
			{
				var methodData = Compiler.GetMethodData(method);

				AddInsideLock(methodData);
			}

			queueSize = totalQueued;
		}

		UpdateQueueMetrics(queueSize);
		SignalEnqueued();
	}

	private void AddInsideLock(MethodData methodData)
	{
		if (!workingSet.Contains(methodData))
		{
			workingSet.Add(methodData);

			Interlocked.Increment(ref totalMethods);
		}

		if (queueSet.Contains(methodData))
			return; // already queued

		var priority = GetCompilePriorityLevel(methodData);

		queue.Enqueue(methodData, priority);
		queueSet.Add(methodData);

		Interlocked.Increment(ref totalQueued);
		Interlocked.Increment(ref totalEnqueueOperations);
	}

	public MethodData Get()
	{
		MethodData methodData;
		int queueSize;
		bool wasEmpty = false;

		lock (queue)
		{
			if (queue.TryDequeue(out methodData, out var priority))
			{
				queueSet.Remove(methodData);

				Interlocked.Decrement(ref totalQueued);
				Interlocked.Increment(ref totalDequeueOperations);

				queueSize = totalQueued;
			}
			else
			{
				queueSize = 0;
				wasEmpty = true;
			}
		}

		if (wasEmpty)
		{
			Interlocked.Increment(ref queueEmptyCount);
			ReportQueueStarvation();
		}

		UpdateQueueMetrics(queueSize);

		return methodData;
	}

	private void UpdateQueueMetrics(int currentQueueSize)
	{
		// Update peak queue size
		int currentPeak = peakQueueSize;
		while (currentQueueSize > currentPeak)
		{
			var original = Interlocked.CompareExchange(ref peakQueueSize, currentQueueSize, currentPeak);
			if (original == currentPeak)
				break;
			currentPeak = original;
		}

		// Periodic queue status reporting - trigger on EITHER time OR operation count
		var currentTicks = queueProfileTimer.ElapsedTicks;
		var currentDequeueCount = totalDequeueOperations;
		var operationsSinceLastReport = currentDequeueCount - lastReportedDequeueCount;

		var timeThresholdMet = currentTicks - lastQueueReportTicks >= QueueReportIntervalTicks;
		var countThresholdMet = operationsSinceLastReport >= QueueReportIntervalOperations;

		if (timeThresholdMet || countThresholdMet)
		{
			// Use CompareExchange to ensure only one thread reports (thread-safe)
			var wasLastReportTicks = Interlocked.Read(ref lastQueueReportTicks);
			if (Interlocked.CompareExchange(ref lastQueueReportTicks, currentTicks, wasLastReportTicks) == wasLastReportTicks)
			{
				var previousDequeueCount = Interlocked.Exchange(ref lastReportedDequeueCount, currentDequeueCount);
				var currentEnqueueCount = totalEnqueueOperations;
				var previousEnqueueCount = Interlocked.Exchange(ref lastReportedEnqueueCount, currentEnqueueCount);
				
				ReportQueueStatus(currentQueueSize, currentTicks, wasLastReportTicks, 
					currentDequeueCount, previousDequeueCount,
					currentEnqueueCount, previousEnqueueCount);
			}
		}
	}

	private void ReportQueueStatus(int currentQueueSize, long currentTicks, long previousTicks,
		long currentDequeueCount, long previousDequeueCount,
		long currentEnqueueCount, long previousEnqueueCount)
	{
		// Calculate instantaneous rates (since last report)
		var ticksDelta = currentTicks - previousTicks;
		var secondsDelta = ticksDelta / (double)Stopwatch.Frequency;

		var dequeueDelta = currentDequeueCount - previousDequeueCount;
		var enqueueDelta = currentEnqueueCount - previousEnqueueCount;

		var dequeueRate = secondsDelta > 0 ? dequeueDelta / secondsDelta : 0;
		var enqueueRate = secondsDelta > 0 ? enqueueDelta / secondsDelta : 0;

		var activeWorkers = pipelinePool?.ActiveWorkers ?? 0;
		var maxWorkers = pipelinePool?.MaxWorkers ?? 0;
		var utilizationPercent = maxWorkers > 0 ? (activeWorkers * 100.0 / maxWorkers) : 0;
		var idleWorkers = maxWorkers - activeWorkers;

		// Calculate CPU usage with equivalent core count
		var cpuPercent = CalculateCpuUsage(currentTicks);
		var equivalentCores = (cpuPercent * processorCount) / 100.0;

		Compiler.PostEvent(
			CompilerEvent.DebugInfo,
			$"[Queue] Size: {currentQueueSize} | Peak: {peakQueueSize} | Empty Events: {queueEmptyCount} | " +
			$"Active: {activeWorkers}/{maxWorkers} ({utilizationPercent:F1}%) | Idle: {idleWorkers} | " +
			$"Enqueue: {enqueueRate:F1}/s | Dequeue: {dequeueRate:F1}/s | " +
			$"CPU: {cpuPercent:F1}% ({equivalentCores:F1}/{processorCount} cores)"
		);
	}

	private double CalculateCpuUsage(long currentTicks)
	{
		try
		{
			currentProcess.Refresh();
			var currentCpuTime = currentProcess.TotalProcessorTime;
			var cpuTimeDelta = (currentCpuTime - lastCpuTime).TotalMilliseconds;
			
			var ticksDelta = currentTicks - lastCpuCheckTicks;
			var wallTimeDelta = (ticksDelta / (double)Stopwatch.Frequency) * 1000.0; // Convert to milliseconds

			lastCpuTime = currentCpuTime;
			lastCpuCheckTicks = currentTicks;

			if (wallTimeDelta > 0 && wallTimeDelta < 60000) // Sanity check: < 60 seconds
			{
				// CPU percentage divided by cores to match Task Manager (0-100% scale)
				var cpuPercent = (cpuTimeDelta / wallTimeDelta / processorCount) * 100.0;
				return Math.Clamp(cpuPercent, 0.0, 100.0);
			}
		}
		catch
		{
			// Ignore any errors in CPU calculation
		}

		return 0.0;
	}

	private void ReportQueueStarvation()
	{
		var activeWorkers = pipelinePool?.ActiveWorkers ?? 0;
		var maxWorkers = pipelinePool?.MaxWorkers ?? 0;
		var idleWorkers = maxWorkers - activeWorkers;

		// Get current CPU usage for starvation report
		var cpuPercent = CalculateCpuUsage(queueProfileTimer.ElapsedTicks);
		var equivalentCores = (cpuPercent * processorCount) / 100.0;

		Compiler.PostEvent(
			CompilerEvent.Warning,
			$"[Queue Starvation] Queue became empty! " +
			$"Active: {activeWorkers}/{maxWorkers} | Idle: {idleWorkers} threads waiting for work | " +
			$"Empty count: {queueEmptyCount} | Peak size was: {peakQueueSize} | " +
			$"Total methods: {totalMethods} | Completed: {totalDequeueOperations} | " +
			$"CPU: {cpuPercent:F1}% ({equivalentCores:F1}/{processorCount} cores)"
		);
	}

	public string GetQueueStatisticsSummary()
	{
		var elapsedSeconds = queueProfileTimer.Elapsed.TotalSeconds;
		var avgEnqueueRate = totalEnqueueOperations / elapsedSeconds;
		var avgDequeueRate = totalDequeueOperations / elapsedSeconds;

		var maxWorkers = pipelinePool?.MaxWorkers ?? 0;
		var avgUtilization = maxWorkers > 0 && elapsedSeconds > 0
			? (totalDequeueOperations / (maxWorkers * elapsedSeconds)) * 100.0
			: 0;

		return $"Queue Statistics Summary:\n" +
			   $"  Total Methods Scheduled: {totalMethods}\n" +
			   $"  Peak Queue Size: {peakQueueSize}\n" +
			   $"  Queue Empty Events: {queueEmptyCount}\n" +
			   $"  Total Enqueue Operations: {totalEnqueueOperations}\n" +
			   $"  Total Dequeue Operations: {totalDequeueOperations}\n" +
			   $"  Average Enqueue Rate: {avgEnqueueRate:F2} methods/sec\n" +
			   $"  Average Dequeue Rate: {avgDequeueRate:F2} methods/sec\n" +
			   $"  Worker Threads: {maxWorkers}\n" +
			   $"  Average Worker Utilization: {avgUtilization:F1}%\n" +
			   $"  Total Elapsed Time: {elapsedSeconds:F2} seconds";
	}

	private static int GetCompilePriorityLevel(MethodData methodData)
	{
		if (methodData.DoNotInline)
			return 200;

		var adjustment = 0;

		if (methodData.HasAggressiveInliningAttribute)
			adjustment += 75;

		if (methodData.Inlined)
			adjustment += 20;

		if (methodData.Method.DeclaringType.IsValueType)
			adjustment += 15;

		if (methodData.Method.IsStatic)
			adjustment += 5;

		if (methodData.HasProtectedRegions)
			adjustment -= 10;

		if (methodData.VirtualCodeSize > 100)
			adjustment -= 75;

		if (methodData.VirtualCodeSize > 50)
			adjustment -= 50;

		if (methodData.VirtualCodeSize < 50)
			adjustment += 5;

		if (methodData.VirtualCodeSize < 30)
			adjustment += 5;

		if (methodData.VirtualCodeSize < 10)
			adjustment += 10;

		if (methodData.AggressiveInlineRequested)
			adjustment += 20;

		if (methodData.Method.IsConstructor)
			adjustment += 10;

		if (methodData.Method.IsTypeConstructor)
			adjustment += 3;

		if (methodData.Version > 3)
			adjustment -= 7;

		if (methodData.Version > 5)
			adjustment -= 15;

		//if (methodData.Method.FullName.StartsWith("System."))
		//	adjustment += 5;

		return 100 - adjustment;
	}

	#region Subscription

	private Action? _onEnqueued;

	public IDisposable Subscribe(Action onEnqueued)
	{
		_onEnqueued += onEnqueued;
		return new Unsubscriber(() => _onEnqueued -= onEnqueued);
	}

	private void SignalEnqueued() => _onEnqueued?.Invoke();

	private sealed class Unsubscriber : IDisposable
	{
		private readonly Action _dispose;

		public Unsubscriber(Action dispose) => _dispose = dispose;

		public void Dispose() => _dispose();
	}

	#endregion Subscription
}
