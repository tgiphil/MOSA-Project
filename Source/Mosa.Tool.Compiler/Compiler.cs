// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Mosa.Compiler.Common.Configuration;
using Mosa.Compiler.Common.Exceptions;
using Mosa.Compiler.Framework;
using Mosa.Compiler.Framework.Trace;
using Mosa.Compiler.MosaTypeSystem.CLR;
using Mosa.Utility.Configuration;

using Mosa.Utility.Configuration;

namespace Mosa.Tool.Compiler;

/// <summary>
/// Class containing the Compiler.
/// </summary>
public class Compiler
{
	#region Data

	protected MosaCompiler compiler;

	protected Settings Settings = new Settings();

	private DateTime CompileStartTime;

	/// <summary>
	/// A string holding a simple usage description.
	/// </summary>
	private readonly string usageString;

	#endregion Data

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the Compiler class.
	/// </summary>
	public Compiler()
	{
		usageString = @"Usage: Mosa.Tool.Compiler.exe -o outputfile --platform [x86|x64] {additional options} inputfiles.

Example: Mosa.Tool.Compiler.exe -o Mosa.HelloWorld.x86.bin -platform x86 Mosa.HelloWorld.x86.dll System.Runtime.dll Mosa.Plug.Korlib.dll Mosa.Plug.Korlib.x86.dll";
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Runs the command line parser and the compilation process.
	/// </summary>
	/// <param name="args">The command line arguments.</param>
	public void Run(string[] args)
	{
		RegisterPlatforms();

		// always print header with version information
		Console.WriteLine("MOSA Compiler, Version {0}.", CompilerVersion.VersionString);
		Console.WriteLine("Copyright 2020 by the MOSA Project. Licensed under the New BSD License.");

		Console.WriteLine();
		Console.WriteLine("Parsing options...");

		try
		{
			LoadArguments(args);

			var sourceFiles = Settings.GetValueList("Compiler.SourceFiles");

			if (sourceFiles == null && sourceFiles.Count == 0)
			{
				throw new Exception("No input file(s) specified.");
			}

			compiler = new MosaCompiler(Settings, CreateCompilerHooks(), new ClrModuleLoader(), new ClrTypeResolver());

			if (string.IsNullOrEmpty(compiler.CompilerSettings.OutputFile))
			{
				throw new Exception("No output file specified.");
			}

			if (compiler.CompilerSettings.Platform == null)
			{
				throw new Exception("No Architecture specified.");
			}

			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
			Debug.AutoFlush = true;

			Console.WriteLine($" > Output file: {compiler.CompilerSettings.OutputFile}");
			Console.WriteLine($" > Input file(s): {string.Join(", ", new List<string>(compiler.CompilerSettings.SourceFiles.ToArray()))}");
			Console.WriteLine($" > Platform: {compiler.CompilerSettings.Platform}");

			Console.WriteLine();
			Console.WriteLine("Compiling ...");
			Console.WriteLine();

			Compile();
		}
		catch (Exception ce)
		{
			ShowError(ce.Message);
			Environment.Exit(1);
			return;
		}
	}

	private static void RegisterPlatforms()
	{
		PlatformRegistry.Add(new Platform.x86.Architecture());
		PlatformRegistry.Add(new Platform.x64.Architecture());
		PlatformRegistry.Add(new Platform.ARMv8A32.Architecture());
	}

	#endregion Public Methods

	#region Private Methods

	private void LoadArguments(string[] args)
	{
		SetDefaultSettings();

		var arguments = Import.RecursiveReader(CommandLineArguments.Map, args);

		Settings.Merge(arguments);

		var sourcefiles = Settings.GetValueList("Compiler.SourceFiles");

		if (sourcefiles != null)
		{
			foreach (var sourcefile in sourcefiles)
			{
				var full = Path.GetFullPath(sourcefile);
				var path = Path.GetDirectoryName(full);

				if (!string.IsNullOrWhiteSpace(path))
				{
					Settings.AddPropertyListValue("SearchPaths", path);
				}
			}
		}

		SetDefault(Settings);
	}

	private void SetDefaultSettings()
	{
		Settings.SetValue(SettingName.Compiler_BaseAddress, 0x00400000);
		Settings.SetValue(SettingName.Compiler_Binary, true);
		Settings.SetValue(SettingName.Compiler_MethodScanner, false);
		Settings.SetValue(SettingName.Compiler_Multithreading, true);
		Settings.SetValue(SettingName.Compiler_Platform, "x86");
		Settings.SetValue(SettingName.Compiler_TraceLevel, 0);
		Settings.SetValue(SettingName.Launcher_PlugKorlib, true);
		Settings.SetValue(SettingName.CompilerDebug_DebugFile, string.Empty);
		Settings.SetValue(SettingName.CompilerDebug_AsmFile, string.Empty);
		Settings.SetValue(SettingName.CompilerDebug_MapFile, string.Empty);
		Settings.SetValue(SettingName.CompilerDebug_NasmFile, string.Empty);
		Settings.SetValue(SettingName.Optimizations_Basic, true);
		Settings.SetValue(SettingName.Optimizations_BitTracker, true);
		Settings.SetValue(SettingName.Optimizations_Inline, true);
		Settings.SetValue(SettingName.Optimizations_Inline_AggressiveMaximum, 24);
		Settings.SetValue(SettingName.Optimizations_Inline_Explicit, true);
		Settings.SetValue(SettingName.Optimizations_Inline_Maximum, 12);
		Settings.SetValue(SettingName.Optimizations_Basic_Window, 5);
		Settings.SetValue(SettingName.Optimizations_LongExpansion, true);
		Settings.SetValue(SettingName.Optimizations_LoopInvariantCodeMotion, true);
		Settings.SetValue(SettingName.Optimizations_Platform, true);
		Settings.SetValue(SettingName.Optimizations_SCCP, true);
		Settings.SetValue(SettingName.Optimizations_Devirtualization, true);
		Settings.SetValue(SettingName.Optimizations_SSA, true);
		Settings.SetValue(SettingName.Optimizations_TwoPass, true);
		Settings.SetValue(SettingName.Optimizations_ValueNumbering, true);
		Settings.SetValue(SettingName.Image_Folder, Path.Combine(Path.GetTempPath(), "MOSA"));
		Settings.SetValue(SettingName.Image_Format, "IMG");
		Settings.SetValue(SettingName.Image_FileSystem, "FAT16");
		Settings.SetValue(SettingName.Multiboot_Version, "v1");
		Settings.SetValue(SettingName.Multiboot_Video, false);
		Settings.SetValue(SettingName.Multiboot_Video_Width, 640);
		Settings.SetValue(SettingName.Multiboot_Video_Height, 480);
		Settings.SetValue(SettingName.Multiboot_Video_Depth, 32);
		Settings.SetValue(SettingName.Emulator, "Qemu");
		Settings.SetValue(SettingName.Emulator_Memory, 128);
		Settings.SetValue(SettingName.Emulator_Serial, "TCPServer");
		Settings.SetValue(SettingName.Emulator_Serial_Host, "127.0.0.1");
		Settings.SetValue(SettingName.Emulator_Serial_Port, 9999);
		Settings.SetValue(SettingName.Emulator_Serial_Pipe, "MOSA");
		Settings.SetValue(SettingName.Launcher_Start, false);
		Settings.SetValue(SettingName.Launcher_Launch, false);
		Settings.SetValue(SettingName.Launcher_Exit, false);
		Settings.SetValue(SettingName.OS_Name, "MOSA");
	}

	private CompilerHooks CreateCompilerHooks()
	{
		CompileStartTime = DateTime.Now;

		var compilerHooks = new CompilerHooks
		{
			NotifyEvent = NotifyEvent,
		};

		return compilerHooks;
	}

	private void Compile()
	{
		compiler.Load();

		compiler.Compile();
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
			Console.WriteLine($"{(DateTime.Now - CompileStartTime).TotalSeconds:0.00} [{threadID}] {compilerEvent.ToText()}{message}");
		}
	}

