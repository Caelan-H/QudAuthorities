using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using HistoryKit;
using Qud.API;
using XRL.EditorFormats.Map;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class CryptOfWarriors : ZoneBuilderSandbox
{
	public const string wallType = "EbonFulcrete";

	public GameObject generateSarcophagusContents()
	{
		return GameObject.create("Ancient Corpse");
	}

	public GameObject generateRelic(string table)
	{
		PopulationResult populationResult = PopulationManager.RollOneFrom(table);
		string text = populationResult.Blueprint;
		if (text == "PriestCryptLoot")
		{
			text = EncountersAPI.GetAnItemBlueprint(delegate(GameObjectBlueprint B)
			{
				double num2 = double.Parse(B.GetPartParameter("Commerce", "Value", "0"));
				return (B.Tier >= 5 || (B.Tier == 4 && num2 >= 800.0)) && B.Tier <= 7 && !B.DescendsFrom("MeleeWeapon") && !B.Name.Contains("Carbide") && !B.Name.Contains("Fullerite") && !B.DescendsFrom("Corpse") && !B.DescendsFrom("Energy Cell") && !B.DescendsFrom("Liquid Fueled Energy Cell") && !B.DescendsFrom("Random Figurine") && !B.DescendsFrom("Gemstone") && num2 >= 200.0;
			});
		}
		if (text == "RoyalCryptLoot")
		{
			text = EncountersAPI.GetAnItemBlueprint(delegate(GameObjectBlueprint B)
			{
				double num = double.Parse(B.GetPartParameter("Commerce", "Value", "0"));
				return (B.Tier >= 5 || (B.Tier == 4 && num >= 800.0)) && B.Tier <= 7 && !B.DescendsFrom("MeleeWeapon") && !B.Name.Contains("Carbide") && !B.Name.Contains("Fullerite") && !B.DescendsFrom("Corpse") && !B.DescendsFrom("Energy Cell") && !B.DescendsFrom("Liquid Fueled Energy Cell") && !B.DescendsFrom("Random Figurine") && !B.DescendsFrom("Gemstone") && num >= 250.0;
			});
		}
		int bonusModChance = 0;
		int setModNumber = 0;
		if (populationResult.Hint != null)
		{
			string[] array = populationResult.Hint.Split(',');
			foreach (string text2 in array)
			{
				if (text2 != null && text2.Contains("SetBonusModChance:"))
				{
					bonusModChance = text2.Split(':')[1].RollCached();
				}
				if (text2 != null && text2.Contains("SetMinModNumber:"))
				{
					setModNumber = text2.Split(':')[1].RollCached();
				}
			}
		}
		return GameObjectFactory.Factory.CreateObject(text, bonusModChance, setModNumber);
	}

	public bool BuildZone(Zone zone)
	{
		List<string> list = new List<string>
		{
			"CryptPeriod1_N", "CryptPeriod1_S", "CryptPeriod1_E", "CryptPeriod1_W", "CryptPeriod2_N", "CryptPeriod2_S", "CryptPeriod2_E", "CryptPeriod2_W", "CryptPeriod3_N", "CryptPeriod3_S",
			"CryptPeriod3_E", "CryptPeriod3_W", "CryptPeriod4_N", "CryptPeriod4_S", "CryptPeriod4_E", "CryptPeriod4_W", "CryptPeriod5_N", "CryptPeriod5_S", "CryptPeriod5_E", "CryptPeriod5_W"
		};
		if (zone.Z == 7)
		{
			list.Add("CryptPeriod1_Large");
			list.Add("CryptPeriod2_Large");
			list.Add("CryptPeriod3_Large");
			list.Add("CryptPeriod4_Large");
			list.Add("CryptPeriod5_Large");
			The.Game.RequireSystem(() => new CryptOfPriestsAnchorSystem());
		}
		else
		{
			The.Game.RequireSystem(() => new CryptOfWarriorsAnchorSystem());
		}
		zone.FillHollowBox(new Box(0, 0, zone.Width - 1, zone.Height - 1), "EbonFulcrete");
		new SpindleFootprint().BuildZone(zone);
		if ((zone.Y == 0 || zone.Y == 2) && (zone.X == 0 || zone.X == 1))
		{
			zone.ClearBox(new Box(zone.Width - 1, 1, zone.Width - 1, zone.Height - 2));
		}
		if ((zone.Y == 0 || zone.Y == 2) && (zone.X == 1 || zone.X == 2))
		{
			zone.ClearBox(new Box(0, 1, 0, zone.Height - 2));
		}
		if ((zone.X == 0 || zone.X == 2) && (zone.Y == 0 || zone.Y == 1))
		{
			zone.ClearBox(new Box(1, zone.Height - 1, zone.Width - 2, zone.Height - 1));
		}
		if ((zone.X == 0 || zone.X == 2) && (zone.Y == 1 || zone.Y == 2))
		{
			zone.ClearBox(new Box(1, 0, zone.Width - 2, 0));
		}
		Dictionary<string, MapFile> dictionary = new Dictionary<string, MapFile>();
		Dictionary<string, Tuple<int, int>> dictionary2 = new Dictionary<string, Tuple<int, int>>();
		foreach (string item in list)
		{
			if (!dictionary.ContainsKey(item))
			{
				dictionary.Add(item, MapFile.LoadWithMods("preset_tile_chunks/" + item + ".rpm"));
			}
		}
		foreach (KeyValuePair<string, MapFile> item2 in dictionary)
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < 80; i++)
			{
				for (int j = 0; j < 25; j++)
				{
					if (item2.Value.Cells[i, j].Objects.Count > 0)
					{
						if (num < i + 1)
						{
							num = i + 1;
						}
						if (num2 < j + 1)
						{
							num2 = j + 1;
						}
					}
				}
			}
			dictionary2.Add(item2.Key, new Tuple<int, int>(num, num2));
		}
		List<Rect2D> maskedAreas = new List<Rect2D>();
		List<Rect2D> cryptFootprintsOnly = new List<Rect2D>();
		if (zone.X == 0 && zone.Y == 1)
		{
			zone.GetCell(70, 12).AddObject("StairsDown");
			zone.GetCell(70, 13).AddObject("StairsDown");
			maskedAreas.Add(new Rect2D(69, 11, 71, 14));
		}
		List<Cell> list2 = (from c in zone.GetCells()
			where c.X > 5 && c.X < 74 && c.Y > 4 && c.Y < 19 && c.Y % 4 == 0 && c.X % 8 == 0
			select c).ToList();
		if (zone.X == 0 && zone.Y == 0)
		{
			maskedAreas.Add(new Rect2D(76, 22, 79, 24));
		}
		if (zone.X == 2 && zone.Y == 0)
		{
			maskedAreas.Add(new Rect2D(0, 22, 3, 24));
		}
		if (zone.X == 0 && zone.Y == 2)
		{
			maskedAreas.Add(new Rect2D(76, 0, 79, 2));
		}
		if (zone.X == 2 && zone.Y == 2)
		{
			maskedAreas.Add(new Rect2D(0, 0, 3, 2));
		}
		if (zone.X == 1 && zone.Y == 0)
		{
			maskedAreas.Add(new Rect2D(0, 19, 79, zone.Height - 1));
			maskedAreas.Add(new Rect2D(25, 20, 54, zone.Height - 1));
		}
		if (zone.X == 1 && zone.Y == 2)
		{
			maskedAreas.Add(new Rect2D(0, 0, 79, 4));
			maskedAreas.Add(new Rect2D(25, 4, 54, 5));
		}
		if (zone.X == 0 && zone.Y == 1)
		{
			maskedAreas.Add(new Rect2D(75, 0, 79, 24));
			maskedAreas.Add(new Rect2D(74, 6, 75, 19));
		}
		if (zone.X == 2 && zone.Y == 1)
		{
			maskedAreas.Add(new Rect2D(0, 0, 4, 24));
			maskedAreas.Add(new Rect2D(4, 6, 5, 19));
		}
		maskedAreas.Add(new Rect2D(35, 8, 45, 16));
		zone.GetCell(36, 9).AddObject("TombPillarPlacement");
		bool flag;
		do
		{
			string text = list.GetRandomElement();
			Tuple<int, int> tuple = dictionary2[text];
			Tuple<int, int> tuple2 = new Tuple<int, int>(tuple.Item1 + 2, tuple.Item2 + 2);
			string[] list3 = new string[4] { "AB", "A", "B", "" };
			flag = false;
			foreach (Cell item3 in list2.InRandomOrderNoAlloc())
			{
				Cell cell = item3;
				Rect2D newRect = new Rect2D(cell.X, cell.Y, cell.X + tuple2.Item1, cell.Y + tuple2.Item2);
				if (cell.X <= 1 || cell.Y <= 1 || cell.X >= zone.Width - tuple2.Item1 || cell.Y >= zone.Height - tuple2.Item2 || maskedAreas.Any((Rect2D r) => r.overlaps(newRect)))
				{
					continue;
				}
				if (text.EndsWith("_N") && cell.Y < 12)
				{
					text = text.Substring(0, text.Length - 2) + "_S";
				}
				if (text.EndsWith("_S") && cell.Y > 12)
				{
					text = text.Substring(0, text.Length - 2) + "_N";
				}
				if (text.EndsWith("_E") && cell.X > 50)
				{
					text = text.Substring(0, text.Length - 2) + "_W";
				}
				if (text.EndsWith("_W") && cell.X < 30)
				{
					text = text.Substring(0, text.Length - 2) + "_E";
				}
				MapFile mapFile = dictionary[text];
				string text2 = list3.GetRandomElement();
				string blueprint = PopulationManager.RollOneFrom("CryptFurniture").Blueprint;
				bool flag2 = false;
				while (true)
				{
					flag = true;
					maskedAreas.Add(newRect);
					cryptFootprintsOnly.Add(new Rect2D(cell.X + 1, cell.Y + 1, cell.X + tuple2.Item1 - 2, cell.Y + tuple2.Item2 - 2));
					int num3 = 0;
					int num4 = 20;
					if (text2 != "AB" && Stat.Random(1, 100) <= num4)
					{
						text2 = "M";
					}
					EaterCryptPlaque eaterCryptPlaque = new EaterCryptPlaque();
					if (text2 == "")
					{
						eaterCryptPlaque.IsEmpty = true;
					}
					string text3;
					if (zone.Z == 8)
					{
						if (Stat.Random(0, 1) == 0)
						{
							text3 = "Warrior";
							eaterCryptPlaque.Caste = "warrior";
						}
						else
						{
							text3 = "Tutor";
							eaterCryptPlaque.Caste = "tutor";
						}
					}
					else if (Stat.Random(0, 1) == 0)
					{
						text3 = "Priest";
						eaterCryptPlaque.Caste = "priest";
					}
					else
					{
						text3 = "RoyalFamily";
						eaterCryptPlaque.Caste = "royal";
					}
					eaterCryptPlaque.GeneratePlaque();
					string table = "CryptReliquary_" + text3;
					Location2D p = Location2D.get(0, 0);
					for (int k = 0; k < tuple.Item1; k++)
					{
						for (int l = 0; l < tuple.Item2; l++)
						{
							foreach (MapFileObjectBlueprint @object in mapFile.Cells[k, l].Objects)
							{
								Cell cell2 = zone.GetCell(k + cell.X + 1, l + cell.Y + 1);
								if (cell2 == null)
								{
									continue;
								}
								if (@object.Name.Contains("AnchorRoomTile"))
								{
									cell2.PaintTile = (((cell2.X + cell2.Y) % 2 == 0) ? "Tiles2/sw_floor_diamond_1.bmp" : "Tiles2/sw_floor_diamond_2.bmp");
									cell2.PaintTileColor = "&K";
									cell2.PaintColorString = "&K";
									cell2.PaintRenderString = '\u0004'.ToString();
								}
								if (@object.Name.Contains("CryptWall"))
								{
									GameObject gameObject = cell2.AddObject(@object.Name);
									Description description = gameObject.GetPart("Description") as Description;
									if (text3 == "Warrior")
									{
										gameObject.pRender.ColorString += "^r";
										description._Short += " In tracery around the margins, warriors assemble in hex phalanxes.";
									}
									if (text3 == "Tutor")
									{
										gameObject.pRender.ColorString += "^C";
										description._Short += " In tracery around the margins, tutors instruct pupils in domed amphitheatres.";
									}
									if (text3 == "Priest")
									{
										gameObject.pRender.ColorString += "^W";
										description._Short += " In tracery around the margins, priests burn incense inside of thuribles.";
									}
									if (text3 == "RoyalFamily")
									{
										gameObject.pRender.ColorString += "^m";
										description._Short += " In tracery around the margins, royals are extolled at sky-kited festivals.";
									}
								}
								else if (GameObjectFactory.Factory.GetBlueprint(@object.Name).DescendsFrom("Crypt Door") || @object.Name == "Crypt Door" || @object.Name.StartsWith("Crypt Double Door"))
								{
									GameObject gameObject2 = cell2.AddObject(@object.Name);
									gameObject2.RemovePart("CryptPlaque");
									gameObject2.AddPart(eaterCryptPlaque.DeepCopy(gameObject2, null));
									if (text3 == "Warrior")
									{
										gameObject2.pRender.DetailColor = "r";
									}
									if (text3 == "Tutor")
									{
										gameObject2.pRender.DetailColor = "C";
									}
									if (text3 == "Priest")
									{
										gameObject2.pRender.DetailColor = "W";
									}
									if (text3 == "RoyalFamily")
									{
										gameObject2.pRender.DetailColor = "m";
									}
								}
								else if (@object.Name == "Crypt Sitter")
								{
									if (text2 != "M" && text2 != "")
									{
										cell2.AddObject("Crypt Sitter Special");
									}
								}
								else if (@object.Name == "Crypt Sitter Special")
								{
									if (text2 != "M" && text2 != "")
									{
										cell2.AddObject(@object.Name);
									}
								}
								else if (@object.Name == "CryptFurnitureSpawner")
								{
									if (text2 != "M")
									{
										cell2.AddObject(blueprint);
									}
								}
								else if (@object.Name == "CryptFurnitureSpawner Flipped")
								{
									if (text2 != "M")
									{
										if (blueprint == "Eater Hologram")
										{
											cell2.AddObject("Eater Hologram Flipped");
										}
										else
										{
											cell2.AddObject(blueprint);
										}
									}
								}
								else if (@object.Name == "Sarcophagus")
								{
									if (text2 != "M")
									{
										GameObject gameObject3 = cell2.AddObject(@object.Name);
										if ((text2.Contains("A") && num3 == 0) || (text2.Contains("B") && num3 == 1))
										{
											GameObject who = cell2.AddObject(generateSarcophagusContents());
											gameObject3.GetPart<Enclosing>().EnterEnclosure(who);
										}
										else
										{
											cell2.AddObject("AnchorRoomTile");
											cell2.PaintTile = (((cell2.X + cell2.Y) % 2 == 0) ? "Tiles2/sw_floor_diamond_1.bmp" : "Tiles2/sw_floor_diamond_2.bmp");
											cell2.PaintTileColor = "&K";
											cell2.PaintColorString = "&K";
											cell2.PaintRenderString = '\u0004'.ToString();
										}
									}
									else
									{
										cell2.AddObject("AnchorRoomTile");
										cell2.PaintTile = (((cell2.X + cell2.Y) % 2 == 0) ? "Tiles2/sw_floor_diamond_1.bmp" : "Tiles2/sw_floor_diamond_2.bmp");
										cell2.PaintTileColor = "&K";
										cell2.PaintColorString = "&K";
										cell2.PaintRenderString = '\u0004'.ToString();
									}
									num3++;
								}
								else if (@object.Name == "Reliquary")
								{
									p = cell2.location;
									if (!(text2 != "M"))
									{
										continue;
									}
									GameObject gameObject4 = cell2.AddObject(@object.Name);
									if (text2.Contains("A"))
									{
										GameObject gameObject5 = generateRelic(table);
										gameObject4.TakeObject(gameObject5, Silent: true, 0);
										int chance = 0;
										if (text3 == "Priest")
										{
											chance = 25;
										}
										if (text3 == "RoyalFamily")
										{
											chance = 75;
										}
										if (If.Chance(chance) && gameObject5.Blueprint != "Grave Goods")
										{
											PopulationResult populationResult = PopulationManager.RollOneFrom("Cybernetics7");
											gameObject4.TakeObject(GameObjectFactory.create(populationResult.Blueprint), Silent: false, 0);
										}
									}
									if (text2.Contains("B"))
									{
										GameObject gameObject6 = generateRelic(table);
										gameObject4.TakeObject(gameObject6, Silent: true, 0);
										int chance2 = 0;
										if (text3 == "Priest")
										{
											chance2 = 25;
										}
										if (text3 == "RoyalFamily")
										{
											chance2 = 75;
										}
										if (If.Chance(chance2) && gameObject6.Blueprint != "Grave Goods")
										{
											PopulationResult populationResult2 = PopulationManager.RollOneFrom("Cybernetics7");
											gameObject4.TakeObject(GameObjectFactory.create(populationResult2.Blueprint), Silent: false, 0);
										}
									}
								}
								else
								{
									cell2.AddObject(@object.Name);
								}
							}
						}
					}
					if (text2 == "M")
					{
						InfluenceMap influenceMap = new InfluenceMap(80, 25);
						zone.SetInfluenceMapWallsDoors(influenceMap.Walls);
						influenceMap.AddSeed(p);
						ZoneBuilderSandbox.PlacePopulationInRegion(zone, influenceMap.Regions[0], "MopangoHideout");
					}
					if (flag2)
					{
						break;
					}
					flag2 = true;
					Vector2i vector2i = new Vector2i(0, 0);
					if (text.EndsWith("_N"))
					{
						vector2i = new Vector2i(0, -newRect.Height - 1);
						text = text.Substring(0, text.Length - 2) + "_S";
					}
					else if (text.EndsWith("_S"))
					{
						vector2i = new Vector2i(0, newRect.Height + 1);
						text = text.Substring(0, text.Length - 2) + "_N";
					}
					else if (text.EndsWith("_E"))
					{
						vector2i = new Vector2i(newRect.Width + 1, 0);
						text = text.Substring(0, text.Length - 2) + "_W";
					}
					else
					{
						if (!text.EndsWith("_W"))
						{
							break;
						}
						vector2i = new Vector2i(-newRect.Width - 1, 0);
						text = text.Substring(0, text.Length - 2) + "_E";
					}
					newRect = new Rect2D(newRect.x1 + vector2i.x, newRect.y1 + vector2i.y, newRect.x2 + vector2i.x, newRect.y2 + vector2i.y);
					if (newRect != Rect2D.invalid && !maskedAreas.Any((Rect2D r) => r.overlaps(newRect)) && new Rect2D(0, 0, 79, 24).Contains(newRect))
					{
						mapFile = dictionary[text];
						tuple = dictionary2[text];
						tuple2 = new Tuple<int, int>(tuple.Item1 + 2, tuple.Item2 + 2);
						cell = zone.GetCell(newRect.x1, newRect.y1);
						continue;
					}
					break;
				}
				break;
			}
		}
		while (flag);
		Stat.ReseedFrom(zone.ZoneID);
		zone.ForeachCell(delegate(Cell c)
		{
			c.SetReachable(State: true);
		});
		zone.GetCell(0, 0).AddObject("ConcreteFloor");
		List<Location2D> cryptExclusionAreas = new List<Location2D>();
		foreach (Rect2D item4 in cryptFootprintsOnly)
		{
			item4.ForEachLocation(delegate(Location2D a)
			{
				cryptExclusionAreas.Add(a);
			});
		}
		List<Cell> cells = (from c in zone.GetCells()
			where !cryptExclusionAreas.Contains(c.location)
			select c).ToList();
		List<Cell> list4 = new List<Cell>();
		foreach (Cell item5 in zone.GetCellsWithTaggedObject("Door"))
		{
			Cell closestCellFromList = item5.getClosestCellFromList(cells);
			list4.Add(closestCellFromList);
		}
		InfluenceMap influenceMap2 = ZoneBuilderSandbox.GenerateInfluenceMap(zone, new List<Point>(), InfluenceMapSeedStrategy.LargestRegion, 100, cryptExclusionAreas, Options.GetOption("OptionDrawInfluenceMaps", "No") == "Yes");
		influenceMap2.Regions.ForEach(delegate(InfluenceMapRegion r)
		{
			r.Tags.Add("connected");
		});
		List<InfluenceMapRegion> list5 = influenceMap2.Regions.Where((InfluenceMapRegion r) => !r.IsEdgeRegion() && r.maxRect.Width >= 6 && r.maxRect.Height >= 6).ToList();
		list5.Sort((InfluenceMapRegion a, InfluenceMapRegion b) => b.maxRect.Area.CompareTo(a.maxRect.Area));
		List<Location2D> spots = list4.Select((Cell d) => d.location).ToList();
		spots.AddRange(list5.Select((InfluenceMapRegion r) => r.Center));
		if (zone.X == 0 && zone.Y == 0)
		{
			spots.Add(getMatchedEdgeConnectionLocation(zone, "tombedge", "e"));
			spots.Add(getMatchedEdgeConnectionLocation(zone, "tombedge", "s"));
		}
		if (zone.X == 2 && zone.Y == 0)
		{
			spots.Add(getMatchedEdgeConnectionLocation(zone, "tombedge", "w"));
			spots.Add(getMatchedEdgeConnectionLocation(zone, "tombedge", "s"));
		}
		if (zone.X == 0 && zone.Y == 2)
		{
			spots.Add(getMatchedEdgeConnectionLocation(zone, "tombedge", "n"));
			spots.Add(getMatchedEdgeConnectionLocation(zone, "tombedge", "e"));
		}
		if (zone.X == 2 && zone.Y == 2)
		{
			spots.Add(getMatchedEdgeConnectionLocation(zone, "tombedge", "w"));
			spots.Add(getMatchedEdgeConnectionLocation(zone, "tombedge", "n"));
		}
		if (zone.X == 1 && (zone.Y == 0 || zone.Y == 2))
		{
			spots.Add(getMatchedEdgeConnectionLocation(zone, "tombedge", "e"));
			spots.Add(getMatchedEdgeConnectionLocation(zone, "tombedge", "w"));
		}
		if (zone.Y == 1 && (zone.X == 0 || zone.X == 2))
		{
			spots.Add(getMatchedEdgeConnectionLocation(zone, "tombedge", "n"));
			spots.Add(getMatchedEdgeConnectionLocation(zone, "tombedge", "s"));
		}
		foreach (Location2D item6 in spots)
		{
			zone.GetCell(item6).ClearWalls();
			if (item6.x == 0 || item6.x == 79 || item6.y == 0 || item6.y == 24)
			{
				zone.GetCell(item6).GetLocalAdjacentCells().ForEach(delegate(Cell c)
				{
					c.ClearWalls();
					c.AddObject("CryptTrail");
				});
			}
		}
		if (spots.Count > 1)
		{
			Location2D source = spots[0];
			spots.Sort((Location2D a, Location2D b) => a.ManhattanDistance(source).CompareTo(b.ManhattanDistance(source)));
			BuildPathWithObject(zone, spots, "CryptTrail", 0, bNoise: false, delegate(int x, int y, int c)
			{
				if (!spots.Contains(Location2D.get(x, y)) && (x == 0 || y == 0 || x == 79 || y == 24))
				{
					return 80;
				}
				if (!zone.GetCell(x, y).IsPassable())
				{
					return 80;
				}
				if (maskedAreas.Any((Rect2D r) => r.Contains(x, y)))
				{
					return int.MaxValue;
				}
				if (cryptExclusionAreas.Contains(Location2D.get(x, y)))
				{
					return int.MaxValue;
				}
				return cryptFootprintsOnly.Any((Rect2D r) => r.Contains(x, y)) ? int.MaxValue : 0;
			});
		}
		if (zone.Z == 7)
		{
			foreach (InfluenceMapRegion item7 in list5)
			{
				Grid<Color4> grid = new Grid<Color4>(item7.maxRect.Width, item7.maxRect.Height);
				grid.fromWFCTemplate("cryptgarden" + Stat.Random(1, 3));
				if (Stat.Random(1, 100) <= 80)
				{
					grid.mirrorHorizontal();
				}
				if (Stat.Random(1, 100) <= 80)
				{
					grid.mirrorVertical();
				}
				string blueprint2 = PopulationManager.RollOneFrom("CryptGardenPlants").Blueprint;
				for (int m = 0; m < item7.maxRect.Width; m++)
				{
					for (int n = 0; n < item7.maxRect.Height; n++)
					{
						Cell cell3 = zone.GetCell(m + item7.maxRect.x1, n + item7.maxRect.y1);
						if (!cell3.HasObject("CryptTrail"))
						{
							cell3.AddObject("CryptTrail");
							if (grid.get(m, n) == Color4.black)
							{
								cell3.AddObject(blueprint2);
							}
							else if (Stat.Random(1, 100) <= 5)
							{
								cell3.AddObject("Ornate Bench");
							}
						}
					}
				}
			}
			List<Cell> list6 = (from c in zone.GetCells()
				where !c.HasObject("CryptTrail") && (from d in c.GetLocalAdjacentCells(1)
					where d.HasObject("CryptTrail")
					select d).Count() > 0
				select c).ToList();
			list6.ShuffleInPlace();
			for (int num5 = 0; num5 < 16 && num5 < list6.Count; num5++)
			{
				list6[num5].AddObject("Tomb Techlight1");
			}
		}
		string seedid = "rocks_" + zone.ZoneID;
		zone.ForeachCell(delegate(Cell c)
		{
			int x2 = c.X;
			int y2 = c.Y;
			if (c.DistanceToEdge() <= 5 && (double)(5 - c.DistanceToEdge()) - sampleSimplexNoise(seedid, x2, y2, zone.Z, 0f, 5f) > 0.0 && !cryptFootprintsOnly.Any((Rect2D r) => r.Contains(x2, y2)) && !maskedAreas.Any((Rect2D r) => r.Contains(x2, y2)) && !c.HasWall() && !c.HasObject("CryptTrail") && !c.HasObject("Burnished Azzurum"))
			{
				c.Clear();
				c.AddObject("Polished Black Marble");
			}
		});
		Grid<bool> grid2 = new Grid<bool>(80, 25);
		if (zone.X == 0 || zone.Y == 1)
		{
			grid2.setRect(0, 0, 8, 24, value: true);
		}
		if (zone.X == 2 || zone.Y == 1)
		{
			grid2.setRect(72, 0, 79, 24, value: true);
		}
		if (zone.Y == 0 || zone.X == 1)
		{
			grid2.setRect(0, 0, 79, 5, value: true);
		}
		if (zone.Y == 2 || zone.X == 1)
		{
			grid2.setRect(0, 19, 79, 24, value: true);
		}
		seedid = "forest_" + zone.ZoneID;
		grid2.forEach(delegate(int x, int y, bool b)
		{
			if (b && sampleSimplexNoise(seedid, x, y, zone.Z, -1f, 1f) > 0.0 && !cryptFootprintsOnly.Any((Rect2D r) => r.Contains(x, y)) && !maskedAreas.Any((Rect2D r) => r.Contains(x, y)))
			{
				Cell cell4 = zone.GetCell(x, y);
				if (!cell4.HasWall() && !cell4.HasObject("CryptTrail"))
				{
					cell4.AddPopulation("CryptHolographicForest");
				}
			}
		});
		influenceMap2 = ZoneBuilderSandbox.GenerateInfluenceMap(zone, new List<Point>(), InfluenceMapSeedStrategy.LargestRegion, 100, cryptExclusionAreas, Options.GetOption("OptionDrawInfluenceMaps", "No") == "Yes");
		influenceMap2.Regions.ForEach(delegate(InfluenceMapRegion r)
		{
			r.Tags.Add("connected");
		});
		if (zone.Z == 7)
		{
			if (ZoneTemplateManager.HasTemplates("CryptOfPriests"))
			{
				ZoneTemplateManager.Templates["CryptOfPriests"].Execute(zone, influenceMap2);
			}
		}
		else if (ZoneTemplateManager.HasTemplates("CryptOfWarriors"))
		{
			ZoneTemplateManager.Templates["CryptOfWarriors"].Execute(zone, influenceMap2);
		}
		if (zone.Z == 8)
		{
			if (The.Game.RequireGameState("CryptOfWarriorsStairsZones", delegate
			{
				List<string> stairZones2 = new List<string>();
				for (int num7 = 0; num7 < 4; num7++)
				{
					stairZones2.Add(CryptOfWarriorsAnchorSystem.warriorsAllowedStairsZones.Where((string a) => !stairZones2.Contains(a)).GetRandomElement());
				}
				return stairZones2;
			}).Contains(zone.ZoneID))
			{
				zone.GetEmptyCells().GetRandomElement().AddObject("StairsUp");
				ZoneBuilderSandbox.EnsureAllVoidsConnected(zone);
			}
		}
		else if (zone.Z == 7 && The.Game.RequireGameState("CryptOfPriestsStairsZones", delegate
		{
			List<string> stairZones = new List<string>();
			for (int num6 = 0; num6 < 3; num6++)
			{
				stairZones.Add(CryptOfPriestsAnchorSystem.priestsAllowedStairsZones.Where((string a) => !stairZones.Contains(a)).GetRandomElement());
			}
			stairZones.Add("JoppaWorld.53.3.0.2.7");
			return stairZones;
		}).Contains(zone.ZoneID))
		{
			if (zone.ZoneID == "JoppaWorld.53.3.0.2.7")
			{
				zone.GetCell(2, 22).Clear();
				zone.GetCell(2, 22).AddObject("StairsUp");
			}
			else
			{
				zone.GetEmptyCells().GetRandomElement().AddObject("Crypt Exit Teleporter");
			}
		}
		zone.GetCell(0, 0).AddObject("Finish_TombOfTheEaters_EnterTheTombOfTheEaters");
		new ChildrenOfTheTomb().BuildZone(zone);
		return true;
	}
}
