namespace XRL.World.Effects;

public class CookingDomainArmor_OnPenetration : ProceduralCookingEffectWithTrigger
{
	public int Tier = 2;

	public override void Init(GameObject target)
	{
		Tier = 2;
		base.Init(target);
	}

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature suffer@s a " + Tier + "X or greater physical penetration,";
	}

	public override string GetTemplatedTriggerDescription()
	{
		return "whenever @thisCreature suffer@s a " + Tier + "X or greater physical penetration,";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "DefenderHit");
		Object.RegisterEffectEvent(this, "DefenderMissileWeaponHit");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "DefenderHit");
		Object.UnregisterEffectEvent(this, "DefenderMissileWeaponHit");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "DefenderHit" || E.ID == "DefenderMissileWeaponHit") && E.GetIntParameter("Penetrations") >= Tier)
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
