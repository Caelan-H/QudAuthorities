namespace XRL.World.Effects;

public class CookingDomainElectric_OnDealingElectricDamage : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature deal@s electric damage, there's a 25% chance";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AttackerDealtDamage");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AttackerDealtDamage");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerDealtDamage" && E.GetParameter("Damage") is Damage damage && damage.IsElectricDamage() && 25.in100())
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
