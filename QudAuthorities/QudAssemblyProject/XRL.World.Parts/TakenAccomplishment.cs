using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class TakenAccomplishment : IPart
{
	public string Text = "You got it!";

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
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID)
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		Trigger(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		Trigger(E.Actor);
		return base.HandleEvent(E);
	}

	public void Trigger(GameObject who)
	{
		if (!Triggered && who != null && who.IsPlayer())
		{
			JournalAPI.AddAccomplishment(Text.Replace("=this=", ParentObject.pRender.DisplayName), Hagiograph.Replace("=this=", ParentObject.DisplayNameOnlyDirect).Replace("=their=", The.Player.GetPronounProvider().PossessiveAdjective), "general", MuralCategoryHelpers.parseCategory(HagiographCategory), MuralCategoryHelpers.parseWeight(HagiographWeight), null, -1L);
			Triggered = true;
		}
	}
}
