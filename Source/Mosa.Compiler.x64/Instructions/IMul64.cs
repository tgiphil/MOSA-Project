// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x64.Instructions;

/// <summary>
/// IMul64
/// </summary>
/// <seealso cref="Mosa.Compiler.x64.X64Instruction" />
public sealed class IMul64 : X64Instruction
{
	internal IMul64()
		: base(1, 2)
	{
	}

	public override bool IsZeroFlagUnchanged => true;

	public override bool IsZeroFlagUndefined => true;

	public override bool IsCarryFlagModified => true;

	public override bool IsSignFlagUnchanged => true;

	public override bool IsSignFlagUndefined => true;

	public override bool IsOverflowFlagModified => true;

	public override bool IsParityFlagUnchanged => true;

	public override bool IsParityFlagUndefined => true;

	public override void Emit(Node node, OpcodeEncoder opcodeEncoder)
	{
		System.Diagnostics.Debug.Assert(node.ResultCount == 1);
		System.Diagnostics.Debug.Assert(node.OperandCount == 2);
		System.Diagnostics.Debug.Assert(node.Result.IsPhysicalRegister);
		System.Diagnostics.Debug.Assert(node.Operand1.IsPhysicalRegister);
		System.Diagnostics.Debug.Assert(node.Result.Register == node.Operand1.Register);

		opcodeEncoder.SuppressByte(0x40);
		opcodeEncoder.Append4Bits(0b0100);
		opcodeEncoder.Append1Bit(0b1);
		opcodeEncoder.Append1Bit(node.Result.Register.RegisterCode >> 3);
		opcodeEncoder.Append1Bit(0b0);
		opcodeEncoder.Append1Bit(node.Operand2.Register.RegisterCode >> 3);
		opcodeEncoder.Append8Bits(0x0F);
		opcodeEncoder.Append8Bits(0xAF);
		opcodeEncoder.Append2Bits(0b11);
		opcodeEncoder.Append3Bits(node.Result.Register.RegisterCode);
		opcodeEncoder.Append3Bits(node.Operand2.Register.RegisterCode);
	}
}
