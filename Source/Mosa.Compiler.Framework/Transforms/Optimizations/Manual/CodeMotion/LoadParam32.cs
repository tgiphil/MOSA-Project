// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Manual.CodeMotion;

/// <summary>
/// Param32
/// </summary>
public sealed class LoadParam32 : BaseCodeMotionTransform
{
	public static readonly LoadParam32 Instance = new();

	private LoadParam32() : base(IR.LoadParam32, TransformType.Manual | TransformType.Optimization)
	{
	}
}
