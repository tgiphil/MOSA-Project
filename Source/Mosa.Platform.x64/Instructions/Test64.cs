// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Platform.x64.Instructions
{
	/// <summary>
	/// Test64
	/// </summary>
	/// <seealso cref="Mosa.Platform.x64.X64Instruction" />
	public sealed class Test64 : X64Instruction
	{
		internal Test64()
			: base(0, 2)
		{
		}

		public override bool IsZeroFlagModified { get { return true; } }

		public override bool IsCarryFlagCleared { get { return true; } }

		public override bool IsCarryFlagModified { get { return true; } }

		public override bool IsSignFlagModified { get { return true; } }

		public override bool IsOverflowFlagCleared { get { return true; } }

		public override bool IsOverflowFlagModified { get { return true; } }

		public override bool IsParityFlagModified { get { return true; } }

		public override void Emit(InstructionNode node, OpcodeEncoder opcodeEncoder)
		{
			System.Diagnostics.Debug.Assert(node.ResultCount == 0);
			System.Diagnostics.Debug.Assert(node.OperandCount == 2);

			if (node.Operand2.IsCPURegister)
			{
				opcodeEncoder.SuppressByte(0x40);
				opcodeEncoder.Append4Bits(0b0100);
				opcodeEncoder.Append1Bit(0b1);
				opcodeEncoder.Append1Bit((node.Operand1.Register.RegisterCode >> 3));
				opcodeEncoder.Append1Bit(0b0);
				opcodeEncoder.Append1Bit((node.Operand2.Register.RegisterCode >> 3));
				opcodeEncoder.Append8Bits(0x85);
				opcodeEncoder.Append2Bits(0b11);
				opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
				opcodeEncoder.Append3Bits(node.Operand2.Register.RegisterCode);
				return;
			}

			if (node.Operand2.IsConstant)
			{
				opcodeEncoder.SuppressByte(0x40);
				opcodeEncoder.Append4Bits(0b0100);
				opcodeEncoder.Append1Bit(0b1);
				opcodeEncoder.Append1Bit((node.Operand1.Register.RegisterCode >> 3));
				opcodeEncoder.Append1Bit(0b0);
				opcodeEncoder.Append1Bit(0b0);
				opcodeEncoder.Append8Bits(0xF7);
				opcodeEncoder.Append2Bits(0b11);
				opcodeEncoder.Append3Bits(0b000);
				opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
				opcodeEncoder.Append32BitImmediate(node.Operand2);
				return;
			}

			throw new Compiler.Common.Exceptions.CompilerException("Invalid Opcode");
		}
	}
}
