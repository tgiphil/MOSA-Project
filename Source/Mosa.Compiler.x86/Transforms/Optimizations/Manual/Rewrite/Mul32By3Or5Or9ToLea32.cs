// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.x86.Transforms.Optimizations.Manual.Rewrite;

[Transform]
public sealed class Mul32By3Or5Or9ToLea32 : BaseTransform
{
	public Mul32By3Or5Or9ToLea32() : base(X86.Mul32, TransformType.Manual | TransformType.Optimization, true)
	{
	}

	public override bool Match(Context context, Transform transform)
	{
		if (context.Result2.IsUsed)
			return false;

		if (!context.Operand1.IsVirtualRegister)
			return false;

		if (!context.Operand2.IsResolved)
			return false;

		if (!(context.Operand2.ConstantUnsigned32 == 3 || context.Operand2.ConstantUnsigned32 == 5 || context.Operand2.ConstantUnsigned32 == 9))
			return false;

		if (AreAnyStatusFlagsUsed(context))
			return false;

		return true;
	}

	public override void Transform(Context context, Transform transform)
	{
		Operand constant = null;

		switch (context.Operand2.ConstantUnsigned32)
		{
			case 3: constant = Operand.Constant32_2; break;  // x * 2 + x = 3x
			case 5: constant = Operand.Constant32_4; break;  // x * 4 + x = 5x
			case 9: constant = Operand.Constant32_8; break;  // x * 8 + x = 9x
		}

		context.SetInstruction(X86.Lea32, context.Result, context.Operand1, context.Operand1, constant, Operand.Constant32_0);
	}
}
