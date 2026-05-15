// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Transforms.Expand;

/// <summary>
/// Transformations
/// </summary>
public static class IRTransforms
{
	public static readonly List<BaseTransform> List = new()
	{
		CheckArrayBounds.Instance,

		ThrowIndexOutOfRange.Instance,
		ThrowOverflow.Instance,
		ThrowDivideByZero.Instance,

		CheckThrowIndexOutOfRange.Instance,
		CheckThrowOverflow.Instance,
		CheckThrowDivideByZero.Instance,

		Switch.Instance,
	};
}
