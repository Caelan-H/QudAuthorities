using System;

namespace XRL.World.Parts;

[Serializable]
public class AISuppressIndependentBehavior : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanAIDoIndependentBehavior");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanAIDoIndependentBehavior")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
