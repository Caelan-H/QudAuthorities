using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

internal class HindrenMysteryExile : IPart
{
	public GlobalLocation Destination;

	private Zone beyLah => HindrenMysteryGamestate.instance.getBeyLahZone();

	public override void Register(GameObject go)
	{
		go.RegisterPartEvent(this, "ZoneFreezing");
		base.Register(go);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		CheckExile();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		CheckExile();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		CheckExile();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneFreezing" && !ParentObject.IsPlayerControlled())
		{
			if (Destination == null)
			{
				ParentObject.Destroy();
			}
			else if (ParentObject.CurrentZone.ZoneID != Destination.ZoneID)
			{
				ParentObject.DirectMoveTo(Destination, 0, forced: true, ignoreCombat: true);
			}
		}
		return base.FireEvent(E);
	}

	public bool CheckExile()
	{
		if (ParentObject.IsBusy())
		{
			return false;
		}
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return false;
		}
		if (ParentObject.IsPlayerControlled())
		{
			return false;
		}
		if (ParentObject.CurrentZone.ZoneID == beyLah.ZoneID)
		{
			if (Destination == null)
			{
				ParentObject.pBrain.PushGoal(new MoveToZone(beyLah.GetZoneFromDirection("W").ZoneID));
				ParentObject.pBrain.StartingCell = null;
			}
			else
			{
				ParentObject.pBrain.PushGoal(new MoveToGlobal(Destination));
				ParentObject.pBrain.StartingCell = Destination;
			}
		}
		else if (Destination == null)
		{
			ParentObject.Destroy();
		}
		else if (ParentObject.CurrentZone.ZoneID != Destination.ZoneID)
		{
			ParentObject.pBrain.PushGoal(new MoveToGlobal(Destination));
		}
		return true;
	}
}
