// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Instructions;

/// <summary>
/// Add64
/// </summary>
/// <seealso cref="Mosa.Compiler.Framework.Instructions.BaseIRInstruction" />
public sealed class Add64 : BaseIRInstruction
{
	public Add64()
		: base(2, 1)
	{
	}

	public override bool IsCommutative => true;
}