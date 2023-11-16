using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class CatacombsMapTemplate
{
	public Grid<Color4> grid;

	public InfluenceMap regions;

	public List<Rect2D> maskingAreas;

	public List<string> stairZones = new List<string>();

	public List<string> properties = new List<string>();

	public CatacombsMapTemplate(int seed)
	{
		Generate(seed);
	}

	public void Generate(int seed)
	{
		int num = 5;
		Random r = new Random(seed);
		Grid<Color4> sourcegrid = new Grid<Color4>(160, 50);
		sourcegrid.fromWFCTemplate("crypt", 3, seed);
		grid = new Grid<Color4>(240, 75);
		grid.transform((int x, int y, Color4 c) => sourcegrid.get((int)((float)x / 1.5f), (int)((float)y / 1.5f)));
		stairZones.Add("JoppaWorld.53.3.2.0.11");
		stairZones.Add("JoppaWorld.53.3.2.2.11");
		for (int i = 0; i < num; i++)
		{
			stairZones.Add(CatacombsAnchorSystem.catacombsAnchorZones.Where((string z) => !stairZones.Contains(z) && CatacombsAnchorSystem.catacombsAllowedStairsZones.Contains(z)).GetRandomElement(r));
		}
		properties.Add("k-Goninon:" + CatacombsAnchorSystem.catacombsAnchorZones.GetRandomElement(r));
		maskingAreas = new List<Rect2D>();
		maskingAreas.Add(new Rect2D(80, 25, 159, 49));
		maskingAreas.Add(new Rect2D(0, 0, 239, 0));
		maskingAreas.Add(new Rect2D(0, 74, 239, 74));
		maskingAreas.Add(new Rect2D(0, 0, 0, 74));
		maskingAreas.Add(new Rect2D(239, 0, 239, 74));
		foreach (Rect2D maskingArea in maskingAreas)
		{
			grid.box(maskingArea, () => Color4.black);
		}
		int num2 = 160;
		int num3 = 0;
		grid.set(73 + num2, 7 + num3, Color4.white);
		grid.set(74 + num2, 7 + num3, Color4.white);
		grid.set(75 + num2, 7 + num3, Color4.white);
		grid.set(73 + num2, 19 + num3, Color4.white);
		grid.set(74 + num2, 19 + num3, Color4.white);
		grid.set(75 + num2, 19 + num3, Color4.white);
		num2 = 160;
		num3 = 50;
		grid.set(73 + num2, 6 + num3, Color4.white);
		grid.set(74 + num2, 6 + num3, Color4.white);
		grid.set(75 + num2, 6 + num3, Color4.white);
		grid.set(73 + num2, 18 + num3, Color4.white);
		grid.set(74 + num2, 18 + num3, Color4.white);
		grid.set(75 + num2, 18 + num3, Color4.white);
		regions = grid.regionalize(delegate(int x, int y, Color4 c)
		{
			if (maskingAreas.Any((Rect2D b) => b.Contains(x, y)))
			{
				return int.MaxValue;
			}
			return (c == Color4.black) ? 21474836 : 0;
		});
		List<InfluenceMapRegion> list = new List<InfluenceMapRegion>();
		Pathfinder pathfinder = new Pathfinder(240, 75);
		regions.Regions.Sort((InfluenceMapRegion a, InfluenceMapRegion b) => a.Center.SquareDistance(regions.Regions.First().Center).CompareTo(b.Center.SquareDistance(regions.Regions.First().Center)));
		foreach (InfluenceMapRegion region in regions.Regions)
		{
			if (list.Count > 0)
			{
				list.Sort((InfluenceMapRegion a, InfluenceMapRegion b) => a.Center.SquareDistance(region.Center).CompareTo(b.Center.SquareDistance(region.Center)));
				InfluenceMapRegion influenceMapRegion = list.First();
				Location2D randomElement = region.Cells.GetRandomElement(r);
				Location2D randomElement2 = influenceMapRegion.Cells.GetRandomElement(r);
				pathfinder.setWeightsFromGrid(grid, delegate(int x, int y, Color4 c)
				{
					if (maskingAreas.Any((Rect2D b) => b.Contains(x, y)))
					{
						return int.MaxValue;
					}
					return (c == Color4.black) ? 100 : 0;
				});
				if (!pathfinder.FindPath(randomElement, randomElement2, bDisplay: false, bOrdinalDirectionsOnly: true, 24300))
				{
					continue;
				}
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					grid.set(step.X, step.Y, Color4.white);
				}
				list.Add(region);
			}
			else
			{
				list.Add(region);
			}
		}
		regions.RecalculateCostOnly();
	}
}
