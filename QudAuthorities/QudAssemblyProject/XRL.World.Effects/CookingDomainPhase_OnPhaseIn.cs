namespace XRL.World.Effects;

public class CookingDomainPhase_OnPhaseIn : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlyEnflamed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature phase@s in,";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterPhaseIn");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterPhaseIn");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterPhaseIn")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
