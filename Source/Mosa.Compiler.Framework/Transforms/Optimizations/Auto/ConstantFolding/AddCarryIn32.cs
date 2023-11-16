// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Auto.ConstantFolding;

[Transform()]
public sealed class AddCarryIn32 : BaseTransform
{
	public AddCarryIn32() : base(IR.AddCarryIn32, TransformType.Auto | TransformType.Optimization)
	{
	}

	public override int Priority => 100;

	public override bool Match(Context context, Transform transform)
	{
		if (!IsResolvedConstant(context.Operand1))
			return false;

		if (!IsResolvedConstant(context.Operand2))
			return false;

		if (!IsResolvedConstant(context.Operand3))
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;

		var t1 = context.Operand1;
		var t2 = context.Operand2;
		var t3 = context.Operand3;

		var e1 = Operand.CreateConstant(Add32(Add32(To32(t1), To32(t2)), BoolTo32(To32(t3))));

		context.SetInstruction(IR.Move32, result, e1);
	}
}
