﻿// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x86.Transforms.Optimizations.Manual.Rewrite;

[Transform]
public sealed class Cmp32ToTest32 : BaseTransform
{
	public Cmp32ToTest32() : base(X86.Cmp32, TransformType.Manual | TransformType.Optimization)
	{
	}

	public override bool Match(Context context, Transform transform)
	{
		if (!context.Operand2.IsResolvedConstant)
			return false;

		if (!context.Operand2.IsConstantZero)
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		context.SetInstruction(X86.Test32, null, context.Operand1, context.Operand1);
	}
}
