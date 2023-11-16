using System;

namespace XRL.World.Parts;

[Serializable]
public class NoRust : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyRusted");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyRusted")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
