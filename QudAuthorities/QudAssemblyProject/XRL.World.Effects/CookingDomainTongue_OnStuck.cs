namespace XRL.World.Effects;

public class CookingDomainTongue_OnStuck : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature get@s stuck, there's a 50% chance";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "EffectApplied");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "EffectApplied");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EffectApplied" && E.GetParameter("Effect") is Stuck && 50.in100())
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
