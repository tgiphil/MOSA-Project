// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Transforms.Exception;

/// <summary>
/// TryStart
/// </summary>
public sealed class TryStart : BaseExceptionTransform
{
	public static readonly TryStart Instance = new();

	private TryStart() : base(IR.TryStart, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		context.Empty();
	}
}
