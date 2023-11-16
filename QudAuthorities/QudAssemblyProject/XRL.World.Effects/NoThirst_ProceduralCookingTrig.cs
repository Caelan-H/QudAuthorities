using System;

namespace XRL.World.Effects;

[Serializable]
public class NoThirst_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they don't thirst for the next 12 hours.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect("NoThirst_ProceduralCookingTriggeredAction_Effect"))
		{
			go.ApplyEffect(new NoThirst_ProceduralCookingTriggeredAction_Effect());
		}
	}
}
[Serializable]
public class NoThirst_ProceduralCookingTriggeredAction_Effect : BasicTriggeredCookingEffect
{
	public override string GetDetails()
	{
		return "@they don't thirst.";
	}

	public NoThirst_ProceduralCookingTriggeredAction_Effect()
	{
		base.Duration = 1;
		base.DisplayName = null;
	}

	public override void ApplyEffect(GameObject Object)
	{
		base.Duration = 1200;
		Object.RegisterEffectEvent(this, "CalculatingThirst");
		base.ApplyEffect(Object);
	}

	public override void RemoveEffect(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "CalculatingThirst");
		base.RemoveEffect(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CalculatingThirst")
		{
			E.SetParameter("Amount", 0);
			return false;
		}
		return base.FireEvent(E);
	}
}
