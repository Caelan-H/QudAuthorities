using XRL.Rules;

namespace XRL.World.Effects;

public class CookingDomainReflect_OnDamagedHighTier : CookingDomainReflect_OnDamaged
{
	public override void Init(GameObject target)
	{
		base.Init(target);
		Tier = Stat.Random(16, 20);
	}

	public override string GetTemplatedTriggerDescription()
	{
		return "whenever @thisCreature take@s damage, there's a 16-20% chance";
	}
}
public class CookingDomainReflect_OnDamaged : ProceduralCookingEffectWithTrigger
{
	public int Tier;

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(8, 10);
		base.Init(target);
	}

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature take@s damage, there's a " + Tier + "% chance";
	}

	public override string GetTemplatedTriggerDescription()
	{
		return "whenever @thisCreature take@s damage, there's a 8-10% chance";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "TookDamage");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "TookDamage");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TookDamage" && Stat.Random(1, 100) <= Tier)
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
