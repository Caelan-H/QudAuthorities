using System;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AITryKeepSteadyDistance : IPart
{
	public int Distance = 5;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			GameObject target = ParentObject.Target;
			if (target != null)
			{
				int num = target.DistanceTo(ParentObject);
				if (num < Distance)
				{
					ParentObject.pBrain.PushGoal(new Flee(target, 2));
				}
				else if (num > Distance)
				{
					ParentObject.pBrain.StepTowards(target.CurrentCell);
				}
			}
		}
		return base.FireEvent(E);
	}
}
