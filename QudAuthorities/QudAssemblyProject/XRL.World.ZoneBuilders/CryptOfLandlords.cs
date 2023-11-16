using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class CryptOfLandlords : ZoneBuilderSandbox
{
	public const string wallType = "MachineWallHotTubing";

	public bool BuildFlames(Zone zone)
	{
		zone.FillHollowBox(new Box(0, 0, zone.Width - 1, zone.Height - 1), "MachineWallHotTubing");
		zone.FillBox(new Box(40, 0, 40, zone.Height - 1), "MachineWallHotTubing", clearFirst: true);
		zone.FillBox(new Box(11, 4, 13, 6), "MachineWallHotTubing");
		zone.FillBox(new Box(0, 0, 79, 4), "MachineWallHotTubing");
		zone.FillBox(new Box(0, 20, 79, 24), "MachineWallHotTubing");
		Grid<Color4> grid = new Grid<Color4>(40, 25);
		grid.fromWFCTemplate("compound3");
		grid = grid.mirrorHorizontal();
		grid = grid.mirrorVertical();
		for (int i = 0; i < 40; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (grid.get(i, j) == Color4.black)
				{
					zone.GetCell(i, j).ClearWalls();
					zone.GetCell(i, j).AddObject("MachineWallHotTubing");
				}
			}
		}
		grid = new Grid<Color4>(40, 25);
		grid.fromWFCTemplate("longhalls2");
		for (int k = 0; k < 40; k++)
		{
			for (int l = 0; l < 25; l++)
			{
				if (grid.get(k, l) == Color4.black)
				{
					zone.GetCell(k + 40, l).ClearWalls();
					zone.GetCell(k + 40, l).AddObject("MachineWallHotTubing");
				}
			}
		}
		Box leftRegion = new Box(1, 1, 39, 23);
		Box rightRegion = new Box(40, 1, 79, 23);
		Box b = new Box(60, 1, 79, 23);
		List<Cell> list = (from cell in zone.GetCells()
			where leftRegion.contains(cell.location) && cell.HasWall()
			select cell).ToList();
		for (int m = 0; m < 20; m++)
		{
			list.RemoveRandomElement()?.ClearWalls();
		}
		List<Cell> list2 = (from cell in zone.GetCells()
			where rightRegion.contains(cell.location) && cell.IsEmpty()
			select cell).ToList();
		for (int n = 0; n < 8; n++)
		{
			list2.RemoveRandomElement()?.AddObject("OpenShaft2");
		}
		new SpindleFootprint().BuildZone(zone);
		zone.ClearBox(new Box(35, 8, 45, 16));
		zone.GetCell(36, 9).AddObject("TombPillarPlacement");
		if (zone.ZoneID == "JoppaWorld.53.3.2.0.9")
		{
			ZoneBuilderSandbox.PlacePrefab(zone, "preset_tile_chunks/CrematoryStairsDown.rpm", zone.GetCell(72, 10), 0, null, delegate(Cell c)
			{
				c.ClearWalls();
			});
		}
		else
		{
			ZoneBuilderSandbox.PlacePrefab(zone, "preset_tile_chunks/CrematoryStairsDown.rpm", zone.GetCell(72, 9), 0, null, delegate(Cell c)
			{
				c.ClearWalls();
			});
		}
		ZoneBuilderSandbox.EnsureAllVoidsConnected(zone);
		if (zone.Y == 0)
		{
			zone.ClearBox(new Box(68, 20, 75, 24));
			zone.GetCell(68, 24).AddObject("ConveyorDrive");
			zone.GetCell(74, 24).AddObject("ConveyorDrive");
			CreateConveyor(Location2D.get(75, 24), Location2D.get(60, 8), zone, (int x, int y) => ((y > 16 && x < 75) || x < 60) ? 9999 : 0, "W", "CrematoryConveyorPad").Concat(CreateConveyor(Location2D.get(59, 8), Location2D.get(0, 8), zone, (int x, int y) => (y > 13 || y < 7 || x > 59) ? 9999 : 0, "W", "CrematoryConveyorPad"));
			CreateConveyor(Location2D.get(69, 24), Location2D.get(60, 16), zone, (int x, int y) => (x > 70 || x < 60) ? 9999 : 0, "W", "CrematoryConveyorPad").Concat(CreateConveyor(Location2D.get(59, 16), Location2D.get(0, 16), zone, (int x, int y) => (y < 14 || y > 20 || x > 59) ? 9999 : 0, "W", "CrematoryConveyorPad"));
			zone.GetCell(0, 8).AddObject("BeltCurtains");
			zone.GetCell(0, 16).AddObject("BeltCurtains");
			zone.GetCell(75, 24).AddObject("BeltCurtains");
			zone.GetCell(69, 24).AddObject("BeltCurtains");
		}
		else
		{
			zone.ClearBox(new Box(68, 0, 75, 5));
			zone.GetCell(68, 0).AddObject("ConveyorDrive");
			zone.GetCell(74, 0).AddObject("ConveyorDrive");
			CreateConveyor(Location2D.get(69, 0), Location2D.get(60, 8), zone, (int x, int y) => (x > 69 || x < 60) ? 9999 : 0, "W", "CrematoryConveyorPad").Concat(CreateConveyor(Location2D.get(59, 8), Location2D.get(0, 8), zone, (int x, int y) => (y > 13 || y < 7 || x > 59) ? 9999 : 0, "W", "CrematoryConveyorPad"));
			CreateConveyor(Location2D.get(75, 0), Location2D.get(60, 16), zone, (int x, int y) => ((x < 75 && y < 16) || x < 60) ? 9999 : 0, "W", "CrematoryConveyorPad").Concat(CreateConveyor(Location2D.get(59, 16), Location2D.get(0, 16), zone, (int x, int y) => (y < 14 || y > 20 || x > 59) ? 9999 : 0, "W", "CrematoryConveyorPad"));
			zone.GetCell(69, 0).AddObject("BeltCurtains");
			zone.GetCell(75, 0).AddObject("BeltCurtains");
			zone.GetCell(0, 8).AddObject("BeltCurtains");
			zone.GetCell(0, 16).AddObject("BeltCurtains");
		}
		List<Cell> list3 = (from cell in zone.GetCells()
			where leftRegion.contains(cell.location) && cell.HasObject("CrematoryConveyorPad")
			select cell).ToList();
		int num = 6;
		for (int num2 = 0; num2 < num; num2++)
		{
			bool flag = false;
			Location2D location2D = list3.RemoveRandomElement().location;
			while (true)
			{
				if (location2D != null && zone.GetCell(location2D) != null)
				{
					zone.GetCell(location2D).walk((Cell c) => c.GetCellFromDirection("N"), delegate(Cell c)
					{
						if (c.HasWall() && !c.IsEdge())
						{
							c.ClearObjectsWithTag("Wall");
							c.AddObject("WalltrapFireCrematory");
							return false;
						}
						return true;
					});
					zone.GetCell(location2D).walk((Cell c) => c.GetCellFromDirection("S"), delegate(Cell c)
					{
						if (c.HasWall())
						{
							c.ClearObjectsWithTag("Wall");
							c.AddObject("WalltrapFireCrematory");
							return false;
						}
						return true;
					});
					zone.GetCell(location2D).walk((Cell c) => c.GetCellFromDirection("E"), delegate(Cell c)
					{
						if (c.HasWall())
						{
							c.ClearObjectsWithTag("Wall");
							c.AddObject("WalltrapFireCrematory");
							return false;
						}
						return true;
					});
					zone.GetCell(location2D).walk((Cell c) => c.GetCellFromDirection("W"), delegate(Cell c)
					{
						if (c.HasWall())
						{
							c.ClearObjectsWithTag("Wall");
							c.AddObject("WalltrapFireCrematory");
							return false;
						}
						return true;
					});
				}
				if (flag)
				{
					break;
				}
				flag = true;
				location2D = Location2D.get(location2D.x, 25 - location2D.y);
			}
		}
		PlaceFans(zone, 0, 40);
		List<Cell> nsArmCells = new List<Cell>();
		List<Cell> ewArmCells = new List<Cell>();
		Predicate<Cell> emptyEnough = delegate(Cell cell)
		{
			if (cell == null)
			{
				return false;
			}
			if (cell.HasObject("CrematoryConveyorPad"))
			{
				return false;
			}
			return (!cell.HasWall()) ? true : false;
		};
		list3.ForEach(delegate(Cell c)
		{
			if (c.Y > 2 && c.Y < 23 && c.AllInDirections(new string[2] { "N", "S" }, 2, emptyEnough))
			{
				nsArmCells.Add(c);
			}
		});
		list3.ForEach(delegate(Cell c)
		{
			if (c.X > 2 && c.X < 38 && c.AllInDirections(new string[2] { "W", "E" }, 2, emptyEnough))
			{
				ewArmCells.Add(c);
			}
		});
		int num3 = 18;
		for (int num4 = 0; num4 < num3; num4++)
		{
			if (nsArmCells.Count == 0 && ewArmCells.Count == 0)
			{
				break;
			}
			int num5 = Stat.Random(1, 2);
			if (num5 == 1 || ewArmCells.Count == 0)
			{
				foreach (Cell placementCell in getPlacementCells(nsArmCells))
				{
					string direction = ((Stat.Random(0, 1) == 0) ? "N" : "S");
					Cell cellFromDirection = placementCell.GetCellFromDirection(direction);
					if (!cellFromDirection.HasObject("FactoryArm") && !cellFromDirection.HasObject("GrabberArm"))
					{
						cellFromDirection.AddObject("FactoryArm").GetPart<FactoryArm>().Direction = direction;
					}
					if (!cellFromDirection.GetCellFromDirection(direction).HasObject("OpenShaft2"))
					{
						cellFromDirection.GetCellFromDirection(direction).AddObject("OpenShaft2");
					}
				}
			}
			else
			{
				if (num5 != 2 && nsArmCells.Count != 0)
				{
					continue;
				}
				foreach (Cell placementCell2 in getPlacementCells(ewArmCells))
				{
					string direction2 = ((Stat.Random(0, 1) == 0) ? "E" : "W");
					Cell cellFromDirection2 = placementCell2.GetCellFromDirection(direction2);
					if (!cellFromDirection2.HasObject("FactoryArm") && !cellFromDirection2.HasObject("GrabberArm"))
					{
						cellFromDirection2.AddObject("FactoryArm").GetPart<FactoryArm>().Direction = direction2;
					}
					if (!cellFromDirection2.GetCellFromDirection(direction2).HasObject("OpenShaft2"))
					{
						cellFromDirection2.GetCellFromDirection(direction2).AddObject("OpenShaft2");
					}
				}
			}
		}
		int num6 = 6;
		for (int num7 = 0; num7 < num6; num7++)
		{
			if (nsArmCells.Count == 0 && ewArmCells.Count == 0)
			{
				break;
			}
			int num8 = Stat.Random(1, 2);
			if (num8 == 1 || ewArmCells.Count == 0)
			{
				foreach (Cell placementCell3 in getPlacementCells(nsArmCells))
				{
					string text = ((Stat.Random(0, 1) == 0) ? "N" : "S");
					Cell cellFromDirection3 = placementCell3.GetCellFromDirection(text);
					if (!cellFromDirection3.HasObject("FactoryArm") && !cellFromDirection3.HasObject("GrabberArm"))
					{
						cellFromDirection3.AddObject("GrabberArm").GetPart<GrabberArm>().Direction = Directions.GetOppositeDirection(text);
					}
				}
			}
			else
			{
				if (num8 != 2 && nsArmCells.Count != 0)
				{
					continue;
				}
				foreach (Cell placementCell4 in getPlacementCells(ewArmCells))
				{
					string text2 = ((Stat.Random(0, 1) == 0) ? "E" : "W");
					Cell cellFromDirection4 = placementCell4.GetCellFromDirection(text2);
					if (!cellFromDirection4.HasObject("FactoryArm") && !cellFromDirection4.HasObject("GrabberArm"))
					{
						cellFromDirection4.AddObject("GrabberArm").GetPart<GrabberArm>().Direction = Directions.GetOppositeDirection(text2);
					}
				}
			}
		}
		InfluenceMap influenceMap = ZoneBuilderSandbox.GenerateInfluenceMap(zone, new List<Point>(), InfluenceMapSeedStrategy.RandomPointFurtherThan4, 100, null, Options.GetOption("OptionDrawInfluenceMaps", "No") == "Yes");
		influenceMap.Regions.ForEach(delegate(InfluenceMapRegion r)
		{
			r.Tags.Add("connected");
		});
		foreach (Cell cell in zone.GetCells(b))
		{
			if (cell.IsEmpty())
			{
				cell.AddObject("AnchorRoomTile");
				cell.PaintTile = (((cell.X + cell.Y) % 2 == 0) ? "Tiles2/sw_floor_diamond_1.bmp" : "Tiles2/sw_floor_diamond_2.bmp");
				cell.PaintTileColor = "&K";
				cell.PaintColorString = "&K";
				cell.PaintRenderString = '\u0004'.ToString();
			}
		}
		if (ZoneTemplateManager.HasTemplates("CryptOfLandlords"))
		{
			ZoneTemplateManager.Templates["CryptOfLandlords"].Execute(zone, influenceMap);
		}
		return true;
	}

	public List<Cell> getPlacementCells(List<Cell> source, int distance = 2)
	{
		if (source.Count == 0)
		{
			return new List<Cell>();
		}
		List<Cell> list = new List<Cell>();
		list.Add(source.RemoveRandomElement());
		int num = Stat.Random(0, 1);
		Cell cell = list[0];
		for (int i = 0; i < distance; i++)
		{
			cell = cell.GetCellFromDirection("E");
			if (cell == null)
			{
				break;
			}
			if (num == 1)
			{
				cell = cell.GetCellFromDirection("E");
			}
			if (cell == null || !source.Contains(cell))
			{
				break;
			}
			source.Remove(cell);
			list.Add(cell);
		}
		cell = list[0];
		for (int j = 0; j < distance; j++)
		{
			cell = cell.GetCellFromDirection("W");
			if (cell == null)
			{
				break;
			}
			if (num == 1)
			{
				cell = cell.GetCellFromDirection("W");
			}
			if (cell == null || !source.Contains(cell))
			{
				break;
			}
			source.Remove(cell);
			list.Add(cell);
		}
		return list;
	}

	public bool BuildSmashers(Zone zone)
	{
		zone.FillHollowBox(new Box(0, 0, zone.Width - 1, zone.Height - 1), "MachineWallHotTubing");
		zone.FillBox(new Box(40, 0, 40, zone.Height - 1), "MachineWallHotTubing", clearFirst: true);
		zone.FillBox(new Box(11, 4, 13, 6), "MachineWallHotTubing");
		zone.FillBox(new Box(0, 0, 79, 4), "MachineWallHotTubing");
		zone.FillBox(new Box(0, 20, 79, 24), "MachineWallHotTubing");
		Grid<Color4> grid = new Grid<Color4>(40, 25);
		grid.fromWFCTemplate("longhalls2");
		grid = grid.mirrorVertical();
		grid = grid.mirrorHorizontal();
		for (int i = 0; i < 40; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (grid.get(i, j) == Color4.black)
				{
					zone.GetCell(i, j).ClearWalls();
					zone.GetCell(i, j).AddObject("MachineWallHotTubing");
				}
			}
		}
		grid = new Grid<Color4>(40, 25);
		grid.fromWFCTemplate("longhalls2");
		grid = grid.mirrorVertical();
		grid = grid.mirrorHorizontal();
		for (int k = 0; k < 40; k++)
		{
			for (int l = 0; l < 25; l++)
			{
				if (grid.get(k, l) == Color4.black)
				{
					zone.GetCell(k + 40, l).ClearWalls();
					zone.GetCell(k + 40, l).AddObject("MachineWallHotTubing");
				}
			}
		}
		Box fullRegion = new Box(1, 1, 79, 23);
		Box rightRegion = new Box(40, 1, 79, 23);
		List<Cell> list = (from cell in zone.GetCells()
			where rightRegion.contains(cell.location) && cell.HasWall()
			select cell).ToList();
		for (int m = 0; m < 20; m++)
		{
			list.RemoveRandomElement()?.ClearWalls();
		}
		List<Cell> list2 = (from cell in zone.GetCells()
			where rightRegion.contains(cell.location) && cell.IsEmpty()
			select cell).ToList();
		for (int n = 0; n < 8; n++)
		{
			list2.RemoveRandomElement()?.AddObject("OpenShaft2");
		}
		new SpindleFootprint().BuildZone(zone);
		zone.ClearBox(new Box(35, 8, 45, 16));
		zone.GetCell(36, 9).AddObject("TombPillarPlacement");
		ZoneBuilderSandbox.EnsureAllVoidsConnected(zone);
		zone.GetCell(79, 7).AddObject("ConveyorDrive");
		zone.GetCell(79, 17).AddObject("ConveyorDrive");
		CreateConveyor(Location2D.get(79, 8), Location2D.get(0, 8), zone, (int x, int y) => (y > 13 || y < 7) ? 9999 : 0, "W", "CrematoryConveyorPad");
		CreateConveyor(Location2D.get(79, 16), Location2D.get(0, 16), zone, (int x, int y) => (y < 14 || y > 20) ? 9999 : 0, "W", "CrematoryConveyorPad");
		zone.GetCell(0, 8).AddObject("BeltCurtains");
		zone.GetCell(0, 16).AddObject("BeltCurtains");
		zone.GetCell(0, 8).AddObject("BeltCurtains");
		zone.GetCell(0, 16).AddObject("BeltCurtains");
		List<Cell> nsArmCells = new List<Cell>();
		List<Cell> ewArmCells = new List<Cell>();
		Predicate<Cell> emptyEnough = delegate(Cell cell)
		{
			if (cell == null)
			{
				return false;
			}
			if (cell.HasObject("CrematoryConveyorPad"))
			{
				return false;
			}
			return (!cell.HasWall()) ? true : false;
		};
		List<Cell> list3 = (from cell in zone.GetCells()
			where fullRegion.contains(cell.location) && cell.HasObject("CrematoryConveyorPad")
			select cell).ToList();
		list3.ForEach(delegate(Cell c)
		{
			if (c.Y > 2 && c.Y < 23 && c.AllInDirections(new string[2] { "N", "S" }, 2, emptyEnough))
			{
				nsArmCells.Add(c);
			}
		});
		list3.ForEach(delegate(Cell c)
		{
			if (c.X > 2 && c.X < 38 && c.AllInDirections(new string[2] { "W", "E" }, 2, emptyEnough))
			{
				ewArmCells.Add(c);
			}
		});
		int num = 18;
		for (int num2 = 0; num2 < num; num2++)
		{
			if (nsArmCells.Count == 0 && ewArmCells.Count == 0)
			{
				break;
			}
			int num3 = Stat.Random(1, 2);
			if (num3 == 1 || ewArmCells.Count == 0)
			{
				foreach (Cell placementCell in getPlacementCells(nsArmCells))
				{
					placementCell.GetCellFromDirection("N").RequireObject("PistonPressElement");
					placementCell.GetCellFromDirection("N").GetCellFromDirection("N").RequireObject("MachineWallHotTubing");
					placementCell.GetCellFromDirection("S").RequireObject("PistonPressElement");
					placementCell.GetCellFromDirection("S").GetCellFromDirection("S").RequireObject("MachineWallHotTubing");
					placementCell.RequireObject("PistonPressController").GetPart<PistonPressController>().Directions = "NS";
				}
			}
			else
			{
				if (num3 != 2 && nsArmCells.Count != 0)
				{
					continue;
				}
				foreach (Cell placementCell2 in getPlacementCells(ewArmCells))
				{
					placementCell2.GetCellFromDirection("E").RequireObject("PistonPressElement");
					placementCell2.GetCellFromDirection("E").GetCellFromDirection("E").RequireObject("MachineWallHotTubing");
					placementCell2.GetCellFromDirection("W").RequireObject("PistonPressElement");
					placementCell2.GetCellFromDirection("W").GetCellFromDirection("W").RequireObject("MachineWallHotTubing");
					placementCell2.RequireObject("PistonPressController");
					placementCell2.RequireObject("PistonPressController").GetPart<PistonPressController>().Directions = "EW";
				}
			}
		}
		PlaceFans(zone, 0, 60);
		InfluenceMap influenceMap = ZoneBuilderSandbox.GenerateInfluenceMap(zone, new List<Point>(), InfluenceMapSeedStrategy.RandomPointFurtherThan4, 100, null, Options.GetOption("OptionDrawInfluenceMaps", "No") == "Yes");
		influenceMap.Regions.ForEach(delegate(InfluenceMapRegion r)
		{
			r.Tags.Add("connected");
		});
		if (ZoneTemplateManager.HasTemplates("CryptOfLandlords"))
		{
			ZoneTemplateManager.Templates["CryptOfLandlords"].Execute(zone, influenceMap);
		}
		return true;
	}

	public bool BuildColumbarium(Zone zone, string room)
	{
		MapBuilder mapBuilder = new MapBuilder();
		if (room == "north")
		{
			mapBuilder.FileName = "ColumbariumN.rpm";
		}
		else if (room == "south")
		{
			mapBuilder.FileName = "ColumbariumS.rpm";
		}
		else
		{
			mapBuilder.FileName = "ColumbariumW.rpm";
		}
		mapBuilder.BuildZone(zone);
		for (int i = 0; i < 5; i++)
		{
			zone.GetEmptyCells().GetRandomElement().AddObject("Urn Porter");
		}
		InfluenceMap influenceMap = ZoneBuilderSandbox.GenerateInfluenceMap(zone, new List<Point>(), InfluenceMapSeedStrategy.RandomPointFurtherThan4, 100, null, Options.GetOption("OptionDrawInfluenceMaps", "No") == "Yes");
		influenceMap.Regions.ForEach(delegate(InfluenceMapRegion r)
		{
			r.Tags.Add("connected");
		});
		if (ZoneTemplateManager.HasTemplates("CryptOfLandlordsUrns"))
		{
			ZoneTemplateManager.Templates["CryptOfLandlordsUrns"].Execute(zone, influenceMap);
		}
		for (int j = 0; j < 80; j++)
		{
			for (int k = 0; k < 25; k++)
			{
				zone.GetCell(j, k).AddObject("AnchorRoomTile");
			}
		}
		return true;
	}

	public void PlaceFans(Zone zone, int minX, int maxX)
	{
		List<Location2D> list = new List<Location2D>();
		List<Location2D> list2 = new List<Location2D>();
		List<Location2D>[] array = new List<Location2D>[2] { list, list2 };
		int[] array2 = new int[2] { -1, 1 };
		int[] array3 = new int[2] { -4, 0 };
		int num = 19;
		int num2 = 0;
		for (int i = minX; i <= maxX - 7; i++)
		{
			for (int j = 0; j < num; j++)
			{
				if (zone.GetCell(i, j).HasObject("CrematoryConveyorPad"))
				{
					list.Add(Location2D.get(i, j));
					break;
				}
			}
			for (int num3 = zone.Height - 1; num3 >= num2; num3--)
			{
				if (zone.GetCell(i, num3).HasObject("CrematoryConveyorPad"))
				{
					list2.Add(Location2D.get(i, num3));
					break;
				}
			}
		}
		List<List<Location2D>> list3 = new List<List<Location2D>>
		{
			new List<Location2D>(),
			new List<Location2D>()
		};
		List<List<Location2D>> list4 = new List<List<Location2D>>
		{
			new List<Location2D>(),
			new List<Location2D>()
		};
		for (int k = 0; k < array.Length; k++)
		{
			int num4 = 0;
			foreach (Location2D item in array[k])
			{
				if (item.x < num4)
				{
					continue;
				}
				Cell cell = zone.GetCell(item);
				while (cell != null && !cell.HasWall() && !cell.HasObjectWithTag("EnsureVoidBlocker"))
				{
					cell = zone.GetCell(cell.X, cell.Y + array2[k]);
					if (cell == null)
					{
						break;
					}
					if ((cell.HasWall() || cell.AnyInDirection("E", 11, (Cell ce) => ce.HasWall())) && !cell.AnyInDirection("E", 11, (Cell ce) => ce.HasObject("CrematoryConveyorPad") || ce.HasObjectWithTag("EnsureVoidBlocker") || ce.HasObject("PistonPressElement")))
					{
						list4[k].Add(cell.location);
						num4 += 11;
						break;
					}
					if ((cell.HasWall() || cell.AnyInDirection("E", 7, (Cell ce) => ce.HasWall())) && !cell.AnyInDirection("E", 7, (Cell ce) => ce.HasObject("CrematoryConveyorPad") || ce.HasObjectWithTag("EnsureVoidBlocker") || ce.HasObject("PistonPressElement")))
					{
						list3[k].Add(cell.location);
						num4 += 7;
						break;
					}
					if (cell.HasWall() || cell.AnyInDirection("E", 11, (Cell ce) => ce.HasWall() || ce.HasObject("PistonPressElement") || ce.HasObjectWithTag("EnsureVoidBlocker") || ce.HasObject("CrematoryConveyorPad")))
					{
						break;
					}
				}
			}
		}
		using (List<Location2D>.Enumerator enumerator = list4[0].ShuffleInPlace().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				Location2D current2 = enumerator.Current;
				ZoneBuilderSandbox.PlacePrefab(zone, "preset_tile_chunks/IndustrialFanRow_9_S.rpm", zone.GetCell(current2.x, current2.y + array3[0]), 0, null, delegate(Cell c)
				{
					c.ClearWalls();
				});
			}
		}
		using (List<Location2D>.Enumerator enumerator = list4[1].ShuffleInPlace().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				Location2D current3 = enumerator.Current;
				ZoneBuilderSandbox.PlacePrefab(zone, "preset_tile_chunks/IndustrialFanRow_9_N.rpm", zone.GetCell(current3.x, current3.y + array3[1]), 0, null, delegate(Cell c)
				{
					c.ClearWalls();
				});
			}
		}
		using (List<Location2D>.Enumerator enumerator = list3[0].ShuffleInPlace().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				Location2D current4 = enumerator.Current;
				ZoneBuilderSandbox.PlacePrefab(zone, "preset_tile_chunks/IndustrialFanRow_9_S.rpm", zone.GetCell(current4.x, current4.y + array3[0]), 0, null, delegate(Cell c)
				{
					c.ClearWalls();
				});
			}
		}
		using List<Location2D>.Enumerator enumerator = list3[1].ShuffleInPlace().GetEnumerator();
		if (enumerator.MoveNext())
		{
			Location2D current5 = enumerator.Current;
			ZoneBuilderSandbox.PlacePrefab(zone, "preset_tile_chunks/IndustrialFanRow_9_N.rpm", zone.GetCell(current5.x, current5.y + array3[1]), 0, null, delegate(Cell c)
			{
				c.ClearWalls();
			});
		}
	}

	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).AddObject("ConcreteFloor");
		The.Game.RequireSystem(() => new CryptOfLandlordsAnchorSystem());
		if (Z.Y == 0 || Z.Y == 2)
		{
			if (Z.X == 2 && !BuildFlames(Z))
			{
				return false;
			}
			if (Z.X == 1 && !BuildSmashers(Z))
			{
				return false;
			}
			if (Z.X == 0 && Z.Y == 0 && !BuildColumbarium(Z, "north"))
			{
				return false;
			}
			if (Z.X == 0 && Z.Y == 2 && !BuildColumbarium(Z, "south"))
			{
				return false;
			}
		}
		else if (Z.X == 0 && !BuildColumbarium(Z, "west"))
		{
			return false;
		}
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				if ((j > 2 && j < 22) || (Z.X == 2 && Z.Y == 1 && i >= 67 && i <= 77 && j >= 22 && j <= 24) || (Z.X == 2 && Z.Y > 0 && i >= 67 && i <= 76 && j >= 0 && j <= 2) || (Z.X == 2 && Z.Y < 2 && i >= 67 && i <= 76 && j >= 22 && j <= 24))
				{
					continue;
				}
				Cell cell = Z.GetCell(i, j);
				if (cell.HasObject("MachineWallHotTubing"))
				{
					while (cell.HasObject("MachineWallHotTubing"))
					{
						cell.FindObject("MachineWallHotTubing").Obliterate();
					}
					cell.AddObject("EbonFulcrete");
				}
			}
		}
		Z.GetCell(0, 0).AddObject("Finish_TombOfTheEaters_EnterTheTombOfTheEaters");
		new ChildrenOfTheTomb().BuildZone(Z);
		return true;
	}
}
