// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Collections.Generic;
using System.IO;
using Mosa.Compiler.Common.Configuration;

namespace Mosa.Utility.Configuration;

public class MosaSettings
{
	#region Constants

	public static class Constant
	{
		public const string MultibootVersion = "v1";

		public const int MaxErrors = 1000;
		public const int ConnectionTimeOut = 10000; // in milliseconds
		public const int TimeOut = 10000; // in milliseconds
		public const int MaxAttempts = 20;
		public const int Port = 11110;
	}

	#endregion Constants

	#region Properties

	public string AsmFile
	{
		get => Settings.GetValue(Name.CompilerDebug_AsmFile, null);
		set => Settings.SetValue(Name.CompilerDebug_AsmFile, value);
	}

	public int BaseAddress
	{
		get => Settings.GetValue(Name.Compiler_BaseAddress, 0);
		set => Settings.SetValue(Name.Compiler_BaseAddress, value);
	}

	public string Bochs
	{
		get => Settings.GetValue(Name.AppLocation_Bochs, null);
		set => Settings.SetValue(Name.AppLocation_Bochs, value);
	}

	public string CompileTimeFile
	{
		get => Settings.GetValue(Name.CompilerDebug_CompileTimeFile, null);
		set => Settings.SetValue(Name.CompilerDebug_CompileTimeFile, value);
	}

	public string DebugFile
	{
		get => Settings.GetValue(Name.CompilerDebug_DebugFile, null);
		set => Settings.SetValue(Name.CompilerDebug_DebugFile, value);
	}

	public bool EmitBinary
	{
		get => Settings.GetValue(Name.Compiler_Binary, true);
		set => Settings.SetValue(Name.Compiler_Binary, true);
	}

	public bool EmitDwarf
	{
		get => Settings.GetValue(Name.Linker_Dwarf, false);
		set => Settings.SetValue(Name.Linker_Dwarf, false);
	}

	public string MultibootVersion
	{
		get => Settings.GetValue(Name.Multiboot_Version, Constant.MultibootVersion);
		set => Settings.SetValue(Name.Multiboot_Version, value);
	}

	public string Emulator
	{
		get => Settings.GetValue(Name.Emulator, null);
		set => Settings.SetValue(Name.Emulator, value);
	}

	public bool EmulatorDisplay
	{
		get => Settings.GetValue(Name.Emulator_Display, false);
		set => Settings.SetValue(Name.Emulator_Display, value);
	}

	public bool EmulatorGDB
	{
		get => Settings.GetValue(Name.Emulator_GDB, false);
		set => Settings.SetValue(Name.Emulator_GDB, value);
	}

	public int EmulatorMemory
	{
		get => Settings.GetValue(Name.Emulator_Memory, 128);
		set => Settings.SetValue(Name.Emulator_Memory, value);
	}

	public int EmulatorCores
	{
		get => Settings.GetValue(Name.Emulator_Cores, 1);
		set => Settings.SetValue(Name.Emulator_Cores, value);
	}

	public string EmulatorSerial
	{
		get => Settings.GetValue(Name.Emulator_Serial, null);
		set => Settings.SetValue(Name.Emulator_Serial, value);
	}

	public string EmulatorSVGA
	{
		get => Settings.GetValue(Name.Emulator_SVGA, "std");
		set => Settings.SetValue(Name.Emulator_SVGA, value);
	}

	public string EmulatorSerialHost
	{
		get => Settings.GetValue(Name.Emulator_Serial_Host, "localhost");
		set => Settings.SetValue(Name.Emulator_Serial_Host, value);
	}

	public string EmulatorSerialPipe
	{
		get => Settings.GetValue(Name.Emulator_Serial_Pipe, "MOSA");
		set => Settings.SetValue(Name.Emulator_Serial_Pipe, value);
	}

	public ushort EmulatorSerialPort
	{
		get => (ushort)Settings.GetValue(Name.Emulator_Serial_Port, 0);
		set => Settings.SetValue(Name.Emulator_Serial_Port, value);
	}

	public string FileSystem
	{
		get => Settings.GetValue(Name.Image_FileSystem, null);
		set => Settings.SetValue(Name.Image_FileSystem, value);
	}

	public string GDB
	{
		get => Settings.GetValue(Name.AppLocation_GDB, null);
		set => Settings.SetValue(Name.AppLocation_GDB, value);
	}

	public string GDBHost
	{
		get => Settings.GetValue(Name.GDB_Host, "localhost");
		set => Settings.SetValue(Name.GDB_Host, value);
	}

	public int GDBPort
	{
		get => Settings.GetValue(Name.GDB_Port, 0);
		set => Settings.SetValue(Name.GDB_Port, value);
	}

