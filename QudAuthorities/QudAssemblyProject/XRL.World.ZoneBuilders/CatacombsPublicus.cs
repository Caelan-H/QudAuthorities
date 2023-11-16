using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using UnityEngine;
using XRL.EditorFormats.Map;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.ZoneBuilders;

public class CatacombsPublicus : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		The.Game.RequireSystem(() => new CatacombsAnchorSystem());
		CatacombsMapTemplate catacombsMapTemplate = The.Game.RequireGameState("CatacombsMapTemplate", () => new CatacombsMapTemplate(GetSeedValue("CatacombsMapTemplate")));
		Z.GetCell(0, 0).AddObject("ConcreteFloor");
		int xo = Z.X * 80;
		int yo = Z.Y * 25;
		for (int x = 0; x < Z.Width; x++)
		{
			int y;
			for (y = 0; y < Z.Height; y++)
			{
				Color4 color = catacombsMapTemplate.grid.get(x + xo, y + yo);
				if (catacombsMapTemplate.maskingAreas.Any((Rect2D r) => r.Contains(x + xo, y + yo)))
				{
					Z.GetCell(x, y).AddObject("EbonFulcrete");
				}
				else if (color == Color4.black)
				{
					Z.GetCell(x, y).AddObject("CatacombWall");
				}
			}
		}
		new SpindleFootprint().BuildZone(Z, SpindleFootprint.FootprintMode.Foundation);
		Z.ClearBox(new Box(35, 8, 45, 16));
		Z.GetCell(36, 9).AddObject("TombPillarPlacement");
		Rect2D zoneBounds = new Rect2D(xo, yo, xo + 79, yo + 24);
		IEnumerable<InfluenceMapRegion> enumerable = catacombsMapTemplate.regions.Regions.Where((InfluenceMapRegion r) => r.BoundingBox.overlaps(zoneBounds));
		List<Location2D> lampSpots = new List<Location2D>();
		Predicate<Cell> walker = delegate(Cell c)
		{
			if (c.HasObjectWithPart("LightSource"))
			{
				return false;
			}
			if (c.HasObjectWithTag("EnsureVoidBlocker"))
			{
				return false;
			}
			if (c.HasWall() && !c.IsEdge())
			{
				c.ClearObjectsWithTag("Wall");
				c.AddObject("CatacombLight");
				lampSpots.Add(c.location);
				return false;
			}
			return true;
		};
		foreach (InfluenceMapRegion item in enumerable)
		{
			Location2D location2D = item.Cells.Where((Location2D c) => c.x % 3 == 0 && c.y % 3 == 0 && zoneBounds.Contains(c.x, c.y)).GetRandomElement();
			if (location2D != null)
			{
				location2D = Location2D.get(location2D.x - xo, location2D.y - yo);
			}
			if (!(location2D != null) || Z.GetCell(location2D) == null || Z.GetCell(location2D).HasObjectWithTag("EnsureVoidBlocker"))
			{
				continue;
			}
			string[] cardinalDirectionList = Directions.CardinalDirectionList;
			foreach (string direction in cardinalDirectionList)
			{
				Z.GetCell(location2D).walk((Cell c) => c.GetCellFromDirection(direction), walker);
			}
		}
		Pathfinder pathfinder = Z.getPathfinder();
		if (Z.X == 0 && Z.Y == 2)
		{
			for (int j = 44; j <= 45; j++)
			{
				Cell[] emptyCellsShuffled = Z.GetEmptyCellsShuffled();
				foreach (Cell cell in emptyCellsShuffled)
				{
					if (cell.Y >= 20)
					{
						continue;
					}
					Location2D location = cell.location;
					Location2D location2 = Z.GetCell(j, 24).location;
					if (!pathfinder.FindPath(location2, location))
					{
						continue;
					}
					foreach (PathfinderNode step in pathfinder.Steps)
					{
						Z.GetCell(step.pos).ClearObjectsWithTag("Wall");
					}
					break;
				}
			}
		}
		InfluenceMap influenceMap = ZoneBuilderSandbox.GenerateInfluenceMap(Z, new List<Point>(), InfluenceMapSeedStrategy.RandomPointFurtherThan4, 100, lampSpots, Options.GetOption("OptionDrawInfluenceMaps", "No") == "Yes");
		influenceMap.Regions.ForEach(delegate(InfluenceMapRegion r)
		{
			r.Tags.Add("connected");
		});
		List<InfluenceMapRegion> list = influenceMap.Regions.Where((InfluenceMapRegion r) => r.Cells.Count > 0 && r != null).ToList();
		list.Sort(delegate(InfluenceMapRegion a, InfluenceMapRegion b)
		{
			if (a.Cells.Count >= 16 && b.Cells.Count < 16)
			{
				return -1;
			}
			if (b.Cells.Count >= 16 && a.Cells.Count < 16)
			{
				return 1;
			}
			if (a.AdjacentRegions.Count == b.AdjacentRegions.Count)
			{
				return 0;
			}
			if (a.AdjacentRegions.Count == 0)
			{
				return 1;
			}
			if (b.AdjacentRegions.Count == 0)
			{
				return -1;
			}
			if (a.AdjacentRegions.Count == 1)
			{
				return -1;
			}
			return (b.AdjacentRegions.Count == 1) ? 1 : 0;
		});
		List<InfluenceMapRegion> list2 = new List<InfluenceMapRegion>();
		for (int k = 0; k < Stat.Random(1, 2); k++)
		{
			list2.Add(list[0]);
			list.RemoveAt(0);
		}
		list2.RemoveAll((InfluenceMapRegion a) => a == null);
		ZoneTemplateManager.Templates["CatacombsPublicus"].Execute(Z, influenceMap);
		int num = 20;
		foreach (InfluenceMapRegion item2 in list2)
		{
			item2.Cells.ForEach(delegate(Location2D loc)
			{
				Z.GetCell(loc).AddObject("AnchorRoomTile");
				Z.GetCell(loc).PaintTile = (((loc.x + loc.y) % 2 == 0) ? "Tiles2/sw_floor_diamond_1.bmp" : "Tiles2/sw_floor_diamond_2.bmp");
				Z.GetCell(loc).PaintTileColor = "&K";
				Z.GetCell(loc).PaintColorString = "&K";
				Z.GetCell(loc).PaintRenderString = '\u0004'.ToString();
			});
			if (Stat.Random(1, 100) <= num)
			{
				item2.Cells.ForEach(delegate(Location2D loc)
				{
					Z.GetCell(loc).AddObject("MopangoHideoutTile");
				});
				ZoneBuilderSandbox.PlacePopulationInRegion(Z, item2, "MopangoHideout");
			}
			else if (PopulationManager.HasPopulation("CatacombsAnchorRoom"))
			{
				ZoneBuilderSandbox.PlacePopulationInRegion(Z, item2, "CatacombsAnchorRoom");
			}
		}
		if (list2.Count == 0)
		{
			Debug.LogError("no anchor room regions!");
		}
		if (Z.X == 0 && Z.Y == 0)
		{
			MapFile mapFile = MapFile.LoadWithMods("MopangoSettlement.rpm");
			for (int l = 0; l < 35; l++)
			{
				for (int m = 0; m < 13; m++)
				{
					Cell cell2 = Z.GetCell(l, m);
					cell2.Clear();
					foreach (MapFileObjectBlueprint @object in mapFile.Cells[l, m].Objects)
					{
						cell2.AddObject(@object.Name);
					}
					if (!cell2.HasWall())
					{
						cell2.AddObject("AnchorRoomTile");
						cell2.AddObject("MopangoHideoutTile");
						cell2.PaintTile = (((cell2.X + cell2.Y) % 2 == 0) ? "Tiles2/sw_floor_diamond_1.bmp" : "Tiles2/sw_floor_diamond_2.bmp");
						cell2.PaintTileColor = "&K";
						cell2.PaintColorString = "&K";
						cell2.PaintRenderString = "\u0004";
					}
				}
			}
			ZoneBuilderSandbox.EnsureAllVoidsConnected(Z);
		}
		if (Z.ZoneID == "JoppaWorld.53.3.2.2.11")
		{
			Cell cell3 = Z.GetCell(41, 12);
			cell3.ClearWalls();
			EnsureCellReachable(Z, cell3);
			cell3.AddObject("StairsUp");
		}
		else if (Z.ZoneID == "JoppaWorld.53.3.2.0.11")
		{
			Cell cell4 = Z.GetCell(41, 13);
			cell4.ClearWalls();
			EnsureCellReachable(Z, cell4);
			cell4.AddObject("StairsUp");
		}
		else
		{
			Z.GetEmptyCells().GetRandomElement().AddObject("Catacombs Exit Teleporter");
		}
		if (catacombsMapTemplate.properties.Contains("k-Goninon:" + Z.ZoneID))
		{
			(from c in Z.GetEmptyCells()
				where !c.HasObject("MopangoHideoutTile")
				select c).GetRandomElement().AddObject("k-Goninon");
		}
		Z.GetCell(0, 0).AddObject("Finish_TombOfTheEaters_EnterTheTombOfTheEaters");
		new ChildrenOfTheTomb().BuildZone(Z);
		return true;
	}
}
