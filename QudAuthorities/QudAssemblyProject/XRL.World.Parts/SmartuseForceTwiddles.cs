using System;

namespace XRL.World.Parts;

[Serializable]
public class SmartuseForceTwiddles : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanSmartUse");
		Object.RegisterPartEvent(this, "CommandSmartUseEarly");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			return false;
		}
		if (E.ID == "CommandSmartUseearly")
		{
			ParentObject.Twiddle();
			return false;
		}
		return base.FireEvent(E);
	}
}
