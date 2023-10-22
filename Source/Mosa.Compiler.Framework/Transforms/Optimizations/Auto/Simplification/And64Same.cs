// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Auto.Simplification;

/// <summary>
/// And64Same
/// </summary>
[Transform("IR.Optimizations.Auto.Simplification")]
public sealed class And64Same : BaseTransform
{
	public And64Same() : base(IRInstruction.And64, TransformType.Auto | TransformType.Optimization)
	{
	}

	public override bool Match(Context context, Transform transform)
	{
		if (!AreSame(context.Operand1, context.Operand2))
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;

		var t1 = context.Operand1;

		context.SetInstruction(IRInstruction.Move64, result, t1);
	}
}
