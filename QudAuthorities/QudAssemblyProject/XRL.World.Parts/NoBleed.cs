using System;

namespace XRL.World.Parts;

[Serializable]
public class NoBleed : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanApplyBleeding");
		Object.RegisterPartEvent(this, "ApplyBleeding");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyBleeding" || E.ID == "ApplyBleeding")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
