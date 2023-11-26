// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.ARM32.CompilerStages;

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

		var r0 = transform.PhysicalRegisters.Allocate64(CPURegister.R0);
		var r1 = transform.PhysicalRegisters.Allocate64(CPURegister.R1);
		var lr = transform.PhysicalRegisters.Allocate64(CPURegister.LR);
		var sp = transform.PhysicalRegisters.Allocate64(CPURegister.SP);

		var d10 = transform.PhysicalRegisters.Allocate64(CPURegister.d10);
		var d11 = transform.PhysicalRegisters.Allocate64(CPURegister.d11);

		var multibootRegister1 = Operand.CreateLabel(MultibootRegister1, Architecture.Is32BitPlatform);
		var multibootRegister2 = Operand.CreateLabel(MultibootRegister2, Architecture.Is32BitPlatform);
		var stackBottom = Operand.CreateLabel(MultibootInitialStack, Architecture.Is32BitPlatform);

		var stackTopOffset = CreateConstant(StackSize - 16);

		var prologueBlock = basicBlocks.CreatePrologueBlock();

		var context = new Context(prologueBlock);

		// Place stack location and size into registers
		context.AppendInstruction(ARM32.Movw, d10, stackBottom);
		context.AppendInstruction(ARM32.Movt, d10, d10, stackBottom);

		context.AppendInstruction(ARM32.Movw, d11, stackTopOffset);
		context.AppendInstruction(ARM32.Movt, d11, d11, stackTopOffset);

		//// Setup the stack and place the sentinel on the stack to indicate the start of the stack
		context.AppendInstruction(ARM32.Mov, sp, d10);
		context.AppendInstruction(ARM32.Add, sp, sp, d11);
		context.AppendInstruction(ARM32.Mov, lr, sp);
		//context.AppendInstruction(X64.MovStore64, null, sp, Operand.Constant64_0, Operand.Constant64_0);
		//context.AppendInstruction(X64.MovStore64, null, sp, Operand.Constant64_16, Operand.Constant64_0);

		//// Place the multiboot address into a static field
		//context.AppendInstruction(X64.MovStore64, null, multibootRegister1, Operand.Constant64_0, r0);
		//context.AppendInstruction(X64.MovStore64, null, multibootRegister2, Operand.Constant64_0, r1);

		//context.AppendInstruction(X64.Call, null, entryPoint);
		//context.AppendInstruction(X64.Ret);

		Compiler.CompileMethod(multibootMethod);
	}
}
