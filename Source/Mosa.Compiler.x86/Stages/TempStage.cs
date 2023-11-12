// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.x86.Transforms.Tweak;

namespace Mosa.Compiler.x86.Stages;

/// <summary>
/// Platform Transformation Stage
/// </summary>
/// <seealso cref="Mosa.Compiler.Framework.Stages.BaseTransformStage" />
public sealed class TempStage : Framework.Stages.BaseTransformStage
{
	public override string Name => "x86." + GetType().Name;

	public TempStage()
		: base(0)
	{
		AddTranform(new Lea32ToLea32v2());
	}
}
