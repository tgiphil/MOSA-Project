// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x86.Instructions;

/// <summary>
/// Popcnt32
/// </summary>
/// <seealso cref="Mosa.Compiler.x86.X86Instruction" />
public sealed class Popcnt32 : X86Instruction
{
	internal Popcnt32()
		: base(1, 1)
	{
	}

	public override bool IsCommutative => true;

	public override bool IsZeroFlagUnchanged => true;

	public override bool IsZeroFlagUndefined => true;

	public override bool IsCarryFlagCleared => true;

	public override bool IsCarryFlagModified => true;

	public override bool IsSignFlagCleared => true;

	public override bool IsSignFlagModified => true;

	public override bool IsOverflowFlagCleared => true;

	public override bool IsOverflowFlagModified => true;

	public override bool IsParityFlagCleared => true;

	public override bool IsParityFlagModified => true;

	public override void Emit(Node node, OpcodeEncoder opcodeEncoder)
	{
		System.Diagnostics.Debug.Assert(node.ResultCount == 1);
		System.Diagnostics.Debug.Assert(node.OperandCount == 1);

		opcodeEncoder.Append8Bits(0xF3);
		opcodeEncoder.Append8Bits(0x0F);
		opcodeEncoder.Append8Bits(0xB8);
		opcodeEncoder.Append2Bits(0b11);
		opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
		opcodeEncoder.Append3Bits(node.Result.Register.RegisterCode);
	}
}
