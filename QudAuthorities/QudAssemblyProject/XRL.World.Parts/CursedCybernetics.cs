using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CursedCybernetics : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginBeingUnequipped");
		Object.RegisterPartEvent(this, "CanBeUnequipped");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanBeUnequipped")
		{
			if (E.HasFlag("Forced"))
			{
				return true;
			}
			if (ParentObject.IsImplant && ParentObject.Implantee == null)
			{
				return true;
			}
			return false;
		}
		if (E.ID == "BeginBeingUnequipped")
		{
			if (E.HasFlag("Forced"))
			{
				return true;
			}
			if (ParentObject.IsImplant && ParentObject.Implantee == null)
			{
				return true;
			}
			if (!E.IsSilent() && !E.HasFlag("SemiForced") && ParentObject.Equipped != null && ParentObject.Equipped.IsPlayer())
			{
				Popup.ShowFail("You can't remove " + ParentObject.the + ParentObject.ShortDisplayName + ".");
			}
			return false;
		}
		return base.FireEvent(E);
	}
}
