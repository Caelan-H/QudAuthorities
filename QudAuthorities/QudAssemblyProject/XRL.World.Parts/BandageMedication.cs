using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class BandageMedication : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Apply")
		{
			if (!E.Actor.HasEffect("Bleeding") && E.Actor.IsPlayer())
			{
				Popup.Show("You aren't bleeding!");
				return true;
			}
			Bleeding bleeding = E.Actor.GetEffect("Bleeding") as Bleeding;
			bleeding.SaveTarget -= "2d6".RollCached();
			if (E.Actor.HasSkill("Firstaid_StaunchWounds"))
			{
				bleeding.SaveTarget -= "2d6".RollCached();
			}
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}
}