	/// <summary>
	/// Shows an error and a short information text.
	/// </summary>
	/// <param name="message">The error message to show.</param>
	private void ShowError(string message)
	{
		Console.WriteLine(usageString);
		Console.WriteLine();
		Console.Write("Error: ");
		Console.WriteLine(message);
		Console.WriteLine();
		Console.WriteLine("Execute 'Mosa.Tool.Compiler.exe --help' for more information.");
		Console.WriteLine();
	}

	private void SetDefault(Settings settings)
	{
		var compilerToolSettings = new CompilerToolSettings(settings);

		if (string.IsNullOrWhiteSpace(compilerToolSettings.TemporaryFolder) || compilerToolSettings.TemporaryFolder != "%DEFAULT%")
		{
			compilerToolSettings.TemporaryFolder = Path.Combine(Path.GetTempPath(), "MOSA");
		}

		if (string.IsNullOrWhiteSpace(compilerToolSettings.ImageFolder) || compilerToolSettings.ImageFolder != "%DEFAULT%")
		{
			compilerToolSettings.ImageFolder = compilerToolSettings.TemporaryFolder;
		}

		if (string.IsNullOrWhiteSpace(compilerToolSettings.DefaultFolder) || compilerToolSettings.DefaultFolder != "%DEFAULT%")
		{
			if (compilerToolSettings.OutputFile != null && compilerToolSettings.OutputFile != "%DEFAULT%")
			{
				compilerToolSettings.DefaultFolder = Path.GetDirectoryName(Path.GetFullPath(compilerToolSettings.OutputFile));
			}
			else
			{
				compilerToolSettings.DefaultFolder = compilerToolSettings.TemporaryFolder;
			}
		}

		var defaultFolder = compilerToolSettings.DefaultFolder;

		string baseFilename;

		if (compilerToolSettings.OutputFile != null && compilerToolSettings.OutputFile != "%DEFAULT%")
		{
			baseFilename = Path.GetFileNameWithoutExtension(compilerToolSettings.OutputFile);
		}
		else if (compilerToolSettings.SourceFiles != null && compilerToolSettings.SourceFiles.Count != 0)
		{
			baseFilename = Path.GetFileNameWithoutExtension(compilerToolSettings.SourceFiles[0]);
		}
		else
		{
			baseFilename = "_mosa_";
		}

		if (compilerToolSettings.OutputFile is null or "%DEFAULT%")
		{
			compilerToolSettings.OutputFile = Path.Combine(defaultFolder, $"{baseFilename}.bin");
		}

		if (compilerToolSettings.ImageFile == "%DEFAULT%")
		{
			compilerToolSettings.ImageFile = Path.Combine(compilerToolSettings.ImageFolder, $"{baseFilename}.{compilerToolSettings.ImageFormat}");
		}

		if (compilerToolSettings.MapFile == "%DEFAULT%")
		{
			compilerToolSettings.MapFile = Path.Combine(defaultFolder, $"{baseFilename}-map.txt");
		}

		if (compilerToolSettings.CompileTimeFile == "%DEFAULT%")
		{
			compilerToolSettings.CompileTimeFile = Path.Combine(defaultFolder, $"{baseFilename}-time.txt");
		}

		if (compilerToolSettings.DebugFile == "%DEFAULT%")
		{
			compilerToolSettings.DebugFile = Path.Combine(defaultFolder, $"{baseFilename}.debug");
		}

		if (compilerToolSettings.InlinedFile == "%DEFAULT%")
		{
			compilerToolSettings.InlinedFile = Path.Combine(defaultFolder, $"{baseFilename}-inlined.txt");
		}

		if (compilerToolSettings.PreLinkHashFile == "%DEFAULT%")
		{
			compilerToolSettings.PreLinkHashFile = Path.Combine(defaultFolder, $"{baseFilename}-prelink-hash.txt");
		}

		if (compilerToolSettings.PostLinkHashFile == "%DEFAULT%")
		{
			compilerToolSettings.PostLinkHashFile = Path.Combine(defaultFolder, $"{baseFilename}-postlink-hash.txt");
		}

		if (compilerToolSettings.AsmFile == "%DEFAULT%")
		{
			compilerToolSettings.AsmFile = Path.Combine(defaultFolder, $"{baseFilename}.asm");
		}

		if (compilerToolSettings.NasmFile == "%DEFAULT%")
		{
			compilerToolSettings.NasmFile = Path.Combine(defaultFolder, $"{baseFilename}.nasm");
		}
	}

	#endregion Private Methods
}
