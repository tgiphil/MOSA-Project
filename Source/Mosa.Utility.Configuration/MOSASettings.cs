// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Collections.Generic;
using Mosa.Compiler.Common.Configuration;

namespace Mosa.Utility.Configuration;

public class MosaSettings
{
	public Settings Settings { get; } = new Settings();

	#region Properties

	public string AsmFile
	{
		get => Settings.GetValue(SettingName.CompilerDebug_AsmFile, null);
		set => Settings.SetValue(SettingName.CompilerDebug_AsmFile, value);
	}

	public int BaseAddress
	{
		get => Settings.GetValue(SettingName.Compiler_BaseAddress, 0);
		set => Settings.SetValue(SettingName.Compiler_BaseAddress, value);
	}

	public string Bochs
	{
		get => Settings.GetValue(SettingName.AppLocation_Bochs, null);
		set => Settings.SetValue(SettingName.AppLocation_Bochs, value);
	}

	public string CompileTimeFile
	{
		get => Settings.GetValue(SettingName.CompilerDebug_CompileTimeFile, null);
		set => Settings.SetValue(SettingName.CompilerDebug_CompileTimeFile, value);
	}

	public string DebugFile
	{
		get => Settings.GetValue(SettingName.CompilerDebug_DebugFile, null);
		set => Settings.SetValue(SettingName.CompilerDebug_DebugFile, value);
	}

	public bool EmitBinary => Settings.GetValue(SettingName.Compiler_Binary, true);

	public bool EmitDwarf => Settings.GetValue(SettingName.Linker_Dwarf, false);

	public string MultibootVersion
	{
		get => Settings.GetValue(SettingName.Multiboot_Version, "v1");
		set => Settings.SetValue(SettingName.Multiboot_Version, value);
	}

	public string Emulator
	{
		get => Settings.GetValue(SettingName.Emulator, null);
		set => Settings.SetValue(SettingName.Emulator, value);
	}

	public bool EmulatorDisplay
	{
		get => Settings.GetValue(SettingName.Emulator_Display, false);
		set => Settings.SetValue(SettingName.Emulator_Display, value);
	}

	public bool EmulatorGDB
	{
		get => Settings.GetValue(SettingName.Emulator_GDB, false);
		set => Settings.SetValue(SettingName.Emulator_GDB, value);
	}

	public int EmulatorMemory
	{
		get => Settings.GetValue(SettingName.Emulator_Memory, 128);
		set => Settings.SetValue(SettingName.Emulator_Memory, value);
	}

	public int EmulatorCores
	{
		get => Settings.GetValue(SettingName.Emulator_Cores, 1);
		set => Settings.SetValue(SettingName.Emulator_Cores, value);
	}

	public string EmulatorSerial
	{
		get => Settings.GetValue(SettingName.Emulator_Serial, null);
		set => Settings.SetValue(SettingName.Emulator_Serial, value);
	}

	public string EmulatorSVGA
	{
		get => Settings.GetValue(SettingName.Emulator_SVGA, "std");
		set => Settings.SetValue(SettingName.Emulator_SVGA, value);
	}

	public string EmulatorSerialHost
	{
		get => Settings.GetValue(SettingName.Emulator_Serial_Host, null);
		set => Settings.SetValue(SettingName.Emulator_Serial_Host, value);
	}

	public string EmulatorSerialPipe
	{
		get => Settings.GetValue(SettingName.Emulator_Serial_Pipe, null);
		set => Settings.SetValue(SettingName.Emulator_Serial_Pipe, value);
	}

	public ushort EmulatorSerialPort
	{
		get => (ushort)Settings.GetValue(SettingName.Emulator_Serial_Port, 0);
		set => Settings.SetValue(SettingName.Emulator_Serial_Port, value);
	}

	public string FileSystem
	{
		get => Settings.GetValue(SettingName.Image_FileSystem, null);
		set => Settings.SetValue(SettingName.Image_FileSystem, value);
	}

	public string GDB
	{
		get => Settings.GetValue(SettingName.AppLocation_GDB, null);
		set => Settings.SetValue(SettingName.AppLocation_GDB, value);
	}

