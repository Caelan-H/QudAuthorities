using System;
using System.Linq;
using Genkit;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

public class RecorporealizationBoothSpawnerBuilder : ZoneBuilderSandbox
{
	public void BuildZone(Zone zone)
	{
		InfluenceMapRegion influenceMapRegion = ZoneBuilderSandbox.GenerateInfluenceMap(zone, null, InfluenceMapSeedStrategy.LargestRegion, 200).Regions.Where((InfluenceMapRegion r) => r.maxRect.Width >= 12 && r.maxRect.Height >= 10).FirstOrDefault();
		Cell cell = null;
		if (influenceMapRegion != null)
		{
			cell = zone.GetCell(influenceMapRegion.maxRect.x1, influenceMapRegion.maxRect.y1);
		}
		if (cell == null)
		{
			cell = (from c in zone.GetCells()
				where c.X < 69 && c.Y < 16
				select c).GetRandomElement();
		}
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				zone.GetCell(cell.X + i, cell.Y + j)?.Clear(null, Important: false, Combat: true);
			}
		}
		cell.AddObject("RemortingNook_11X19");
	}
}
[Serializable]
public class RecorporealizationBoothSpawner : IPart
{
	public string Group = "A";

	public int Period = 1;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			Cell parameter = E.GetParameter<Cell>("Cell");
			new RecorporealizationBoothSpawnerBuilder().BuildZone(parameter.ParentZone);
			ParentObject.Destroy();
		}
		return base.FireEvent(E);
	}
}
