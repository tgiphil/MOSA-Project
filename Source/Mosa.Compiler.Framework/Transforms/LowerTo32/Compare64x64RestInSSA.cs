﻿// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace Mosa.Compiler.Framework.Transforms.LowerTo32;

public sealed class Compare64x64RestInSSA : BaseLowerTo32Transform
{
	public Compare64x64RestInSSA() : base(IR.Compare64x64, TransformType.Manual | TransformType.Optimization)
	{
	}

	public override bool Match(Context context, Transform transform)
	{
		if (!base.Match(context, transform))
			return false;

		if (context.ConditionCode is ConditionCode.Equal or ConditionCode.NotEqual)
			return false;

		if (!transform.IsInSSAForm)
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		var result = context.Result;
		var operand1 = context.Operand1;
		var operand2 = context.Operand2;

		transform.SplitOperand(result, out Operand resultLow, out _);

		var condition = context.ConditionCode;
		var conditionUnsigned = context.ConditionCode.GetUnsigned();

		var nextBlock = transform.Split(context);
		var newBlocks = transform.CreateNewBlockContexts(5, context.Label);

		Framework.Transform.UpdatePhiTargets(nextBlock.Block.NextBlocks, context.Block, nextBlock.Block);

		var op0Low = transform.VirtualRegisters.Allocate32();
		var op0High = transform.VirtualRegisters.Allocate32();
		var op1Low = transform.VirtualRegisters.Allocate32();
		var op1High = transform.VirtualRegisters.Allocate32();

		context.SetInstruction(IR.GetLow32, op0Low, operand1);
		context.AppendInstruction(IR.GetHigh32, op0High, operand1);
		context.AppendInstruction(IR.GetLow32, op1Low, operand2);
		context.AppendInstruction(IR.GetHigh32, op1High, operand2);

		// Compare high
		context.AppendInstruction(IR.Branch32, ConditionCode.Equal, null, op0High, op1High, newBlocks[1].Block);
		context.AppendInstruction(IR.Jmp, newBlocks[0].Block);

		// Branch if check already gave results
		if (condition == ConditionCode.Equal)
		{
			newBlocks[0].AppendInstruction(IR.Jmp, newBlocks[3].Block);
		}
		else
		{
			newBlocks[0].AppendInstruction(IR.Branch32, condition, null, op0High, op1High, newBlocks[2].Block);
			newBlocks[0].AppendInstruction(IR.Jmp, newBlocks[3].Block);
		}

		// Compare low
		newBlocks[1].AppendInstruction(IR.Branch32, conditionUnsigned, null, op0Low, op1Low, newBlocks[2].Block);
		newBlocks[1].AppendInstruction(IR.Jmp, newBlocks[3].Block);

		// Success
		newBlocks[2].AppendInstruction(IR.Jmp, newBlocks[4].Block);

		// Failed
		newBlocks[3].AppendInstruction(IR.Jmp, newBlocks[4].Block);

		// Exit
		newBlocks[4].AppendInstruction(IR.Phi64, resultLow, Operand.Constant64_1, Operand.Constant64_0);
		newBlocks[4].PhiBlocks = new List<BasicBlock>(2) { newBlocks[2].Block, newBlocks[3].Block };
		newBlocks[4].AppendInstruction(IR.Jmp, nextBlock.Block);
	}
}
