namespace XRL.World.Effects;

public class CookingDomainPhase_OnPhaseOut : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlyEnflamed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature phase@s out,";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterPhaseOut");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterPhaseOut");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterPhaseOut")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
