using System;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedObject : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetDisplayNameEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Object.HasProperName && !E.Object.HasTagOrProperty("NoAnimatedNamePrefix"))
		{
			E.AddAdjective("animated", 5);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "HasPowerConnectors");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "HasPowerConnectors")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
