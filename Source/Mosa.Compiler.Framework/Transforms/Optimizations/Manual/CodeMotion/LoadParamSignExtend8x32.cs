// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Manual.CodeMotion;

/// <summary>
/// LoadParamSignExtend8x32
/// </summary>
public sealed class LoadParamSignExtend8x32 : BaseCodeMotionTransform
{
	public static readonly LoadParamSignExtend8x32 Instance = new();

	private LoadParamSignExtend8x32() : base(IR.LoadParamSignExtend8x32, TransformType.Manual | TransformType.Optimization)
	{
	}
}
