// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework.Core;

namespace Mosa.Compiler.Framework.Analysis;

public abstract class BaseBlockOrder
{
	public List<BasicBlock> NewBlockOrder { get; protected set; }

	public abstract void Analyze(BasicBlocks basicBlocks);

	public abstract int GetLoopDepth(BasicBlock block);

	public abstract int GetLoopIndex(BasicBlock block);
}
