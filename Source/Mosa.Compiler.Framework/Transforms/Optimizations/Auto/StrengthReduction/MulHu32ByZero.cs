// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Auto.StrengthReduction;

[Transform()]
public sealed class MulHu32ByZero : BaseTransform
{
	public MulHu32ByZero() : base(IR.MulHu32, TransformType.Auto | TransformType.Optimization)
	{
	}

	public override int Priority => 80;

	public override bool Match(Context context, Transform transform)
	{
		if (!context.Operand2.IsConstantZero)
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;

		var e1 = Operand.CreateConstant(To32(0));

		context.SetInstruction(IR.Move32, result, e1);
	}
}

[Transform()]
public sealed class MulHu32ByZero_v1 : BaseTransform
{
	public MulHu32ByZero_v1() : base(IR.MulHu32, TransformType.Auto | TransformType.Optimization)
	{
	}

	public override int Priority => 80;

	public override bool Match(Context context, Transform transform)
	{
		if (!context.Operand1.IsConstantZero)
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;

		var e1 = Operand.CreateConstant(To32(0));

		context.SetInstruction(IR.Move32, result, e1);
	}
}
