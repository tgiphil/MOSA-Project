// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x64.CompilerStages;

public sealed class MultibootStage : Framework.Platform.BaseMultibootStage
{
	protected override void Finalization()
	{
		CreateMultibootMethod();

		WriteMultibootHeader(Linker.EntryPoint);
	}

	private void CreateMultibootMethod()
	{
		var basicBlocks = new BasicBlocks();

		var methodCompiler = new MethodCompiler(Compiler, multibootMethod, basicBlocks, 0);
		methodCompiler.MethodData.DoNotInline = true;

		var transform = new Transform();
		transform.SetCompiler(Compiler);
		transform.SetMethodCompiler(methodCompiler);

		var initializeMethod = TypeSystem.GetMethod("Mosa.Runtime.StartUp", "Initialize");
		var entryPoint = Operand.CreateLabel(initializeMethod, Architecture.Is32BitPlatform);

		var rbp = transform.PhysicalRegisters.Allocate64(CPURegister.RBP);
		var rsp = transform.PhysicalRegisters.Allocate64(CPURegister.RSP);

		var stackBottom = Operand.CreateLabel(MultibootInitialStack, Architecture.Is32BitPlatform);

		var prologueBlock = basicBlocks.CreatePrologueBlock();

		var context = new Context(prologueBlock);

		// Create stack frame
		context.AppendInstruction(X64.Mov32, rsp, stackBottom);

		// Create stack sentinel
		context.AppendInstruction(X64.Push64, null, Operand.Constant32_0);
		context.AppendInstruction(X64.Push64, null, Operand.Constant32_0);

		// Push registers onto the new stack
		context.AppendInstruction(X64.Mov32, rbp, rsp);
		context.AppendInstruction(X64.Pushad);
		context.AppendInstruction(X64.Push64, null, rbp);

		context.AppendInstruction(X64.Call, null, entryPoint);
		context.AppendInstruction(X64.Ret);

		Compiler.CompileMethod(multibootMethod);
	}
}
