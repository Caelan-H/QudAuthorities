using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Sitting : IPart
{
	public bool bFirst = true;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && bFirst)
		{
			bFirst = false;
			ParentObject.ApplyEffect(new XRL.World.Effects.Sitting());
		}
		return true;
	}
}
