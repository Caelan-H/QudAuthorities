using System;
using System.Collections.Generic;
using Genkit;
using XRL.World.AI.Pathfinding;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class MoveTo : IMovementGoal
{
	public string dZone;

	public int dCx;

	public int dCy;

	public int MaxTurns = -1;

	public bool careful;

	public bool overridesCombat;

	public int shortBy;

	public bool wandering;

	public bool global;

	public bool juggernaut;

	[NonSerialized]
	public static List<AICommandList> CommandList = new List<AICommandList>();

	public MoveTo()
	{
	}

	public MoveTo(bool careful, bool overridesCombat, int shortBy, bool wandering, bool global, bool juggernaut, int MaxTurns)
		: this()
	{
		this.careful = careful;
		this.overridesCombat = overridesCombat;
		this.shortBy = shortBy;
		this.wandering = wandering;
		this.global = global;
		this.juggernaut = juggernaut;
		this.MaxTurns = MaxTurns;
	}

	public MoveTo(Cell C, bool careful = false, bool overridesCombat = false, int shortBy = 0, bool wandering = false, bool global = false, bool juggernaut = false, int MaxTurns = -1)
		: this(careful, overridesCombat, shortBy, wandering, global, juggernaut, MaxTurns)
	{
		if (C != null)
		{
			Zone parentZone = C.ParentZone;
			if (parentZone != null)
			{
				dZone = parentZone.ZoneID;
			}
			dCx = C.X;
			dCy = C.Y;
		}
	}

	public MoveTo(Location2D L, bool careful = false, bool overridesCombat = false, int shortBy = 0, bool wandering = false, bool global = false, bool juggernaut = false, int MaxTurns = -1)
		: this(careful, overridesCombat, shortBy, wandering, global, juggernaut, MaxTurns)
	{
		if (L != null)
		{
			dCx = L.x;
			dCy = L.y;
		}
	}

	public MoveTo(GameObject obj, bool careful = false, bool overridesCombat = false, int shortBy = 0, bool wandering = false, bool global = false, bool juggernaut = false, int MaxTurns = -1)
		: this(obj.GetCurrentCell(), careful, overridesCombat, shortBy, wandering, global, juggernaut, MaxTurns)
	{
	}

	public MoveTo(string ZoneID, int Cx, int Cy, bool careful = false, bool overridesCombat = false, int shortBy = 0, bool wandering = false, bool global = false, bool juggernaut = false, int MaxTurns = -1)
		: this(careful, overridesCombat, shortBy, wandering, global, juggernaut, MaxTurns)
	{
		dZone = ZoneID;
		dCx = Cx;
		dCy = Cy;
	}

	public MoveTo(GlobalLocation loc, bool careful = false, bool overridesCombat = false, int shortBy = 0, bool wandering = false, bool global = false, bool juggernaut = false, int MaxTurns = -1)
		: this(loc.ZoneID, loc.CellX, loc.CellY, careful, overridesCombat, shortBy, wandering, global, juggernaut, MaxTurns)
	{
	}

	public override bool CanFight()
	{
		return !overridesCombat;
	}

	public override bool Finished()
	{
		if (!ParentBrain.isMobile())
		{
			return true;
		}
		Cell currentCell = base.ParentObject.CurrentCell;
		if (currentCell != null)
		{
			if (currentCell.X == dCx && currentCell.Y == dCy)
			{
				return true;
			}
			if (shortBy > 0 && currentCell.DistanceTo(dCx, dCy) <= shortBy)
			{
				return true;
			}
		}
		return false;
	}

	public override void TakeAction()
	{
		if (!base.ParentObject.IsMobile())
		{
			FailToParent();
			return;
		}
		string text = base.ParentObject.CurrentZone?.ZoneID;
		if (text == null)
		{
			Pop();
			return;
		}
		if (string.IsNullOrEmpty(dZone))
		{
			dZone = text;
		}
		Cell currentCell = base.ParentObject.CurrentCell;
		if (text == dZone)
		{
			Cell cell = currentCell.ParentZone.GetCell(dCx, dCy);
			if (cell == null || cell == currentCell || (shortBy == 1 && cell.IsAdjacentTo(currentCell)) || (shortBy > 1 && cell.PathDistanceTo(currentCell) <= shortBy))
			{
				Pop();
				return;
			}
			CommandList.Clear();
			Event @event = Event.New("AIGetMovementMutationList", "TargetCell", cell);
			@event.SetParameter("List", CommandList);
			base.ParentObject.FireEvent(@event);
			if (AICommandList.HandleCommandList(CommandList, base.ParentObject, cell))
			{
				return;
			}
		}
		FindPath findPath = new FindPath(text, currentCell.X, currentCell.Y, dZone, dCx, dCy, global, careful, Juggernaut: juggernaut, Looker: base.ParentObject);
		base.ParentObject.UseEnergy(1000, "Pathfinding");
		if (findPath.bFound)
		{
			findPath.Directions.Reverse();
			int num2 = findPath.Directions.Count - shortBy;
			if (MaxTurns > -1)
			{
				Pop();
				if (num2 > MaxTurns)
				{
					num2 = MaxTurns;
				}
			}
			for (int i = 0; i < num2; i++)
			{
				PushGoal(new Step(findPath.Directions[i], careful, overridesCombat, wandering, juggernaut, null, global));
			}
		}
		else
		{
			FailToParent();
		}
	}

	public Cell GetDestinationCell()
	{
		Zone zone = The.ZoneManager.GetZone(dZone);
		if (zone != null)
		{
			Cell cell = zone.GetCell(dCx, dCy);
			if (cell != null)
			{
				return cell;
			}
		}
		return null;
	}
}
