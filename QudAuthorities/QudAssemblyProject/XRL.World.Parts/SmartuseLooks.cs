using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class SmartuseLooks : IPart
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
			if (E.GetGameObjectParameter("User").IsPlayer())
			{
				return false;
			}
		}
		else if (E.ID == "CommandSmartUseEarly" && E.GetGameObjectParameter("User").IsPlayer())
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				Look.ShowLooker(0, cell.X, cell.Y);
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
