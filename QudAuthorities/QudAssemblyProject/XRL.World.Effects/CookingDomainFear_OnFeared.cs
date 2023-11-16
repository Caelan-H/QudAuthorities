namespace XRL.World.Effects;

public class CookingDomainFear_OnFeared : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature become@s afraid,";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "FearApplied");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "FearApplied");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "FearApplied")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
