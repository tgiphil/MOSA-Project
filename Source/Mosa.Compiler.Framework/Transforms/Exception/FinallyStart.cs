// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Stages;

namespace Mosa.Compiler.Framework.Transforms.Exception;

/// <summary>
/// FinallyStart
/// </summary>
public sealed class FinallyStart : BaseExceptionTransform
{
	public FinallyStart() : base(IRInstruction.FinallyStart, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		var exceptionManager = transform.GetManager<ExceptionManager>();

		// Remove from header blocks
		transform.BasicBlocks.RemoveHeaderBlock(context.Block);

		var exceptionVirtualRegister = context.Result;
		var leaveTargetVirtualRegister = context.Result2;
		var exceptionRegister = transform.PhysicalRegisters.AllocateObject(transform.Architecture.ExceptionRegister);

		context.SetInstruction(IRInstruction.KillAll);
		context.AppendInstruction(IRInstruction.Gen, exceptionRegister);
		context.AppendInstruction(IRInstruction.Gen, transform.LeaveTargetRegister);

		context.AppendInstruction(IRInstruction.MoveObject, exceptionVirtualRegister, exceptionRegister);
		context.AppendInstruction(IRInstruction.MoveObject, leaveTargetVirtualRegister, transform.LeaveTargetRegister);

		exceptionManager.ExceptionVirtualRegisters.Add(context.Block, exceptionVirtualRegister);
		exceptionManager.LeaveTargetVirtualRegisters.Add(context.Block, leaveTargetVirtualRegister);
	}
}