	public string ImageFirmware
	{
		get => Settings.GetValue(Name.Image_Firmware, null);
		set => Settings.SetValue(Name.Image_Firmware, value);
	}

	public string ImageFolder
	{
		get => Settings.GetValue(Name.Image_Folder, null);
		set => Settings.SetValue(Name.Image_Folder, value);
	}

	public string DefaultFolder
	{
		get => Settings.GetValue(Name.DefaultFolder, null);
		set => Settings.SetValue(Name.DefaultFolder, value);
	}

	public string TemporaryFolder
	{
		get => Settings.GetValue(Name.TemporaryFolder, null);
		set => Settings.SetValue(Name.TemporaryFolder, value);
	}

	public string ImageFile
	{
		get => Settings.GetValue(Name.Image_ImageFile, null);
		set => Settings.SetValue(Name.Image_ImageFile, value);
	}

	public string ImageFormat
	{
		get => Settings.GetValue(Name.Image_Format, null);
		set => Settings.SetValue(Name.Image_Format, value);
	}

	public string InlinedFile
	{
		get => Settings.GetValue(Name.CompilerDebug_InlinedFile, null);
		set => Settings.SetValue(Name.CompilerDebug_InlinedFile, value);
	}

	public bool LauncherExit
	{
		get => Settings.GetValue(Name.Launcher_Exit, false);
		set => Settings.SetValue(Name.Launcher_Exit, value);
	}

	public bool Launcher
	{
		get => Settings.GetValue(Name.Launcher_Launch, false);
		set => Settings.SetValue(Name.Launcher_Launch, value);
	}

	public bool LauncherStart
	{
		get => Settings.GetValue(Name.Launcher_Start, false);
		set => Settings.SetValue(Name.Launcher_Start, value);
	}

	public bool LaunchGDB
	{
		get => Settings.GetValue(Name.Launcher_GDB, false);
		set => Settings.SetValue(Name.Launcher_GDB, value);
	}

	public bool LaunchDebugger
	{
		get => Settings.GetValue(Name.Launcher_Debugger, false);
		set => Settings.SetValue(Name.Launcher_Debugger, value);
	}

	public string LinkerFormat => Settings.GetValue(Name.Linker_Format, "elf32");

	public string MapFile
	{
		get => Settings.GetValue(Name.CompilerDebug_MapFile, null);
		set => Settings.SetValue(Name.CompilerDebug_MapFile, value);
	}

	public int MaxThreads => Settings.GetValue(Name.Compiler_Multithreading_MaxThreads, 0);

	public bool MethodScanner => Settings.GetValue(Name.Compiler_MethodScanner, false);

	public string Mkisofs
	{
		get => Settings.GetValue(Name.AppLocation_Mkisofs, null);
		set => Settings.SetValue(Name.AppLocation_Mkisofs, value);
	}

	public bool Multithreading
	{
		get => Settings.GetValue(Name.Compiler_Multithreading, true);
		set => Settings.SetValue(Name.Compiler_Multithreading, value);
	}

	public string NasmFile
	{
		get => Settings.GetValue(Name.CompilerDebug_NasmFile, null);
		set => Settings.SetValue(Name.CompilerDebug_NasmFile, value);
	}

	public string Ndisasm

	{
		get => Settings.GetValue(Name.AppLocation_Ndisasm, null);
		set => Settings.SetValue(Name.AppLocation_Ndisasm, value);
	}

	public string OutputFile
	{
		get => Settings.GetValue(Name.Compiler_OutputFile, null);
		set => Settings.SetValue(Name.Compiler_OutputFile, value);
	}

	public string Platform
	{
		get => Settings.GetValue(Name.Compiler_Platform, "x86");
		set => Settings.SetValue(Name.Compiler_Platform, value);
	}

	public bool PlugKorlib
	{
		get => Settings.GetValue(Name.Launcher_PlugKorlib, false);
		set => Settings.SetValue(Name.Launcher_PlugKorlib, value);
	}

	public string PostLinkHashFile
	{
		get => Settings.GetValue(Name.CompilerDebug_PostLinkHashFile, null);
		set => Settings.SetValue(Name.CompilerDebug_PostLinkHashFile, value);
	}

	public string PreLinkHashFile
	{
		get => Settings.GetValue(Name.CompilerDebug_PreLinkHashFile, null);
		set => Settings.SetValue(Name.CompilerDebug_PreLinkHashFile, value);
	}

	public string QEMU
	{
		get => Settings.GetValue(Name.AppLocation_Qemu, null);
		set => Settings.SetValue(Name.AppLocation_Qemu, value);
	}

