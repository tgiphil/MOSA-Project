// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Instructions;

/// <summary>
/// LoadParamSignExtend8x64
/// </summary>
/// <seealso cref="Mosa.Compiler.Framework.Instructions.BaseIRInstruction" />
public sealed class LoadParamSignExtend8x64 : BaseIRInstruction
{
	public LoadParamSignExtend8x64()
		: base(1, 1)
	{
	}

	public override bool IsMemoryRead => true;

	public override bool IsParameterLoad => true;
}