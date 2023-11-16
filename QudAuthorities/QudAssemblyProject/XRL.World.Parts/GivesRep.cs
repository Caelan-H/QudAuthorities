using System;
using System.Collections.Generic;
using System.Text;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Language;
using XRL.Liquids;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class GivesRep : IPart
{
	public bool wasParleyed;

	public int repValue = 200;

	[NonSerialized]
	public List<FriendorFoe> relatedFactions = new List<FriendorFoe>();

	private bool KillRepDone;

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		GivesRep givesRep = (GivesRep)base.DeepCopy(Parent, MapInv);
		givesRep.relatedFactions = new List<FriendorFoe>(relatedFactions.Count);
		foreach (FriendorFoe relatedFaction in relatedFactions)
		{
			givesRep.relatedFactions.Add(new FriendorFoe(relatedFaction));
		}
		return givesRep;
	}

	public override void SaveData(SerializationWriter Writer)
	{
		Writer.Write(relatedFactions.Count);
		foreach (FriendorFoe relatedFaction in relatedFactions)
		{
			Writer.Write(relatedFaction.faction);
			Writer.Write(relatedFaction.status);
			Writer.Write(relatedFaction.reason);
		}
		base.SaveData(Writer);
	}

	public override void LoadData(SerializationReader Reader)
	{
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string faction = Reader.ReadString();
			string status = Reader.ReadString();
			string reason = Reader.ReadString();
			relatedFactions.Add(new FriendorFoe(faction, status, reason));
		}
		base.LoadData(Reader);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != BeginConversationEvent.ID && ID != CanGiveDirectionsEvent.ID && ID != GetPointsOfInterestEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.StandardChecks(this, E.Actor))
		{
			E.Add(ParentObject, ParentObject.BaseDisplayName);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginConversationEvent E)
	{
		if (E.SpeakingWith == ParentObject && E.CanTrade && E.Conversation.NodesByID.TryGetValue("Start", out var value) && value.bCloseable)
		{
			BaseLiquid liquid = LiquidVolume.getLiquid(ParentObject.GetWaterRitualLiquid(IComponent<GameObject>.ThePlayer));
			string text = "Your thirst is mine, my water is yours.";
			if (E.Actor.HasPart("SociallyRepugnant"))
			{
				text = SociallyRepugnant.waterRitualOptions.GetRandomElement();
			}
			value.AddChoice("{{G|" + text + " {{g|[begin water ritual" + ((Options.SifrahWaterRitual == "Never") ? ("; {{C|1}} dram of " + liquid.GetName(null)) : "") + "]}}}}", "*waterritual").Ordinal = 980;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanGiveDirectionsEvent E)
	{
		if (E.SpeakingWith == ParentObject && !E.PlayerCompanion)
		{
			E.CanGiveDirections = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		AppendReputationDescription(E.Postfix);
		if (wasParleyed)
		{
			E.Postfix.Compound("You are water-bonded with " + ParentObject.GetPronounProvider().Objective + ".", "\n");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		int num = 0;
		if (E.Killer != null && E.Killer.IsPlayerControlled() && !KillRepDone)
		{
			KillRepDone = true;
			if (wasParleyed && E.Killer != ParentObject && (!E.Accidental || ParentObject.IsHostileTowards(E.Killer)))
			{
				AchievementManager.SetAchievement("ACH_VIOLATE_WATER_RITUAL");
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("You violated the covenant of the water ritual and killed your bonded kith. You are cursed.\n\n");
				foreach (Faction item in Factions.loop())
				{
					if (item.Visible && !item.HatesPlayer)
					{
						The.Game.PlayerReputation.modify(item.Name, varyRep(-100), null, stringBuilder);
					}
				}
				Popup.Show(stringBuilder.ToString());
			}
			foreach (string key in ParentObject.pBrain.FactionMembership.Keys)
			{
				if (Factions.get(key).Visible)
				{
					The.Game.PlayerReputation.modify(key, varyRep(-repValue));
				}
			}
			foreach (FriendorFoe relatedFaction in relatedFactions)
			{
				if (Factions.get(relatedFaction.faction).Visible)
				{
					if (relatedFaction.status == "friend")
					{
						The.Game.PlayerReputation.modify(relatedFaction.faction, varyRep(-repValue));
					}
					else if (relatedFaction.status == "dislike")
					{
						num = repValue / 2;
						The.Game.PlayerReputation.modify(relatedFaction.faction, varyRep(num));
					}
					else if (relatedFaction.status == "hate")
					{
						The.Game.PlayerReputation.modify(relatedFaction.faction, varyRep(repValue));
					}
					else
					{
						Debug.LogError("Unknown status " + relatedFaction.status + " for " + relatedFaction.faction);
					}
				}
			}
			string text = ParentObject.a + ParentObject.DisplayName;
			if (wasParleyed)
			{
				JournalAPI.AddAccomplishment("You slew your bonded kith " + text + ", violating the covenant of the water ritual and earning the emnity of all.", "Blasphemously, the traitor " + text + " attacked =name=, " + ParentObject.its + " water-sib, and =name= was forced to slay " + ParentObject.it + ". Deep in grief, =name= weeped for one year.", "general", JournalAccomplishment.MuralCategory.Slays, JournalAccomplishment.MuralWeight.High, null, -1L);
			}
			else
			{
				JournalAPI.AddAccomplishment("You slew " + text + ".", HistoricStringExpander.ExpandString("In the month of " + Calendar.getMonth() + " of " + Calendar.getYear() + ", brave =name= slew <spice.commonPhrases.odious.!random> " + text + " in single combat."), "general", JournalAccomplishment.MuralCategory.Slays, JournalAccomplishment.MuralWeight.Low, null, -1L);
				ItemNaming.Opportunity(E.Killer, ParentObject, null, "Kill", 6, 0, 0, 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "FactionsAdded");
		Object.RegisterPartEvent(this, "GetConversationNode");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "FactionsAdded")
		{
			FillInRelatedFactions(Initial: true);
		}
		if (E.ID == "GetConversationNode" && E.GetStringParameter("GotoID") == "*waterritual")
		{
			E.SetParameter("ConversationNode", WaterRitualNode.waterRitual);
		}
		return base.FireEvent(E);
	}

	public void DumpRelatedFactions(string When)
	{
	}

	public void ResetRelatedFactions()
	{
		Brain pBrain = ParentObject.pBrain;
		foreach (FriendorFoe relatedFaction in relatedFactions)
		{
			int num = 0;
			if (relatedFaction.status == "friend")
			{
				num = 100;
			}
			else if (relatedFaction.status == "dislike")
			{
				num = -50;
			}
			else if (relatedFaction.status == "hate")
			{
				num = -100;
			}
			if (pBrain.FactionFeelings.ContainsKey(relatedFaction.faction))
			{
				if (pBrain.FactionFeelings[relatedFaction.faction] == num)
				{
					pBrain.FactionFeelings.Remove(relatedFaction.faction);
				}
				else
				{
					pBrain.FactionFeelings[relatedFaction.faction] -= num;
				}
			}
		}
		relatedFactions.Clear();
	}

	public int FillInRelatedFactions(bool Initial = false)
	{
		return FillInRelatedFactions(Stat.Random(1, 3), Initial);
	}

	public int FillInRelatedFactions(int FactionCount, bool Initial = false)
	{
		short num = 0;
		List<string[]> list = new List<string[]>(3);
		if (HasPropertyOrTag("staticFaction1"))
		{
			list.Add(GetPropertyOrTag("staticFaction1").Split(','));
			num = (short)(num + 1);
		}
		if (HasPropertyOrTag("staticFaction2"))
		{
			list.Add(GetPropertyOrTag("staticFaction2").Split(','));
			num = (short)(num + 1);
		}
		if (HasPropertyOrTag("staticFaction3"))
		{
			list.Add(GetPropertyOrTag("staticFaction3").Split(','));
			num = (short)(num + 1);
		}
		while (list.Count < 3)
		{
			list.Add(new string[3]);
		}
		string propertyOrTag = ParentObject.GetPropertyOrTag("NoHateFactions");
		string propertyOrTag2 = ParentObject.GetPropertyOrTag("NoFriendFactions");
		if (FactionCount < num)
		{
			FactionCount = num;
		}
		Brain pBrain = ParentObject.pBrain;
		for (int i = 1; i <= FactionCount; i++)
		{
			FriendorFoe friendorFoe = ((relatedFactions.Count >= i) ? relatedFactions[i - 1] : new FriendorFoe());
			string[] array = list[i - 1];
			string text = ((!string.IsNullOrEmpty(friendorFoe.faction)) ? friendorFoe.faction : ((!string.IsNullOrEmpty(array[0])) ? (friendorFoe.faction = array[0]) : (friendorFoe.faction = GenerateFriendOrFoe.getRandomFaction(ParentObject))));
			string text2;
			if (!string.IsNullOrEmpty(friendorFoe.status))
			{
				text2 = friendorFoe.status;
			}
			else if (!string.IsNullOrEmpty(array[1]))
			{
				text2 = (friendorFoe.status = array[1]);
			}
			else
			{
				int num2 = Stat.Random(1, 100);
				text2 = (friendorFoe.status = ((num2 <= 10) ? "friend" : ((num2 > 55) ? "hate" : "dislike")));
			}
			if (text2 != "friend" && text2 != "dislike" && text2 != "hate")
			{
				Debug.LogWarning("had unknown status '" + text2 + "', using dislike");
				text2 = (friendorFoe.status = "dislike");
			}
			if (propertyOrTag != null && (text2 == "dislike" || text2 == "hate") && propertyOrTag.Contains(text))
			{
				text2 = (friendorFoe.status = "friend");
			}
			if (propertyOrTag2 != null && text2 == "friend" && propertyOrTag2.Contains(text))
			{
				text2 = (friendorFoe.status = "dislike");
			}
			string value = null;
			if (!string.IsNullOrEmpty(friendorFoe.reason))
			{
				value = friendorFoe.reason;
			}
			else if (!string.IsNullOrEmpty(array[2]))
			{
				value = (friendorFoe.reason = array[2]);
			}
			switch (text2)
			{
			case "friend":
				if (pBrain.FactionFeelings.ContainsKey(text))
				{
					if (Initial)
					{
						pBrain.FactionFeelings[text] += 100;
					}
				}
				else
				{
					pBrain.FactionFeelings.Add(text, 100);
				}
				if (string.IsNullOrEmpty(value))
				{
					value = (friendorFoe.reason = (ParentObject.HasTag("StaticLikeReason") ? ParentObject.GetTag("StaticLikeReason") : ((!ParentObject.BelongsToFaction("highly entropic beings")) ? GenerateFriendOrFoe.getLikeReason() : GenerateFriendOrFoe_HEB.getLikeReason())));
				}
				break;
			case "dislike":
				if (pBrain.FactionFeelings.ContainsKey(text))
				{
					if (Initial)
					{
						pBrain.FactionFeelings[text] -= 50;
					}
				}
				else
				{
					pBrain.FactionFeelings.Add(text, -50);
				}
				if (string.IsNullOrEmpty(value))
				{
					value = (friendorFoe.reason = (ParentObject.HasTag("StaticHateReason") ? ParentObject.GetTag("StaticHateReason") : ((!ParentObject.BelongsToFaction("highly entropic beings")) ? GenerateFriendOrFoe.getHateReason() : GenerateFriendOrFoe_HEB.getHateReason())));
				}
				break;
			case "hate":
				if (pBrain.FactionFeelings.ContainsKey(text))
				{
					if (Initial)
					{
						pBrain.FactionFeelings[text] -= 100;
					}
				}
				else
				{
					pBrain.FactionFeelings.Add(text, -100);
				}
				if (string.IsNullOrEmpty(value))
				{
					value = (friendorFoe.reason = (ParentObject.HasTag("StaticHateReason") ? ParentObject.GetTag("StaticHateReason") : ((!ParentObject.BelongsToFaction("highly entropic beings")) ? GenerateFriendOrFoe.getHateReason() : GenerateFriendOrFoe_HEB.getHateReason())));
				}
				break;
			default:
				throw new Exception("internal inconsistency");
			}
			if (relatedFactions.Count < i)
			{
				relatedFactions.Add(friendorFoe);
			}
		}
		return FactionCount;
	}

	public static int varyRep(int rep)
	{
		float num = Stat.Random(-5, 5);
		float num2 = rep;
		num2 += 0.1f * num2 * num / 5f;
		num2 = 5 * (int)Math.Round(num2 / 5f);
		return (int)num2;
	}

	public static int varyRepUp(int rep)
	{
		float num = Stat.Random(0, 10);
		float num2 = rep;
		num2 += 0.1f * num2 * num / 10f;
		num2 = 10 * (int)Math.Round(num2 / 10f);
		return (int)num2;
	}

	public void AppendReputationDescription(StringBuilder SB)
	{
		if (SB.Length > 0 && SB[SB.Length - 1] != '\n')
		{
			SB.Append('\n');
		}
		List<string> list = new List<string>(2);
		foreach (string key in ParentObject.pBrain.FactionMembership.Keys)
		{
			Faction ifExists = Factions.getIfExists(key);
			if (ifExists != null && ifExists.Visible)
			{
				list.Add("{{C|" + ifExists.getFormattedName() + "}}");
			}
		}
		if (list.Count > 0)
		{
			SB.Append("{{C|-----}}\nLoved by ").Append(Grammar.MakeAndList(list)).Append(".\n");
		}
		foreach (FriendorFoe relatedFaction in relatedFactions)
		{
			Faction ifExists2 = Factions.getIfExists(relatedFaction.faction);
			if (ifExists2 != null && ifExists2.Visible)
			{
				SB.Append('\n');
				if (relatedFaction.status == "friend")
				{
					SB.Append("Admired");
				}
				else if (relatedFaction.status == "dislike")
				{
					SB.Append("Disliked");
				}
				else if (relatedFaction.status == "hate")
				{
					SB.Append("Hated");
				}
				SB.Append(" by {{C|").Append(Faction.getFormattedName(relatedFaction.faction)).Append("}} for ")
					.Append(relatedFaction.reason)
					.Append(".\n");
			}
		}
	}
}
