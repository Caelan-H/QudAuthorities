using System;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AIShootAndScoot : IPart
{
	public string Duration = "1d3";

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIAfterMissile");
		Object.RegisterPartEvent(this, "AIAfterThrow");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIAfterMissile" || E.ID == "AIAfterThrow")
		{
			GameObject target = ParentObject.Target;
			if (target != null)
			{
				ParentObject.pBrain.PushGoal(new Flee(target, Stat.Roll(Duration)));
			}
			return false;
		}
		return base.FireEvent(E);
	}
}
