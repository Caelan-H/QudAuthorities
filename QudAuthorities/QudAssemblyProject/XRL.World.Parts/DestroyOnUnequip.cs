using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class DestroyOnUnequip : IPart
{
	public string Message = "The mote of light is extinguished.";

	public override bool SameAs(IPart p)
	{
		if ((p as DestroyOnUnequip).Message != Message)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		if (E.Actor.IsPlayer() && !string.IsNullOrEmpty(Message))
		{
			Popup.Show(GameText.VariableReplace(Message, ParentObject));
		}
		ParentObject.Destroy();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginBeingUnequipped");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginBeingUnequipped")
		{
			if (E.HasFlag("DestroyOnUnequipDeclined"))
			{
				return false;
			}
			if (E.GetIntParameter("AutoEquipTry") > 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Owner");
				if (gameObjectParameter != null && gameObjectParameter.IsPlayer() && Popup.ShowYesNoCancel(ParentObject.The + ParentObject.DisplayNameOnly + " will be destroyed if " + ParentObject.itis + " unequipped. Do you want to continue?") != 0)
				{
					E.SetParameter("DestroyOnUnequipDeclined", 1);
					return false;
				}
			}
		}
		return base.FireEvent(E);
	}
}
