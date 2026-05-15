// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Transforms.CheckedConversion;

/// <summary>
/// CheckedConversionI32ToI16
/// </summary>
public sealed class CheckedConversionI32ToI16 : BaseCheckedConversionTransform
{
	public static readonly CheckedConversionI32ToI16 Instance = new();

	private CheckedConversionI32ToI16() : base(IR.CheckedConversionI32ToI16, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		CallCheckOverflow(transform, context, "I4ToI2");
	}
}
