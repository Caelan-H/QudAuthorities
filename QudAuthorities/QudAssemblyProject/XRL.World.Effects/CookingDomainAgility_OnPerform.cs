namespace XRL.World.Effects;

public class CookingDomainAgility_OnPerformCriticalHit : ProceduralCookingEffectWithTrigger
{
	public int Tier;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature perform@s a critical hit,";
	}

	public override string GetTemplatedTriggerDescription()
	{
		return "whenever @thisCreature perform@s a critical hit,";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AttackerCriticalHit");
		Object.RegisterEffectEvent(this, "MissileAttackerCriticalHit");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AttackerCriticalHit");
		Object.UnregisterEffectEvent(this, "MissileAttackerCriticalHit");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerCriticalHit" || E.ID == "MissileAttackerCriticalHit")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
