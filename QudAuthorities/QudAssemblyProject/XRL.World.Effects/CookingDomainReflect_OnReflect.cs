using XRL.Rules;

namespace XRL.World.Effects;

public class CookingDomainReflect_OnReflectedDamage : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlyEnflamed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature reflect@s damage, there's a 50% chance";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ReflectedDamage");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "ReflectedDamage");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ReflectedDamage" && Stat.Random(1, 100) <= 50)
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
public class CookingDomainReflect_OnReflectedDamageHighTier : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlyEnflamed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature reflect@s damage, there's a 100% chance";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ReflectedDamage");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "ReflectedDamage");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ReflectedDamage")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
