using System;
using System.Linq;

namespace XRL.World.Parts;

[Serializable]
public class PotentialHindrenLookClue : IPart
{
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
		if (E.ID == "ObjectCreated" && HindrenMysteryGamestate.instance != null && HindrenMysteryGamestate.instance.lookClues.Any((HindrenClueLook c) => c.target == ParentObject.Blueprint))
		{
			ParentObject.AddPart(new HindrenClueItem());
		}
		return base.FireEvent(E);
	}
}
