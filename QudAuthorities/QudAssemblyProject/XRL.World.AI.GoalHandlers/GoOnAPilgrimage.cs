using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class GoOnAPilgrimage : GoalHandler
{
	public static int TargetWx = 5;

	public static int TargetWy = 2;

	public static int TargetXx = 1;

	public static int TargetYx = 1;

	public static int TargetZx = 10;

	public string TargetObject = "StiltWell";

	public string TargetZoneID = "JoppaWorld.5.2.1.1.10";

	public string TargetEntranceZoneID = "JoppaWorld.5.2.1.2.10";

	public GoOnAPilgrimage()
	{
	}

	public GoOnAPilgrimage(int Wx, int Wy, int Xx, int Yx, int Zx, string targetObject, string zoneID, string entranceZoneID)
	{
		TargetWx = Wx;
		TargetWy = Wy;
		TargetXx = Xx;
		TargetYx = Yx;
		TargetZx = Zx;
		TargetObject = targetObject;
		TargetZoneID = zoneID;
		TargetEntranceZoneID = entranceZoneID;
	}

	public override bool Finished()
	{
		return false;
	}

	public override void Create()
	{
	}

	public void MoveToTargetZone()
	{
		if (base.CurrentZone.ZoneID == TargetEntranceZoneID)
		{
			if (base.CurrentCell.X == 38 && base.CurrentCell.Y == 0)
			{
				ParentBrain.PushGoal(new Step("N"));
			}
			else
			{
				ParentBrain.PushGoal(new MoveTo(TargetEntranceZoneID, 37 + Stat.Random(0, 3), 0));
			}
		}
		else
		{
			ParentBrain.PushGoal(new MoveToGlobal(TargetEntranceZoneID, 37 + Stat.Random(0, 3), 0));
		}
	}

	public void MoveToWell()
	{
		if (TargetObject == null)
		{
			TargetObject = base.CurrentZone.GetObjectsWithPart("Brain").GetRandomElement().GetBlueprint()
				.Name;
		}
		if (base.AdjacentObjects.Contains(TargetObject))
		{
			base.ParentObject.GetPart<AIPilgrim>().FoundTarget = true;
			Pop();
			ParentBrain.PushGoal(new WanderRandomly(6));
			return;
		}
		GameObject gameObject = base.CurrentZone.FindObjectExcludingSelf(TargetObject, base.ParentObject);
		if (gameObject != null)
		{
			List<Cell> localAdjacentCells = gameObject.pPhysics.CurrentCell.GetLocalAdjacentCells();
			Algorithms.RandomShuffleInPlace(localAdjacentCells, Stat.Rand);
			for (int i = 0; i < localAdjacentCells.Count; i++)
			{
				if (localAdjacentCells[i].IsEmpty())
				{
					ParentBrain.PushGoal(new MoveTo(base.CurrentZone.ZoneID, localAdjacentCells[i].X, localAdjacentCells[i].Y));
					return;
				}
			}
			ParentBrain.PushGoal(new WanderRandomly(6));
		}
		else
		{
			FailToParent();
		}
	}

	public override void TakeAction()
	{
		if (base.ParentObject.InZone(TargetZoneID))
		{
			MoveToWell();
		}
		else
		{
			MoveToTargetZone();
		}
	}
}
