// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace Mosa.Utility.UnitTestBisector.Supervisor;

internal readonly struct StateSnapshot(bool found, bool completed, int iterationNumber, int totalIterationCount, int passCount, int nextIndex, string lastExitKind, int lastExitCode)
{
	public bool Found { get; } = found;

	public bool Completed { get; } = completed;

	public int IterationNumber { get; } = iterationNumber;

	public int TotalIterationCount { get; } = totalIterationCount;

	public int PassCount { get; } = passCount;

	public int NextIndex { get; } = nextIndex;

	public string LastExitKind { get; } = lastExitKind;

	public int LastExitCode { get; } = lastExitCode;
}
