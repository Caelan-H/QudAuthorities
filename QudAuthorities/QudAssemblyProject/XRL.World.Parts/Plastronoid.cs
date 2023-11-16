using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class Plastronoid : IPart
{
	public string FlocksWith = "Plastronoid";

	public int MinDistance = 1;

	public int MaxDistance = 1;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIBored");
		Object.RegisterPartEvent(this, "TakingAction");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		Brain pBrain;
		if (!ParentObject.IsPlayer() && ParentObject.pPhysics.CurrentCell != null && !ParentObject.pPhysics.CurrentCell.ParentZone.IsWorldMap() && (E.ID == "AIBored" || E.ID == "TakingAction") && (!(E.ID == "TakingAction") || !50.in100()))
		{
			pBrain = ParentObject.pBrain;
			if (ParentObject.CurrentCell != null)
			{
				if (ParentObject.CurrentCell.X % 2 != 1)
				{
					goto IL_014b;
				}
				Cell cellFromDirection = ParentObject.CurrentCell.GetCellFromDirection("W");
				if (cellFromDirection != null && cellFromDirection.IsEmpty())
				{
					pBrain.PushGoal(new Step("W"));
				}
				else
				{
					if (!ParentObject.CurrentCell.GetCellFromDirection("E").IsEmpty())
					{
						pBrain.PushGoal(new Step(Directions.GetRandomDirection()));
						goto IL_014b;
					}
					pBrain.PushGoal(new Step("E"));
				}
			}
		}
		goto IL_0420;
		IL_014b:
		if (ParentObject.pPhysics.CurrentCell.Y % 2 != 1)
		{
			goto IL_01fe;
		}
		Cell cellFromDirection2 = ParentObject.CurrentCell.GetCellFromDirection("N");
		if (cellFromDirection2 != null && cellFromDirection2.IsEmpty())
		{
			pBrain.PushGoal(new Step("N"));
		}
		else
		{
			Cell cellFromDirection3 = ParentObject.CurrentCell.GetCellFromDirection("S");
			if (cellFromDirection3 == null || !cellFromDirection3.IsEmpty())
			{
				pBrain.PushGoal(new Step(Directions.GetRandomDirection()));
				goto IL_01fe;
			}
			pBrain.PushGoal(new Step("S"));
		}
		goto IL_0420;
		IL_0420:
		return base.FireEvent(E);
		IL_01fe:
		if (ParentObject.CurrentCell != null)
		{
			List<GameObject> list = Event.NewGameObjectList();
			foreach (GameObject item in ParentObject.CurrentZone.FastCombatSquareVisibility(ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, 8, ParentObject))
			{
				if (item != ParentObject && item.Blueprint == FlocksWith && item.CurrentCell != null)
				{
					if (item.CurrentCell.PathDistanceTo(ParentObject.pPhysics.CurrentCell) <= MinDistance)
					{
						pBrain.Think("I'm too close to" + FlocksWith + ".");
						pBrain.PushGoal(new Step(Directions.GetRandomDirection()));
						pBrain.PushGoal(new Step(Directions.GetRandomDirection()));
						return true;
					}
					list.Add(item);
				}
			}
			if (list.Count > 0)
			{
				int num = 0;
				int num2 = 0;
				foreach (GameObject item2 in list)
				{
					num += item2.CurrentCell.X;
					num2 += item2.CurrentCell.Y;
				}
				num /= list.Count;
				num2 /= list.Count;
				Cell cell = ParentObject.CurrentCell.ParentZone.GetCell(num, num2);
				if (pBrain.Target != null && pBrain.Target.CurrentCell != null)
				{
					cell = pBrain.Target.pPhysics.CurrentCell;
				}
				if (cell.PathDistanceTo(ParentObject.CurrentCell) > MaxDistance)
				{
					pBrain.PushGoal(new MoveTo(cell, careful: false, overridesCombat: false, 0, wandering: false, global: false, juggernaut: false, 3));
				}
			}
			else
			{
				pBrain.Think("I can't find a creature to flock with.");
			}
		}
		goto IL_0420;
	}
}