	public string GDBHost
	{
		get => Settings.GetValue(SettingName.GDB_Host, "localhost");
		set => Settings.SetValue(SettingName.GDB_Host, value);
	}

	public int GDBPort
	{
		get => Settings.GetValue(SettingName.GDB_Port, 0);
		set => Settings.SetValue(SettingName.GDB_Port, value);
	}

	public string ImageFirmware
	{
		get => Settings.GetValue(SettingName.Image_Firmware, null);
		set => Settings.SetValue(SettingName.Image_Firmware, value);
	}

	public string ImageFolder
	{
		get => Settings.GetValue(SettingName.Image_Folder, null);
		set => Settings.SetValue(SettingName.Image_Folder, value);
	}

	public string DefaultFolder
	{
		get => Settings.GetValue(SettingName.DefaultFolder, null);
		set => Settings.SetValue(SettingName.DefaultFolder, value);
	}

	public string TemporaryFolder
	{
		get => Settings.GetValue(SettingName.TemporaryFolder, null);
		set => Settings.SetValue(SettingName.TemporaryFolder, value);
	}

	public string ImageFile
	{
		get => Settings.GetValue(SettingName.Image_ImageFile, null);
		set => Settings.SetValue(SettingName.Image_ImageFile, value);
	}

	public string ImageFormat
	{
		get => Settings.GetValue(SettingName.Image_Format, null);
		set => Settings.SetValue(SettingName.Image_Format, value);
	}

	public string InlinedFile
	{
		get => Settings.GetValue(SettingName.CompilerDebug_InlinedFile, null);
		set => Settings.SetValue(SettingName.CompilerDebug_InlinedFile, value);
	}

	public bool LauncherExit
	{
		get => Settings.GetValue(SettingName.Launcher_Exit, false);
		set => Settings.SetValue(SettingName.Launcher_Exit, value);
	}

	public bool LauncherStart
	{
		get => Settings.GetValue(SettingName.Launcher_Start, false);
		set => Settings.SetValue(SettingName.Launcher_Start, value);
	}

	public bool LaunchGDB
	{
		get => Settings.GetValue(SettingName.Launcher_GDB, false);
		set => Settings.SetValue(SettingName.Launcher_GDB, value);
	}

	public bool LaunchDebugger
	{
		get => Settings.GetValue(SettingName.Launcher_Debugger, false);
		set => Settings.SetValue(SettingName.Launcher_Debugger, value);
	}

	public string LinkerFormat => Settings.GetValue(SettingName.Linker_Format, "elf32");

	public string MapFile
	{
		get => Settings.GetValue(SettingName.CompilerDebug_MapFile, null);
		set => Settings.SetValue(SettingName.CompilerDebug_MapFile, value);
	}

	public int MaxThreads => Settings.GetValue(SettingName.Compiler_Multithreading_MaxThreads, 0);

	public bool MethodScanner => Settings.GetValue(SettingName.Compiler_MethodScanner, false);

	public string Mkisofs
	{
		get => Settings.GetValue(SettingName.AppLocation_Mkisofs, null);
		set => Settings.SetValue(SettingName.AppLocation_Mkisofs, value);
	}

	public bool Multithreading
	{
		get => Settings.GetValue(SettingName.Compiler_Multithreading, true);
		set => Settings.SetValue(SettingName.Compiler_Multithreading, value);
	}

	public string NasmFile
	{
		get => Settings.GetValue(SettingName.CompilerDebug_NasmFile, null);
		set => Settings.SetValue(SettingName.CompilerDebug_NasmFile, value);
	}

	public string Ndisasm

	{
		get => Settings.GetValue(SettingName.AppLocation_Ndisasm, null);
		set => Settings.SetValue(SettingName.AppLocation_Ndisasm, value);
	}

	public string OutputFile
	{
		get => Settings.GetValue(SettingName.Compiler_OutputFile, null);
		set => Settings.SetValue(SettingName.Compiler_OutputFile, value);
	}

	public string Platform
	{
		get => Settings.GetValue(SettingName.Compiler_Platform, "x86");
		set => Settings.SetValue(SettingName.Compiler_Platform, value);
	}

