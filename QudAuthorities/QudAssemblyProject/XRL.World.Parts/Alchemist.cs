using System;

namespace XRL.World.Parts;

[Serializable]
public class Alchemist : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveItemList");
		Object.RegisterPartEvent(this, "AlchemistExplode");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveItemList")
		{
			E.AddAICommand("AlchemistExplode");
		}
		else if (E.ID == "AlchemistExplode")
		{
			ParentObject.Explode(15000, null, "10d10+250", 1f, Neutron: true);
		}
		return base.FireEvent(E);
	}
}
