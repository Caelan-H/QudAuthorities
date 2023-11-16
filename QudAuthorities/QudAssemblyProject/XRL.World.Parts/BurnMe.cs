using System;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class BurnMe : IActivePart
{
	public string who;

	public BurnMe()
	{
	}

	public BurnMe(string who)
		: this()
	{
		this.who = who;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == IdleQueryEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (TryBurnMe(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool TryBurnMe(GameObject actor)
	{
		if (ParentObject.GetIntProperty("DroppedByPlayer") != 0)
		{
			return false;
		}
		if (!actor.BelongsToFaction(who))
		{
			return false;
		}
		if (ParentObject.IsAflame())
		{
			return false;
		}
		if (actor.pBrain == null)
		{
			return false;
		}
		if (ParentObject.CurrentCell == null)
		{
			return false;
		}
		actor.pBrain.PushGoal(new DelegateGoal(delegate(GoalHandler h)
		{
			if (actor.DistanceTo(ParentObject) <= 1)
			{
				ParentObject.pPhysics.Temperature = ParentObject.pPhysics.FlameTemperature * 2;
				actor.UseEnergy(1000, "Item Ignite");
			}
			h.FailToParent();
		}));
		actor.pBrain.PushGoal(new MoveTo(ParentObject));
		return true;
	}
}