	public bool PlugKorlib
	{
		get => Settings.GetValue(SettingName.Launcher_PlugKorlib, false);
		set => Settings.SetValue(SettingName.Launcher_PlugKorlib, value);
	}

	public string PostLinkHashFile
	{
		get => Settings.GetValue(SettingName.CompilerDebug_PostLinkHashFile, null);
		set => Settings.SetValue(SettingName.CompilerDebug_PostLinkHashFile, value);
	}

	public string PreLinkHashFile
	{
		get => Settings.GetValue(SettingName.CompilerDebug_PreLinkHashFile, null);
		set => Settings.SetValue(SettingName.CompilerDebug_PreLinkHashFile, value);
	}

	public string QEMU
	{
		get => Settings.GetValue(SettingName.AppLocation_Qemu, null);
		set => Settings.SetValue(SettingName.AppLocation_Qemu, value);
	}

	public string QEMUBios
	{
		get => Settings.GetValue(SettingName.AppLocation_QemuBIOS, null);
		set => Settings.SetValue(SettingName.AppLocation_QemuBIOS, value);
	}

	public string QEMUEdk2X86
	{
		get => Settings.GetValue(SettingName.AppLocation_QemuEDK2X86, null);
		set => Settings.SetValue(SettingName.AppLocation_QemuEDK2X86, value);
	}

	public string QEMUEdk2X64
	{
		get => Settings.GetValue(SettingName.AppLocation_QemuEDK2X64, null);
		set => Settings.SetValue(SettingName.AppLocation_QemuEDK2X64, value);
	}

	public string QEMUEdk2ARM
	{
		get => Settings.GetValue(SettingName.AppLocation_QemuEDK2ARM, null);
		set => Settings.SetValue(SettingName.AppLocation_QemuEDK2ARM, value);
	}

	public string QemuImg
	{
		get => Settings.GetValue(SettingName.AppLocation_QemuImg, null);
		set => Settings.SetValue(SettingName.AppLocation_QemuImg, value);
	}

	public bool LauncherTest
	{
		get => Settings.GetValue(SettingName.Launcher_Test, false);
		set => Settings.SetValue(SettingName.Launcher_Test, value);
	}

	public bool LauncherSerialConsole
	{
		get => Settings.GetValue(SettingName.Launcher_Serial_Console, false);
		set => Settings.SetValue(SettingName.Launcher_Serial_Console, value);
	}

	public bool LauncherSerialFile
	{
		get => Settings.GetValue(SettingName.Launcher_Serial_File, false);
		set => Settings.SetValue(SettingName.Launcher_Serial_File, value);
	}

	public int EmulatorMaxRuntime
	{
		get => Settings.GetValue(SettingName.Emulator_MaxRuntime, 10);
		set => Settings.SetValue(SettingName.Emulator_MaxRuntime, 10);
	}

	public List<string> SearchPaths => Settings.GetValueList(SettingName.SearchPaths);

	public List<string> SourceFiles => Settings.GetValueList(SettingName.Compiler_SourceFiles);

	public string FileSystemRootInclude
	{
		get => Settings.GetValue(SettingName.Image_FileSystem_RootInclude, null);
		set => Settings.SetValue(SettingName.Image_FileSystem_RootInclude, value);
	}

	public string VmwarePlayer
	{
		get => Settings.GetValue(SettingName.AppLocation_VmwarePlayer, null);
		set => Settings.SetValue(SettingName.AppLocation_VmwarePlayer, value);
	}

	public string VmwareWorkstation
	{
		get => Settings.GetValue(SettingName.AppLocation_VmwareWorkstation, null);
		set => Settings.SetValue(SettingName.AppLocation_VmwareWorkstation, value);
	}

	public string VirtualBox
	{
		get => Settings.GetValue(SettingName.AppLocation_VirtualBox, null);
		set => Settings.SetValue(SettingName.AppLocation_VirtualBox, value);
	}

	public string OSName
	{
		get => Settings.GetValue(SettingName.OS_Name, null);
		set => Settings.SetValue(SettingName.OS_Name, value);
	}

	#endregion Properties

	public MosaSettings(Settings settings)
	{
		Settings.Merge(settings);
	}
}
