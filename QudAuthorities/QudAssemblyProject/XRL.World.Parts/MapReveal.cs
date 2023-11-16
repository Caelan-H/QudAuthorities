using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class MapReveal : IPart
{
	public string Duration = "50";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != GetItemElementsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Read", "read", "ActivateMapReveal", null, 'r');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateMapReveal")
		{
			if (!E.Actor.IsPlayer())
			{
				return false;
			}
			if (!string.IsNullOrEmpty(ParentObject.Owner))
			{
				if (Popup.ShowYesNoCancel(ParentObject.The + ParentObject.ShortDisplayNameSingle + ParentObject.Is + " not owned by you, and using " + ParentObject.them + " will consume " + ParentObject.them + ". Are you sure you want to do so?") != 0)
				{
					return false;
				}
			}
			else if (!string.IsNullOrEmpty(ParentObject.InInventory?.Owner) && Popup.ShowYesNoCancel(ParentObject.InInventory.The + ParentObject.InInventory.ShortDisplayNameSingle + ParentObject.InInventory.Is + " not owned by you, and using " + ParentObject.the + ParentObject.ShortDisplayName + " will consume " + ParentObject.them + ". Are you sure you want to do so?") != 0)
			{
				return false;
			}
			if (E.Item.IsTemporary || !E.Actor.FireEvent("CheckRealityDistortionUsability"))
			{
				Popup.Show(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("seem") + " to be behaving as nothing more than an ordinary piece of paper.");
				E.Actor.UseEnergy(1000, "Item Failure");
				E.RequestInterfaceExit();
				return true;
			}
			Popup.Show(ParentObject.Itis + " a map of your surroundings!");
			int num = Duration.RollCached();
			GameObject gameObject = GameObject.create("AmbientOmniscience");
			gameObject.RequirePart<AmbientOmniscience>().IsRealityDistortionBased = true;
			if (num > 0)
			{
				gameObject.AddPart(new Temporary(num));
			}
			E.Actor.CurrentZone.GetCell(0, 0).AddObject(gameObject);
			ParentObject.Destroy();
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("travel", 10);
		E.Add("scholarship", 4);
		E.Add("time", 2);
		return base.HandleEvent(E);
	}
}
