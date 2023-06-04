// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.IR;

/// <summary>
/// AddOverflowOut32
/// </summary>
/// <seealso cref="Mosa.Compiler.Framework.IR.BaseIRInstruction" />
public sealed class AddOverflowOut32 : BaseIRInstruction
{
	public AddOverflowOut32()
		: base(2, 2)
	{
	}

	public override bool IsCommutative => true;
}