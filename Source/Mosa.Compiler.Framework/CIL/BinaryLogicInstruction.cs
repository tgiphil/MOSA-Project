﻿// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.MosaTypeSystem;
using System;

namespace Mosa.Compiler.Framework.CIL
{
	/// <summary>
	/// Intermediate representation of a IL binary logic instruction.
	/// </summary>
	/// <seealso cref="Mosa.Compiler.Framework.CIL.BinaryInstruction" />
	public sealed class BinaryLogicInstruction : BinaryInstruction
	{
		#region Operand Table

		/// <summary>
		/// Operand table according to ISO/IEC 23271:2006 (E), Partition III, 1.5, Table 5.
		/// </summary>
		private static readonly StackTypeCode[][] opTable = new StackTypeCode[][] {
			new StackTypeCode[] { StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown },
			new StackTypeCode[] { StackTypeCode.Unknown, StackTypeCode.Int32,   StackTypeCode.Unknown, StackTypeCode.N,       StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown },
			new StackTypeCode[] { StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Int64,   StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown },
			new StackTypeCode[] { StackTypeCode.Unknown, StackTypeCode.N,       StackTypeCode.Unknown, StackTypeCode.N,       StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown },
			new StackTypeCode[] { StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown },
			new StackTypeCode[] { StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown },
			new StackTypeCode[] { StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown },
			new StackTypeCode[] { StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown, StackTypeCode.Unknown },
		};

		#endregion Operand Table

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="BinaryLogicInstruction"/> class.
		/// </summary>
		/// <param name="opcode">The opcode.</param>
		public BinaryLogicInstruction(OpCode opcode)
			: base(opcode, 1)
		{
		}

		#endregion Construction

		#region Methods

		/// <summary>
		/// Validates the instruction operands and creates a matching variable for the result.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="compiler">The compiler.</param>
		public override void Resolve(Context context, BaseMethodCompiler compiler)
		{
			base.Resolve(context, compiler);

			var stackTypeForOperand1 = context.Operand1.Type.GetStackTypeCode();
			var stackTypeForOperand2 = context.Operand2.Type.GetStackTypeCode();

			var result = opTable[(int)stackTypeForOperand1][(int)stackTypeForOperand2];

			if (result == StackTypeCode.Unknown)
			{
				throw new InvalidOperationException("Invalid virtualLocal result of instruction: " + result.ToString() + " (" + context.Operand1 + ")");
			}

			context.Result = compiler.CreateVirtualRegister(compiler.TypeSystem.GetStackTypeFromCode(result));
		}

		#endregion Methods
	}
}
