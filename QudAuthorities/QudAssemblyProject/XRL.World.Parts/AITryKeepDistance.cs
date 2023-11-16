using System;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AITryKeepDistance : IPart
{
	public float AttackThreshold = 0.5f;

	public string Duration = "1d2";

	public bool NotStuck;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetHostileWalkRadiusEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetHostileWalkRadiusEvent E)
	{
		if (!NotStuck || !E.Actor.HasEffect("Stuck"))
		{
			E.MaxRadius(2);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AICantAttackRange");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AICantAttackRange")
		{
			GameObject target = ParentObject.Target;
			if (target != null && (float)ParentObject.hitpoints > (float)ParentObject.baseHitpoints * AttackThreshold && (!NotStuck || !target.HasEffect("Stuck")))
			{
				ParentObject.pBrain.PushGoal(new Flee(target, Stat.Roll(Duration)));
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
