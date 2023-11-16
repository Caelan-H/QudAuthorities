using System;
using System.Collections.Generic;
using XRL.World.AI.Pathfinding;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class MoveToZone : IMovementGoal
{
	private GlobalLocation Target;

	private int Tries;

	public bool OverridesCombat;

	public MoveToZone()
	{
	}

	public MoveToZone(GlobalLocation Target, bool OverridesCombat = false)
		: this()
	{
		this.Target = Target;
		this.OverridesCombat = OverridesCombat;
	}

	public MoveToZone(string ZoneID, bool OverridesCombat = false, int MaxTurns = -1)
		: this(new GlobalLocation(), OverridesCombat)
	{
		Target.ZoneID = ZoneID;
	}

	public override bool Finished()
	{
		return base.ParentObject.InZone(Target.ZoneID);
	}

	public override void TakeAction()
	{
		Tries++;
		if (!ParentBrain.isMobile())
		{
			FailToParent();
			return;
		}
		if (base.ParentObject.CurrentZone.ZoneID == null)
		{
			Pop();
			return;
		}
		if (Target == null)
		{
			Pop();
			return;
		}
		if (base.ParentObject.InZone(Target.ZoneID))
		{
			Pop();
			return;
		}
		base.ParentObject.UseEnergy(1000);
		int num = Target.ParasangX * 3 + Target.ZoneX;
		int num2 = Target.ParasangY * 3 + Target.ZoneY;
		int zoneZ = Target.ZoneZ;
		List<string> list = new List<string>();
		if (base.ParentObject.CurrentZone.wX * 3 + base.ParentObject.CurrentZone.X < num)
		{
			list.Add("E");
		}
		if (base.ParentObject.CurrentZone.wX * 3 + base.ParentObject.CurrentZone.X > num)
		{
			list.Add("W");
		}
		if (base.ParentObject.CurrentZone.wY * 3 + base.ParentObject.CurrentZone.Y < num2)
		{
			list.Add("S");
		}
		if (base.ParentObject.CurrentZone.wY * 3 + base.ParentObject.CurrentZone.Y > num2)
		{
			list.Add("N");
		}
		if (base.ParentObject.CurrentZone.Z < zoneZ)
		{
			GameObject gameObject = base.ParentObject.CurrentZone.FindClosestObjectWithPart(base.ParentObject, "StairsDown");
			if (gameObject != null)
			{
				base.ParentObject.pBrain.PushGoal(new Step("D", careful: false, OverridesCombat, wandering: false, juggernaut: false, null, allowUnbuilt: true));
				base.ParentObject.pBrain.PushGoal(new MoveTo(gameObject));
				return;
			}
		}
		if (base.ParentObject.CurrentZone.Z > zoneZ)
		{
			GameObject gameObject2 = base.ParentObject.CurrentZone.FindClosestObjectWithPart(base.ParentObject, "StairsUp");
			if (gameObject2 != null)
			{
				base.ParentObject.pBrain.PushGoal(new Step("U", careful: false, OverridesCombat, wandering: false, juggernaut: false, null, allowUnbuilt: true));
				base.ParentObject.pBrain.PushGoal(new MoveTo(gameObject2));
				return;
			}
		}
		if (base.ParentObject.CurrentZone.Z != zoneZ || list.Count == 0)
		{
			list.Add("N");
			list.Add("S");
			list.Add("E");
			list.Add("W");
		}
		string text = list[new Random().Next(0, list.Count - 1)];
		List<Cell> list2 = new List<Cell>();
		if (text == "N")
		{
			for (int i = 0; i < base.CurrentZone.Width; i++)
			{
				list2.Add(base.CurrentZone.GetCell(i, 0));
			}
		}
		if (text == "S")
		{
			for (int j = 0; j < base.CurrentZone.Width; j++)
			{
				list2.Add(base.CurrentZone.GetCell(j, base.CurrentZone.Height - 1));
			}
		}
		if (text == "W")
		{
			for (int k = 0; k < base.CurrentZone.Height; k++)
			{
				list2.Add(base.CurrentZone.GetCell(0, k));
			}
		}
		if (text == "E")
		{
			for (int l = 0; l < base.CurrentZone.Height; l++)
			{
				list2.Add(base.CurrentZone.GetCell(base.CurrentZone.Width - 1, l));
			}
		}
		list2.ShuffleInPlace();
		for (int m = 0; m < list2.Count; m++)
		{
			if (!list2[m].IsReachable())
			{
				continue;
			}
			FindPath findPath = new FindPath(base.ParentObject.CurrentZone.ZoneID, base.ParentObject.CurrentCell.X, base.ParentObject.CurrentCell.Y, base.ParentObject.CurrentZone.ZoneID, list2[m].X, list2[m].Y, PathGlobal: true, PathUnlimited: false, base.ParentObject);
			if (!findPath.bFound)
			{
				break;
			}
			findPath.Directions.Reverse();
			PushChildGoal(new Step(text, careful: false, overridesCombat: false, wandering: false, juggernaut: false, null, allowUnbuilt: true));
			{
				foreach (string direction in findPath.Directions)
				{
					PushChildGoal(new Step(direction, careful: false, OverridesCombat, wandering: false, juggernaut: false, null, allowUnbuilt: true));
				}
				break;
			}
		}
	}
}
