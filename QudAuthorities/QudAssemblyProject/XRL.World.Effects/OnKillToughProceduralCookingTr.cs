namespace XRL.World.Effects;

public class OnKillToughProceduralCookingTrigger : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "when you kill a tough mob";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "Killed");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "Killed");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Killed")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter != null && gameObjectParameter.Con(null, IgnoreHideCon: true) >= 5)
			{
				Trigger();
			}
			Remove(base.Object);
		}
		return base.FireEvent(E);
	}
}
