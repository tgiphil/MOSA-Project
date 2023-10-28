// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace Mosa.Compiler.Framework.Transforms.Exception;

/// <summary>
/// ExceptionStart
/// </summary>
public sealed class ExceptionStart : BaseExceptionTransform
{
	public ExceptionStart() : base(IRInstruction.ExceptionStart, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		var exceptionVirtualRegister = context.Result;
		var exceptionRegister = transform.PhysicalRegisters.AllocateObject(transform.Architecture.ExceptionRegister);

		context.SetInstruction(IRInstruction.KillAll);
		context.AppendInstruction(IRInstruction.Gen, exceptionRegister);
		context.AppendInstruction(IRInstruction.MoveObject, exceptionVirtualRegister, exceptionRegister);
	}
}
