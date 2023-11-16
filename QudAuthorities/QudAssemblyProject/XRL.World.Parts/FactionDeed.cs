using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class FactionDeed : IPart
{
	public string Faction = "";

	public string Amount = "200";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetItemElementsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!string.IsNullOrEmpty(Faction))
		{
			E.AddTag("{{y|[" + Factions.get(Faction).DisplayName + " chapter]}}", -60);
		}
		else
		{
			E.AddTag("{{y|[chapter unspecified]}}", -60);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Activate", "entangle text", "ActivateFactionDeed", null, 'n', FireOnActor: false, 15);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("time", 10);
		if (string.IsNullOrEmpty(Faction))
		{
			E.Add("chance", 5);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateFactionDeed")
		{
			if (!E.Actor.IsPlayer())
			{
				return false;
			}
			if (!string.IsNullOrEmpty(ParentObject.Owner))
			{
				if (Popup.ShowYesNoCancel(ParentObject.Does("are", int.MaxValue, null, null, AsIfKnown: false, Single: true) + " not owned by you, and using " + ParentObject.them + " will consume " + ParentObject.them + ". Are you sure you want to do so?") != 0)
				{
					return false;
				}
			}
			else if (!string.IsNullOrEmpty(ParentObject.InInventory?.Owner) && Popup.ShowYesNoCancel(ParentObject.InInventory.Does("are", int.MaxValue, null, null, AsIfKnown: false, Single: true) + " not owned by you, and using " + ParentObject.t() + " will consume " + ParentObject.them + ". Are you sure you want to do so?") != 0)
			{
				return false;
			}
			if (E.Item.IsTemporary || !E.Actor.FireEvent("CheckRealityDistortionUsability"))
			{
				Popup.ShowFail(ParentObject.Does("seem") + " to be behaving as nothing more than an ordinary piece of paper.");
				E.Actor.UseEnergy(1000, "Item Failure");
				E.RequestInterfaceExit();
				return false;
			}
			string text = Faction;
			if (string.IsNullOrEmpty(text))
			{
				List<Faction> list = new List<Faction>();
				foreach (Faction item in Factions.loop())
				{
					if (item.Visible)
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
				int num = Popup.ShowOptionList("Choose a faction's chapter to insert your good deed.", list2.ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
				if (num == -1)
				{
					return false;
				}
				text = list[num].Name;
			}
			Faction ifExists = Factions.getIfExists(text);
			int num2 = Amount.RollCached();
			if (Options.SifrahRealityDistortion && E.Actor.IsPlayer())
			{
				int num3 = 3;
				if (ifExists != null)
				{
					if (ifExists.Old)
					{
						num3++;
					}
					num3 += ifExists.HighlyEntropicBeingWorshipAttitude / -50;
				}
				if (num3 < 3)
				{
					num3 = 3;
				}
				RealityDistortionSifrah realityDistortionSifrah = new RealityDistortionSifrah(ParentObject, "FactionDeed", "inserting a deed into history", E.Actor.Stat("Ego"), num3);
				realityDistortionSifrah.Play(ParentObject);
				if (realityDistortionSifrah.Abort || realityDistortionSifrah.InterfaceExitRequested)
				{
					return false;
				}
				num2 = num2 * realityDistortionSifrah.Performance / 100;
				if (num2 == 0)
				{
					Popup.ShowFail("The operation fails.");
					ParentObject.Destroy();
					return false;
				}
			}
			int num4 = GivesRep.varyRep(num2);
			int dayOfYear = Stat.Random(0, 438000);
			Popup.Show("You add the following entry into the {{K|Annals of Qud}}.\n\n\"On the " + Calendar.getDay(dayOfYear) + " of " + Calendar.getMonth(dayOfYear) + ", {{Y|" + IComponent<GameObject>.ThePlayer.BaseDisplayName + "}} became " + ((num4 >= 0) ? "admired" : "despised") + " by " + XRL.World.Faction.getFormattedName(text) + " for " + ((num4 >= 0) ? GenerateFriendOrFoe.getLikeReason() : GenerateFriendOrFoe.getHateReason()) + ".\"");
			The.Game.PlayerReputation.modify(text, num4);
			if (text.StartsWith("villagers of ") || text.Equals("Hindren"))
			{
				string vName;
				if (text.Equals("Hindren"))
				{
					vName = "Bey Lah";
				}
				else
				{
					vName = text.Substring(13);
				}
				List<JournalMapNote> mapNotes = JournalAPI.GetMapNotes((JournalMapNote note) => note.text == vName);
				if (mapNotes.Count > 0)
				{
					mapNotes.GetRandomElement().Reveal();
				}
			}
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Faction == "[random]")
		{
			List<Faction> list = new List<Faction>(64);
			List<string> list2 = new List<string>();
			foreach (Faction item in Factions.loop())
			{
				if (item.Visible)
				{
					list2.Add(item.DisplayName);
					list.Add(item);
				}
			}
			Faction = list.GetRandomElement().Name;
		}
		return base.HandleEvent(E);
	}
}
