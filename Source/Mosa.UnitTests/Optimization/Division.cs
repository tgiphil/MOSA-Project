// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace Mosa.UnitTests.Optimization;

public static class Division
{
	[MosaUnitTest(Series = "U4")]
	public static uint DivisionU4By3(uint a)
	{
		return a / 3u;
	}

	[MosaUnitTest(Series = "U4")]
	public static uint DivisionU4By7(uint a)
	{
		return a / 7u;
	}

	[MosaUnitTest(Series = "U4")]
	public static uint DivisionU4By11(uint a)
	{
		return a / 11u;
	}

	[MosaUnitTest(Series = "U4")]
	public static uint DivisionU4By13(uint a)
	{
		return a / 13u;
	}

	[MosaUnitTest(Series = "I4")]
	public static int DivisionI4By3(int a)
	{
		return a / 3;
	}

	[MosaUnitTest(Series = "I4")]
	public static int DivisionBy7(int a)
	{
		return a / 7;
	}

	[MosaUnitTest(Series = "I4")]
	public static int DivisionI4By11(int a)
	{
		return a / 11;
	}

	[MosaUnitTest(Series = "I4")]
	public static int DivisionI4By13(int a)
	{
		return a / 13;
	}

	[MosaUnitTest(Series = "I4")]
	public static int DivisionI4ByN3(int a)
	{
		return a / -3;
	}

	[MosaUnitTest(Series = "I4")]
	public static int DivisionI4ByN7(int a)
	{
		return a / -7;
	}

	[MosaUnitTest(Series = "I4")]
	public static int DivisionI4ByN11(int a)
	{
		return a / -11;
	}

	[MosaUnitTest(Series = "I4")]
	public static int DivisionI4ByN13(int a)
	{
		return a / -13;
	}

	[MosaUnitTest(Series = "I4")]
	public static int DivisionI4By2(int a)
	{
		return a / 2;
	}

	[MosaUnitTest(Series = "I4")]
	public static int DivisionI4By4(int a)
	{
		return a / 4;
	}

	[MosaUnitTest(Series = "I4")]
	public static int DivisionI4By8(int a)
	{
		return a / 8;
	}
}
