/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 */

namespace Mosa.DeviceDrivers
{
	/// <summary>
	/// Interface to a region of memory
	/// </summary>
	public interface IMemory
	{
		/// <summary>
		/// Gets the address.
		/// </summary>
		/// <value>The address.</value>
		uint Address { get; }

		/// <summary>
		/// Gets the size.
		/// </summary>
		/// <value>The size.</value>
		uint Size { get; }

		/// <summary>
		/// Gets or sets the <see cref="System.Byte"/> at the specified index.
		/// </summary>
		/// <value></value>
		byte this[uint index] { get; set; }

		/// <summary>
		/// Read8s the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		byte Read8(uint index);

		/// <summary>
		/// Write8s the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="value">The value.</param>
		void Write8(uint index, byte value);

		// ushort Read16(uint index);
		// void Write16(uint index, ushort value);
		// uint Read32(uint index);
		// void Write32(uint index, uint value);
	}

}
