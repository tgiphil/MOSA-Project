// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Platform.x64.Instructions
{
	/// <summary>
	/// Comisd
	/// </summary>
	/// <seealso cref="Mosa.Platform.x64.X64Instruction" />
	public sealed class Comisd : X64Instruction
	{
		public override int ID { get { return 344; } }

		internal Comisd()
			: base(0, 2)
		{
		}

		public override bool IsZeroFlagModified { get { return true; } }

		public override bool IsCarryFlagModified { get { return true; } }

		public override bool IsSignFlagCleared { get { return true; } }

		public override bool IsSignFlagModified { get { return true; } }

		public override bool IsOverflowFlagCleared { get { return true; } }

		public override bool IsOverflowFlagModified { get { return true; } }

		public override bool IsParityFlagModified { get { return true; } }

		public override void Emit(InstructionNode node, BaseCodeEmitter emitter)
		{
			System.Diagnostics.Debug.Assert(node.ResultCount == 0);
			System.Diagnostics.Debug.Assert(node.OperandCount == 2);

			emitter.OpcodeEncoder.SuppressByte(0x40);
			emitter.OpcodeEncoder.Append4Bits(0b0100);
			emitter.OpcodeEncoder.Append1Bit(0b0);
			emitter.OpcodeEncoder.Append1Bit((node.Operand1.Register.RegisterCode >> 3) & 0x1);
			emitter.OpcodeEncoder.Append1Bit(0b0);
			emitter.OpcodeEncoder.Append1Bit((node.Operand2.Register.RegisterCode >> 3) & 0x1);
			emitter.OpcodeEncoder.Append4Bits(0b0110);
			emitter.OpcodeEncoder.Append4Bits(0b0110);
			emitter.OpcodeEncoder.Append4Bits(0b0000);
			emitter.OpcodeEncoder.Append4Bits(0b1111);
			emitter.OpcodeEncoder.Append4Bits(0b0010);
			emitter.OpcodeEncoder.Append4Bits(0b1111);
			emitter.OpcodeEncoder.Append2Bits(0b11);
			emitter.OpcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
			emitter.OpcodeEncoder.Append3Bits(node.Operand2.Register.RegisterCode);
		}
	}
}
