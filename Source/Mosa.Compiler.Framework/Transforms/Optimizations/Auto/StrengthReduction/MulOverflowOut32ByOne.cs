// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Auto.StrengthReduction;

public sealed class MulOverflowOut32ByOne : BaseTransform
{
	public MulOverflowOut32ByOne() : base(IR.MulOverflowOut32, TransformType.Auto | TransformType.Optimization)
	{
	}

	public override int Priority => 80;

	public override bool Match(Context context, Transform transform)
	{
		if (!context.Operand2.IsConstantOne)
			return false;

		if (IsResult2Used(context))
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;
		var result2 = context.Result2;

		var t1 = context.Operand1;

		var e1 = Operand.Constant32_0;

		context.SetInstruction(IR.Move32, result, t1);
		context.AppendInstruction(IR.Move32, result2, e1);
	}
}