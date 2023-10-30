// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.ARM32.Instructions;

/// <summary>
/// StrS16 - Halfword Data Transfer
/// </summary>
/// <seealso cref="Mosa.Compiler.ARM32.ARM32Instruction" />
public sealed class StrS16 : ARM32Instruction
{
	internal StrS16()
		: base(0, 3)
	{
	}

	public override void Emit(Node node, OpcodeEncoder opcodeEncoder)
	{
		System.Diagnostics.Debug.Assert(node.ResultCount == 0);
		System.Diagnostics.Debug.Assert(node.OperandCount == 3);

		if (node.Operand1.IsPhysicalRegister && node.Operand2.IsConstant && node.Operand3.IsPhysicalRegister)
		{
			opcodeEncoder.Append4Bits(GetConditionCode(node.ConditionCode));
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.StatusRegister == StatusRegister.UpDirection ? 1 : 0);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append4Bits(node.Operand1.Register.RegisterCode);
			opcodeEncoder.Append4Bits(node.Operand3.Register.RegisterCode);
			opcodeEncoder.Append4BitImmediateHighNibble(node.Operand2);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append4BitImmediate(node.Operand2);
			return;
		}

		if (node.Operand1.IsPhysicalRegister && node.Operand2.IsPhysicalRegister && node.Operand3.IsPhysicalRegister)
		{
			opcodeEncoder.Append4Bits(GetConditionCode(node.ConditionCode));
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.StatusRegister == StatusRegister.UpDirection ? 1 : 0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append4Bits(node.Operand1.Register.RegisterCode);
			opcodeEncoder.Append4Bits(node.Operand3.Register.RegisterCode);
			opcodeEncoder.Append4Bits(0b0000);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append1Bit(0b1);
			opcodeEncoder.Append4Bits(node.Operand2.Register.RegisterCode);
			return;
		}

		throw new Compiler.Common.Exceptions.CompilerException("Invalid Opcode");
	}
}
