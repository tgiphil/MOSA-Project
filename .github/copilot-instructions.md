# MOSA Project - GitHub Copilot Instructions

## Project Overview

MOSA (Managed Operating System Alliance) is an open-source project that natively executes .NET applications within a virtual hypervisor or on bare metal hardware. The project consists of:

- **Compiler**: A high-quality, multithreaded, cross-platform, optimizing .NET compiler
- **Kernel**: A small, micro-kernel operating system
- **Device Drivers Framework**: A modular device drivers framework and device drivers
- **Debugger**: QEMU-based debugger

## Project Structure

The MOSA solution contains 66+ projects organized as follows:

### Key Components

- **Mosa.Compiler.Framework**: Core compiler framework and infrastructure
- **Mosa.Compiler.{x86,x64,ARM32,ARM64}**: Architecture-specific compiler backends
- **Mosa.Compiler.MosaTypeSystem**: Custom type system for .NET metadata
- **Mosa.Kernel.BareMetal**: Micro-kernel operating system
- **Mosa.Kernel.BareMetal.{x86,x64,ARM32,ARM64}**: Architecture-specific kernel implementations
- **Mosa.Korlib**: MOSA's core library (similar to mscorlib)
- **Mosa.Runtime**: Runtime support code
- **Mosa.DeviceSystem**: Device driver framework
- **Mosa.DeviceDriver**: Device driver implementations
- **Mosa.Tool.***: Various tools (Compiler, Debugger, Explorer, Launcher)
- **Mosa.Utility.***: Utility libraries
- **Mosa.BareMetal.{TestWorld,CoolWorld}**: Demo applications

## Coding Standards

### General Guidelines

1. **License Header**: All source files must include the copyright header:
   ```csharp
   // Copyright (c) MOSA Project. Licensed under the New BSD License.
   ```

2. **Code Style**:
   - Use **tabs for indentation** (size: 4)
   - Opening braces on new line (Allman style)
   - Trim trailing whitespace
   - Insert final newline in all files
   - Use `var` for built-in types and when type is apparent

3. **File Organization**:
   - Sort using directives with System directives first
   - Use file-scoped namespaces when appropriate
   - Keep related code together

4. **Naming Conventions**:
   - PascalCase for types, methods, properties, and public fields
   - camelCase for private fields, parameters, and local variables
   - Use descriptive names that clearly indicate purpose

### Language Features

- **Target Framework**: .NET 10.0
- **Language Version**: C# 14.0
- **Nullable Reference Types**: Disabled project-wide
- **Implicit Usings**: Enabled

### Code Analysis

- Projects use `Common.ruleset` for code analysis
- Follow Roslynator analyzer recommendations
- Address all warnings before committing

## Architecture-Specific Guidelines

### Multi-Architecture Support

MOSA supports multiple CPU architectures: x86, x64, ARM32, and ARM64. When working on architecture-specific code:

1. **Location**: Architecture-specific implementations are in separate projects (e.g., `Mosa.Compiler.x86`, `Mosa.Compiler.x64`)
2. **Base Classes**: Extend from `BaseArchitecture` for architecture implementations
3. **Consistency**: Maintain parallel implementations across architectures when adding features
4. **Testing**: Consider testing on multiple architectures when changing shared code

### Key Architectural Concepts

- **Physical Registers**: Each architecture defines its register set
- **Stack Frame/Pointer Registers**: Architecture-specific stack management
- **Calling Conventions**: Platform-specific calling conventions
- **ELF Machine Types**: Architecture-specific ELF binary format

## Compiler Framework

### Type System

The MOSA Type System converts raw metadata from dnlib to custom models:

1. **Root**: `TypeSystem` provides access to built-in types and modules
2. **Core Classes**: `MosaUnit`, `MosaModule`, `MosaType`, `MosaMethod`, `MosaField`
3. **Immutability**: Type system instances are immutable after population
4. **Workflow**:
   - Load modules via `IModuleLoader`
   - Get metadata via `IMetadata`
   - Initialize type system via `TypeSystem.Load(IMetadata)`

