// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.ARM32.Instructions;

/// <summary>
/// Uxth - Unsigned Extend Halfword
/// </summary>
/// <seealso cref="Mosa.Compiler.ARM32.ARM32Instruction" />
public sealed class Uxth : ARM32Instruction
{
	internal Uxth()
		: base(1, 1)
	{
	}

	public override void Emit(Node node, OpcodeEncoder opcodeEncoder)
	{
		System.Diagnostics.Debug.Assert(node.ResultCount == 1);
		System.Diagnostics.Debug.Assert(node.OperandCount == 1);

		if (node.Operand1.IsPhysicalRegister)
		{
			opcodeEncoder.Append4Bits(GetConditionCode(node.ConditionCode));
			opcodeEncoder.Append4Bits(0b0110);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append4Bits(0b1111);
			opcodeEncoder.Append4Bits(node.Result.Register.RegisterCode);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append4Bits(0b0111);
			opcodeEncoder.Append4Bits(node.Operand1.Register.RegisterCode);
			return;
		}

		throw new Compiler.Common.Exceptions.CompilerException("Invalid Opcode");
	}
}
