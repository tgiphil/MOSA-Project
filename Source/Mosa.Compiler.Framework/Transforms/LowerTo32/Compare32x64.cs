﻿// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace Mosa.Compiler.Framework.Transforms.LowerTo32;

public sealed class Compare32x64 : BaseLowerTo32Transform
{
	public Compare32x64() : base(IR.Compare32x64, TransformType.Manual | TransformType.Optimization)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;
		var operand1 = context.Operand1;
		var operand2 = context.Operand2;
		var condition = context.ConditionCode;

		context.SetInstruction(IR.Compare32x32, condition, result, operand1, operand2);
		context.AppendInstruction(IR.To64, result, Operand.Constant32_0);
	}
}
