// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Transforms.CheckedConversion;

/// <summary>
/// CheckedConversionR4ToI16
/// </summary>
public sealed class CheckedConversionR4ToI16 : BaseCheckedConversionTransform
{
	public static readonly CheckedConversionR4ToI16 Instance = new();

	private CheckedConversionR4ToI16() : base(IR.CheckedConversionR4ToI16, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		CallCheckOverflow(transform, context, "R4ToI2");
	}
}
