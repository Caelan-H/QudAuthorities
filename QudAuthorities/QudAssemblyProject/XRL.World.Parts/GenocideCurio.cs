using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class GenocideCurio : IPart
{
	public string Display;

	public string Creature;

	public string Template;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("On activation, this item obliterates all nearby members of a faction chosen from among those that existed during the time of the sultanate.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Activate", "activate", "ActivateGenocideCurio", null, 'a');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateGenocideCurio")
		{
			List<Faction> list = new List<Faction>();
			foreach (Faction item in Factions.loop())
			{
				if (item.Visible && item.Old)
				{
					list.Add(item);
				}
			}
			list.Sort((Faction a, Faction b) => a.DisplayName.CompareTo(b.DisplayName));
			List<string> list2 = new List<string>(list.Count);
			foreach (Faction item2 in list)
			{
				list2.Add(item2.DisplayName);
			}
			if (E.Actor != null && E.Actor.IsPlayer())
			{
				int num = Popup.ShowOptionList("Obliterate all nearby members of what faction?", list2.ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
				if (num != -1)
				{
					string Faction = list[num].Name;
					if (E.Actor != null && E.Actor.IsPlayer())
					{
						Popup.Show("You activate " + ParentObject.the + ParentObject.ShortDisplayName + " and toss it into the air.");
					}
					foreach (GameObject @object in E.Actor.CurrentZone.GetObjects((GameObject o) => !o.IsPlayer() && o.BelongsToFaction(Faction)))
					{
						@object.Sparksplatter();
						@object.Die(E.Actor, "critical existence failure", "You experienced a lack of existential support.", @object.It + " @@experienced a lack of existential support.");
					}
					ParentObject.Destroy();
					E.RequestInterfaceExit();
				}
			}
		}
		return base.HandleEvent(E);
	}
}
