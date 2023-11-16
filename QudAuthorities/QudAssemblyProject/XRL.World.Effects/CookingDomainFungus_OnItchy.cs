namespace XRL.World.Effects;

public class CookingDomainFungus_OnItchy : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature get@s itchy skin,";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeforeApplyFungalInfection");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeforeApplyFungalInfection");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyFungalInfection")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
