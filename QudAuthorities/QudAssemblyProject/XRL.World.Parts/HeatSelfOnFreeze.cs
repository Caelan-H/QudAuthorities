using System;

namespace XRL.World.Parts;

[Serializable]
public class HeatSelfOnFreeze : IPart
{
	public int HeatCooldown;

	public string HeatFrequency = "1";

	public string HeatAmount = "60";

	public bool bAmountIsPercentage = true;

	public string HeatVerb = "vibrate";

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (HeatCooldown > 0)
			{
				HeatCooldown--;
			}
			if (ParentObject.IsFrozen() && HeatCooldown <= 0)
			{
				HeatCooldown = HeatFrequency.RollCached();
				if (ParentObject.IsVisible())
				{
					IComponent<GameObject>.XDidY(ParentObject, HeatVerb, "to warm " + ParentObject.itself, "!", null, ParentObject);
				}
				int amount = ((!bAmountIsPercentage) ? HeatAmount.RollCached() : (HeatAmount.RollCached() * (ParentObject.pPhysics.BrittleTemperature - ParentObject.pPhysics.Temperature) / 100));
				ParentObject.TemperatureChange(amount);
			}
		}
		return base.FireEvent(E);
	}
}
