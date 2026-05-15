// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace Mosa.Utility.UnitTestBisector.Supervisor;

internal static class Program
{
	private static void Main(string[] args)
	{
		var supervisor = new ProcessSupervisor();

		var returnCode = supervisor.Start(args);

		Environment.Exit(returnCode);
	}
}
