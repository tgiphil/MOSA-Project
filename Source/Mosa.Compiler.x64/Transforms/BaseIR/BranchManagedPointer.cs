// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x64.Transforms.BaseIR;

/// <summary>
/// BranchManagedPointer
/// </summary>
[Transform("x64.BaseIR")]
public sealed class BranchManagedPointer : BaseIRTransform
{
	public BranchManagedPointer() : base(IR.BranchManagedPointer, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		Framework.Transform.MoveConstantRight(context);

		var target = context.BranchTargets[0];
		var condition = context.ConditionCode;
		var operand1 = context.Operand1;
		var operand2 = context.Operand2;

		context.SetInstruction(X64.Cmp32, null, operand1, operand2);
		context.AppendInstruction(X64.Branch, condition, target);
	}
}