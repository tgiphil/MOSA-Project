// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Platforms;

namespace Mosa.Utility.UnitTests;

internal static class Program
{
	private static void Main(string[] args)
	{
		RegisterPlatforms();

		var unitTestSystem = new UnitTestSystem();
		var returnCode = unitTestSystem.Start(args);

		Environment.Exit(returnCode);
	}

	private static void RegisterPlatforms()
	{
		PlatformRegistrations.Register();
	}
}
