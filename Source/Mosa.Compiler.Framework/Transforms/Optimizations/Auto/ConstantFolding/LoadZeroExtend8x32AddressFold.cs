// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Auto.ConstantFolding;

/// <summary>
/// LoadZeroExtend8x32AddressFold
/// </summary>
[Transform("IR.Optimizations.Auto.ConstantFolding")]
public sealed class LoadZeroExtend8x32AddressFold : BaseTransform
{
	public LoadZeroExtend8x32AddressFold() : base(IRInstruction.LoadZeroExtend8x32, TransformType.Auto | TransformType.Optimization)
	{
	}

	public override bool Match(Context context, TransformContext transform)
	{
		if (!context.Operand1.IsVirtualRegister)
			return false;

		if (!context.Operand2.IsResolvedConstant)
			return false;

		if (context.Operand2.ConstantUnsigned64 != 0)
			return false;

		if (!context.Operand1.IsDefinedOnce)
			return false;

		if (context.Operand1.Definitions[0].Instruction != IRInstruction.AddressOf)
			return false;

		if (!IsParameter(context.Operand1.Definitions[0].Operand1))
			return false;

		return true;
	}

	public override void Transform(Context context, TransformContext transform)
	{
		var result = context.Result;

		var t1 = context.Operand1.Definitions[0].Operand1;

		context.SetInstruction(IRInstruction.LoadParamZeroExtend8x32, result, t1);
	}
}