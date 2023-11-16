using XRL.Rules;

namespace XRL.World.Effects;

public class CookingDomainCold_OnDealingColdDamage : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature deal@s cold damage, there's a 25% chance";
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
		if (E.ID == "AttackerDealtDamage" && E.GetParameter("Damage") is Damage damage && damage.HasAttribute("Cold") && Stat.Random(1, 100) <= 25)
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
