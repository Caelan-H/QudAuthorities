using System.Collections.Generic;
using XRL.Rules;
using XRL.World;

namespace XRL;

public class Tools
{
	public static bool BoxOverlap(Box Box1, Box Box2)
	{
		if (Box1.x1 > Box2.x2)
		{
			return false;
		}
		if (Box1.x2 < Box2.x1)
		{
			return false;
		}
		if (Box1.y1 > Box2.y2)
		{
			return false;
		}
		if (Box1.y2 < Box2.y1)
		{
			return false;
		}
		return true;
	}

	public static List<Box> GenerateBoxes(BoxGenerateOverlap Overlap, Range NumberOfBoxees, Range Width, Range Height, Range Volume)
	{
		return GenerateBoxes(new List<Box>(), Overlap, NumberOfBoxees, Width, Height, Volume, new Range(0, 79), new Range(0, 24));
	}

	public static List<Box> GenerateBoxes(Box OutOfBounds, BoxGenerateOverlap Overlap, Range NumberOfBoxees, Range Width, Range Height, Range Volume)
	{
		return GenerateBoxes(new List<Box> { OutOfBounds }, Overlap, NumberOfBoxees, Width, Height, Volume, new Range(0, 79), new Range(0, 24));
	}

	public static List<Box> GenerateBoxes(List<Box> OutOfBounds, BoxGenerateOverlap Overlap, Range NumberOfBoxees, Range Width, Range Height, Range Volume, Range XRange, Range YRange)
	{
		List<Box> list = new List<Box>();
		while (true)
		{
			int num = Stat.Random(NumberOfBoxees.Min, NumberOfBoxees.Max);
			list.Clear();
			for (int i = 0; i < num; i++)
			{
				Box box;
				while (true)
				{
					IL_0025:
					box = new Box(Stat.Random(XRange.Min, XRange.Max), Stat.Random(YRange.Min, YRange.Max), Stat.Random(XRange.Min, XRange.Max), Stat.Random(YRange.Min, YRange.Max));
					if (box.Width < Width.Min || box.Width > Width.Max || box.Height < Height.Min || box.Height > Height.Max || (Volume != null && (box.Width * box.Height < Volume.Min || box.Width * box.Height > Volume.Max)))
					{
						continue;
					}
					foreach (Box OutOfBound in OutOfBounds)
					{
						if (BoxOverlap(OutOfBound, box))
						{
							goto IL_0025;
						}
					}
					break;
				}
				list.Add(box);
			}
			if (Overlap == BoxGenerateOverlap.Irrelevant)
			{
				break;
			}
			using List<Box>.Enumerator enumerator = list.GetEnumerator();
			while (true)
			{
				if (enumerator.MoveNext())
				{
					Box current = enumerator.Current;
					foreach (Box item in list)
					{
						if (current != item && ((Overlap == BoxGenerateOverlap.AlwaysOverlap && !BoxOverlap(current, item)) || (Overlap == BoxGenerateOverlap.NeverOverlap && BoxOverlap(current, item))))
						{
							goto end_IL_01a5;
						}
					}
					continue;
				}
				return list;
				continue;
				end_IL_01a5:
				break;
			}
		}
		return list;
	}

	public static void Box(Zone Z, Box B, string Blueprint, int Chance)
	{
		for (int i = B.x1; i <= B.x2; i++)
		{
			for (int j = B.y1; j <= B.y2; j++)
			{
				if ((i == B.x1 || i == B.x2 || j == B.y1 || j == B.y2) && Stat.Random(1, 100) <= Chance)
				{
					Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
				}
			}
		}
	}

	public static void FillBox(Zone Z, Box B, string Blueprint)
	{
		for (int i = B.x1; i <= B.x2; i++)
		{
			for (int j = B.y1; j <= B.y2; j++)
			{
				Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			}
		}
	}

	public static void FillBox(Zone Z, Box B, string Blueprint, int Chance)
	{
		for (int i = B.x1; i <= B.x2; i++)
		{
			for (int j = B.y1; j <= B.y2; j++)
			{
				if (Z.GetCell(i, j).IsEmpty() && Stat.Random(1, 100) <= Chance)
				{
					Z.GetCell(i, j).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
				}
			}
		}
	}

	public static void ClearBox(Zone Z, Box B)
	{
		for (int i = B.x1; i <= B.x2; i++)
		{
			for (int j = B.y1; j <= B.y2; j++)
			{
				Z.GetCell(i, j).Clear();
			}
		}
	}
}
