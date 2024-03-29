using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class StandaloneMarkovBook : IPart
{
	public string Chain = "";

	public override bool AllowStaticRegistration()
	{
		return true;
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
			ParentObject.RequirePart<MarkovBook>().SetContents(Stat.Random(0, 2147483646), Chain);
			ParentObject.RemovePart(this);
		}
		return base.FireEvent(E);
	}
}
