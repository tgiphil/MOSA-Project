// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Runtime;

namespace Mosa.Kernel.BareMetal.MultibootSpecification;

public struct MultibootV2
{
	public const uint Magic = 0x36D76289;

	private readonly Pointer Pointer;

	public bool IsAvailable => !Pointer.IsNull;

	public Pointer BootLine => GetEntryValuePointer(1, 8);

	public Pointer BootloaderNamePointer => GetEntryValuePointer(2, 8);

	public uint MemoryLower => GetEntryValue32(4, 8);

	public uint MemoryUpper => GetEntryValue32(4, 12);

	public uint EntrySize => GetEntryValue32(6, 8);

	public uint EntryVersion => GetEntryValue32(6, 12);

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

	public MultibootV2MemoryMapEntry FirstEntry => new(GetEntryValuePointer(6, 16));

	public Pointer Framebuffer => GetEntryValuePointer(8, 8);

	public Pointer RSDPv1 => GetEntryValuePointer(14, 8);

	public Pointer RSDPv2 => GetEntryValuePointer(15, 8);

	private Pointer GetStructurePointer(int type)
	{
		var at = Pointer + 16;

		uint entryType;

		while ((entryType = at.Load32()) != 0)
		{
			if (entryType == type)
				return at;

			var size = at.Load32(4);

			at += (size + 7) & ~7;
		}

		return Pointer.Zero;
	}

	private Pointer GetStructureEntryPointer(int type, int offset)
	{
		var entry = GetStructurePointer(type);

		if (entry.IsNull)
			return Pointer.Zero;

		return entry + 8;
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

	public MultibootV2(Pointer entry)
	{
		Pointer = entry;
	}

}
