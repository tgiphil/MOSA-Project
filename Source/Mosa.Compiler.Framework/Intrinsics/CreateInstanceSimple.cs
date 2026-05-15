// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Intrinsics;

/// <summary>
/// Intrinsic Methods
/// </summary>
internal static partial class IntrinsicMethods
{
	[IntrinsicMethod("Mosa.Runtime.Intrinsic::CreateInstanceSimple")]
	private static void CreateInstanceSimple(Context context, Transform transform)
	{
		var ctor = context.Operand1;
		var thisObject = context.Operand2;
		var result = context.Result;

		context.SetInstruction(IR.CallDynamic, null, ctor, thisObject);
		context.AppendInstruction(IR.MoveObject, result, thisObject);
	}
}
