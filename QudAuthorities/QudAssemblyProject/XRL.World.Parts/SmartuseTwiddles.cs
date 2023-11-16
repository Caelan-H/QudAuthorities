using System;

namespace XRL.World.Parts;

[Serializable]
public class SmartuseTwiddles : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanSmartUse");
		Object.RegisterPartEvent(this, "CommandSmartUse");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			return false;
		}
		if (E.ID == "CommandSmartUse")
		{
			ParentObject.Twiddle();
		}
		return base.FireEvent(E);
	}
}
