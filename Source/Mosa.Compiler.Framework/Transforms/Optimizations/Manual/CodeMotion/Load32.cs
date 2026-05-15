// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Manual.CodeMotion;

/// <summary>
/// Load32
/// </summary>
public sealed class Load32 : BaseCodeMotionTransform
{
	public static readonly Load32 Instance = new();

	private Load32() : base(IR.Load32, TransformType.Manual | TransformType.Optimization)
	{
	}
}
