using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GodshroomCap : IPart
{
	public bool Controlled;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Eaten");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Eaten")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			if (gameObjectParameter.IsPlayer())
			{
				if (gameObjectParameter.HasEffect("FungalVisionary"))
				{
					gameObjectParameter.GetEffect("FungalVisionary").Duration += 1000;
				}
				else
				{
					gameObjectParameter.ApplyEffect(new FungalVisionary(1000));
				}
			}
		}
		return base.FireEvent(E);
	}
}
