// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Transforms.Runtime;

/// <summary>
/// GetVirtualFunctionPtr
/// </summary>
public sealed class GetVirtualFunctionPtr : BaseRuntimeTransform
{
	public static readonly GetVirtualFunctionPtr Instance = new();

	private GetVirtualFunctionPtr() : base(IR.GetVirtualFunctionPtr, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		SetVMCall(transform, context, "GetVirtualFunctionPtr", context.Result, context.GetOperands());
	}
}
