using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class GolgothaChutes
{
	public static Dictionary<string, string[]> Tiles;

	public static void GenTiles()
	{
		if (Tiles == null)
		{
			Tiles = new Dictionary<string, string[]>();
			Tiles.Add("", new string[4] { "....", "....", "....", "...." });
			Tiles.Add("NSTART", new string[3] { "NNN.", "NNN.", "CCC." });
			Tiles.Add("NEND", new string[3] { "SSS.", ">>>.", "...." });
			Tiles.Add("SSTART", new string[3] { "CCC.", "SSS.", "SSS." });
			Tiles.Add("SEND", new string[3] { "....", ">>>.", "NNN." });
			Tiles.Add("ESTART", new string[3] { "CEEE", "CEEE", "CEEE" });
			Tiles.Add("EEND", new string[3] { ".>WW", ".>WW", ".>WW" });
			Tiles.Add("WSTART", new string[3] { "WWWC", "WWWC", "WWWC" });
			Tiles.Add("WEND", new string[3] { "EE>.", "EE>.", "EE>." });
			Tiles.Add("NS", new string[3] { "SSS.", "SSS.", "SSS." });
			Tiles.Add("SN", new string[3] { "NNN.", "NNN.", "NNN." });
			Tiles.Add("EW", new string[3] { "WWWW", "WWWW", "WWWW" });
			Tiles.Add("WE", new string[3] { "EEEE", "EEEE", "EEEE" });
			Tiles.Add("NW", new string[3] { "WSS.", "WWS.", "WWW." });
			Tiles.Add("WN", new string[3] { "NNN.", "ENN.", "EEN." });
			Tiles.Add("SW", new string[3] { "WWW.", "WWN.", "WNN." });
			Tiles.Add("WS", new string[3] { "EES.", "ESS.", "SSS." });
			Tiles.Add("NE", new string[3] { "SSEE", "SEEE", "EEEE" });
			Tiles.Add("EN", new string[3] { "NNNW", "NNWW", "NWWW" });
			Tiles.Add("SE", new string[3] { "EEEE", "NEEE", "NNEE" });
			Tiles.Add("ES", new string[3] { "SWWW", "SSWW", "SSSW" });
		}
	}

	public static GolgothaTemplate GenerateGolgothaTemplate()
	{
		GolgothaTemplate obj = new GolgothaTemplate
		{
			MainBuilding = new Box(29, 1, 47, 6)
		};
		obj.Chutes = Tools.GenerateBoxes(obj.MainBuilding.Grow(1), BoxGenerateOverlap.NeverOverlap, new Range(4, 4), new Range(10, 76), new Range(8, 20), new Range(200, 2000));
		return obj;
	}

	public bool BuildZone(Zone Z)
	{
		GenTiles();
		ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
		GolgothaTemplate golgothaTemplate = null;
		if (zoneManager.GetZoneColumnProperty(Z.ZoneID, "Builder.Golgotha.GolgothaTemplate") != null)
		{
			golgothaTemplate = (GolgothaTemplate)zoneManager.GetZoneColumnProperty(Z.ZoneID, "Builder.Golgotha.GolgothaTemplate");
		}
		else
		{
			golgothaTemplate = GenerateGolgothaTemplate();
			zoneManager.SetZoneColumnProperty(Z.ZoneID, "Builder.Golgotha.GolgothaTemplate", golgothaTemplate);
		}
		if (Z.Z == 15)
		{
			List<NoiseMapNode> list = new List<NoiseMapNode>();
			Tools.FillBox(Z, new Box(0, 0, 79, 24), "Verdigris");
			Tools.ClearBox(Z, new Box(1, 1, 78, 23));
			int num = 0;
			foreach (Box chute in golgothaTemplate.Chutes)
			{
				Box box = chute.Grow(-1);
				int num2 = Stat.Random(0, 3);
				if (num2 == 0)
				{
					for (int i = box.x1; i <= box.x2 && i < 77; i++)
					{
						if (i != 1)
						{
							Z.GetCell(i, box.y1).AddObject("Verdigris");
						}
					}
					for (int j = box.y1; j <= box.y2 && j < 22; j++)
					{
						if (j != 1)
						{
							Z.GetCell(box.x2, j).AddObject("Verdigris");
						}
					}
				}
				if (num2 == 1)
				{
					for (int k = box.x1; k <= box.x2 && k < 77; k++)
					{
						if (k != 1)
						{
							Z.GetCell(k, box.y1).AddObject("Verdigris");
						}
					}
					for (int l = box.y1; l <= box.y2 && l < 22; l++)
					{
						if (l != 1)
						{
							Z.GetCell(box.x2, l).AddObject("Verdigris");
						}
					}
				}
				if (num2 == 2)
				{
					for (int m = box.x1; m <= box.x2 && m < 77; m++)
					{
						if (m != 1)
						{
							Z.GetCell(m, box.y2).AddObject("Verdigris");
						}
					}
					for (int n = box.y1; n <= box.y2 && n < 22; n++)
					{
						if (n != 1)
						{
							Z.GetCell(box.x1, n).AddObject("Verdigris");
						}
					}
				}
				if (num2 == 3)
				{
					for (int num3 = box.x1; num3 <= box.x2 && num3 < 77; num3++)
					{
						if (num3 != 1)
						{
							Z.GetCell(num3, box.y1).AddObject("Verdigris");
						}
					}
					for (int num4 = box.y1; num4 <= box.y2 && num4 < 22; num4++)
					{
						if (num4 != 1)
						{
							Z.GetCell(box.x1, num4).AddObject("Verdigris");
						}
					}
				}
				string text = (string)XRLCore.Core.Game.ZoneManager.GetZoneProperty(Z.GetZoneIDFromDirection("U"), "Chute" + num + "EndY", bClampToLevel30: true);
				string obj = (string)XRLCore.Core.Game.ZoneManager.GetZoneProperty(Z.GetZoneIDFromDirection("D"), "Chute" + num + "StartY", bClampToLevel30: true);
				int num5 = (box.Width - 2) / 4;
				int num6 = (box.Height - 2) / 3;
				if (num5 < 1)
				{
					num5 = 2;
				}
				if (num6 < 1)
				{
					num6 = 2;
				}
				if (text == null)
				{
					text = Stat.Random(0, num6 - 1).ToString();
				}
				if (obj == null)
				{
					Stat.Random(0, num6 - 1).ToString();
				}
				int x = num5 - 4 + box.x1 + 1;
				int y = num6 - 3 + box.y1 + 1;
				list.Add(new NoiseMapNode(x, y));
				num++;
			}
			NoiseMap noiseMap = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, 40, 5, 100, 0, 6, 0, 1, list, 2);
			noiseMap = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, 60, 5, 100, 0, 5, 0, 1, null, 2);
			for (int num7 = 0; num7 < 80; num7++)
			{
				for (int num8 = 0; num8 < 25; num8++)
				{
					if ((double)noiseMap.Noise[num7, num8] > 0.01 && Z.GetCell(num7, num8).IsEmpty())
					{
						Z.GetCell(num7, num8).AddObject(GameObjectFactory.Factory.CreateObject("GooPuddle"));
					}
				}
			}
			noiseMap = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, 60, 5, 100, 0, 5, 0, 1, null, 2);
			for (int num9 = 0; num9 < 80; num9++)
			{
				for (int num10 = 0; num10 < 25; num10++)
				{
					if ((double)noiseMap.Noise[num9, num10] > 0.01 && Z.GetCell(num9, num10).IsEmpty())
					{
						Z.GetCell(num9, num10).AddObject(GameObjectFactory.Factory.CreateObject("SludgePuddle"));
					}
				}
			}
			noiseMap = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, 60, 5, 100, 0, 5, 0, 1, null, 2);
			for (int num11 = 0; num11 < 80; num11++)
			{
				for (int num12 = 0; num12 < 25; num12++)
				{
					if ((double)noiseMap.Noise[num11, num12] > 0.01 && Z.GetCell(num11, num12).IsEmpty())
					{
						Z.GetCell(num11, num12).AddObject(GameObjectFactory.Factory.CreateObject("OozePuddle"));
					}
				}
			}
			noiseMap = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, 25, 5, 100, 0, 5, 0, 1, null, 2);
			for (int num13 = 0; num13 < 80; num13++)
			{
				for (int num14 = 0; num14 < 25; num14++)
				{
					if ((double)noiseMap.Noise[num13, num14] > 0.01 && Z.GetCell(num13, num14).IsEmpty())
					{
						Z.GetCell(num13, num14).Clear();
						Z.GetCell(num13, num14).AddObject(GameObjectFactory.Factory.CreateObject("Garbage"));
					}
				}
			}
			Tools.FillBox(Z, golgothaTemplate.MainBuilding, "Verdigris");
			Tools.ClearBox(Z, golgothaTemplate.MainBuilding.Grow(-1));
			int x2 = golgothaTemplate.MainBuilding.Grow(-1).x1 + 2;
			int num15 = golgothaTemplate.MainBuilding.Grow(-1).y1 + 2;
			Cell cell = Z.GetCell(x2, num15);
			cell.Clear();
			cell.AddObject("FlyingWhitelistArea");
			cell.AddObject("Platform");
			Cell cell2 = Z.GetCell(x2, num15 - 1);
			cell2.Clear();
			cell2.AddObject("ElevatorSwitch");
			Z.GetCell((golgothaTemplate.MainBuilding.x1 + golgothaTemplate.MainBuilding.x2) / 2, golgothaTemplate.MainBuilding.y2).Clear();
		}
		else
		{
			if (Z.Z > 10)
			{
				for (int num16 = 0; num16 < Z.Width; num16++)
				{
					for (int num17 = 0; num17 < Z.Height; num17++)
					{
						Z.GetCell(num16, num17).AddObject(GameObjectFactory.Factory.CreateObject("Shale"));
					}
				}
			}
			Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Dirty"));
			if (Z.Z == 10)
			{
				Z.GetCell(0, 0).AddObject("DaylightWidget");
			}
			Tools.FillBox(Z, golgothaTemplate.MainBuilding, "Verdigris");
			Tools.ClearBox(Z, golgothaTemplate.MainBuilding.Grow(-1));
			int x3 = golgothaTemplate.MainBuilding.Grow(-1).x1 + 2;
			int y2 = golgothaTemplate.MainBuilding.Grow(-1).y1 + 2;
			Cell cell3 = Z.GetCell(x3, y2);
			cell3.Clear();
			cell3.AddObject("FlyingWhitelistArea");
			cell3.AddObject("OpenShaft");
			Cell cell4 = Z.GetCell((golgothaTemplate.MainBuilding.x1 + golgothaTemplate.MainBuilding.x2) / 2, golgothaTemplate.MainBuilding.y2);
			cell4.Clear();
			cell4.AddObject("Purple Security Door");
			Cell cell5 = Z.GetCell((golgothaTemplate.MainBuilding.x1 + golgothaTemplate.MainBuilding.x2) / 2 + 1, golgothaTemplate.MainBuilding.y2 - 1);
			cell5.Clear();
			cell5.AddObject("DoorSwitch");
			int num18 = 0;
			foreach (Box chute2 in golgothaTemplate.Chutes)
			{
				if (Z.Z > 10)
				{
					Tools.FillBox(Z, chute2, "Verdigris");
				}
				string text2 = (string)XRLCore.Core.Game.ZoneManager.GetZoneProperty(Z.GetZoneIDFromDirection("U"), "Chute" + num18 + "EndY");
				string text3 = (string)XRLCore.Core.Game.ZoneManager.GetZoneProperty(Z.GetZoneIDFromDirection("D"), "Chute" + num18 + "StartY");
				int num19 = (chute2.Width - 2) / 4;
				int num20 = (chute2.Height - 2) / 3;
				if (num19 < 1)
				{
					num19 = 2;
				}
				if (num20 < 1)
				{
					num20 = 2;
				}
				if (text2 == null)
				{
					text2 = Stat.Random(0, num20 - 1).ToString();
				}
				if (text3 == null)
				{
					text3 = Stat.Random(0, num20 - 1).ToString();
				}
				Convert.ToInt32(text2);
				Convert.ToInt32(text3);
				XRLCore.Core.Game.ZoneManager.SetZoneProperty(Z.ZoneID, "Chute" + num18 + "StartY", text2);
				XRLCore.Core.Game.ZoneManager.SetZoneProperty(Z.ZoneID, "Chute" + num18 + "EndY", text3);
				TunnelMaker tunnelMaker = ((Z.Z % 2 != 0) ? new TunnelMaker(num19, num20, text2, text3, "WNS") : new TunnelMaker(num19, num20, text2, text3, "ENS"));
				bool flag = Z.Z % 2 == 0;
				for (int num21 = 0; num21 < tunnelMaker.Width; num21++)
				{
					for (int num22 = 0; num22 < tunnelMaker.Height; num22++)
					{
						int num23 = num21 * 4 + chute2.x1 + 1;
						int num24 = num22 * 3 + chute2.y1 + 1;
						string text4 = tunnelMaker.Map[num21, num22];
						switch (text4)
						{
						case "N":
						case "S":
						case "E":
						case "W":
							text4 = (((!flag || num21 != 0) && (flag || num21 != tunnelMaker.Width - 1)) ? (text4 + "END") : (text4 + "START"));
							break;
						}
						string[] array = Tiles[text4];
						bool flag2 = false;
						for (int num25 = 0; num25 < 4; num25++)
						{
							for (int num26 = 0; num26 < 3; num26++)
							{
								Z.GetCell(num23 + num25, num24 + num26).Clear();
								char c = array[num26][num25];
								switch (c)
								{
								case '.':
									if (Z.Z > 10)
									{
										Z.GetCell(num23 + num25, num24 + num26).AddObject("Verdigris");
									}
									continue;
								case '>':
									Z.GetCell(num23 + num25, num24 + num26).AddObject("SlimyShaft");
									flag2 = true;
									continue;
								case 'C':
									if (Z.Z > 10)
									{
										Z.GetCell(num23 + num25, num24 + num26).AddObject("ConveyorDrive");
									}
									continue;
								}
								if (Z.Z > 10)
								{
									GameObject gameObject = GameObjectFactory.Factory.CreateObject("ConveyorPad");
									ConveyorPad obj2 = gameObject.GetPart("ConveyorPad") as ConveyorPad;
									obj2.Direction = c.ToString();
									obj2.Connections = c.ToString();
									Z.GetCell(num23 + num25, num24 + num26).AddObject(gameObject);
								}
							}
						}
						if (flag2 && Z.Z == 10)
						{
							Tools.Box(Z, new Box(num23 - 1, num24 - 1, Math.Min(num23 + 5, 79), Math.Min(num24 + 4, 24)), "Verdigris", 80);
						}
					}
				}
				if (Z.Z > 10)
				{
					int num27 = 0;
					int num28 = 0;
					int num29 = Stat.Random(6, 10);
					for (; num27 < 100; num27++)
					{
						if (num28 >= num29)
						{
							break;
						}
						int x4 = Stat.Random(chute2.x1, chute2.x2);
						int y3 = Stat.Random(chute2.y1, chute2.y2);
						if (!Z.GetCell(x4, y3).HasObjectWithIntProperty("Wall"))
						{
							continue;
						}
						bool flag3 = false;
						foreach (Cell cardinalAdjacentCell in Z.GetCell(x4, y3).GetCardinalAdjacentCells(bLocalOnly: true))
						{
							if (cardinalAdjacentCell.HasObjectWithPart("ConveyorPad"))
							{
								flag3 = true;
								break;
							}
						}
						if (flag3)
						{
							num28++;
							Z.GetCell(x4, y3).Clear();
							GameObject gameObject2 = null;
							if (num18 == 0)
							{
								gameObject2 = GameObjectFactory.Factory.CreateObject("WalltrapFire");
							}
							if (num18 == 1)
							{
								gameObject2 = GameObjectFactory.Factory.CreateObject("WalltrapAcid");
							}
							if (num18 == 2)
							{
								gameObject2 = GameObjectFactory.Factory.CreateObject("WalltrapShock");
							}
							if (num18 == 3)
							{
								gameObject2 = GameObjectFactory.Factory.CreateObject("WalltrapCrabs");
							}
							Walltrap walltrap = gameObject2.GetPart("Walltrap") as Walltrap;
							if (num18 == 0)
							{
								walltrap.TurnInterval = Stat.Random(4, 8);
							}
							if (num18 == 1)
							{
								walltrap.TurnInterval = Stat.Random(12, 16);
							}
							if (num18 == 2)
							{
								walltrap.TurnInterval = Stat.Random(4, 8);
							}
							if (num18 == 3)
							{
								walltrap.TurnInterval = Stat.Random(4, 16);
							}
							walltrap.CurrentTurn = Stat.Random(0, walltrap.TurnInterval - 2);
							Z.GetCell(x4, y3).AddObject(gameObject2);
						}
					}
				}
				num18++;
			}
			if (Z.Z != 10)
			{
				NoiseMap noiseMap2 = new NoiseMap(80, 25, 10, 4, 3, Stat.Random(0, 2), Stat.Random(15, 35), Stat.Random(35, 50), 5, 3, 1, 1, null);
				foreach (List<NoiseMapNode> value in noiseMap2.AreaNodes.Values)
				{
					foreach (NoiseMapNode item in value)
					{
						Z.GetCell(item.x, item.y).ClearObjectsWithIntProperty("Wall");
					}
				}
			}
			if (Z.Z != 10)
			{
				NoiseMap noiseMap2 = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, 10, 5, 100, 0, 6, 0, 1, null, 2);
				for (int num30 = 0; num30 < 80; num30++)
				{
					for (int num31 = 0; num31 < 25; num31++)
					{
						if ((double)noiseMap2.Noise[num30, num31] > 0.01 && Z.GetCell(num30, num31).IsEmpty() && !Z.GetCell(num30, num31).HasObjectWithPart("SpawnBlocker"))
						{
							Z.GetCell(num30, num31).AddObject(GameObjectFactory.Factory.CreateObject("Garbage"));
						}
					}
				}
				noiseMap2 = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, 20, 5, 100, 0, 5, 0, 1, null, 2);
				for (int num32 = 0; num32 < 80; num32++)
				{
					for (int num33 = 0; num33 < 25; num33++)
					{
						if ((double)noiseMap2.Noise[num32, num33] > 0.01 && Z.GetCell(num32, num33).IsEmpty() && Z.GetCell(num32, num33).HasObjectWithPart("SpawnBlocker"))
						{
							Z.GetCell(num32, num33).AddObject(GameObjectFactory.Factory.CreateObject("GooPuddle"));
						}
					}
				}
				noiseMap2 = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, 20, 5, 100, 0, 5, 0, 1, null, 2);
				for (int num34 = 0; num34 < 80; num34++)
				{
					for (int num35 = 0; num35 < 25; num35++)
					{
						if ((double)noiseMap2.Noise[num34, num35] > 0.01 && Z.GetCell(num34, num35).IsEmpty() && Z.GetCell(num34, num35).HasObjectWithPart("SpawnBlocker"))
						{
							Z.GetCell(num34, num35).AddObject(GameObjectFactory.Factory.CreateObject("SludgePuddle"));
						}
					}
				}
				noiseMap2 = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, 20, 5, 100, 0, 5, 0, 1, null, 2);
				for (int num36 = 0; num36 < 80; num36++)
				{
					for (int num37 = 0; num37 < 25; num37++)
					{
						if ((double)noiseMap2.Noise[num36, num37] > 0.01 && Z.GetCell(num36, num37).IsEmpty() && Z.GetCell(num36, num37).HasObjectWithPart("SpawnBlocker"))
						{
							Z.GetCell(num36, num37).AddObject(GameObjectFactory.Factory.CreateObject("OozePuddle"));
						}
					}
				}
			}
			Z.ClearReachableMap();
			foreach (Box chute3 in golgothaTemplate.Chutes)
			{
				for (int num38 = chute3.x1 + 1; num38 < chute3.x2; num38++)
				{
					int num39 = chute3.y1 + 1;
					while (num39 < chute3.y2)
					{
						if (!Z.GetCell(num38, num39).HasObjectWithPart("StairsDown"))
						{
							num39++;
							continue;
						}
						goto IL_138d;
					}
					continue;
					IL_138d:
					Z.BuildReachableMap(num38, num39, bClearFirst: false);
					break;
				}
			}
		}
		for (int num40 = 0; num40 < 80; num40++)
		{
			for (int num41 = 0; num41 < 25; num41++)
			{
				if (Z.GetCell(num40, num41).HasObjectWithPart("LiquidVolume"))
				{
					LiquidVolume liquidVolume = Z.GetCell(num40, num41).GetFirstObjectWithPart("LiquidVolume").GetPart("LiquidVolume") as LiquidVolume;
					if (liquidVolume.MaxVolume == -1 && liquidVolume.Volume > 500 && Stat.Random(1, 100) <= 20)
					{
						Z.GetCell(num40, num41).AddObject(GameObjectFactory.Factory.CreateObject("EelSpawn"));
					}
				}
			}
		}
		return true;
	}
}
