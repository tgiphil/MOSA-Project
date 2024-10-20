// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.Instructions;

/// <summary>
/// StoreParam64
/// </summary>
public sealed class StoreParam64 : BaseIRInstruction
{
	public StoreParam64()
		: base(2, 0)
	{
	}

	public override bool IsMemoryWrite => true;

	public override bool IsParameterStore => true;
}
