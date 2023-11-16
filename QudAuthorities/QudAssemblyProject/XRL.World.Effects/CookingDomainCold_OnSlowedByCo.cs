namespace XRL.World.Effects;

public class CookingDomainCold_OnSlowedByCold : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlySlowed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature @is slowed by cold,";
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
			if (CurrentlySlowed)
			{
				if (base.Object.pPhysics.Temperature > base.Object.pPhysics.FreezeTemperature)
				{
					CurrentlySlowed = false;
				}
			}
			else if (base.Object.pPhysics.Temperature <= base.Object.pPhysics.FreezeTemperature)
			{
				CurrentlySlowed = true;
				Trigger();
			}
		}
		return base.FireEvent(E);
	}
}
