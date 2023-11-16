using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class NaturalEquipment : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AllowHugeHandsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AllowHugeHandsEvent E)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
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
			if (!E.HasFlag("Forced"))
			{
				return false;
			}
		}
		else if (E.ID == "BeginBeingUnequipped" && !E.HasFlag("Forced") && ParentObject.Equipped != null)
		{
			string text = "You can't remove " + ParentObject.the + ParentObject.DisplayNameOnly + "!";
			E.SetParameter("FailureMessage", text);
			if (!E.IsSilent() && !E.HasFlag("SemiForced") && ParentObject.Equipped.IsPlayer() && E.GetIntParameter("AutoEquipTry") <= 1)
			{
				Popup.Show(text);
			}
			return false;
		}
		return base.FireEvent(E);
	}
}
