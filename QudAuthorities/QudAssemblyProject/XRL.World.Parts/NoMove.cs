using System;

namespace XRL.World.Parts;

[Serializable]
public class NoMove : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginMove");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginMove")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
