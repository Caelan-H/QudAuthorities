using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatorSpray : IPart
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
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			if (E.Item.IsBroken() || E.Item.IsRusted())
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail("The sprayer head won't move.");
				}
				return false;
			}
			if (E.Actor.IsPlayer() && !E.Item.Understood())
			{
				E.Item.MakeUnderstood();
				Popup.Show(E.Item.Itis + " " + E.Item.an() + "!");
			}
			Cell cell = null;
			if (E.Actor != null && E.Actor.IsPlayer())
			{
				cell = E.Actor.pPhysics.PickDirection();
			}
			if (cell == null)
			{
				return false;
			}
			List<GameObject> list = new List<GameObject>();
			List<string> list2 = new List<string>();
			char c = 'a';
			List<char> list3 = new List<char>();
			foreach (GameObject @object in cell.Objects)
			{
				if (@object.HasTagOrProperty("Animatable"))
				{
					list.Add(@object);
					list2.Add(@object.DisplayNameOnlyStripped);
					list3.Add(c);
					c = (char)(c + 1);
				}
			}
			if (list.Count == 0)
			{
				Popup.ShowFail("There's nothing viable to animate here.");
				return false;
			}
			int num = Popup.ShowOptionList("", list2.ToArray(), list3.ToArray(), 0, "Choose a piece of furniture or other viable object to animate.", 60, RespectOptionNewlines: false, AllowEscape: true);
			if (num < 0)
			{
				return false;
			}
			GameObject gameObject = list[num];
			if (gameObject.HasPart("Brain") && E.Actor.IsPlayer())
			{
				Popup.ShowFail("You can't animate an object that already has a brain.");
				return false;
			}
			Popup.Show("You imbue " + gameObject.t() + " with life.");
			JournalAPI.AddAccomplishment("You imbued " + gameObject.an() + " with life. Why?", "While traveling in " + Grammar.GetProsaicZoneName(The.Player.CurrentZone) + ", =name= performed a sacred ritual with " + gameObject.an() + ", imbuing " + gameObject.them + " with life and arranging " + gameObject.them + " " + HistoricStringExpander.ExpandString("<spice.elements." + IComponent<GameObject>.ThePlayerMythDomain + ".babeTrait.!random>") + ". Many of the local denizens declared it a miracle. Some weren't so sure.", "general", JournalAccomplishment.MuralCategory.CommitsFolly, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			AnimateObject.Animate(gameObject, E.Actor, ParentObject);
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}
}
