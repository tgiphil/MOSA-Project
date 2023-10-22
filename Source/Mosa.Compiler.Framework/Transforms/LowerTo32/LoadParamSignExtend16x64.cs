﻿// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace Mosa.Compiler.Framework.Transforms.LowerTo32;

public sealed class LoadParamSignExtend16x64 : BaseLower32Transform
{
	public LoadParamSignExtend16x64() : base(IRInstruction.LoadParamSignExtend16x64, TransformType.Manual | TransformType.Optimization)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;
		var operand1 = context.Operand1;

		transform.SplitOperand(operand1, out Operand op0Low, out _);

		var resultLow = transform.VirtualRegisters.Allocate32();
		var resultHigh = transform.VirtualRegisters.Allocate32();

		context.SetInstruction(IRInstruction.LoadParamSignExtend16x32, resultLow, op0Low);
		context.AppendInstruction(IRInstruction.ArithShiftRight32, resultHigh, resultLow, Operand.Constant32_31);
		context.AppendInstruction(IRInstruction.To64, result, resultLow, resultHigh);
	}
}
