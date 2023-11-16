namespace XRL.World.Effects;

public class CookingDomainArtifact_OnIdentify : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlySlowed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature identify an artifact,";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ExaminedCompleteSuccess");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "ExaminedCompleteSuccess");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ExaminedCompleteSuccess")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
