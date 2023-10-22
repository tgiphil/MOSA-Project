// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Auto.ConstantFolding;

/// <summary>
/// IfThenElse32AlwaysTrue
/// </summary>
[Transform("IR.Optimizations.Auto.ConstantFolding")]
public sealed class IfThenElse32AlwaysTrue : BaseTransform
{
	public IfThenElse32AlwaysTrue() : base(IRInstruction.IfThenElse32, TransformType.Auto | TransformType.Optimization)
	{
	}

	public override int Priority => 100;

	public override bool Match(Context context, Transform transform)
	{
		if (!IsResolvedConstant(context.Operand1))
			return false;

		if (IsZero(context.Operand1))
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;

		var t1 = context.Operand2;

		context.SetInstruction(IRInstruction.Move32, result, t1);
	}
}
