using System;

namespace XRL.World.Parts;

[Serializable]
public class TemperatureOnEat : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "OnEat");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			E.GetGameObjectParameter("Eater").pPhysics.Temperature += 100;
		}
		return base.FireEvent(E);
	}
}
