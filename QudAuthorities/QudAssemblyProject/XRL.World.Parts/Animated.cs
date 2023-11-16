using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Animated : IPart
{
	public int ChanceOneIn = 10000;

	public override bool SameAs(IPart p)
	{
		if ((p as Animated).ChanceOneIn != ChanceOneIn)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Stat.Random(1, ChanceOneIn) == 1)
		{
			AnimateObject.Animate(ParentObject);
		}
		return base.HandleEvent(E);
	}
}
