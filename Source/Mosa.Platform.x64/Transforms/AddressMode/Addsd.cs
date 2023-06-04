// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;

namespace Mosa.Platform.x64.Transforms.AddressMode;

/// <summary>
/// Addsd
/// </summary>
[Transform("x64.AddressMode")]
public sealed class Addsd : BaseAddressModeTransform
{
	public Addsd() : base(X64.Addsd, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, TransformContext transform)
	{
		AddressModeConversionCummulative(context, X64.Movsd);
	}
}