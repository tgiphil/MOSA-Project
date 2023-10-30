// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x64.Instructions;

/// <summary>
/// Neg64
/// </summary>
/// <seealso cref="Mosa.Compiler.x64.X64Instruction" />
public sealed class Neg64 : X64Instruction
{
	internal Neg64()
		: base(1, 1)
	{
	}

	public override bool IsZeroFlagUnchanged => true;

	public override bool IsZeroFlagUndefined => true;

	public override bool IsCarryFlagUnchanged => true;

	public override bool IsCarryFlagUndefined => true;

	public override bool IsSignFlagUnchanged => true;

	public override bool IsSignFlagUndefined => true;

	public override bool IsOverflowFlagUnchanged => true;

	public override bool IsOverflowFlagUndefined => true;

	public override bool IsParityFlagUnchanged => true;

	public override bool IsParityFlagUndefined => true;

	public override void Emit(Node node, OpcodeEncoder opcodeEncoder)
	{
		System.Diagnostics.Debug.Assert(node.ResultCount == 1);
		System.Diagnostics.Debug.Assert(node.OperandCount == 1);
		System.Diagnostics.Debug.Assert(node.Result.IsPhysicalRegister);
		System.Diagnostics.Debug.Assert(node.Operand1.IsPhysicalRegister);
		System.Diagnostics.Debug.Assert(node.Result.Register == node.Operand1.Register);

		opcodeEncoder.SuppressByte(0x40);
		opcodeEncoder.Append4Bits(0b0100);
		opcodeEncoder.Append1Bit(0b1);
		opcodeEncoder.Append1Bit(0b0);
		opcodeEncoder.Append1Bit(0b0);
		opcodeEncoder.Append1Bit(node.Result.Register.RegisterCode >> 3);
		opcodeEncoder.Append8Bits(0xF7);
		opcodeEncoder.Append2Bits(0b11);
		opcodeEncoder.Append3Bits(0b011);
		opcodeEncoder.Append3Bits(node.Result.Register.RegisterCode);
	}
}
