using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class SlogGladsItem : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Unequipped");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Unequipped")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("UnequippingObject");
			gameObjectParameter?.GetPart<SlogGlands>()?.Unmutate(gameObjectParameter);
		}
		return base.FireEvent(E);
	}
}
