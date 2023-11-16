using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class TemperatureOnEntering : IPart
{
	public string Amount = "0";

	public bool Max;

	public int MaxTemp = 400;

	public bool OnWielderHit;

	public override bool SameAs(IPart p)
	{
		TemperatureOnEntering temperatureOnEntering = p as TemperatureOnEntering;
		if (temperatureOnEntering.Amount == Amount)
		{
			return temperatureOnEntering.MaxTemp == MaxTemp;
		}
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ProjectileEntering");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ProjectileEntering")
		{
			Cell obj = E.GetParameter("Cell") as Cell;
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			int phase = ParentObject.GetPhase();
			int num = MyPowerLoadBonus(int.MinValue, 100, 10);
			foreach (GameObject item in obj.GetObjectsWithPart("Physics"))
			{
				if (!Max || (Stat.RollMax(Amount) > 0 && item.pPhysics.Temperature < MaxTemp) || (Stat.RollMax(Amount) < 0 && item.pPhysics.Temperature > MaxTemp))
				{
					int num2 = Amount.RollCached();
					if (num != 0)
					{
						num2 = num2 * (100 + num) / 100;
					}
					item.TemperatureChange(num2, gameObjectParameter, Radiant: false, MinAmbient: false, MaxAmbient: false, phase);
				}
			}
		}
		return base.FireEvent(E);
	}
}
