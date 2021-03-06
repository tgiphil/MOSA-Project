// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.IR
{
	/// <summary>
	/// LoadParamInt32
	/// </summary>
	/// <seealso cref="Mosa.Compiler.Framework.IR.BaseIRInstruction" />
	public sealed class LoadParamInt32 : BaseIRInstruction
	{
		public override int ID { get { return 82; } }

		public LoadParamInt32()
			: base(1, 1)
		{
		}

		public override bool IsMemoryRead { get { return true; } }

		public override bool IsParameterLoad { get { return true; } }
	}
}
