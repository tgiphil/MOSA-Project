// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Runtime;

namespace Mosa.Kernel.BareMetal.MultibootSpecification;

public struct MultibootV2
{
	public const uint Magic = 0x36D76289;

	private readonly Pointer Pointer;

	public readonly bool IsAvailable => !Pointer.IsNull;

	public Pointer BootLine => GetEntryValuePointer(1, 4);

	public Pointer BootloaderNamePointer => GetEntryValuePointer(2, 4);

	public uint MemoryLower => GetEntryValue32(4, 4);

	public uint MemoryUpper => GetEntryValue32(4, 8);

	public uint EntrySize => GetEntryValue32(6, 4);

	public uint EntryVersion => GetEntryValue32(6, 8);

	public uint Entries
	{
		get
		{
			var size = GetEntryValue32(6, 4);

			if (size == 0)
				return 0;

			return (size - 16) / EntrySize;
		}
	}

	public MultibootV2MemoryMapEntry FirstEntry => new(GetEntryValuePointer(6, 12));

	public Pointer FrameBuffer => GetEntryValuePointer(8, 4);

	public Pointer FrameBufferWidth => GetEntryValuePointer(8, 16);

	public Pointer FrameBufferHeight => GetEntryValuePointer(8, 20);

	public Pointer FrameBufferPitch => GetEntryValuePointer(8, 12);

	public Pointer FrameBufferBitPerPixel => GetEntryValuePointer(8, 24);

	public Pointer FrameBufferType => GetEntryValuePointer(8, 25);

	public Pointer RSDPv1 => GetEntryValuePointer(14, 4);

	public Pointer RSDPv2 => GetEntryValuePointer(15, 4);

	public MultibootV2(Pointer entry)
	{
		Pointer = entry;
	}

	private Pointer GetStructurePointer(int type)
	{
		for (var at = Pointer + 8; ;)
		{
			var entryType = at.Load32();

			if (entryType == 0)
				return Pointer.Zero;

			if (entryType == type)
				return at;

			var size = at.Load32(4);

			at += (size + 7) & ~7;
		}
	}

	private Pointer GetStructureEntryPointer(int type, int offset)
	{
		var entry = GetStructurePointer(type);

		if (entry.IsNull)
			return Pointer.Zero;

		return entry + offset;
	}

	private uint GetEntryValue32(int type, int offset)
	{
		var entry = GetStructureEntryPointer(type, offset);

		if (entry.IsNull)
			return 0;

		return entry.Load32();
	}

	private Pointer GetEntryValuePointer(int type, int offset)
	{
		var entry = GetStructureEntryPointer(type, offset);

		if (entry.IsNull)
			return Pointer.Zero;

		return entry.LoadPointer();
	}
}
