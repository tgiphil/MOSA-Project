# MOSA Project – Copilot Coding Agent Instructions

## Project Overview

MOSA (Managed Operating System Alliance) is an open-source project that natively executes .NET applications within a virtual hypervisor or on bare metal hardware. It consists of:

- **Compiler** – A high-quality, multithreaded, cross-platform, optimizing .NET compiler
- **Kernel** – A small micro-kernel operating system
- **Device Drivers Framework** – A modular device driver framework
- **Debugger** – A QEMU-based debugger for testing and debugging

Website: http://www.mosa-project.org/

## Repository Layout

```
/
├── Source/                        # All C# source code
│   ├── Mosa.sln                   # Windows solution (all projects)
│   ├── Mosa.Linux.sln             # Linux/macOS solution (cross-platform subset)
│   ├── Directory.Build.props      # Global MSBuild properties (TargetFramework, LangVersion, OutputPath)
│   ├── .editorconfig              # Code style rules
│   ├── Common.ruleset             # Code analysis ruleset
│   ├── Compile.bat / Compile.sh   # Top-level build scripts
│   ├── Mosa.Compiler.*/           # Compiler infrastructure (Framework, Common, x86, x64, ARM32, ARM64, MosaTypeSystem)
│   ├── Mosa.Runtime.*/            # Platform-specific runtimes (x86, x64, ARM32, ARM64)
│   ├── Mosa.Kernel.BareMetal.*/   # Bare metal kernel implementations per platform
│   ├── Mosa.BareMetal.*/          # Demo/sample applications (Starter, TestWorld, CoolWorld) × architectures
│   ├── Mosa.Tool.*/               # GUI & CLI tools (Compiler, Launcher, Debugger, Explorer, Bootstrap)
│   ├── Mosa.Plug.Korlib.*/        # Platform-specific .NET core library plug-ins
│   ├── Mosa.DeviceDriver/         # Device driver implementations
│   ├── Mosa.DeviceSystem/         # Device system abstraction layer
│   ├── Mosa.FileSystem/           # File system implementation
│   ├── Mosa.Korlib/               # Core .NET library implementation for MOSA
│   ├── Mosa.TinyCoreLib/          # Minimal core library for embedded scenarios
│   ├── Mosa.UnitTests/            # xUnit unit test projects
│   ├── Mosa.Utility.*/            # Utility projects (UnitTests runner, Launcher, BootImage, etc.)
│   ├── Mosa.Packages/             # NuGet package spec files (.nuspec)
│   └── Mosa.Templates/            # NuGet project templates
├── Tests/                         # Test runner shell/batch scripts
├── Demos/                         # Demo launch scripts
├── .github/workflows/             # GitHub Actions CI/CD
│   ├── builds.yml                 # Main build, test, and packaging pipeline
│   └── docs.yml                   # Documentation build & deploy
└── bin/                           # Build output (all projects output here; gitignored)
```

## Language & Framework

- **Language**: C# 14.0
- **Target Framework**: .NET 10.0 (`net10.0`)
- **Nullable reference types**: Disabled (`<Nullable>disable</Nullable>`)
- **Implicit usings**: Enabled
- **All projects output to**: `bin/` (relative to repo root), configured in `Source/Directory.Build.props`

## Building

### Windows
```bat
# Release build (all projects)
dotnet build Source/Mosa.sln /p:Version=2.6.1.0

# Debug build
dotnet build Source/Mosa.sln
```

### Linux / macOS
```sh
# Use the cross-platform solution (excludes Windows-only GUI projects)
dotnet restore Source/Mosa.Linux.sln
dotnet build Source/Mosa.Linux.sln
```

Build scripts are also available: `Source/Compile.bat`, `Source/Compile.sh`, `Source/Compile-Debug.bat`.

## Running xUnit Tests

```sh
# Run all xUnit tests (dotnet test works on all platforms)
dotnet test Source/Mosa.sln        # Windows
dotnet test Source/Mosa.Linux.sln  # Linux/macOS
```

xUnit projects: `Mosa.Compiler.Framework.xUnit`, `Mosa.Compiler.Common.xUnit`.

## Running MOSA-Specific Unit Tests (requires QEMU)

These tests compile and run bare-metal code through the MOSA compiler and verify execution in QEMU:

```sh
# Windows (optimization level 0–9)
bin\Mosa.Utility.UnitTests.exe -check -o5 -counters

# Linux/macOS
dotnet bin/Mosa.Utility.UnitTests.dll -check -o5 -counters
```

Demo/integration tests:
```sh
# Windows
bin\Mosa.Tool.Launcher.Console.exe bin\Mosa.BareMetal.TestWorld.x86.dll -o5 -check -test

# Linux/macOS
dotnet bin/Mosa.Tool.Launcher.Console.dll bin/Mosa.BareMetal.TestWorld.x86.dll -o5 -check -test
```

