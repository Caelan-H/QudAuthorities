using System;
using System.Collections.Generic;

namespace XRL.World.Conversations;

[Serializable]
public class FixedHashSet
{
	private struct Slot
	{
		public ulong Hash;

		public int Next;
	}

	[NonSerialized]
	private static int[] EmptyBuckets = new int[0];

	[NonSerialized]
	private static Slot[] EmptySlots = new Slot[0];

	private int[] Buckets = EmptyBuckets;

	private Slot[] Slots = EmptySlots;

	private int Last;

	private int Free = -1;

	private int Length;

	public int Count => Length;

	public FixedHashSet()
	{
	}

	public FixedHashSet(ICollection<ulong> Range)
	{
		IncreaseCapacity(Range.Count);
		foreach (ulong item in Range)
		{
			Add(item);
		}
	}

	public bool Contains(ulong Hash)
	{
		if (Length == 0)
		{
			return false;
		}
		ulong num = Hash % (ulong)Buckets.Length;
		for (int num2 = Buckets[num] - 1; num2 >= 0; num2 = Slots[num2].Next)
		{
			if (Slots[num2].Hash == Hash)
			{
				return true;
			}
		}
		return false;
	}

	public bool Add(params string[] Values)
	{
		ulong num = 0uL;
		for (int i = 0; i < Values.Length; i++)
		{
			num = SDBM(Values[i], num);
		}
		if (num != 0)
		{
			return Add(num);
		}
		return false;
	}

	public bool Add(ulong Hash)
	{
		ulong num = 0uL;
		if (Length > 0)
		{
			num = Hash % (ulong)Buckets.Length;
			for (int num2 = Buckets[num] - 1; num2 >= 0; num2 = Slots[num2].Next)
			{
				if (Slots[num2].Hash == Hash)
				{
					return false;
				}
			}
		}
		int num3;
		if (Free >= 0)
		{
			num3 = Free;
			Free = Slots[num3].Next;
		}
		else
		{
			if (Last == Slots.Length)
			{
				IncreaseCapacity();
				num = Hash % (ulong)Buckets.Length;
			}
			num3 = Last;
			Last++;
		}
		Slots[num3].Hash = Hash;
		Slots[num3].Next = Buckets[num] - 1;
		Buckets[num] = num3 + 1;
		Length++;
		return true;
	}

	public static ulong SDBM(string Seed, ulong Hash = 0uL)
	{
		int i = 0;
		for (int length = Seed.Length; i < length; i++)
		{
			Hash = Seed[i] + (Hash << 6) + (Hash << 16) - Hash;
		}
		return Hash;
	}

	public bool Remove(ulong Hash)
	{
		ulong num = Hash % (ulong)Buckets.Length;
		int num2 = -1;
		for (int num3 = Buckets[num] - 1; num3 >= 0; num3 = Slots[num3].Next)
		{
			if (Slots[num3].Hash == Hash)
			{
				if (num2 < 0)
				{
					Buckets[num] = Slots[num3].Next + 1;
				}
				else
				{
					Slots[num2].Next = Slots[num3].Next;
				}
				Slots[num3].Hash = 0uL;
				Slots[num3].Next = Free;
				if (--Length == 0)
				{
					Last = 0;
					Free = -1;
				}
				else
				{
					Free = num3;
				}
				return true;
			}
			num2 = num3;
		}
		return false;
	}

	private void IncreaseCapacity(int To = -1)
	{
		int num = Math.Max((Slots.Length == 0) ? 4 : (Slots.Length * 2), To);
		Slot[] array = new Slot[num];
		if (Slots != null)
		{
			Array.Copy(Slots, 0, array, 0, Last);
		}
		int[] array2 = new int[num];
		for (int i = 0; i < Last; i++)
		{
			ulong num2 = array[i].Hash % (ulong)num;
			array[i].Next = array2[num2] - 1;
			array2[num2] = i + 1;
		}
		Slots = array;
		Buckets = array2;
	}

	public ulong[] GetValues()
	{
		ulong[] array = new ulong[Slots.Length];
		for (int i = 0; i < Slots.Length; i++)
		{
			array[i] = Slots[i].Hash;
		}
		return array;
	}
}
