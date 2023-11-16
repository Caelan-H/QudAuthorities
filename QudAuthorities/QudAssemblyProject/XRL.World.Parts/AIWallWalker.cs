using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class AIWallWalker : IPart
{
	public int chanceToWander = 25;

	private static List<Cell> cells = new List<Cell>();

	public bool WalkToNearbyWall()
	{
		cells.Clear();
		foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
		{
			if (!adjacentCell.HasObjectWithTag("Wall"))
			{
				continue;
			}
			bool flag = false;
			foreach (Cell adjacentCell2 in adjacentCell.GetAdjacentCells())
			{
				if (adjacentCell2.IsPassable())
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				cells.Add(adjacentCell);
			}
		}
		if (cells.Count > 0)
		{
			Cell randomElement = cells.GetRandomElement();
			ParentObject.DirectMoveTo(randomElement, 1000);
			cells.Clear();
			return true;
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIBoredEvent.ID)
		{
			return ID == CanJoinPartyLeaderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (chanceToWander.in100() && WalkToNearbyWall())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanJoinPartyLeaderEvent E)
	{
		if (!E.TargetCell.HasWalkableWallFor(ParentObject) && !E.TargetCell.HasObject("StairsDown") && !E.TargetCell.HasObject("StairsUp"))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIFailCombatPathfind");
		Object.RegisterPartEvent(this, "BeforeTakeAction");
		Object.RegisterPartEvent(this, "KeepZoneCachedForPlayerJoin");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeTakeAction")
		{
			if (ParentObject.CurrentCell != null && !ParentObject.CurrentCell.HasObjectWithTag("Wall") && WalkToNearbyWall())
			{
				return false;
			}
		}
		else if (E.ID == "AIFailCombatPathfind")
		{
			GameObject obj = E.GetGameObjectParameter("Target");
			if (!GameObject.validate(ref obj))
			{
				return true;
			}
			if (ParentObject.CurrentCell == null)
			{
				return true;
			}
			int num = int.MaxValue;
			Cell cell = null;
			foreach (Cell adjacentCell in ParentObject.pPhysics.CurrentCell.GetAdjacentCells())
			{
				if (!adjacentCell.HasObjectWithTag("Wall"))
				{
					continue;
				}
				bool flag = false;
				foreach (Cell adjacentCell2 in adjacentCell.GetAdjacentCells())
				{
					if (adjacentCell2.IsPassable())
					{
						flag = true;
						break;
					}
				}
				if (flag && adjacentCell.DistanceTo(obj) < num)
				{
					cell = adjacentCell;
					num = adjacentCell.DistanceTo(obj);
				}
			}
			if (cell != null && cell.DistanceTo(obj) < ParentObject.pPhysics.CurrentCell.DistanceTo(obj))
			{
				ParentObject.DirectMoveTo(cell);
				ParentObject.UseEnergy(1000);
				return false;
			}
		}
		else if (E.ID == "KeepZoneCachedForPlayerJoin")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
