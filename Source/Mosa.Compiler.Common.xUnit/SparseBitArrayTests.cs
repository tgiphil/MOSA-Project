// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Xunit;

namespace Mosa.Compiler.Common.xUnit;

public class SparseBitArrayTests
{
	[Fact]
	public void AllZeroAfterCreation()
	{
		var bitarray = new SparseBitArray();

		Assert.False(bitarray.Get(0));
		Assert.False(bitarray.Get(1));
		Assert.False(bitarray.Get(2));
		Assert.False(bitarray.Get(100));
		Assert.False(bitarray.Get(1000));
	}

	[Fact]
	public void SimpleSets()
	{
		var bitarray = new SparseBitArray();
		bitarray.Set(0, true);
		bitarray.Set(1, true);
		bitarray.Set(2, true);
		bitarray.Set(3, true);

		Assert.True(bitarray.Get(0));
		Assert.True(bitarray.Get(1));
		Assert.True(bitarray.Get(2));
		Assert.True(bitarray.Get(3));
		Assert.False(bitarray.Get(4));
	}

	[Fact]
	public void SetLower32Bits()
	{
		var bitarray = new SparseBitArray();

		// Test bits 0-31 (lower 32 bits of first ulong)
		for (int i = 0; i < 32; i++)
		{
			bitarray.Set(i, true);
		}

		for (int i = 0; i < 32; i++)
		{
			Assert.True(bitarray.Get(i));
		}

		Assert.False(bitarray.Get(32));
	}

	[Fact]
	public void SetUpper32Bits()
	{
		var bitarray = new SparseBitArray();

		// Test bits 32-63 (upper 32 bits of first ulong)
		// This test would fail with the bug where 1u was used instead of 1ul
		for (int i = 32; i < 64; i++)
		{
			bitarray.Set(i, true);
		}

		for (int i = 32; i < 64; i++)
		{
			Assert.True(bitarray.Get(i));
		}

		Assert.False(bitarray.Get(0));
		Assert.False(bitarray.Get(31));
		Assert.False(bitarray.Get(64));
	}

	[Fact]
	public void SetAll64Bits()
	{
		var bitarray = new SparseBitArray();

		// Test all 64 bits in the first ulong element
		for (int i = 0; i < 64; i++)
		{
			bitarray.Set(i, true);
		}

		for (int i = 0; i < 64; i++)
		{
			Assert.True(bitarray.Get(i));
		}
	}

	[Fact]
	public void SetAndClearUpper32Bits()
	{
		var bitarray = new SparseBitArray();

		// Set bits 32-63
		for (int i = 32; i < 64; i++)
		{
			bitarray.Set(i, true);
		}

		// Verify they are set
		for (int i = 32; i < 64; i++)
		{
			Assert.True(bitarray.Get(i));
		}

		// Clear them
		for (int i = 32; i < 64; i++)
		{
			bitarray.Set(i, false);
		}

		// Verify they are cleared
		for (int i = 32; i < 64; i++)
		{
			Assert.False(bitarray.Get(i));
		}
	}

	[Fact]
	public void SetSpecificUpperBits()
	{
		var bitarray = new SparseBitArray();

		// Test specific problematic bit positions
		bitarray.Set(32, true);  // First bit in upper 32
		bitarray.Set(63, true);  // Last bit in upper 32
		bitarray.Set(48, true);  // Middle bit in upper 32

		Assert.True(bitarray.Get(32));
		Assert.True(bitarray.Get(48));
		Assert.True(bitarray.Get(63));

		Assert.False(bitarray.Get(31));
		Assert.False(bitarray.Get(47));
		Assert.False(bitarray.Get(64));
	}

	[Fact]
	public void SetAcrossMultipleUlongs()
	{
		var bitarray = new SparseBitArray();

		// Set bits across multiple ulong boundaries
		bitarray.Set(0, true);
		bitarray.Set(31, true);
		bitarray.Set(32, true);
		bitarray.Set(63, true);
		bitarray.Set(64, true);
		bitarray.Set(127, true);

		Assert.True(bitarray.Get(0));
		Assert.True(bitarray.Get(31));
		Assert.True(bitarray.Get(32));
		Assert.True(bitarray.Get(63));
		Assert.True(bitarray.Get(64));
		Assert.True(bitarray.Get(127));
	}

	[Fact]
	public void SetAllWithPreallocatedSize()
	{
		var bitarray = new SparseBitArray(128);
		bitarray.SetAll(true);

		// Verify bits are set across multiple ulongs
		for (int i = 0; i < 128; i++)
		{
			Assert.True(bitarray.Get(i));
		}

		bitarray.SetAll(false);

		for (int i = 0; i < 128; i++)
		{
			Assert.False(bitarray.Get(i));
		}
	}
}
