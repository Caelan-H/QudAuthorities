using XRL.Rules;

namespace XRL.World.Effects;

public class CookingDomainHeat_OnDealingHeatDamage : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature deal@s fire damage, there's a 10% chance";
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
		if (E.ID == "AttackerDealtDamage" && E.GetParameter("Damage") is Damage damage && (damage.HasAttribute("Fire") || damage.HasAttribute("Heat")) && Stat.Random(1, 100) <= 10)
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
