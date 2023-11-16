using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class EatenAccomplishment : IPart
{
	public string Text = "You ate it!";

	public bool Triggered;

	public string Hagiograph;

	public string HagiographCategory;

	public string HagiographWeight;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == InventoryActionEvent.ID)
			{
				return !Triggered;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Eat" && !Triggered && E.Actor.IsPlayer())
		{
			JournalAPI.AddAccomplishment(Text.Replace("=this=", ParentObject.pRender.DisplayName).Replace("=this.a=", ParentObject.a), Hagiograph.Replace("=this=", ParentObject.DisplayNameOnlyDirect).Replace("=this.a=", ParentObject.a), "general", MuralCategoryHelpers.parseCategory(HagiographCategory), MuralCategoryHelpers.parseWeight(HagiographWeight), null, -1L);
			Triggered = true;
		}
		return base.HandleEvent(E);
	}
}
