namespace XRL.World.Effects;

public class CookingDomainRegenLowtier_OnSalve : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature use@s a salve or ubernostrum injector,";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ApplyingSalve");
		Object.RegisterEffectEvent(this, "ApplyingUbernostrum");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "ApplyingSalve");
		Object.UnregisterEffectEvent(this, "ApplyingUbernostrum");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyingSalve" || E.ID == "ApplyingUbernostrum")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
