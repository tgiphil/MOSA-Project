// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace Mosa.Compiler.Framework.Transforms.Runtime;

/// <summary>
/// MemoryCopy
/// </summary>
public sealed class MemoryCopy : BaseRuntimeTransform
{
	public MemoryCopy() : base(IR.MemoryCopy, TransformType.Manual | TransformType.Transform)
	{
	}

	public override int Priority => -10;

	public override bool Match(Context context, Transform transform)
	{
		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		SetVMCall(transform, context, "MemoryCopy", context.Result, context.GetOperands());
	}
}
