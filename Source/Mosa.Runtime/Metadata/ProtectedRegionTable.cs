// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System;

namespace Mosa.Runtime
{
	/// <summary>
	/// ProtectedRegionTable
	/// </summary>
	public struct ProtectedRegionTable
	{
		#region layout

		// uint			_numberOfRegions;
		// UIntPtr[]	_protectedRegionDefinitions

		#endregion layout

		private UIntPtr Ptr;

		public ProtectedRegionTable(UIntPtr ptr)
		{
			Ptr = ptr;
		}

		public bool IsNull => Ptr == UIntPtr.Zero;

		public uint NumberOfRegions => Intrinsic.Load32(Ptr);

		public ProtectedRegionDefinition GetProtectedRegionDefinition(uint slot)
		{
			return new ProtectedRegionDefinition(Intrinsic.LoadPointer(Ptr, 4 + (UIntPtr.Size * (int)slot)));
		}
	}
}
