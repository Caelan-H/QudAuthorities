using System;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class EngulfingWanders : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction" && ParentObject.GetPart("Engulfing") is Engulfing engulfing)
		{
			if (engulfing.Engulfed != null)
			{
				if (!ParentObject.pBrain.HasGoal("FleeLocation"))
				{
					ParentObject.pBrain.Goals.Clear();
					ParentObject.pBrain.PushGoal(new FleeLocation(ParentObject.CurrentCell, "2d4".RollCached()));
				}
			}
			else if (ParentObject.pBrain.HasGoal("FleeLocation"))
			{
				ParentObject.pBrain.Goals.Clear();
			}
		}
		return base.FireEvent(E);
	}
}
