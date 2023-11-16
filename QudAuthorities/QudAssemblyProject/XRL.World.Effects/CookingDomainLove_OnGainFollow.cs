namespace XRL.World.Effects;

public class CookingDomainLove_OnGainFollower : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlySlowed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature gain@s a new follower,";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "GainedNewFollower");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "GainedNewFollower");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GainedNewFollower")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
