// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x64.Transforms.BaseIR;

/// <summary>
/// Or64
/// </summary>
[Transform("x64.BaseIR")]
public sealed class Or64 : BaseIRTransform
{
	public Or64() : base(IR.Or64, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		context.ReplaceInstruction(X64.Or64);
	}
}