// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Instructions;

/// <summary>
/// Store64
/// </summary>
/// <seealso cref="Mosa.Compiler.Framework.Instructions.BaseIRInstruction" />
public sealed class Store64 : BaseIRInstruction
{
	public Store64()
		: base(3, 0)
	{
	}

	public override bool IsMemoryWrite => true;
}