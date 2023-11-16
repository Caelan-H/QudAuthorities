namespace XRL.World.Effects;

public class CookingDomainHeat_OnEnflamed : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlyEnflamed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature @is set on fire,";
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
			if (CurrentlyEnflamed)
			{
				if (base.Object.pPhysics.Temperature < base.Object.pPhysics.FlameTemperature)
				{
					CurrentlyEnflamed = false;
				}
			}
			else if (base.Object.pPhysics.Temperature >= base.Object.pPhysics.FlameTemperature)
			{
				CurrentlyEnflamed = true;
				Trigger();
			}
		}
		return base.FireEvent(E);
	}
}
