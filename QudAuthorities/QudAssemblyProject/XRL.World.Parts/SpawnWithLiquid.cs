using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SpawnWithLiquid : IPart
{
	public bool spawned;

	public string LiquidObject = "SaltyWaterPuddle";

	public int AdjacentPoolChance = 25;

	public override bool SameAs(IPart p)
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
			if (spawned)
			{
				return true;
			}
			spawned = true;
			Cell cell = ParentObject.GetCurrentCell();
			if (cell != null && (!cell.HasObjectWithPart("LiquidVolume") || cell.GetFirstObjectWithPart("LiquidVolume").LiquidVolume.MaxVolume >= 0))
			{
				cell.AddObject(LiquidObject);
				foreach (Cell localCardinalAdjacentCell in cell.GetLocalCardinalAdjacentCells())
				{
					if (localCardinalAdjacentCell != null && Stat.Random(1, 100) <= AdjacentPoolChance)
					{
						localCardinalAdjacentCell.AddObject(LiquidObject);
					}
				}
			}
		}
		return true;
	}
}
