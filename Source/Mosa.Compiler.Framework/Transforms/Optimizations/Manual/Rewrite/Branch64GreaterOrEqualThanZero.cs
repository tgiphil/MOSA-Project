﻿// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Manual.Rewrite;

public sealed class Branch64GreaterOrEqualThanZero : BaseTransform
{
	public Branch64GreaterOrEqualThanZero() : base(IRInstruction.Branch64, TransformType.Manual | TransformType.Optimization)
	{
	}

	public override bool Match(Context context, TransformContext transform)
	{
		if (context.ConditionCode != ConditionCode.UnsignedGreaterOrEqual)
			return false;

		if (!IsZero(context.Operand2))
			return false;

		return true;
	}

	public override void Transform(Context context, TransformContext transform)
	{
		var target = context.BranchTargets[0];

		var phiBlock = GetOtherBranchTarget(context.Block, target);

		context.SetInstruction(IRInstruction.Jmp, target);

		RemoveRemainingInstructionInBlock(context);

		TransformContext.UpdatePhiBlock(phiBlock);
	}
}