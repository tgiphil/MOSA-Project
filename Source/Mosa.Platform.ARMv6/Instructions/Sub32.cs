// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;
using Mosa.Compiler.Common;

namespace Mosa.Platform.ARMv6.Instructions
{
	/// <summary>
	/// Sub32 - Subtract
	/// </summary>
	/// <seealso cref="Mosa.Platform.ARMv6.ARMv6Instruction" />
	public sealed class Sub32 : ARMv6Instruction
	{
		public override int ID { get { return 657; } }

		internal Sub32()
			: base(1, 3)
		{
		}

		protected override void Emit(InstructionNode node, ARMv6CodeEmitter emitter)
		{
			EmitDataProcessingInstruction(node, emitter, Bits.b0010);
		}
	}
}
