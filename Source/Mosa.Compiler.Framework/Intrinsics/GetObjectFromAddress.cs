﻿// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace Mosa.Compiler.Framework.Intrinsics;

/// <summary>
/// Intrinsic Methods
/// </summary>
internal static partial class IntrinsicMethods
{
	[IntrinsicMethod("Mosa.Runtime.Intrinsic::GetObjectFromAddress")]
	private static void GetObjectFromAddress(Context context, Transform transform)
	{
		context.SetInstruction(IR.MoveObject, context.Result, context.Operand1);
	}
}
