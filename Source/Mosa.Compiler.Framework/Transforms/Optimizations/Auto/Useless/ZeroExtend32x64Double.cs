// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Auto.Useless;

/// <summary>
/// ZeroExtend32x64Double
/// </summary>
[Transform("IR.Optimizations.Auto.Useless")]
public sealed class ZeroExtend32x64Double : BaseTransform
{
	public ZeroExtend32x64Double() : base(IRInstruction.ZeroExtend32x64, TransformType.Auto | TransformType.Optimization)
	{
	}

	public override int Priority => 85;

	public override bool Match(Context context, Transform transform)
	{
		if (!context.Operand1.IsVirtualRegister)
			return false;

		if (!context.Operand1.IsDefinedOnce)
			return false;

		if (context.Operand1.Definitions[0].Instruction != IRInstruction.ZeroExtend32x64)
			return false;

		if (IsConstant(context.Operand1.Definitions[0].Operand1))
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;

		var t1 = context.Operand1.Definitions[0].Operand1;

		context.SetInstruction(IRInstruction.Move64, result, t1);
	}
}
