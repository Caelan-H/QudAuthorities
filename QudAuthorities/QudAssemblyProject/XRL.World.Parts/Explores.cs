using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class Explores : IPart
{
	public int ExploreOneIn = 200;

	public string ExploreMessage = "=subject.T= =verb:float= off.";

	public bool WaitForLeader;

	[NonSerialized]
	private DelegateGoal WaitGoal;

	[NonSerialized]
	private bool WantExplore;

	public override bool WantTurnTick()
	{
		return !WantExplore;
	}

	public override bool WantTenTurnTick()
	{
		return !WantExplore;
	}

	public override bool WantHundredTurnTick()
	{
		return !WantExplore;
	}

	public override void TurnTick(long TurnNumber)
	{
		Tick();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		Tick();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		Tick();
	}

	public void Tick(int Increment = 1)
	{
		if (Stat.Random(1, ExploreOneIn) <= Increment)
		{
			WantExplore = true;
		}
	}

	public bool Explore()
	{
		Brain pBrain = ParentObject.pBrain;
		if (pBrain?.PartyLeader == null || pBrain.Staying)
		{
			return false;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null || cell.OnWorldMap())
		{
			return false;
		}
		if (!cell.IsReachable() || !cell.ParentZone.IsActive())
		{
			return false;
		}
		List<Cell> cells = cell.ParentZone.GetCells(IsValidTarget);
		if (cells.IsNullOrEmpty())
		{
			return false;
		}
		if (WaitGoal == null)
		{
			WaitGoal = new DelegateGoal(WaitAction, WaitFinished)
			{
				SetCanFight = false,
				SetNonAggressive = true
			};
		}
		EmitMessage(GameText.VariableReplace(ExploreMessage, ParentObject));
		pBrain.PushGoal(WaitGoal);
		pBrain.PushGoal(new MoveTo(cells.GetRandomElement()));
		return true;
	}

	private bool IsValidTarget(Cell C)
	{
		if (!C.IsReallyExplored() && C.IsReachable())
		{
			return C.IsPassable(ParentObject);
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (WantExplore)
			{
				return ID == AIBoredEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (WantExplore)
		{
			WantExplore = false;
			if (Explore())
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public bool WaitFinished(GoalHandler Goal)
	{
		if (WaitForLeader)
		{
			return Goal.ParentBrain.PartyLeader?.InSameOrAdjacentCellTo(Goal.ParentObject) ?? true;
		}
		return Goal.CurrentCell.IsReallyExplored();
	}

	public void WaitAction(GoalHandler Goal)
	{
		Goal.ParentObject.ForfeitTurn();
	}
}
