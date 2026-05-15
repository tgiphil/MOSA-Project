// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Transforms.CheckedConversion;

/// <summary>
/// CheckedConversionU32ToI8
/// </summary>
public sealed class CheckedConversionU32ToI8 : BaseCheckedConversionTransform
{
	public static readonly CheckedConversionU32ToI8 Instance = new();

	private CheckedConversionU32ToI8() : base(IR.CheckedConversionU32ToI8, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		CallCheckOverflow(transform, context, "U4ToI1");
	}
}
