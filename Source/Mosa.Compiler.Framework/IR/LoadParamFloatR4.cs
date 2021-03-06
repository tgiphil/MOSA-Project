// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.IR
{
	/// <summary>
	/// LoadParamFloatR4
	/// </summary>
	/// <seealso cref="Mosa.Compiler.Framework.IR.BaseIRInstruction" />
	public sealed class LoadParamFloatR4 : BaseIRInstruction
	{
		public override int ID { get { return 80; } }

		public LoadParamFloatR4()
			: base(1, 1)
		{
		}

		public override bool IsMemoryRead { get { return true; } }

		public override bool IsParameterLoad { get { return true; } }
	}
}
