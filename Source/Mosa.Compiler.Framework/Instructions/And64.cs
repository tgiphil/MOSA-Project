// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Instructions;

/// <summary>
/// And64
/// </summary>
/// <seealso cref="Mosa.Compiler.Framework.Instructions.BaseIRInstruction" />
public sealed class And64 : BaseIRInstruction
{
	public And64()
		: base(2, 1)
	{
	}

	public override bool IsCommutative => true;
}