using System;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class Uplift : IPart
{
	public string AdditionalBaseTemplate;

	public string AdditionalSpecializationTemplate;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ObjectCreated");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectCreated")
		{
			HeroMaker.MakeHero(ParentObject, AdditionalBaseTemplate, AdditionalSpecializationTemplate);
		}
		return base.FireEvent(E);
	}
}
