// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;
using System.IO;
using System.Reflection;
using Mosa.Compiler.Common.Configuration;
using Mosa.Compiler.Framework;
using Mosa.Utility.Configuration;

namespace Mosa.Utility.Launcher;

public class BaseLauncher
{
	public CompilerHooks CompilerHooks { get; }

	public MosaSettings MosaSettings { get; }

	public Settings ConfigurationSettings => MosaSettings.Settings;

	public BaseLauncher(Settings settings, CompilerHooks compilerHook)
	{
		CompilerHooks = compilerHook;

		MosaSettings = new MosaSettings();
		MosaSettings.SetDetfaultSettings();

		SetDefaultSettings(MosaSettings.Settings);

		MosaSettings.Merge(settings);

		MosaSettings.NormalizeSettings();

		SetDefaults();
	}

	private void SetDefaultSettings(Settings settings)
	{
		settings.SetValue("Emulator", "Qemu");
		settings.SetValue("Emulator.Memory", 128);
		settings.SetValue("Emulator.Cores", 1);

		//settings.SetValue("Emulator.Serial", "none");
		settings.SetValue("Emulator.Serial.Host", "127.0.0.1");
		settings.SetValue("Emulator.Serial.Port", 9999);
		settings.SetValue("Emulator.Serial.Pipe", "MOSA");
		settings.SetValue("Launcher.PlugKorlib", true);
	}

	private void SetDefaults()
	{
		if (string.IsNullOrWhiteSpace(MosaSettings.TemporaryFolder) || MosaSettings.TemporaryFolder != "%DEFAULT%")
		{
			MosaSettings.TemporaryFolder = Path.Combine(Path.GetTempPath(), "MOSA");
		}

		if (string.IsNullOrWhiteSpace(MosaSettings.DefaultFolder) || MosaSettings.DefaultFolder != "%DEFAULT%")
		{
			if (MosaSettings.OutputFile != null && MosaSettings.OutputFile != "%DEFAULT%")
			{
				MosaSettings.DefaultFolder = Path.GetDirectoryName(Path.GetFullPath(MosaSettings.OutputFile));
			}
			else
			{
				MosaSettings.DefaultFolder = MosaSettings.TemporaryFolder;
			}
		}

		if (MosaSettings.ImageFolder != null && MosaSettings.ImageFolder != "%DEFAULT%")
		{
			MosaSettings.ImageFolder = MosaSettings.DefaultFolder;
		}

		string baseFilename;

		if (MosaSettings.OutputFile != null && MosaSettings.OutputFile != "%DEFAULT%")
		{
			baseFilename = Path.GetFileNameWithoutExtension(MosaSettings.OutputFile);
		}
		else if (MosaSettings.SourceFiles != null && MosaSettings.SourceFiles.Count != 0)
		{
			baseFilename = Path.GetFileNameWithoutExtension(MosaSettings.SourceFiles[0]);
		}
		else if (MosaSettings.ImageFile != null && MosaSettings.ImageFile != "%DEFAULT%")
		{
			baseFilename = Path.GetFileNameWithoutExtension(MosaSettings.ImageFile);
		}
		else
		{
			baseFilename = "_mosa_";
		}

		var defaultFolder = MosaSettings.DefaultFolder;

		if (MosaSettings.OutputFile is null or "%DEFAULT%")
		{
			MosaSettings.OutputFile = Path.Combine(defaultFolder, $"{baseFilename}.bin");
		}

		if (MosaSettings.ImageFile == "%DEFAULT%")
		{
			MosaSettings.ImageFile = Path.Combine(MosaSettings.ImageFolder, $"{baseFilename}.{MosaSettings.ImageFormat}");
		}

		if (MosaSettings.MapFile == "%DEFAULT%")
		{
			MosaSettings.MapFile = Path.Combine(defaultFolder, $"{baseFilename}-map.txt");
		}

		if (MosaSettings.CompileTimeFile == "%DEFAULT%")
		{
			MosaSettings.CompileTimeFile = Path.Combine(defaultFolder, $"{baseFilename}-time.txt");
		}

		if (MosaSettings.DebugFile == "%DEFAULT%")
		{
			MosaSettings.DebugFile = Path.Combine(defaultFolder, $"{baseFilename}.debug");
		}

		if (MosaSettings.InlinedFile == "%DEFAULT%")
		{
			MosaSettings.InlinedFile = Path.Combine(defaultFolder, $"{baseFilename}-inlined.txt");
		}

		if (MosaSettings.PreLinkHashFile == "%DEFAULT%")
		{
			MosaSettings.PreLinkHashFile = Path.Combine(defaultFolder, $"{baseFilename}-prelink-hash.txt");
		}

		if (MosaSettings.PostLinkHashFile == "%DEFAULT%")
		{
			MosaSettings.PostLinkHashFile = Path.Combine(defaultFolder, $"{baseFilename}-postlink-hash.txt");
		}

		if (MosaSettings.AsmFile == "%DEFAULT%")
		{
			MosaSettings.AsmFile = Path.Combine(defaultFolder, $"{baseFilename}.asm");
		}

		if (MosaSettings.NasmFile == "%DEFAULT%")
		{
			MosaSettings.NasmFile = Path.Combine(defaultFolder, $"{baseFilename}.nasm");
		}
	}

	protected void Output(string status)
	{
		if (status == null)
			return;

		OutputEvent(status);
	}

	protected virtual void OutputEvent(string status)
	{
		CompilerHooks.NotifyStatus?.Invoke(status);
	}

	protected static byte[] GetResource(string path, string name)
	{
		var newname = path.Replace(".", "._").Replace(@"\", "._").Replace("/", "._").Replace("-", "_") + "." + name;
		return GetResource(newname);
	}

	protected static byte[] GetResource(string name)
	{
		var assembly = Assembly.GetExecutingAssembly();
		var stream = assembly.GetManifestResourceStream("Mosa.Utility.Launcher.Resources." + name);
		var binary = new BinaryReader(stream);
		return binary.ReadBytes((int)stream.Length);
	}

	protected static string Quote(string location)
	{
		return '"' + location + '"';
	}

	protected Process CreateApplicationProcess(string app, string args)
	{
		Output($"Starting Application: {app}");
		Output($"Arguments: {args}");

		var process = new Process();

		process.StartInfo.FileName = app;
		process.StartInfo.Arguments = args;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = false;
		process.StartInfo.RedirectStandardError = false;
		process.StartInfo.CreateNoWindow = true;

		return process;
	}

	protected Process LaunchApplication(string app, string args)
	{
		Output($"Launching Application: {app}");
		Output($"Arguments: {args}");

		var start = new ProcessStartInfo
		{
			FileName = app,
			Arguments = args,
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		return Process.Start(start);
	}

	protected Process LaunchConsoleApplication(string app, string args)
	{
		Output($"Launching Console Application: {app}");
		Output($"Arguments: {args}");

		var start = new ProcessStartInfo
		{
			FileName = app,
			Arguments = args,
			UseShellExecute = false,
			CreateNoWindow = false,
			RedirectStandardOutput = false,
			RedirectStandardError = false
		};

		return Process.Start(start);
	}

	protected string GetOutput(Process process)
	{
		var output = process.StandardOutput.ReadToEnd();

		process.WaitForExit();

		var error = process.StandardError.ReadToEnd();

		return output + error;
	}

	protected Process LaunchApplicationWithOutput(string app, string arg)
	{
		var process = LaunchApplication(app, arg);

		var output = GetOutput(process);
		Output(output);

		return process;
	}

	protected string NullToEmpty(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return string.Empty;

		return value;
	}
}
