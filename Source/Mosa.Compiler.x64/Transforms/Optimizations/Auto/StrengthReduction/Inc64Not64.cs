// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x64.Transforms.Optimizations.Auto.StrengthReduction;

[Transform()]
public sealed class Inc64Not64 : BaseTransform
{
	public Inc64Not64() : base(X64.Inc64, TransformType.Auto | TransformType.Optimization)
	{
	}

	public override bool Match(Context context, Transform transform)
	{
		if (!context.Operand1.IsVirtualRegister)
			return false;

		if (!context.Operand1.IsDefinedOnce)
			return false;

		if (context.Operand1.Definitions[0].Instruction != X64.Not64)
			return false;

		if (!IsVirtualRegister(context.Operand1.Definitions[0].Operand1))
			return false;

		if (IsCarryFlagUsed(context))
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;

		var t1 = context.Operand1.Definitions[0].Operand1;

		context.SetInstruction(X64.Neg64, result, t1);
	}
}
