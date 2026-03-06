// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;
using System.Text;

namespace Mosa.Compiler.Framework.Diagnostics;

/// <summary>
/// Diagnostics for tracking ToString() calls and their call stacks
/// </summary>
public static class StringificationDiagnostics
{
	private static long operandToStringCalls;
	private static long nodeToStringCalls;
	private static long lastReportedOperandCalls;
	private static long lastReportedNodeCalls;
	private static readonly object reportLock = new();
	private static DateTime lastReportTime = DateTime.UtcNow;
	private static readonly TimeSpan ReportInterval = TimeSpan.FromSeconds(3);

	// Stack trace sampling (capture every Nth call to avoid overhead)
	private const int SampleRate = 10000;
	private static readonly List<string> operandStackSamples = new();
	private static readonly List<string> nodeStackSamples = new();
	private const int MaxSamplesPerType = 50;

	public static void RecordOperandToString()
	{
		var count = Interlocked.Increment(ref operandToStringCalls);

		// Sample stack traces periodically
		if (count % SampleRate == 0 && operandStackSamples.Count < MaxSamplesPerType)
		{
			lock (operandStackSamples)
			{
				if (operandStackSamples.Count < MaxSamplesPerType)
				{
					operandStackSamples.Add(CaptureRelevantStackTrace());
				}
			}
		}

		CheckAndReport();
	}

	public static void RecordNodeToString()
	{
		var count = Interlocked.Increment(ref nodeToStringCalls);

		// Sample stack traces periodically
		if (count % SampleRate == 0 && nodeStackSamples.Count < MaxSamplesPerType)
		{
			lock (nodeStackSamples)
			{
				if (nodeStackSamples.Count < MaxSamplesPerType)
				{
					nodeStackSamples.Add(CaptureRelevantStackTrace());
				}
			}
		}

		CheckAndReport();
	}

	private static void CheckAndReport()
	{
		var now = DateTime.UtcNow;
		if (now - lastReportTime < ReportInterval)
			return;

		lock (reportLock)
		{
			// Double-check after acquiring lock
			now = DateTime.UtcNow;
			if (now - lastReportTime < ReportInterval)
				return;

			var currentOperand = Interlocked.Read(ref operandToStringCalls);
			var currentNode = Interlocked.Read(ref nodeToStringCalls);

			var operandDelta = currentOperand - lastReportedOperandCalls;
			var nodeDelta = currentNode - lastReportedNodeCalls;
			var elapsedSeconds = (now - lastReportTime).TotalSeconds;

			Console.WriteLine($"[StringificationDiagnostics] Operand.ToString: {currentOperand:N0} total ({operandDelta / elapsedSeconds:F0}/sec), Node.ToString: {currentNode:N0} total ({nodeDelta / elapsedSeconds:F0}/sec)");

			lastReportedOperandCalls = currentOperand;
			lastReportedNodeCalls = currentNode;
			lastReportTime = now;
		}
	}

	public static void Reset()
	{
		Interlocked.Exchange(ref operandToStringCalls, 0);
		Interlocked.Exchange(ref nodeToStringCalls, 0);
		lastReportedOperandCalls = 0;
		lastReportedNodeCalls = 0;
		lastReportTime = DateTime.UtcNow;

		lock (operandStackSamples)
		{
			operandStackSamples.Clear();
		}

		lock (nodeStackSamples)
		{
			nodeStackSamples.Clear();
		}
	}

	public static string GetSummary()
	{
		var sb = new StringBuilder();
		var totalOperand = Interlocked.Read(ref operandToStringCalls);
		var totalNode = Interlocked.Read(ref nodeToStringCalls);

		sb.AppendLine();
		sb.AppendLine("=== Stringification Diagnostics Summary ===");
		sb.AppendLine($"Operand.ToString() calls: {totalOperand:N0}");
		sb.AppendLine($"Node.ToString() calls: {totalNode:N0}");
		sb.AppendLine();

		if (operandStackSamples.Count > 0)
		{
			sb.AppendLine($"Operand.ToString() sampled call stacks ({operandStackSamples.Count} samples):");
			lock (operandStackSamples)
			{
				var grouped = operandStackSamples
					.GroupBy(s => s)
					.OrderByDescending(g => g.Count())
					.Take(10);

				foreach (var group in grouped)
				{
					sb.AppendLine($"  Count: {group.Count()}");
					sb.AppendLine($"  {group.Key}");
					sb.AppendLine();
				}
			}
		}

		if (nodeStackSamples.Count > 0)
		{
			sb.AppendLine($"Node.ToString() sampled call stacks ({nodeStackSamples.Count} samples):");
			lock (nodeStackSamples)
			{
				var grouped = nodeStackSamples
					.GroupBy(s => s)
					.OrderByDescending(g => g.Count())
					.Take(10);

				foreach (var group in grouped)
				{
					sb.AppendLine($"  Count: {group.Count()}");
					sb.AppendLine($"  {group.Key}");
					sb.AppendLine();
				}
			}
		}

		return sb.ToString();
	}

	private static string CaptureRelevantStackTrace()
	{
		var stackTrace = new StackTrace(2, false); // Skip RecordXXX and ToString methods
		var frames = stackTrace.GetFrames();

		if (frames == null || frames.Length == 0)
			return "<no stack>";

		// Take up to 5 relevant frames, filtering out system frames
		var relevantFrames = frames
			.Where(f =>
			{
				var method = f.GetMethod();
				if (method == null)
					return false;

				var declaringType = method.DeclaringType;
				if (declaringType == null)
					return false;

				var ns = declaringType.Namespace ?? string.Empty;

				// Include Mosa.Compiler frames, exclude System/Microsoft/etc
				return ns.StartsWith("Mosa.Compiler");
			})
			.Take(5)
			.Select(f =>
			{
				var method = f.GetMethod();
				return $"{method?.DeclaringType?.Name}.{method?.Name}";
			})
			.ToArray();

		return relevantFrames.Length > 0
			? string.Join(" <- ", relevantFrames)
			: "<no relevant frames>";
	}
}