### Transforms

MOSA uses a transformation-based compiler architecture:

- **BaseBlockTransform**: Base class for block-level transformations
- **Priority**: Transformations have priorities for ordering
- **Process Method**: Implement `Process(Transform transform)` for custom transformations
- **Context**: Use `Context` for accessing and manipulating instructions

### Common Patterns

1. **Register Allocation**: Use architecture-specific register sets
2. **IR Instructions**: Work with intermediate representation instructions
3. **Code Generation**: Backend transforms IR to native instructions
4. **Optimization**: Apply transformations in stages

## Build and Test

### Building

```bash
# Windows
Compile.bat

# Linux
./Compile.sh
```

### Project Configuration

- **Output Directory**: `../../bin` (relative to project)
- **Build Configuration**: Defined in `Directory.Build.props`
- **Solution Files**: `Mosa.sln` (Windows), `Mosa.Linux.sln` (Linux)

### Testing

- Unit tests use xUnit.net framework
- Projects with `.xUnit` suffix contain test code
- Test projects: `Mosa.Compiler.Common.xUnit`, `Mosa.Compiler.Framework.xUnit`, `Mosa.UnitTests.*`

## Device Drivers

When working on device drivers:

1. **Framework**: Use `Mosa.DeviceSystem` for device abstraction
2. **Implementation**: Actual drivers are in `Mosa.DeviceDriver`
3. **Modularity**: Design drivers to be modular and reusable
4. **Platform Support**: Consider cross-platform compatibility

## Kernel Development

### BareMetal Kernel

- **Micro-kernel Design**: Keep kernel minimal and modular
- **Architecture Variants**: Maintain separate implementations per architecture
- **Boot Process**: Understand bootloader and initialization sequence
- **System Calls**: Use defined system call interface

### Korlib

MOSA's core library (`Mosa.Korlib`) provides essential types and runtime support:

- Implements subset of .NET Base Class Library
- Optimized for bare metal execution
- No dependencies on standard .NET runtime

## Common Tasks

### Adding a New Transformation

1. Extend `BaseBlockTransform` or appropriate base class
2. Implement `Process(Transform transform)` method
3. Set appropriate priority for ordering
4. Register transformation in appropriate stage

### Adding Instruction Support

1. Define instruction in architecture-specific project
2. Implement encoding/decoding logic
3. Add transformation rules
4. Update instruction list generation

### Working with Metadata

1. Use `MosaType`, `MosaMethod`, `MosaField` for type system access
2. Respect immutability of type system after loading
3. Use `IMetadata` interface for metadata access
4. Handle generic types and methods appropriately

## Dependencies

Key external libraries used:

- **dnlib**: .NET metadata reading (MIT License)
- **xUnit.net**: Unit testing (Apache License)
- **SharpDisasm**: Disassembly for debugging (BSD License)
- **DockPanel Suite**: UI docking panels (MIT License)

## Best Practices

1. **Performance**: MOSA is a systems project - optimize for performance where it matters
2. **Memory Management**: Be mindful of memory allocations and garbage collection
3. **Safety**: Use safe practices, but understand when unsafe code is necessary
4. **Documentation**: Document complex algorithms and non-obvious code
5. **Testing**: Write tests for new functionality
6. **Cross-platform**: Consider all supported architectures when making changes
7. **Backwards Compatibility**: Maintain compatibility with existing code when possible

## Resources

- Website: http://www.mosa-project.org/
- Repository: https://github.com/mosa/MOSA-Project
- License: New BSD License
- Discord: https://discord.gg/tRNMn3npsv

## Important Notes

- This is a bare-metal and low-level systems project
- Many standard .NET assumptions don't apply
- Understanding of assembly language and computer architecture is essential
- Code runs without standard .NET runtime or OS services
- Focus on minimal dependencies and optimal performance

## When Contributing

1. Follow the established code style (.editorconfig)
2. Include appropriate license headers
3. Test on multiple architectures if relevant
4. Update documentation for significant changes
5. Consider performance implications
6. Maintain consistency with existing patterns
