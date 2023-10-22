// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x64.Instructions;

/// <summary>
/// MovStore16
/// </summary>
/// <seealso cref="Mosa.Compiler.x64.X64Instruction" />
public sealed class MovStore16 : X64Instruction
{
	internal MovStore16()
		: base(0, 3)
	{
	}

	public override bool IsMemoryWrite => true;

	public override void Emit(Node node, OpcodeEncoder opcodeEncoder)
	{
		System.Diagnostics.Debug.Assert(node.ResultCount == 0);
		System.Diagnostics.Debug.Assert(node.OperandCount == 3);

		if (node.Operand1.IsCPURegister && node.Operand1.Register.RegisterCode == 5 && node.Operand2.IsConstantZero && node.Operand3.IsCPURegister)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand3.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0x89);
			opcodeEncoder.Append2Bits(0b01);
			opcodeEncoder.Append3Bits(node.Operand3.Register.RegisterCode);
			opcodeEncoder.Append3Bits(0b101);
			opcodeEncoder.Append8Bits(0x00);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand1.Register.RegisterCode == 5 && node.Operand2.IsCPURegister && node.Operand3.IsCPURegister)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand3.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(node.Operand2.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0x89);
			opcodeEncoder.Append2Bits(0b01);
			opcodeEncoder.Append3Bits(node.Operand3.Register.RegisterCode);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(node.Operand2.Register.RegisterCode);
			opcodeEncoder.Append3Bits(0b101);
			opcodeEncoder.Append8Bits(0x00);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand1.Register.RegisterCode == 4 && node.Operand2.IsConstantZero && node.Operand3.IsCPURegister)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand3.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0x89);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(node.Operand3.Register.RegisterCode);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append3Bits(0b100);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand1.Register.RegisterCode == 4 && node.Operand2.IsConstant && node.Operand2.ConstantSigned32 >= -128 && node.Operand2.ConstantSigned32 <= 127 && node.Operand3.IsCPURegister)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand3.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0x89);
			opcodeEncoder.Append2Bits(0b01);
			opcodeEncoder.Append3Bits(node.Operand3.Register.RegisterCode);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append8BitImmediate(node.Operand2);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand1.Register.RegisterCode == 4 && node.Operand2.IsConstant && node.Operand3.IsCPURegister)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand3.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0x89);
			opcodeEncoder.Append2Bits(0b10);
			opcodeEncoder.Append3Bits(node.Operand3.Register.RegisterCode);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append8BitImmediate(node.Operand2);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand2.IsCPURegister && node.Operand3.IsCPURegister)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand3.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(node.Operand2.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0x89);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(node.Operand3.Register.RegisterCode);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(node.Operand2.Register.RegisterCode);
			opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand2.IsConstantZero && node.Operand3.IsCPURegister)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand3.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand1.Register.RegisterCode >> 3);
			opcodeEncoder.Append8Bits(0x89);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(node.Operand3.Register.RegisterCode);
			opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand2.IsConstant && node.Operand2.ConstantSigned32 >= -128 && node.Operand2.ConstantSigned32 <= 127 && node.Operand3.IsCPURegister)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand3.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand1.Register.RegisterCode >> 3);
			opcodeEncoder.Append8Bits(0x89);
			opcodeEncoder.Append2Bits(0b01);
			opcodeEncoder.Append3Bits(node.Operand3.Register.RegisterCode);
			opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
			opcodeEncoder.Append8BitImmediate(node.Operand2);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand2.IsConstant && node.Operand3.IsCPURegister)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand3.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand1.Register.RegisterCode >> 3);
			opcodeEncoder.Append8Bits(0x89);
			opcodeEncoder.Append2Bits(0b10);
			opcodeEncoder.Append3Bits(node.Operand3.Register.RegisterCode);
			opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
			opcodeEncoder.Append32BitImmediate(node.Operand2);
			return;
		}

		if (node.Operand1.IsConstant && node.Operand2.IsConstantZero && node.Operand3.IsCPURegister)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand3.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0x89);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(node.Operand3.Register.RegisterCode);
			opcodeEncoder.Append3Bits(0b101);
			opcodeEncoder.Append32BitImmediate(node.Operand1);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand1.Register.RegisterCode == 5 && node.Operand2.IsConstantZero && node.Operand3.IsConstant)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0xC7);
			opcodeEncoder.Append2Bits(0b01);
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append3Bits(0b101);
			opcodeEncoder.Append8Bits(0x00);
			opcodeEncoder.Append16BitImmediate(node.Operand3);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand1.Register.RegisterCode == 5 && node.Operand2.IsCPURegister && node.Operand3.IsConstant)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand2.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0xC7);
			opcodeEncoder.Append2Bits(0b01);
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(node.Operand2.Register.RegisterCode);
			opcodeEncoder.Append3Bits(0b101);
			opcodeEncoder.Append8Bits(0x00);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand1.Register.RegisterCode == 4 && node.Operand2.IsConstantZero && node.Operand3.IsConstant)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0xC7);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append16BitImmediate(node.Operand3);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand1.Register.RegisterCode == 4 && node.Operand2.IsConstant && node.Operand2.ConstantSigned32 >= -128 && node.Operand2.ConstantSigned32 <= 127 && node.Operand3.IsConstant)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0xC7);
			opcodeEncoder.Append2Bits(0b01);
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append8BitImmediate(node.Operand2);
			opcodeEncoder.Append16BitImmediate(node.Operand3);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand1.Register.RegisterCode == 4 && node.Operand2.IsConstant && node.Operand3.IsConstant)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0xC7);
			opcodeEncoder.Append2Bits(0b10);
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append8BitImmediate(node.Operand2);
			opcodeEncoder.Append32BitImmediate(node.Operand3);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand2.IsCPURegister && node.Operand3.IsConstant)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand2.Register.RegisterCode >> 3);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0xC7);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append3Bits(0b100);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(node.Operand2.Register.RegisterCode);
			opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
			opcodeEncoder.Append16BitImmediate(node.Operand3);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand2.IsConstantZero && node.Operand3.IsConstant)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand1.Register.RegisterCode >> 3);
			opcodeEncoder.Append8Bits(0xC7);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
			opcodeEncoder.Append16BitImmediate(node.Operand3);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand2.IsConstant && node.Operand2.ConstantSigned32 >= -128 && node.Operand2.ConstantSigned32 <= 127 && node.Operand3.IsConstant)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand1.Register.RegisterCode >> 3);
			opcodeEncoder.Append8Bits(0xC7);
			opcodeEncoder.Append2Bits(0b01);
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
			opcodeEncoder.Append8BitImmediate(node.Operand2);
			opcodeEncoder.Append16BitImmediate(node.Operand3);
			return;
		}

		if (node.Operand1.IsCPURegister && node.Operand2.IsConstant && node.Operand3.IsConstant)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(node.Operand1.Register.RegisterCode >> 3);
			opcodeEncoder.Append8Bits(0xC7);
			opcodeEncoder.Append2Bits(0b10);
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
			opcodeEncoder.Append32BitImmediate(node.Operand2);
			opcodeEncoder.Append16BitImmediate(node.Operand3);
			return;
		}

		if (node.Operand1.IsConstant && node.Operand2.IsConstantZero && node.Operand3.IsConstant)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0xC7);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append3Bits(0b101);
			opcodeEncoder.Append32BitImmediate(node.Operand1);
			opcodeEncoder.Append16BitImmediate(node.Operand3);
			return;
		}

		if (node.Operand1.IsConstant && node.Operand2.IsConstant && node.Operand3.IsConstant)
		{
			opcodeEncoder.Append8Bits(0x66);
			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append8Bits(0xC7);
			opcodeEncoder.Append2Bits(0b00);
			opcodeEncoder.Append3Bits(0b000);
			opcodeEncoder.Append3Bits(0b101);
			opcodeEncoder.Append32BitImmediateWithOffset(node.Operand1, node.Operand2);
			opcodeEncoder.Append16BitImmediate(node.Operand3);
			return;
		}

		if (node.Operand1.IsConstant && node.Operand2.IsConstant && node.Operand3.IsCPURegister)
		{
			return;
		}

		throw new Compiler.Common.Exceptions.CompilerException("Invalid Opcode");
	}
}
