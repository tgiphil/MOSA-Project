// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.ARM32.Transforms.BaseIR;

/// <summary>
/// MulUnsigned32
/// </summary>
public sealed class MulUnsigned32 : BaseIRTransform
{
	public static readonly MulUnsigned32 Instance = new();

	private MulUnsigned32() : base(IR.MulUnsigned32, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		Framework.Core.Transform.MoveConstantRight(context);

		Translate(transform, context, ARM32.Mul, false);
	}
}
