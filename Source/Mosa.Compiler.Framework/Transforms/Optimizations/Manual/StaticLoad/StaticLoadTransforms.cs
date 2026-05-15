// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Transforms.Optimizations.Manual.StaticLoad;

/// <summary>
/// Transformations
/// </summary>
public static class StaticLoadTransforms
{
	public static readonly List<BaseTransform> List = new()
	{
		Load32.Instance,
		Load64.Instance,
	};
}
