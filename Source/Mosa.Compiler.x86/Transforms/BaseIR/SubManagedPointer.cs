// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x86.Transforms.BaseIR;

/// <summary>
/// Sub32
/// </summary>
[Transform("x86.BaseIR")]
public sealed class SubManagedPointer : BaseIRTransform
{
	public SubManagedPointer() : base(IR.SubManagedPointer, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		context.ReplaceInstruction(X86.Sub32);
	}
}