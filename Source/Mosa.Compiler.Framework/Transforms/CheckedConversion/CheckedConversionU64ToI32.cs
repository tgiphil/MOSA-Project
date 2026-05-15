// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Transforms.CheckedConversion;

/// <summary>
/// CheckedConversionU64ToI32
/// </summary>
public sealed class CheckedConversionU64ToI32 : BaseCheckedConversionTransform
{
	public static readonly CheckedConversionU64ToI32 Instance = new();

	private CheckedConversionU64ToI32() : base(IR.CheckedConversionU64ToI32, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		CallCheckOverflow(transform, context, "U8ToI4");
	}
}
