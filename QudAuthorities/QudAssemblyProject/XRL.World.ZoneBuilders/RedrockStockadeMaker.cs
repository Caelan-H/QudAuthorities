using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class RedrockStockadeMaker : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z, bool ClearCombatObjectsFirst, string WallObject, string ZoneTable, string Widgets, string CustomBigRoomPopulation = null, string CustomBigAlternateRoomPopulation = null, string CustomSmallRoomPopulation = null, string CustomSmallAlternateRoomPopulation = null, string BoxWidth = "30-50", string BoxHeight = "16-24", bool SpecialRedrockBuilder = false, string InsideStocakdeTable = null)
	{
		Box box = null;
		Box box2 = null;
		int num = 30;
		int num2 = Convert.ToInt32(Stat.Roll(BoxWidth));
		int num3 = Convert.ToInt32(Stat.Roll(BoxHeight));
		while (true)
		{
			if (num > 0)
			{
				List<Box> list = Tools.GenerateBoxes(BoxGenerateOverlap.Irrelevant, new Range(1, 1), new Range(num2), new Range(num3), null);
				if (list == null)
				{
					return false;
				}
				box = list[0].Grow(-1);
				box2 = box;
				if (list[0].rect.allPoints().Any((Location2D p) => Z.GetCell(p).HasObject("StairsUp")))
				{
					num--;
					continue;
				}
				for (int i = list[0].x1; i <= list[0].x2; i++)
				{
					for (int j = list[0].y1; j <= list[0].y2; j++)
					{
						if (Z.IsReachable(i, j))
						{
							goto end_IL_00fd;
						}
					}
				}
				num--;
				continue;
			}
			if (num >= 0)
			{
				break;
			}
			return true;
			continue;
			end_IL_00fd:
			break;
		}
		Z.ClearBox(box, "CanyonMarker");
		if (ClearCombatObjectsFirst)
		{
			for (int k = box.x1; k <= box.x2; k++)
			{
				for (int m = box.y1; m <= box.y2; m++)
				{
					while (true)
					{
						using (List<GameObject>.Enumerator enumerator = Z.GetCell(k, m).GetObjectsWithPart("Combat").GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								GameObject current = enumerator.Current;
								Z.GetCell(k, m).RemoveObject(current);
								continue;
							}
						}
						break;
					}
				}
			}
		}
		Z.ClearBox(box, "CanyonMarker");
		for (int n = box.x1; n <= box.x2; n++)
		{
			for (int num4 = box.y1; num4 <= box.y2; num4++)
			{
				Dirty.PaintCell(Z.GetCell(n, num4));
			}
		}
		Z.FillRoundHollowBox(box, WallObject);
		Z.FillRoundHollowBox(box.Grow(-1), WallObject);
		List<Location2D> list2 = box.Grow(-2).rect.locations.ToList();
		List<Box> list3 = new List<Box>();
		if (box2.Valid)
		{
			foreach (Box B in Tools.GenerateBoxes(new List<Box>(), BoxGenerateOverlap.NeverOverlap, new Range(1, 8), new Range(9, 40), new Range(8, 14), new Range(6, 999), new Range(box2.x1, box2.x2), new Range(box2.y1, box2.y2)))
			{
				if (!B.Valid || !B.Grow(-1).Valid)
				{
					continue;
				}
				if (B.x1 == box.x1 || B.x2 == box.x2 || B.y1 == box.y1 || B.y2 == box.y2)
				{
					Z.FillRoundHollowBox(B, WallObject);
					list3.Add(B);
					list2.RemoveAll((Location2D l) => B.rect.locations.Contains(l));
				}
				else
				{
					Z.FillRoundHollowBox(B.Grow(-1), WallObject);
					list3.Add(B.Grow(-1));
					list2.RemoveAll((Location2D l) => B.Grow(-1).rect.locations.Contains(l));
				}
			}
		}
		bool flag = false;
		if ((double)num2 >= BoxWidth.GetCachedDieRoll().Average() && (double)num3 >= BoxHeight.GetCachedDieRoll().Average())
		{
			flag = true;
		}
		int num5 = 0;
		foreach (Box item in list3)
		{
			if (CustomBigRoomPopulation != null)
			{
				bool flag2 = 5.in100();
				string table = ((!flag2) ? CustomSmallRoomPopulation : CustomSmallAlternateRoomPopulation);
				if (item.Volume > 25 && item.Height > 6 && item.Width > 4)
				{
					if (flag)
					{
						if (flag2)
						{
							ZoneBuilderSandbox.PlacePopulationInRect(Z, item.rect, "LegendaryRedrockSnapjawRemains");
						}
						else
						{
							ZoneBuilderSandbox.PlacePopulationInRect(Z, item.rect, "LegendaryRedrockSnapjaw");
						}
						flag = false;
					}
					table = ((!flag2) ? CustomBigRoomPopulation : CustomBigAlternateRoomPopulation);
					ZoneBuilderSandbox.PlacePopulationInRect(Z, item.rect, table);
				}
				else
				{
					ZoneBuilderSandbox.PlacePopulationInRect(Z, item.rect, table);
				}
			}
			else if (item.Volume > 25 && item.Height > 6 && item.Width > 4)
			{
				BuildingTemplate buildingTemplate = new BuildingTemplate(item.Width - 2, item.Height - 2, 1, FullSquare: true);
				for (int num6 = 1; num6 < buildingTemplate.Width - 1; num6++)
				{
					for (int num7 = 1; num7 < buildingTemplate.Height - 1; num7++)
					{
						if (buildingTemplate.Map[num6, num7] == BuildingTemplateTile.Wall)
						{
							Z.GetCell(num6 + item.x1, num7 + item.y1).AddObject(WallObject);
						}
						if (buildingTemplate.Map[num6, num7] == BuildingTemplateTile.Door)
						{
							Z.GetCell(num6 + item.x1, num7 + item.y1).ClearObjectsWithIntProperty("Wall");
						}
					}
				}
			}
			num = 0;
			while (num < 1000)
			{
				num++;
				int num8 = 0;
				int num9 = 0;
				if (Stat.Random(0, 1) == 0)
				{
					num8 = Stat.Random(item.x1 + 2, item.x2 - 2);
					num9 = ((Stat.Random(0, 1) != 0) ? item.y2 : item.y1);
				}
				else
				{
					num9 = Stat.Random(item.y1 + 2, item.y2 - 2);
					num8 = ((Stat.Random(0, 1) != 0) ? item.x2 : item.x1);
				}
				if (num8 != box.x1 && num8 != box.x2 && num9 != box.y1 && num9 != box.y2 && ((!Z.GetCell(num8 - 1, num9).HasObjectWithIntProperty("Wall") && !Z.GetCell(num8 + 1, num9).HasObjectWithIntProperty("Wall")) || (!Z.GetCell(num8, num9 - 1).HasObjectWithIntProperty("Wall") && !Z.GetCell(num8, num9 + 1).HasObjectWithIntProperty("Wall"))))
				{
					Z.GetCell(num8, num9).ClearObjectsWithIntProperty("Wall");
					break;
				}
			}
		}
		if (num5 == 0)
		{
			for (num = 1000; num > 0; num--)
			{
				Point randomPoint = box.Grow(-1).RandomPoint;
				if (Z.GetCell(randomPoint.X, randomPoint.Y).IsEmpty())
				{
					break;
				}
			}
		}
		if (ZoneTable != null)
		{
			string[] array = ZoneTable.Split(',');
			for (int num10 = 0; num10 < array.Length; num10++)
			{
				foreach (PopulationResult item2 in PopulationManager.Generate(array[num10], "zonetier", Z.NewTier.ToString()))
				{
					ZoneBuilderSandbox.PlaceObjectInArea(Z, Z.area, GameObjectFactory.Factory.CreateObject(item2.Blueprint), 0, 0, item2.Hint);
				}
			}
		}
		if (Widgets != null)
		{
			string[] array = Widgets.Split(',');
			foreach (string blueprint in array)
			{
				Z.GetCell(0, 0).AddObject(blueprint);
			}
		}
		int num11 = 0;
		for (int num12 = 0; num12 < 2; num12++)
		{
			int num13 = Stat.Random(1, 15);
			if (((uint)num13 & (true ? 1u : 0u)) != 0)
			{
				num11 = Stat.Random(box.x1 + 3, box.x2 - 3);
				if (!Z.GetCell(num11, box.y1 + 2).HasObjectWithIntProperty("Wall") && Z.GetCell(num11, box.y1).HasObjectWithIntProperty("Wall"))
				{
					Z.GetCell(num11, box.y1).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(num11, box.y1).AddObject("BrinestalkArrowslit");
					Z.GetCell(num11, box.y1 + 1).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(num11, box.y1 + 1).AddObject("BrinestalkArrowslit");
				}
			}
			if (((uint)num13 & 2u) != 0)
			{
				num11 = Stat.Random(box.x1 + 3, box.x2 - 3);
				if (!Z.GetCell(num11, box.y2 - 2).HasObjectWithIntProperty("Wall") && Z.GetCell(num11, box.y2).HasObjectWithIntProperty("Wall"))
				{
					Z.GetCell(num11, box.y2).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(num11, box.y2 - 1).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(num11, box.y2).AddObject("BrinestalkArrowslit");
					Z.GetCell(num11, box.y2 - 1).AddObject("BrinestalkArrowslit");
				}
			}
			if (((uint)num13 & 4u) != 0)
			{
				int y = Stat.Random(box.y1 + 3, box.y2 - 3);
				if (!Z.GetCell(box.x1 + 2, y).HasObjectWithIntProperty("Wall") && Z.GetCell(box.x1, y).HasObjectWithIntProperty("Wall"))
				{
					Z.GetCell(box.x1, y).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(box.x1 + 1, y).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(box.x1, y).AddObject("BrinestalkArrowslit");
					Z.GetCell(box.x1 + 1, y).AddObject("BrinestalkArrowslit");
				}
			}
			if (((uint)num13 & 8u) != 0)
			{
				int y2 = Stat.Random(box.y1 + 3, box.y2 - 3);
				if (!Z.GetCell(box.x2 - 2, y2).HasObjectWithIntProperty("Wall") && Z.GetCell(box.x2, y2).HasObjectWithIntProperty("Wall"))
				{
					Z.GetCell(box.x2, y2).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(box.x2 - 1, y2).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(box.x2, y2).AddObject("BrinestalkArrowslit");
					Z.GetCell(box.x2 - 1, y2).AddObject("BrinestalkArrowslit");
				}
			}
		}
		int num14 = 0;
		bool flag3;
		do
		{
			num14++;
			flag3 = false;
			int num15 = Stat.Random(1, 15);
			int num16 = -1;
			int num17 = -1;
			int num18 = -1;
			int num19 = -1;
			for (int num20 = box.x1 + 3; num20 <= box.x2 - 3; num20++)
			{
				if (Z.GetCell(num20, box.y1).HasObjectWithBlueprint("CanyonMarker"))
				{
					num16 = num20;
				}
				if (Z.GetCell(num20, box.y2).HasObjectWithBlueprint("CanyonMarker"))
				{
					num17 = num20;
				}
			}
			for (int num21 = box.y1 + 3; num21 <= box.y2 - 3; num21++)
			{
				if (Z.GetCell(box.x1, num21).HasObjectWithBlueprint("CanyonMarker"))
				{
					num19 = num21;
				}
				if (Z.GetCell(box.x2, num21).HasObjectWithBlueprint("CanyonMarker"))
				{
					num18 = num21;
				}
			}
			if (((uint)num15 & (true ? 1u : 0u)) != 0 || num16 != -1)
			{
				num11 = Stat.Random(box.x1 + 3, box.x2 - 3);
				if (num16 != -1 && num14 < 10)
				{
					num11 = num16;
				}
				if (!Z.GetCell(num11, box.y1 + 2).HasObjectWithIntProperty("Wall"))
				{
					Z.GetCell(num11, box.y1).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(num11, box.y1 + 1).ClearObjectsWithIntProperty("Wall");
					flag3 = true;
				}
				if (!Z.GetCell(num11 + 1, box.y1 + 2).HasObjectWithIntProperty("Wall"))
				{
					Z.GetCell(num11 + 1, box.y1).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(num11 + 1, box.y1 + 1).ClearObjectsWithIntProperty("Wall");
					flag3 = true;
				}
			}
			if (((uint)num15 & 2u) != 0 || num17 != -1)
			{
				num11 = Stat.Random(box.x1 + 3, box.x2 - 3);
				if (num17 != -1 && num14 < 10)
				{
					num11 = num17;
				}
				if (!Z.GetCell(num11, box.y2 - 2).HasObjectWithIntProperty("Wall"))
				{
					Z.GetCell(num11, box.y2).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(num11, box.y2 - 1).ClearObjectsWithIntProperty("Wall");
					flag3 = true;
				}
				if (!Z.GetCell(num11 + 1, box.y2 - 2).HasObjectWithIntProperty("Wall"))
				{
					Z.GetCell(num11 + 1, box.y2).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(num11 + 1, box.y2 - 1).ClearObjectsWithIntProperty("Wall");
					flag3 = true;
				}
			}
			if (((uint)num15 & 4u) != 0 || num18 != -1)
			{
				int num22 = Stat.Random(box.y1 + 3, box.y2 - 3);
				if (num18 != -1 && num14 < 10)
				{
					num22 = num18;
				}
				if (!Z.GetCell(box.x1 + 2, num22).HasObjectWithIntProperty("Wall"))
				{
					Z.GetCell(box.x1, num22).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(box.x1 + 1, num22).ClearObjectsWithIntProperty("Wall");
					flag3 = true;
				}
				if (!Z.GetCell(box.x1 + 2, num22 + 1).HasObjectWithIntProperty("Wall"))
				{
					Z.GetCell(box.x1, num22 + 1).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(box.x1 + 1, num22 + 1).ClearObjectsWithIntProperty("Wall");
					flag3 = true;
				}
			}
			if (((uint)num15 & 8u) != 0 || num19 != -1)
			{
				int num23 = Stat.Random(box.y1 + 3, box.y2 - 3);
				if (num19 != -1 && num14 < 10)
				{
					num23 = num19;
				}
				if (!Z.GetCell(box.x2 - 2, num23).HasObjectWithIntProperty("Wall"))
				{
					Z.GetCell(box.x2, num23).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(box.x2 - 1, num23).ClearObjectsWithIntProperty("Wall");
					flag3 = true;
				}
				if (!Z.GetCell(box.x2 - 2, num23 + 1).HasObjectWithIntProperty("Wall"))
				{
					Z.GetCell(box.x2, num23 + 1).ClearObjectsWithIntProperty("Wall");
					Z.GetCell(box.x2 - 1, num23 + 1).ClearObjectsWithIntProperty("Wall");
					flag3 = true;
				}
			}
		}
		while (!flag3);
		if (InsideStocakdeTable != null)
		{
			ZoneBuilderSandbox.PlacePopulationInRegion(Z, list2, InsideStocakdeTable);
		}
		return true;
	}
}
