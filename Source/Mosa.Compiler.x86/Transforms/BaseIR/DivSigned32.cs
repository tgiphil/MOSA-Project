// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x86.Transforms.BaseIR;

/// <summary>
/// DivSigned32
/// </summary>
[Transform("x86.BaseIR")]
public sealed class DivSigned32 : BaseIRTransform
{
	public DivSigned32() : base(IR.DivSigned32, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		var operand1 = context.Operand1;
		var operand2 = context.Operand2;
		var result = context.Result;

		var v1 = transform.VirtualRegisters.Allocate32();
		var v2 = transform.VirtualRegisters.Allocate32();
		var v3 = transform.VirtualRegisters.Allocate32();

		context.SetInstruction(X86.Mov32, v2, operand1);
		context.AppendInstruction(X86.Cdq32, v3, v2);
		context.AppendInstruction2(X86.IDiv32, v1, result, v3, v2, operand2);
	}
}