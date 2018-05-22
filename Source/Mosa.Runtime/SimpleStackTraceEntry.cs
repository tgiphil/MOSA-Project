// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Runtime.Metadata;
using System;

namespace Mosa.Runtime
{
	/// <summary>
	/// Holds information about a single stacktrace entry
	/// </summary>
	public struct SimpleStackTraceEntry
	{
		private string methodName;
		public MethodDefinition MethodDefinition;
		public uint Offset;

		unsafe public string MethodName
		{
			get
			{
				if (MethodDefinition.IsNull)
					return null;

				if (methodName == null)
					methodName = MethodDefinition.Name;

				return methodName;
			}
		}

		/// <summary>
		/// Returns a human readable text of this entry
		/// </summary>
		/// <returns></returns>
		unsafe public string ToStringBuffer()
		{
			return "0x" +
				MethodDefinition.Method.ToUInt32().ToString("x") +
				"+0x" +
				Offset.ToString("x") +
				" " +
				methodName.Substring(MethodName.IndexOf(' ') + 1);
		}

		/// <summary>
		/// Skip defines, if this entry should be displayed, or not.
		/// </summary>
		public bool Skip
		{
			get
			{
				if (!Valid)
					return true;
				if (MethodName == null)
					return true;
				return MethodName.IndexOf("System.Void Mosa.Kernel.x86.Panic::") >= 0;
			}
		}

		public bool Valid
		{
			get { return MethodName != null; }
		}
	}
}
