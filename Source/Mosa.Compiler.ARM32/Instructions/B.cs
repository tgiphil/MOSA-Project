// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.ARM32.Instructions;

/// <summary>
/// B - Call a subroutine
/// </summary>
/// <seealso cref="Mosa.Compiler.ARM32.ARM32Instruction" />
public sealed class B : ARM32Instruction
{
	internal B()
		: base(0, 0)
	{
	}

	public override bool IsFlowNext => false;

	public override bool IsConditionalBranch => true;

	public override void Emit(InstructionNode node, OpcodeEncoder opcodeEncoder)
	{
		System.Diagnostics.Debug.Assert(node.ResultCount == 0);
		System.Diagnostics.Debug.Assert(node.OperandCount == 0);

		opcodeEncoder.Append4Bits(GetConditionCode(node.ConditionCode));
		opcodeEncoder.Append4Bits(0b1010);
		opcodeEncoder.EmitRelative24(node.BranchTargets[0].Label);
	}
}