// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x86.Instructions;

/// <summary>
/// AndN32
/// </summary>
/// <seealso cref="Mosa.Compiler.x86.X86Instruction" />
public sealed class AndN32 : X86Instruction
{
	internal AndN32()
		: base(1, 2)
	{
	}

	public override bool IsCommutative => true;

	public override bool IsSignFlagModified => true;

	public override bool IsOverflowFlagCleared => true;

	public override bool IsOverflowFlagModified => true;

	public override bool IsParityFlagUnchanged => true;

	public override bool IsParityFlagUndefined => true;

	public override void Emit(Node node, OpcodeEncoder opcodeEncoder)
	{
		System.Diagnostics.Debug.Assert(node.ResultCount == 1);
		System.Diagnostics.Debug.Assert(node.OperandCount == 2);

		opcodeEncoder.Append8Bits(0xC4);
		opcodeEncoder.Append1Bit(0b1);
		opcodeEncoder.Append1Bit(0b1);
		opcodeEncoder.Append1Bit(0b1);
		opcodeEncoder.Append4Bits(0b0001);
		opcodeEncoder.Append1Bit(0b0);
		opcodeEncoder.Append1Bit(0b0);
		opcodeEncoder.Append4BitsNot(node.Operand1.Register.RegisterCode);
		opcodeEncoder.Append1Bit(0b0);
		opcodeEncoder.Append2Bits(0b00);
		opcodeEncoder.Append8Bits(0xF2);
		opcodeEncoder.Append2Bits(0b11);
		opcodeEncoder.Append3Bits(node.Operand2.Register.RegisterCode);
		opcodeEncoder.Append3Bits(node.Result.Register.RegisterCode);
	}
}
