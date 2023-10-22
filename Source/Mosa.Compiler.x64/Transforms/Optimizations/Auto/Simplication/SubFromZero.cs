// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x64.Transforms.Optimizations.Auto.Simplication;

/// <summary>
/// SubFromZero
/// </summary>
[Transform("x64.Optimizations.Auto.Simplication")]
public sealed class SubFromZero : BaseTransform
{
	public SubFromZero() : base(X64.Sub64, TransformType.Auto | TransformType.Optimization)
	{
	}

	public override bool Match(Context context, Transform transform)
	{
		if (!context.Operand1.IsResolvedConstant)
			return false;

		if (context.Operand1.ConstantUnsigned64 != 0)
			return false;

		if (!IsVirtualRegister(context.Operand2))
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;

		var t1 = context.Operand2;

		context.SetInstruction(X64.Neg64, result, t1);
	}
}
