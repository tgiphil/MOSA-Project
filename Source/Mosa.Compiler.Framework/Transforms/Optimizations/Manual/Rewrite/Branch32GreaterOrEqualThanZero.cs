﻿// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Manual.Rewrite;

public sealed class Branch32GreaterOrEqualThanZero : BaseTransform
{
	public Branch32GreaterOrEqualThanZero() : base(IR.Branch32, TransformType.Manual | TransformType.Optimization)
	{
	}

	public override bool Match(Context context, Transform transform)
	{
		if (context.ConditionCode != ConditionCode.UnsignedGreaterOrEqual)
			return false;

		if (!IsZero(context.Operand2))
			return false;

		if (context.Block.NextBlocks.Count == 1)
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		var target = context.BranchTarget1;

		var phiBlock = GetOtherBranchTarget(context.Block, target);

		context.SetInstruction(IR.Jmp, target);

		RemoveRemainingInstructionInBlock(context);

		Framework.Transform.UpdatePhiBlock(phiBlock);
	}
}
