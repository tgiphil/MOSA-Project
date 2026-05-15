// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.x86.Transforms.AddressMode;

/// <summary>
/// Divsd
/// </summary>
public sealed class Divsd : BaseAddressModeTransform
{
	public static readonly Divsd Instance = new();

	private Divsd() : base(X86.Divsd, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		AddressModeConversion(context, X86.Movsd);
	}
}
