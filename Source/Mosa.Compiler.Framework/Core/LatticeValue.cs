// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System.Diagnostics;
using System.Text;

namespace Mosa.Compiler.Framework.Core;

internal sealed class LatticeValue
{
	private enum LatticeStatusType
	{ Unknown, OverDefined, SingleConstant, MultipleConstants }

	private enum ReferenceStatusType
	{ Unknown, DefinedNotNull, OverDefined }

	private const int MaxConstants = 4;

	private LatticeStatusType Status;

	private ReferenceStatusType ReferenceStatus;

	private int ConstantCount;

	private readonly ulong[] ConstantValues = new ulong[MaxConstants];

	public IEnumerable<ulong> GetConstants()
	{
		if (ConstantCount == 0)
			yield break;

		for (var i = 0; i < ConstantCount && i < ConstantValues.Length; i++)
		{
			yield return ConstantValues[i];
		}
	}

	public ulong ConstantUnsignedLongInteger => ConstantValues[0];

	public long ConstantSignedLongInteger => (long)ConstantValues[0];

	public bool ConstantsContainZero
	{
		get
		{
			if (ConstantCount == 0)
				return false;
			for (var i = 0; i < ConstantCount && i < ConstantValues.Length; i++)
			{
				if (ConstantValues[i] == 0)
					return true;
			}
			return false;
		}
	}

	public Operand Operand { get; }

	public bool IsOverDefined
	{
		get => Status == LatticeStatusType.OverDefined;
		set { Status = LatticeStatusType.OverDefined; Debug.Assert(value); }
	}

	public bool IsUnknown => Status == LatticeStatusType.Unknown;

	public bool IsSingleConstant
	{
		get => Status == LatticeStatusType.SingleConstant;
		set { Status = LatticeStatusType.SingleConstant; Debug.Assert(value); }
	}

	public bool HasMultipleConstants => Status == LatticeStatusType.MultipleConstants;

	public bool HasOnlyConstants => Status is LatticeStatusType.SingleConstant or LatticeStatusType.MultipleConstants;

	public bool IsVirtualRegister { get; set; }

	public bool IsReferenceType { get; set; }

	public bool IsReferenceDefinedUnknown => ReferenceStatus == ReferenceStatusType.Unknown;

	public bool IsReferenceDefinedNotNull
	{
		get => ReferenceStatus == ReferenceStatusType.DefinedNotNull;
		set
		{
			Debug.Assert(value);
			ReferenceStatus = ReferenceStatusType.DefinedNotNull;
		}
	}

	public bool IsReferenceOverDefined
	{
		get => ReferenceStatus == ReferenceStatusType.OverDefined;
		set
		{
			Debug.Assert(value);
			ReferenceStatus = ReferenceStatusType.OverDefined;
		}
	}

	public LatticeValue(Operand operand)
	{
		Operand = operand;

		IsVirtualRegister = operand.IsVirtualRegister;
		IsReferenceType = operand.IsObject;

		if (IsVirtualRegister)
		{
			Status = LatticeStatusType.Unknown;
			IsVirtualRegister = true;
		}
		else if (operand.IsUnresolvedConstant)
		{
			IsOverDefined = true;
		}
		else if (operand.IsConstant && operand.IsInteger)
		{
			AddConstant(operand.ConstantUnsigned64);
		}
		else if (operand.IsNull)
		{
			AddConstant(0);
		}
		else
		{
			IsOverDefined = true;
		}

		if (!IsReferenceType || !IsVirtualRegister)
		{
			ReferenceStatus = ReferenceStatusType.OverDefined;
		}
		else
		{
			ReferenceStatus = ReferenceStatusType.Unknown;
		}
	}

	public bool AddConstant(ulong value)
	{
		if (Status == LatticeStatusType.OverDefined)
			return false;

		for (var i = 0; i < ConstantCount && i < ConstantValues.Length; i++)
		{
			if (ConstantValues[i] == value)
				return false;
		}

		if (ConstantCount == 0)
		{
			ConstantValues[0] = value;
			ConstantCount = 1;
			Status = LatticeStatusType.SingleConstant;
			return true;
		}
		else if (ConstantCount < MaxConstants)
		{
			ConstantValues[ConstantCount] = value;
			ConstantCount++;
			Status = LatticeStatusType.MultipleConstants;
			return true;
		}

		ConstantCount = 0;
		Status = LatticeStatusType.OverDefined;
		return true;
	}

	public void AddConstant(long value)
	{
		AddConstant((ulong)value);
	}

	public bool AreConstantsEqual(LatticeValue other)
	{
		if (!other.IsSingleConstant || !IsSingleConstant)
			return false;

		return other.ConstantUnsignedLongInteger == ConstantUnsignedLongInteger;
	}

	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.Append($"{Operand} : {Status}");

		if (IsSingleConstant)
		{
			sb.Append($" = {ConstantUnsignedLongInteger}");
		}
		else if (HasMultipleConstants)
		{
			sb.Append($" ({ConstantCount}) =");
			for (var i = 0; i < ConstantCount && i < ConstantValues.Length; i++)
			{
				sb.Append($" {ConstantValues[i]},");
			}
			if (sb.Length > 0 && sb[sb.Length - 1] == ',')
				sb.Length--;
		}

		sb.Append(" [null: ");
		if (IsReferenceOverDefined)
			sb.Append("OverDefined");
		else if (IsReferenceDefinedNotNull)
			sb.Append("NotNull");
		else if (IsReferenceDefinedUnknown)
			sb.Append("Unknown");
		sb.Append(']');

		return sb.ToString();
	}
}
