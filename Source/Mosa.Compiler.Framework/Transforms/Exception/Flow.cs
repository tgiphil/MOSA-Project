// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Transforms.Exception;

/// <summary>
/// Flow
/// </summary>
public sealed class Flow : BaseExceptionTransform
{
	public static readonly Flow Instance = new();

	private Flow() : base(IR.Flow, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		context.Empty();
	}
}
