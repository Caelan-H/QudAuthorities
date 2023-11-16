using System;

namespace XRL.World.Parts;

[Serializable]
public class ImmuneToConfusionGas : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanApplyConfusionGas");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyConfusionGas")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
