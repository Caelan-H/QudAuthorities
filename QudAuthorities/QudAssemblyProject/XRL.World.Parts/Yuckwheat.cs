using System;

namespace XRL.World.Parts;

[Serializable]
public class Yuckwheat : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "OnEat");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			gameObjectParameter.RemoveEffect("Confused");
			gameObjectParameter.RemoveEffect("Poisoned");
		}
		return base.FireEvent(E);
	}
}