QEMU (`qemu-system-x86`) must be installed for unit tests and demo tests. On Linux: `sudo apt-get install qemu-system-x86`. On macOS: `brew install qemu`.

## Code Style

Enforced via `Source/.editorconfig`:

- **Indentation**: Tabs, width 4
- **Brace style**: Allman (open brace on its own line for all constructs)
- **Modifier order**: `private, async, protected, public, internal, volatile, abstract, new, override, sealed, static, virtual, extern, readonly, unsafe, file, required`
- **`var` usage**: Preferred for built-in types and when type is apparent
- **Compound assignment**: Preferred (`+=`, `-=`, etc.)
- **Auto-properties**: Preferred

No explicit CLI linting tool is configured; rely on the EditorConfig settings and `Common.ruleset` for code analysis.

## CI/CD Pipeline

Defined in `.github/workflows/builds.yml`. Key facts:

- Triggered on every push and PR (except `Source/Docs/**` changes)
- **Build version**: `2.6.1.<run_number>`
- **Platforms**: Windows (`windows-latest`), Linux (`ubuntu-latest`), macOS (`macos-26`)
- **Stage order**:
  1. Platform builds (`dotnet build` + `dotnet test`)
  2. Unit tests at optimization levels 0–9 (parallel matrix, need QEMU)
  3. Demo tests at optimization levels 0–9 (parallel matrix, need QEMU)
  4. Compile tests (x64, ARM32 without running)
  5. NuGet packaging & publishing (master branch only)
  6. Artifact merge
- NuGet packages published: `Mosa.Tools.Package`, `Mosa.Tools.Package.Qemu`, `Mosa.Platform`, `Mosa.Platform.x86`, `Mosa.DeviceSystem`, `Mosa.Templates`

Documentation CI is in `.github/workflows/docs.yml` (Sphinx, deploys to GitHub Pages on the `docs` branch).

## Key Architectural Concepts

- **Compiler pipeline**: The compiler takes a .NET assembly (`*.dll`) and compiles it to native code for a target architecture. The main entry point tool is `Mosa.Tool.Launcher.Console` (CLI) or `Mosa.Tool.Launcher` (GUI).
- **Optimization levels**: `-o0` through `-o9` control compiler optimization aggressiveness. Changes to the compiler should be tested at multiple optimization levels.
- **Platform backends**: Each target architecture (x86, x64, ARM32, ARM64) has its own compiler project (`Mosa.Compiler.x86`, etc.) and runtime (`Mosa.Runtime.x86`, etc.).
- **Plug system**: `Mosa.Plug.Korlib.*` projects provide platform-specific overrides of the .NET core library methods that cannot be compiled naively (e.g., intrinsics, platform-specific primitives).
- **Bare metal execution**: Applications in `Mosa.BareMetal.*` are full OS images. They link against the kernel and runtime and produce bootable disk images.
- **Device drivers**: The `Mosa.DeviceDriver` project contains drivers built on top of the `Mosa.DeviceSystem` abstraction layer.

## Adding / Modifying Code

- **Compiler changes**: Work in `Source/Mosa.Compiler.Framework/` for platform-agnostic changes; use the platform-specific projects for architecture-specific transformations.
- **New unit tests**: Add xUnit tests to `Source/Mosa.Compiler.Framework.xUnit/` or `Source/Mosa.Compiler.Common.xUnit/`. Bare-metal unit tests live in `Source/Mosa.UnitTests/`.
- **New bare-metal demo**: Create a new project under `Source/Mosa.BareMetal.<Name>.x86/` (and other architectures as needed), referencing the kernel and runtime.
- **New device driver**: Add a class implementing the appropriate interface in `Source/Mosa.DeviceDriver/`.
- **Solution files**: When adding a new project, add it to both `Source/Mosa.sln` (Windows) and `Source/Mosa.Linux.sln` (Linux/macOS) unless the project is Windows-only (WinForms/WPF GUI).

## Common Pitfalls

- Do **not** add `<OutputPath>` to individual `.csproj` files; the global `Directory.Build.props` already sets `OutputPath` to `../../bin`.
- Do **not** enable nullable reference types in project files; the global setting disables them intentionally.
- The `bin/` directory at the repo root is the unified output folder for all projects. It is not a subdirectory of `Source/`.
- MOSA targets bare-metal environments, so code inside `Mosa.Korlib`, `Mosa.TinyCoreLib`, and `Mosa.Plug.Korlib.*` cannot use standard .NET BCL features freely — these are reimplementations/overrides for the MOSA runtime.
- The `Mosa.Linux.sln` intentionally excludes Windows-only projects (e.g., WinForms-based tools). When working cross-platform, build and test with `Mosa.Linux.sln`.
- Unit tests that exercise the full compiler + QEMU execution pipeline require QEMU to be installed locally. xUnit tests (`dotnet test`) do not require QEMU.
