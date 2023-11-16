namespace XRL.World.Effects;

public class CookingDomainTeleport_OnTeleport : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlyEnflamed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature teleport@s,";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterTeleport");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterTeleport");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterTeleport")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
