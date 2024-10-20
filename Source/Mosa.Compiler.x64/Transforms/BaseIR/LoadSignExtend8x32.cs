// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x64.Transforms.BaseIR;

/// <summary>
/// LoadSignExtend8x32
/// </summary>
public sealed class LoadSignExtend8x32 : BaseIRTransform
{
	public LoadSignExtend8x32() : base(IR.LoadSignExtend8x32, TransformType.Manual | TransformType.Transform)
	{
	}

	public override void Transform(Context context, Transform transform)
	{
		transform.OrderLoadStoreOperands(context);

		context.SetInstruction(X64.MovsxLoad8, context.Result, context.Operand1, context.Operand2);
	}
}
