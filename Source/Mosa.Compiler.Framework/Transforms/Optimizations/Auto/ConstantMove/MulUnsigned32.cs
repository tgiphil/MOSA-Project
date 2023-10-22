// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Auto.ConstantMove;

/// <summary>
/// MulUnsigned32
/// </summary>
[Transform("IR.Optimizations.Auto.ConstantMove")]
public sealed class MulUnsigned32 : BaseTransform
{
	public MulUnsigned32() : base(IRInstruction.MulUnsigned32, TransformType.Auto | TransformType.Optimization)
	{
	}

	public override bool Match(Context context, Transform transform)
	{
		if (!IsResolvedConstant(context.Operand1))
			return false;

		if (IsResolvedConstant(context.Operand2))
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;

		var t1 = context.Operand1;
		var t2 = context.Operand2;

		context.SetInstruction(IRInstruction.MulUnsigned32, result, t2, t1);
	}
}
