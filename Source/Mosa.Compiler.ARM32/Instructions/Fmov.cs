// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.ARM32.Instructions;

/// <summary>
/// Fmov - Copies aloating-point immediate constant into register
/// </summary>
public sealed class Fmov : ARM32Instruction
{
	internal Fmov()
		: base(1, 2)
	{
	}

	public override void Emit(Node node, OpcodeEncoder opcodeEncoder)
	{
		System.Diagnostics.Debug.Assert(node.ResultCount == 1);
		System.Diagnostics.Debug.Assert(node.OperandCount == 2);

		if (node.Operand1.IsConstant)
		{
			opcodeEncoder.Append4Bits(0b0001);
			opcodeEncoder.Append4Bits(0b1110);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Result.IsR4 ? 0 : 1);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append8BitImmediate(node.Operand2);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append4Bits(0b0000);
			opcodeEncoder.Append1Bit(0b0);
			return;
		}

		if (node.Operand1.IsPhysicalRegister)
		{
			opcodeEncoder.Append4Bits(0b0011);
			opcodeEncoder.Append3Bits(0b110);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append2Bits(0b11);
			opcodeEncoder.Append4Bits(0b0000);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append4Bits(node.Operand1.Register.RegisterCode);
			opcodeEncoder.Append4Bits(node.Result.Register.RegisterCode);
			return;
		}

		throw new Compiler.Common.Exceptions.CompilerException("Invalid Opcode");
	}
}
