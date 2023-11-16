// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x86.Instructions;

/// <summary>
/// Sar32
/// </summary>
public sealed class Sar32 : X86Instruction
{
	internal Sar32()
		: base(1, 2)
	{
	}

	public override bool IsZeroFlagModified => true;

	public override bool IsCarryFlagModified => true;

	public override bool IsSignFlagModified => true;

	public override bool IsOverflowFlagModified => true;

	public override bool IsParityFlagModified => true;

	public override void Emit(Node node, OpcodeEncoder opcodeEncoder)
	{
		System.Diagnostics.Debug.Assert(node.ResultCount == 1);
		System.Diagnostics.Debug.Assert(node.OperandCount == 2);
		System.Diagnostics.Debug.Assert(node.Result.IsPhysicalRegister);
		System.Diagnostics.Debug.Assert(node.Operand1.IsPhysicalRegister);
		System.Diagnostics.Debug.Assert(node.Result.Register == node.Operand1.Register);

		if (node.Operand2.IsPhysicalRegister)
		{
			opcodeEncoder.Append8Bits(0xD3);
			opcodeEncoder.Append2Bits(0b11);
			opcodeEncoder.Append3Bits(0b111);
			opcodeEncoder.Append3Bits(node.Result.Register.RegisterCode);
			return;
		}

		if (node.Operand2.IsConstantOne)
		{
			opcodeEncoder.Append8Bits(0xD1);
			opcodeEncoder.Append2Bits(0b11);
			opcodeEncoder.Append3Bits(0b111);
			opcodeEncoder.Append3Bits(node.Result.Register.RegisterCode);
			return;
		}

		if (node.Operand2.IsConstant)
		{
			opcodeEncoder.Append8Bits(0xC1);
			opcodeEncoder.Append2Bits(0b11);
			opcodeEncoder.Append3Bits(0b111);
			opcodeEncoder.Append3Bits(node.Result.Register.RegisterCode);
			opcodeEncoder.Append8BitImmediate(node.Operand2);
			return;
		}

		throw new Compiler.Common.Exceptions.CompilerException("Invalid Opcode");
	}
}
