namespace XRL.World.Effects;

public class OnHealProceduralCookingTrigger : ProceduralCookingEffectWithTrigger
{
	public bool Armed;

	public override string GetTriggerDescription()
	{
		return "when you heal to full health from half";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeginTakeAction");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (!base.Object.isDamaged() && Armed)
			{
				Armed = false;
				Trigger();
			}
			else if (base.Object.isDamaged(0.5, inclusive: true))
			{
				Armed = true;
			}
		}
		return base.FireEvent(E);
	}
}
