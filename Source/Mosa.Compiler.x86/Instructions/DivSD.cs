// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x86.Instructions;

/// <summary>
/// Divsd
/// </summary>
/// <seealso cref="Mosa.Compiler.x86.X86Instruction" />
public sealed class Divsd : X86Instruction
{
	internal Divsd()
		: base(1, 2)
	{
	}

	public override void Emit(Node node, OpcodeEncoder opcodeEncoder)
	{
		System.Diagnostics.Debug.Assert(node.ResultCount == 1);
		System.Diagnostics.Debug.Assert(node.OperandCount == 2);
		System.Diagnostics.Debug.Assert(node.Result.IsPhysicalRegister);
		System.Diagnostics.Debug.Assert(node.Operand1.IsPhysicalRegister);
		System.Diagnostics.Debug.Assert(node.Result.Register == node.Operand1.Register);

		opcodeEncoder.Append8Bits(0xF2);
		opcodeEncoder.Append8Bits(0x0F);
		opcodeEncoder.Append8Bits(0x5E);
		opcodeEncoder.Append2Bits(0b11);
		opcodeEncoder.Append3Bits(node.Result.Register.RegisterCode);
		opcodeEncoder.Append3Bits(node.Operand2.Register.RegisterCode);
	}
}
