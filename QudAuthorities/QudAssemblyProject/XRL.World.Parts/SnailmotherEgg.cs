using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class SnailmotherEgg : IPart
{
	public int SpawnTurns = int.MinValue;

	public string SpawnBlueprint = "Ickslug";

	public string ReplaceBlueprint = "BrokenSnailmotherEgg";

	public string SpawnVerb = "hatch";

	public GameObject SpawnedBy;

	public bool AdjustAttitude;

	public bool SlimesplatterOnSpawn = true;

	public string MaxSpawn = "5";

	public string SpawnTime = "18-22";

	public int SpawnChance = 50;

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return;
		}
		if (SpawnTurns == int.MinValue)
		{
			SpawnTurns = SpawnTime.RollCached();
		}
		SpawnTurns--;
		if (SpawnTurns > 0)
		{
			return;
		}
		Cell cell2 = ParentObject.CurrentCell;
		List<Cell> adjacentCells = cell2.GetAdjacentCells();
		adjacentCells.Add(cell2);
		int num = 0;
		int num2 = MaxSpawn.RollCached();
		foreach (Cell item in adjacentCells)
		{
			if (num >= num2)
			{
				break;
			}
			if (item.IsEmpty() && SpawnChance.in100())
			{
				GameObject gameObject = GameObject.create(SpawnBlueprint);
				if (AdjustAttitude && GameObject.validate(ref SpawnedBy))
				{
					gameObject.TakeOnAttitudesOf(SpawnedBy);
				}
				gameObject.MakeActive();
				item.AddObject(gameObject);
			}
		}
		DidX(SpawnVerb);
		if (!string.IsNullOrEmpty(ReplaceBlueprint))
		{
			cell.AddObject(ReplaceBlueprint);
		}
		if (SlimesplatterOnSpawn)
		{
			ParentObject.Slimesplatter(bSelfsplatter: false);
		}
		ParentObject.Destroy();
	}
}
