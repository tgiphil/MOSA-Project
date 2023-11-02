// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;
using Mosa.Compiler.Framework;
using Mosa.Compiler.MosaTypeSystem.CLR;
using Mosa.Utility.Configuration;

namespace Mosa.Tool.Compiler;

/// <summary>
/// Compiler
/// </summary>
public class Compiler
{
	#region Data

	private static Stopwatch Stopwatch = new Stopwatch();

	#endregion Data

	#region Public Methods

	/// <summary>
	/// Runs the command line parser and the compilation process.
	/// </summary>
	/// <param name="args">The command line arguments.</param>
	public int Run(string[] args)
	{
		RegisterPlatforms();

		// always print header with version information
		Console.WriteLine("MOSA Compiler, Version {0}.", CompilerVersion.VersionString);
		Console.WriteLine("Copyright 2023 by the MOSA Project. Licensed under the New BSD License.");

		Output($"Current Directory: {Environment.CurrentDirectory}");

		Stopwatch.Start();

		try
		{
			var mosaSettings = new MosaSettings();

			mosaSettings.LoadAppLocations();
			mosaSettings.SetDetfaultSettings();
			mosaSettings.LoadArguments(args);
			SetRequiredSettings(mosaSettings);
			mosaSettings.ExpandSearchPaths();
			mosaSettings.NormalizeSettings();
			mosaSettings.AddStandardPlugs();
			mosaSettings.UpdateFileAndPathSettings();

			Output($"Compiling: {mosaSettings.SourceFiles[0]}");

			if (mosaSettings.SourceFiles == null && mosaSettings.SourceFiles.Count == 0)
			{
				Output("ERROR: No input file(s) specified.");
				return 1;
			}

			var compiler = new MosaCompiler(mosaSettings, CreateCompilerHooks(), new ClrModuleLoader(), new ClrTypeResolver());

			if (string.IsNullOrEmpty(compiler.MosaSettings.OutputFile))
			{
				Output("ERROR: No output file specified.");
				return 1;
			}

			if (compiler.MosaSettings.Platform == null)
			{
				Output("ERROR: No Architecture specified.");
				return 1;
			}

			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
			Debug.AutoFlush = true;

			Output($"Input file(s): {string.Join(", ", new List<string>(compiler.MosaSettings.SourceFiles.ToArray()))}");
			Output($"Output file: {compiler.MosaSettings.OutputFile}");
			Output($"Platform: {compiler.MosaSettings.Platform}");

			compiler.Load();

			compiler.Compile();
		}
		catch (Exception ce)
		{
			Output($"Exception: {ce.Message}");
			Output($"Exception: {ce.StackTrace}");
			return 1;
		}

		return 0;
	}

	private static void RegisterPlatforms()
	{
		PlatformRegistry.Add(new Mosa.Compiler.x86.Architecture());
		PlatformRegistry.Add(new Mosa.Compiler.x64.Architecture());
		PlatformRegistry.Add(new Mosa.Compiler.ARM32.Architecture());
	}

	#endregion Public Methods

	#region Private Methods

	private static void SetRequiredSettings(MosaSettings mosaSettings)
	{
		mosaSettings.LauncherExit = false;
	}

	private CompilerHooks CreateCompilerHooks()
	{
		var compilerHooks = new CompilerHooks
		{
			NotifyEvent = NotifyEvent,
		};

		return compilerHooks;
	}

	private void NotifyEvent(CompilerEvent compilerEvent, string message, int threadID)
	{
		if (compilerEvent != CompilerEvent.MethodCompileEnd
			&& compilerEvent != CompilerEvent.MethodCompileStart
			&& compilerEvent != CompilerEvent.Counter
			&& compilerEvent != CompilerEvent.SetupStageStart
			&& compilerEvent != CompilerEvent.SetupStageEnd
			&& compilerEvent != CompilerEvent.FinalizationStageStart
			&& compilerEvent != CompilerEvent.FinalizationStageEnd)
		{
			message = string.IsNullOrWhiteSpace(message) ? string.Empty : $": {message}";
			Output($"{compilerEvent.ToText()}{message}");
		}
	}

	private static void Output(string status)
	{
		Console.WriteLine($"{Stopwatch.Elapsed.TotalSeconds:00.00} | {status}");
	}

	#endregion Private Methods
}
