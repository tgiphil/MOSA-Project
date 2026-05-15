// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;

namespace Mosa.Compiler.Common
{
	public static class Frame
	{
		public static string MethodName => new StackTrace().GetFrame(1).GetMethod().Name;
	}
}