	public string QEMUBios
	{
		get => Settings.GetValue(Name.AppLocation_QemuBIOS, null);
		set => Settings.SetValue(Name.AppLocation_QemuBIOS, value);
	}

	public string QEMUEdk2X86
	{
		get => Settings.GetValue(Name.AppLocation_QemuEDK2X86, null);
		set => Settings.SetValue(Name.AppLocation_QemuEDK2X86, value);
	}

	public string QEMUEdk2X64
	{
		get => Settings.GetValue(Name.AppLocation_QemuEDK2X64, null);
		set => Settings.SetValue(Name.AppLocation_QemuEDK2X64, value);
	}

	public string QEMUEdk2ARM
	{
		get => Settings.GetValue(Name.AppLocation_QemuEDK2ARM, null);
		set => Settings.SetValue(Name.AppLocation_QemuEDK2ARM, value);
	}

	public string QemuImg
	{
		get => Settings.GetValue(Name.AppLocation_QemuImg, null);
		set => Settings.SetValue(Name.AppLocation_QemuImg, value);
	}

	public bool LauncherTest
	{
		get => Settings.GetValue(Name.Launcher_Test, false);
		set => Settings.SetValue(Name.Launcher_Test, value);
	}

	public bool LauncherSerialConsole
	{
		get => Settings.GetValue(Name.Launcher_Serial_Console, false);
		set => Settings.SetValue(Name.Launcher_Serial_Console, value);
	}

	public bool LauncherSerialFile
	{
		get => Settings.GetValue(Name.Launcher_Serial_File, false);
		set => Settings.SetValue(Name.Launcher_Serial_File, value);
	}

	public int EmulatorMaxRuntime
	{
		get => Settings.GetValue(Name.Emulator_MaxRuntime, 10);
		set => Settings.SetValue(Name.Emulator_MaxRuntime, 10);
	}

	public List<string> SearchPaths => Settings.GetValueList(Name.SearchPaths);

	public List<string> SourceFiles => Settings.GetValueList(Name.Compiler_SourceFiles);

	public string FileSystemRootInclude
	{
		get => Settings.GetValue(Name.Image_FileSystem_RootInclude, null);
		set => Settings.SetValue(Name.Image_FileSystem_RootInclude, value);
	}

	public string VmwarePlayer
	{
		get => Settings.GetValue(Name.AppLocation_VmwarePlayer, null);
		set => Settings.SetValue(Name.AppLocation_VmwarePlayer, value);
	}

	public string VmwareWorkstation
	{
		get => Settings.GetValue(Name.AppLocation_VmwareWorkstation, null);
		set => Settings.SetValue(Name.AppLocation_VmwareWorkstation, value);
	}

	public string VirtualBox
	{
		get => Settings.GetValue(Name.AppLocation_VirtualBox, null);
		set => Settings.SetValue(Name.AppLocation_VirtualBox, value);
	}

	public string OSName
	{
		get => Settings.GetValue(Name.OS_Name, null);
		set => Settings.SetValue(Name.OS_Name, value);
	}

	public int MaxErrors
	{
		get => Settings.GetValue(Name.UnitTest_MaxErrors, Constant.MaxErrors);
		set => Settings.SetValue(Name.UnitTest_MaxErrors, value);
	}

	public int TimeOut
	{
		get => Settings.GetValue(Name.UnitTest_Connection_TimeOut, Constant.TimeOut);
		set => Settings.SetValue(Name.UnitTest_Connection_TimeOut, value);
	}

	public int ConnectionTimeOut
	{
		get => Settings.GetValue(Name.UnitTest_Connection_TimeOut, Constant.ConnectionTimeOut);
		set => Settings.SetValue(Name.UnitTest_Connection_TimeOut, value);
	}

	public int MaxAttempts
	{
		get => Settings.GetValue(Name.UnitTest_Connection_MaxAttempts, Constant.MaxAttempts);
		set => Settings.SetValue(Name.UnitTest_Connection_MaxAttempts, value);
	}

	public string Filter
	{
		get => Settings.GetValue(Name.UnitTest_Filter, null);
		set => Settings.SetValue(Name.UnitTest_Filter, value);
	}

	public int TraceLevel
	{
		get => Settings.GetValue(Name.Compiler_TraceLevel, 0);
		set => Settings.SetValue(Name.Compiler_TraceLevel, value);
	}

	public Settings Settings { get; } = new Settings();

	#endregion Properties

	public MosaSettings()
	{
		Settings = new Settings();
	}

	public MosaSettings(Settings settings)
	{
		Merge(settings);
	}

	public void Merge(Settings settings)
	{
		Settings.Merge(settings);
	}

	public void LoadAppSettings()
	{
		AppLocationsSettings.GetAppLocationSettings(Settings);
	}

