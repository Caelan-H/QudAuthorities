using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class SpindleNegotiation : IPart
{
	public bool bQualified;

	public bool bArrived;

	public bool bNegotiated;

	public bool bChaosed;

	public bool bRemovedDelegates;

	public long TimeArrived;

	public List<string> DelegateFactions = new List<string>();

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (The.Game.HasFinishedQuestStep("The Earl of Omonporch", "Secure the Spindle") && !bRemovedDelegates)
		{
			Zone parentZone = ParentObject.pPhysics.currentCell.ParentZone;
			for (int i = 0; i < 80; i++)
			{
				for (int j = 0; j < 25; j++)
				{
					Cell cell = parentZone.GetCell(i, j);
					foreach (GameObject item in cell.GetObjectsWithPart("Brain"))
					{
						if (item.HasIntProperty("IsDelegate"))
						{
							cell.RemoveObject(item);
						}
					}
				}
			}
			bRemovedDelegates = true;
		}
		if (!bArrived && bQualified && Calendar.TotalTimeTicks - TimeArrived >= 3600)
		{
			bArrived = true;
			ParentObject.GetPart<ConversationScript>().ConversationID = "AsphodelArrived";
			XRLCore.Core.Game.SetIntGameState("DelegationOn", 1);
			AddDelegates();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginSpindleNegotiation");
		base.Register(Object);
	}

	public List<string> GetTop4Factions()
	{
		List<string> list = new List<string>();
		int num = 0;
		List<string> factionNames = Factions.getFactionNames();
		factionNames.Sort(new FactionRepComparerRandomSortEqual());
		int num2 = 0;
		while (num < 4)
		{
			if (Factions.get(factionNames[num2]).Visible)
			{
				list.Add(factionNames[num2]);
				num++;
			}
			num2++;
		}
		return list;
	}

	public GameObject GetDelegateForFaction(string Faction)
	{
		GameObject gameObject = GameObject.create("Delegate");
		gameObject.AddPart(new DelegateSpawner(Faction));
		return gameObject;
	}

	public void AddDelegates()
	{
		List<Cell> list = new List<Cell>();
		ParentObject.pPhysics.CurrentCell.GetConnectedSpawnLocations(4, list);
		if (list.Count < 4)
		{
			List<Cell> emptyCells = ParentObject.pPhysics.CurrentCell.ParentZone.GetEmptyCells();
			for (int i = 0; i < emptyCells.Count; i++)
			{
				if (list.Count >= 4)
				{
					break;
				}
				list.Add(emptyCells[i]);
			}
		}
		Algorithms.RandomShuffleInPlace(list, Stat.Rand);
		int num = 0;
		foreach (string top4Faction in GetTop4Factions())
		{
			GameObject delegateForFaction = GetDelegateForFaction(top4Faction);
			list[num].AddObject(delegateForFaction);
			num++;
			DelegateFactions.Add(top4Faction);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginSpindleNegotiation")
		{
			if (bChaosed)
			{
				Popup.Show("That was awful!");
				return true;
			}
			if (bNegotiated)
			{
				Popup.Show("Your friends may lease the Spindle, as we agreed.");
				return true;
			}
			if (bArrived)
			{
				string[] array = new string[5];
				List<string> list = ((DelegateFactions.Count >= 4) ? DelegateFactions : GetTop4Factions());
				List<string> list2 = new List<string>();
				for (int i = 0; i < list.Count; i++)
				{
					list2.Add(Faction.getFormattedName(list[i]));
				}
				array[0] = "Share the burden across all allies. [-{{C|50}} reputation with each attending faction]";
				array[1] = "Share the burden between two allies. [-{{C|100}} reputation with two attending factions of your choice]";
				array[2] = "Spare one faction of all obligation by betraying a second faction and selling their secrets to Asphodel. [-{{C|800}} with the betrayed faction, +{{C|200}} reputation with the spared faction + a faction heirloom]";
				array[3] = "Invoke the Chaos Spiel. [????????, +{{C|300}} reputation with {{C|highly entropic beings}}]";
				array[4] = "Take time to weigh the options.";
				char[] hotkeys = new char[5] { 'a', 'b', 'c', 'd', 'e' };
				string text = "";
				for (int j = 0; j < list.Count; j++)
				{
					text = text + list[j] + ", ";
				}
				int num = Popup.ShowOptionList("", array, hotkeys, 1, "The First Council of Omonporch has begun. Choose how to appease Asphodel.", 75);
				if (num == 4 || num < 0)
				{
					return true;
				}
				switch (num)
				{
				case 0:
					Popup.Show("The pact is struck. The Barathrumites may lease control of the Spindle, and all the attending factions owe a debt to Asphodel.");
					foreach (string item in list)
					{
						XRLCore.Core.Game.PlayerReputation.modify(item, -50);
					}
					XRLCore.Core.Game.FinishQuestStep("The Earl of Omonporch", "Secure the Spindle");
					bNegotiated = true;
					ParentObject.GetPart<ConversationScript>().ConversationID = "AsphodelDone";
					return true;
				case 1:
				{
					int num2 = Popup.ShowOptionList("", list2.ToArray(), null, 1, "Choose a faction to share the burden. [-{{C|100}} reputation]");
					string text5 = list[num2];
					if (num2 < 0)
					{
						return true;
					}
					list2.Remove(Faction.getFormattedName(text5));
					list.Remove(text5);
					int num3 = Popup.ShowOptionList("", list2.ToArray(), null, 1, "Choose a faction to share the burden. [-{{C|100}} reputation]");
					if (num3 < 0)
					{
						return true;
					}
					string faction = list[num3];
					Popup.Show("The pact is struck. The Barathrumites may lease control of the Spindle, and the chosen factions owe a debt to Asphodel.");
					XRLCore.Core.Game.PlayerReputation.modify(text5, -100);
					XRLCore.Core.Game.PlayerReputation.modify(faction, -100);
					XRLCore.Core.Game.FinishQuestStep("The Earl of Omonporch", "Secure the Spindle");
					bNegotiated = true;
					ParentObject.GetPart<ConversationScript>().ConversationID = "AsphodelDone";
					return true;
				}
				case 2:
				{
					int num4 = Popup.ShowOptionList("", list2.ToArray(), null, 1, "Choose a faction to betray. [-{{C|800}} reputation]");
					string text6 = list[num4];
					if (num4 < 0)
					{
						return true;
					}
					list2.Remove(Faction.getFormattedName(text6));
					list.Remove(text6);
					int num5 = Popup.ShowOptionList("", list2.ToArray(), null, 1, "Choose a faction to spare from obligation to Asphodel. [+{{C|200}} reputation and a faction heirloom]");
					if (num5 < 0)
					{
						return true;
					}
					string text7 = list[num5];
					GameObject heirloom = Factions.get(text7).GetHeirloom();
					Popup.Show("The pact is struck. The Barathrumites may lease control the Spindle.");
					Popup.Show("The delegate for " + Faction.getFormattedName(text7) + " says, 'Live and drink, " + IComponent<GameObject>.ThePlayer.formalAddressTerm + ". We won't forget this.'");
					XRLCore.Core.Game.PlayerReputation.modify(text7, 200);
					IComponent<GameObject>.ThePlayer.ReceiveObject(heirloom);
					Popup.Show("The delegate for " + Faction.getFormattedName(text7) + " gives you " + heirloom.a + heirloom.ShortDisplayName + "!");
					Popup.Show("The delegate for " + Faction.getFormattedName(text6) + " says, 'Betrayer! May you choke on your own spittle! We won't forget this.'");
					XRLCore.Core.Game.PlayerReputation.modify(text6, -800);
					XRLCore.Core.Game.FinishQuestStep("The Earl of Omonporch", "Secure the Spindle");
					bNegotiated = true;
					ParentObject.GetPart<ConversationScript>().ConversationID = "AsphodelDone";
					return true;
				}
				case 3:
				{
					Popup.Show("You ponder how best to sow chaos with your words.");
					XRLCore.Core.Game.PlayerReputation.modify("highly entropic beings", GivesRep.varyRep(300));
					List<string> top4Factions = GetTop4Factions();
					top4Factions.Add("Flowers");
					top4Factions.Add("Consortium");
					string text2 = "";
					string text3 = "";
					bool flag = false;
					for (int k = 0; k < 3; k++)
					{
						while (!flag)
						{
							text2 = top4Factions.GetRandomElement();
							text3 = top4Factions.GetRandomElement();
							if (!text2.Equals(text3))
							{
								flag = true;
							}
						}
						flag = false;
						string text4 = (string.Equals(text3, "highly entropic beings") ? GenerateFriendOrFoe_HEB.getHateReason() : GenerateFriendOrFoe.getHateReason());
						Popup.Show("You yell, 'I cannot believe {{C|" + Faction.getFormattedName(text2) + "}} don't despise {{C|" + Faction.getFormattedName(text3) + "}} for " + text4 + ".'");
						Popup.Show("Due to your revelation, " + Faction.getFormattedName(text2) + " change their opinion of " + Faction.getFormattedName(text3) + ".");
						Factions.get(text2).setFactionFeeling(text3, -100);
						XRLCore.Core.Game.PlayerReputation.modify(text2, GivesRep.varyRep(200));
						XRLCore.Core.Game.PlayerReputation.modify(text3, GivesRep.varyRep(-200));
					}
					bChaosed = true;
					ParentObject.GetPart<ConversationScript>().ConversationID = "AsphodelChaos";
					XRLCore.Core.Game.SetIntGameState("DelegationOn", 0);
					Popup.Show("Asphodel yells, '{{R|You ruined the First Council of Omonporch, you barbaric lout!}}'");
					ParentObject.pBrain.GetAngryAt(IComponent<GameObject>.ThePlayer, -100);
					AchievementManager.SetAchievement("ACH_CHAOS_SPIEL");
					return true;
				}
				}
			}
			else
			{
				if (!HasEnoughFriends())
				{
					Popup.Show("You don't have enough allied factions. Come back when you're favored by {{C|4}} or more factions.");
					return true;
				}
				ParentObject.GetPart<ConversationScript>().ConversationID = "AsphodelWaiting";
				if (TimeArrived == 0L)
				{
					TimeArrived = Calendar.TotalTimeTicks;
				}
				int num6 = Math.Max(1, 3 - (int)Math.Ceiling((float)(Calendar.TotalTimeTicks - TimeArrived) / 1200f));
				Popup.Show("The council will be convened! Come back in " + num6 + " " + ((num6 == 1) ? "day" : "days") + ".");
				bQualified = true;
			}
		}
		return base.FireEvent(E);
	}

	public bool HasEnoughFriends()
	{
		List<string> top4Factions = GetTop4Factions();
		if (XRLCore.Core.Game.PlayerReputation.get(top4Factions[3]) < 250)
		{
			return false;
		}
		return true;
	}
}
