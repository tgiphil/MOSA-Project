// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Instructions;

/// <summary>
/// Move32
/// </summary>
/// <seealso cref="Mosa.Compiler.Framework.Instructions.BaseIRInstruction" />
public sealed class Move32 : BaseIRInstruction
{
	public Move32()
		: base(1, 1)
	{
	}

	public override bool IsMove => true;
}