	public void LoadArguments(string[] args)
	{
		var settings = Import.RecursiveReader(CommandLineArguments.Map, args);

		Settings.Merge(settings);
	}

	public void SetDetfaultSettings()
	{
		Settings.SetValue("Compiler.MethodScanner", false);
		Settings.SetValue("Compiler.Multithreading", true);
		Settings.SetValue("Compiler.Platform", "x86");

		Settings.SetValue("Compiler.MethodScanner", false);
		Settings.SetValue("Compiler.Multithreading", true);

		Settings.SetValue("Compiler.BaseAddress", 0x00500000);  // Change to constant
		EmitBinary = true;
		TraceLevel = 0;

		Settings.SetValue("CompilerDebug.DebugFile", "%DEFAULT%");
		Settings.SetValue("CompilerDebug.AsmFile", "%DEFAULT%");
		Settings.SetValue("CompilerDebug.MapFile", "%DEFAULT%");
		Settings.SetValue("CompilerDebug.InlinedFile", "%DEFAULT%");
		Settings.SetValue("CompilerDebug.NasmFile", string.Empty);

		Settings.SetValue("Optimizations.Basic", true);
		Settings.SetValue("Optimizations.BitTracker", true);
		Settings.SetValue("Optimizations.Inline", true);
		Settings.SetValue("Optimizations.Inline.AggressiveMaximum", 24); // Change to constant
		Settings.SetValue("Optimizations.Inline.Explicit", true);
		Settings.SetValue("Optimizations.Inline.Maximum", 12); // Change to constant
		Settings.SetValue("Optimizations.Basic.Window", 5); // Change to constant
		Settings.SetValue("Optimizations.LongExpansion", true);
		Settings.SetValue("Optimizations.LoopInvariantCodeMotion", true);
		Settings.SetValue("Optimizations.Platform", true);
		Settings.SetValue("Optimizations.SCCP", true);
		Settings.SetValue("Optimizations.Devirtualization", true);
		Settings.SetValue("Optimizations.SSA", true);
		Settings.SetValue("Optimizations.TwoPass", true);
		Settings.SetValue("Optimizations.ValueNumbering", true);

		Settings.SetValue("Multiboot.Video", false);
		Settings.SetValue("Multiboot.Video.Width", 640);
		Settings.SetValue("Multiboot.Video.Height", 480);
		Settings.SetValue("Multiboot.Video.Depth", 32);

		Settings.SetValue("Emulator.Display", false);
		Settings.SetValue("Emulator.Serial", "TCPServer");
		Settings.SetValue("Emulator.Serial.Host", "127.0.0.1");
		Settings.SetValue("Emulator.Serial.Port", Constant.Port);
		Settings.SetValue("Emulator.Serial.Pipe", "MOSA");

		Settings.SetValue("Multiboot.Version", "v1");   // Change to constant

		Settings.SetValue("Image.Firmware", "bios");
		Settings.SetValue("Image.Folder", Path.Combine(Path.GetTempPath(), "MOSA-UnitTest"));
		Settings.SetValue("Image.Format", "IMG");   // Change to constant
		Settings.SetValue("Image.FileSystem", "FAT16"); // Change to constant
		Settings.SetValue("Image.ImageFile", "%DEFAULT%");

		Settings.SetValue("OS.Name", "MOSA");

		Settings.SetValue("UnitTest.MaxErrors", Constant.MaxErrors);
		Settings.SetValue("UnitTest.TimeOut", Constant.TimeOut);
		Settings.SetValue("UnitTest.Connection.TimeOut", Constant.ConnectionTimeOut);
		Settings.SetValue("UnitTest.Connection.MaxAttempts", Constant.MaxAttempts);

		Settings.SetValue("Launcher.PlugKorlib", true);

		Settings.SetValue("Emulator", "Qemu");
		Settings.SetValue("Emulator.Memory", 128);
		Settings.SetValue("Emulator.Cores", 1);

		Settings.SetValue("Launcher.Start", false);
		Settings.SetValue("Launcher.Launch", false);
		Settings.SetValue("Launcher.Exit", true);
	}

	public void NormalizeSettings()
	{
		ImageFormat = ImageFormat == null ? string.Empty : ImageFormat.ToLowerInvariant().Trim();
		FileSystem = FileSystem == null ? string.Empty : FileSystem.ToLowerInvariant().Trim();
		EmulatorSerial = EmulatorSerial == null ? string.Empty : EmulatorSerial.ToLowerInvariant().Trim();
		Emulator = Emulator == null ? string.Empty : Emulator.ToLowerInvariant().Trim();
		Platform = Platform.ToLowerInvariant().Trim();
	}
}
