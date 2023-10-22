// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.ARM32.Instructions;

/// <summary>
/// Rev16 - Byte-Reverse Packed Halfword
/// </summary>
/// <seealso cref="Mosa.Compiler.ARM32.ARM32Instruction" />
public sealed class Rev16 : ARM32Instruction
{
	internal Rev16()
		: base(1, 3)
	{
	}

	public override void Emit(Node node, OpcodeEncoder opcodeEncoder)
	{
		System.Diagnostics.Debug.Assert(node.ResultCount == 1);
		System.Diagnostics.Debug.Assert(node.OperandCount == 3);

		opcodeEncoder.Append32Bits(0x00000000);
	}
}
