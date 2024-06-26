// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x64.Transforms.BaseIR;

/// <summary>
/// ShiftLeft64
/// </summary>
public sealed class ShiftLeft64 : BaseIRTransform
{
	public ShiftLeft64() : base(IR.ShiftLeft64, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		context.ReplaceInstruction(X64.Shl64);
	}
}
