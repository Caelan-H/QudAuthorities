using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Empty_Tonic_Applicator : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyTonic");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyTonic")
		{
			return E.GetGameObjectParameter("Target").ApplyEffect(new Bleeding("1d2+2", 30, ParentObject, Stack: false));
		}
		return base.FireEvent(E);
	}
}
