// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x64.Transforms.BaseIR;

/// <summary>
/// MoveObject
/// </summary>
[Transform("x64.BaseIR")]
public sealed class MoveObject : BaseIRTransform
{
	public MoveObject() : base(IR.MoveObject, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		context.ReplaceInstruction(X64.Mov32);
	}
}