using System;
using System.Collections.Generic;
using System.Text;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Annals;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.ZoneBuilders;

namespace XRL.World;

[Serializable]
public class Faction
{
	public const string HEIRLOOM_TIER = "5-6";

	public bool Visible = true;

	public bool Old = true;

	public bool HatesPlayer;

	public bool Pettable;

	public bool Plural = true;

	public bool ExtradimensionalVersions = true;

	public bool FormatWithArticle;

	public int HighlyEntropicBeingWorshipAttitude = -100;

	public string WaterRitualLiquid;

	public string WaterRitualSkill;

	public int WaterRitualSkillCost = -1;

	public bool WaterRitualBuyMostValuableItem;

	public int WaterRitualFungusInfect = -1;

	public int WaterRitualHermitOath = -1;

	public int WaterRitualSkillPointAmount = -1;

	public int WaterRitualSkillPointCost = -1;

	public string WaterRitualMutation;

	public int WaterRitualMutationCost = -1;

	public string WaterRitualGifts;

	public string WaterRitualItems;

	public string WaterRitualItemBlueprint;

	public int WaterRitualItemCost = -1;

	public string WaterRitualBlueprints;

	public string WaterRitualRecipe;

	public string WaterRitualRecipeText;

	public bool WaterRitualJoin = true;

	public int WaterRitualRandomMentalMutation = -1;

	public string WaterRitualAltBehaviorPart;

	public string WaterRitualAltBehaviorTag;

	public string WaterRitualAltLiquid;

	public string WaterRitualAltSkill;

	public int WaterRitualAltSkillCost = -1;

	public string WaterRitualAltGifts;

	public string WaterRitualAltItems;

	public string WaterRitualAltItemBlueprint;

	public int WaterRitualAltItemCost = -1;

	public string WaterRitualAltBlueprints;

	public string Name = "?";

	public string _DisplayName;

	public string Parent;

	public Dictionary<string, int> FactionFeeling = new Dictionary<string, int>();

	public int InitialPlayerReputation = int.MinValue;

	public Dictionary<string, int> PartReputation = new Dictionary<string, int>();

	public List<FactionInterest> Interests = new List<FactionInterest>();

	public List<string> HolyPlaces = new List<string>();

	public bool BuyTargetedSecrets = true;

	public bool SellTargetedSecrets = true;

	public string BuyDescription;

	public string SellDescription;

	public string BothDescription;

	public string InterestsBlurb;

	private string _TargetedSecretString;

	private string _GossipSecretString;

	private string _NoBuySecretString;

	private string _NoSellSecretString;

	[NonSerialized]
	private static List<string> BuyOnly = new List<string>(8);

	[NonSerialized]
	private static List<string> SellOnly = new List<string>(8);

	[NonSerialized]
	private static List<string> Both = new List<string>(8);

	public string DisplayName
	{
		get
		{
			if (_DisplayName == null)
			{
				DisplayName = Name;
			}
			if (Name != null && Name.StartsWith("SultanCult") && XRLCore.Core != null && XRLCore.Core.Game != null)
			{
				_DisplayName = The.Game.GetStringGameState("CultDisplayName_" + Name, Name);
			}
			return _DisplayName;
		}
		set
		{
			_DisplayName = value;
		}
	}

	public string TargetedSecretString
	{
		get
		{
			if (_TargetedSecretString == null)
			{
				_TargetedSecretString = "include:" + Name;
			}
			return _TargetedSecretString;
		}
	}

	public string GossipSecretString
	{
		get
		{
			if (_GossipSecretString == null)
			{
				_GossipSecretString = "gossip:" + Name;
			}
			return _GossipSecretString;
		}
	}

	public string NoBuySecretString
	{
		get
		{
			if (_NoBuySecretString == null)
			{
				_NoBuySecretString = "nobuy:" + Name;
			}
			return _NoBuySecretString;
		}
	}

	public string NoSellSecretString
	{
		get
		{
			if (_NoSellSecretString == null)
			{
				_NoSellSecretString = "nosell:" + Name;
			}
			return _NoSellSecretString;
		}
	}

	public string Heirloom
	{
		get
		{
			if (!The.Game.HasStringGameState("Heirloom_" + Name))
			{
				Stat.ReseedFrom("Heirloom_" + Name);
				List<string> list = new List<string>(Items.ItemTableNames.Keys);
				The.Game.SetStringGameState("Heirloom_" + Name, list.GetRandomElement());
			}
			return The.Game.GetStringGameState("Heirloom_" + Name);
		}
		set
		{
			The.Game.SetStringGameState("Heirloom_" + Name, value);
		}
	}

	public string HeirloomID
	{
		get
		{
			return The.Game.GetStringGameState("HeirloomID_" + Name);
		}
		set
		{
			if (value != null)
			{
				The.Game.SetStringGameState("HeirloomID_" + Name, value);
			}
			else
			{
				The.Game.RemoveStringGameState("HeirloomID_" + Name);
			}
		}
	}

	public static Reputation PlayerReputation => The.Game.PlayerReputation;

	public int CurrentReputation => The.Game.PlayerReputation.get(this);

	public Faction(string Name)
	{
		this.Name = Name;
	}

	public Faction(string Name = null, bool Visibility = true, string DisplayName = null, bool Old = true, string WaterRitualLiquid = "water")
	{
		this.Name = Name;
		Visible = Visibility;
		this.Old = Old;
		this.WaterRitualLiquid = WaterRitualLiquid;
		this.DisplayName = DisplayName ?? Name;
	}

	public static string getFormattedName(string factionName)
	{
		if (factionName == "*")
		{
			return "everyone";
		}
		try
		{
			return Factions.get(factionName).getFormattedName();
		}
		catch
		{
			Debug.Log("Failed to get faction: " + factionName);
			return "";
		}
	}

	public static string getFeelingDescription(string factionName)
	{
		Faction faction = Factions.get(factionName);
		string formattedName = faction.getFormattedName();
		formattedName = char.ToUpper(formattedName[0]) + ((formattedName.Length > 1) ? formattedName.Substring(1) : string.Empty);
		string text = (faction.Pettable ? ("{{C|" + formattedName + "}} will usually let you pet them. ") : "");
		string text2 = (faction.Pettable ? ("{{C|" + formattedName + "}} won't usually let you pet them. ") : "");
		return PlayerReputation.getAttitude(factionName) switch
		{
			-2 => "{{C|" + formattedName + "}} " + (faction.Plural ? "despise" : "despises") + " you. Even docile " + (faction.Plural ? "ones" : "members") + " will attack you.\n\n" + text2 + "You aren't welcome in their holy places.", 
			-1 => "{{C|" + formattedName + "}} " + (faction.Plural ? "dislike" : "dislikes") + " you, but docile " + (faction.Plural ? "ones" : "members") + " won't attack you.\n\n" + text2 + "You aren't welcome in their holy places.", 
			1 => "{{C|" + formattedName + "}} " + (faction.Plural ? "favor" : "favors") + " you.\n\nAggressive " + (faction.Plural ? "ones" : "members") + " won't attack you.\n\n" + text + "You are welcome in their holy places.", 
			2 => "{{C|" + formattedName + "}} " + (faction.Plural ? "revere" : "reveres") + " you and " + (faction.Plural ? "consider" : "considers") + " you one of their own.\n\n" + text + "You are welcome in their holy places.", 
			_ => "{{C|" + formattedName + "}} " + (faction.Plural ? "don't" : "doesn't") + " care about you, but aggressive " + (faction.Plural ? "ones" : "members") + " will attack you.\n\n" + text2 + "You aren't welcome in their holy places.", 
		};
	}

	public static string getPreferredSecretDescription(string factionName)
	{
		Faction faction = Factions.get(factionName);
		string formattedName = faction.getFormattedName();
		formattedName = char.ToUpper(formattedName[0]) + formattedName.Substring(1);
		formattedName = "{{C|" + formattedName + "}}";
		string value = " " + (faction.Plural ? "are" : "is");
		BuyOnly.Clear();
		SellOnly.Clear();
		Both.Clear();
		bool flag = false;
		foreach (FactionInterest interest in faction.Interests)
		{
			if (interest.WillBuy != interest.WillSell && (interest.WillBuy || interest.WillSell))
			{
				flag = true;
				break;
			}
		}
		foreach (FactionInterest interest2 in faction.Interests)
		{
			string description = interest2.GetDescription(faction);
			if (string.IsNullOrEmpty(description))
			{
				continue;
			}
			if (interest2.WillBuy && interest2.WillSell)
			{
				if (flag)
				{
					BuyOnly.Add(description);
					SellOnly.Add(description);
				}
				else
				{
					Both.Add(description);
				}
			}
			else if (interest2.WillBuy)
			{
				BuyOnly.Add(description);
			}
			else if (interest2.WillSell)
			{
				SellOnly.Add(description);
			}
		}
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = faction.HasInterestIn("sultan", Buy: true);
		bool flag5 = faction.HasInterestIn("sultan", Buy: false, Sell: true);
		if (!flag4 || !flag5)
		{
			if (faction.Name.StartsWith("SultanCult"))
			{
				if (flag || flag4 || flag5)
				{
					if (!flag4)
					{
						BuyOnly.Add("the sultan they worship");
					}
					if (!flag5)
					{
						SellOnly.Add("the sultan they worship");
					}
				}
				else
				{
					Both.Add("the sultan they worship");
				}
			}
			else
			{
				foreach (JournalSultanNote sultanNote in JournalAPI.GetSultanNotes())
				{
					if (!sultanNote.Has(faction.TargetedSecretString))
					{
						continue;
					}
					if (faction.BuyTargetedSecrets && faction.SellTargetedSecrets && !flag && !flag4 && !flag5 && string.IsNullOrEmpty(faction.BuyDescription) && string.IsNullOrEmpty(faction.SellDescription))
					{
						Both.Add("sultans they admire or despise");
						break;
					}
					if (faction.BuyTargetedSecrets && !flag4)
					{
						if (string.IsNullOrEmpty(faction.BuyDescription))
						{
							BuyOnly.Add("sultans they admire or despise");
						}
						else
						{
							flag2 = true;
						}
					}
					if (faction.SellTargetedSecrets && !flag5)
					{
						if (string.IsNullOrEmpty(faction.SellDescription))
						{
							SellOnly.Add("sultans they admire or despise");
						}
						else
						{
							flag3 = true;
						}
					}
					break;
				}
			}
		}
		bool flag6 = false;
		if (BuyOnly.Count > 0 && string.IsNullOrEmpty(faction.BuyDescription) && !faction.HasInterestIn("gossip", Buy: true) && !faction.HasInterestIn(faction.GossipSecretString, Buy: true))
		{
			BuyOnly.Add("gossip that's about them");
			flag6 = true;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (!string.IsNullOrEmpty(faction.InterestsBlurb))
		{
			stringBuilder.Append(formattedName).Append(faction.InterestsBlurb);
		}
		if (Both.Count > 0)
		{
			stringBuilder.Compound(formattedName, ".\n\n").Append(value).Append(string.IsNullOrEmpty(faction.BothDescription) ? " interested in trading secrets about " : faction.BothDescription)
				.Append(Grammar.MakeAndList(Both));
		}
		if (SellOnly.Count > 0)
		{
			stringBuilder.Compound(formattedName, ".\n\n").Append(value).Append(string.IsNullOrEmpty(faction.SellDescription) ? " interested in sharing secrets about " : faction.SellDescription)
				.Append(Grammar.MakeAndList(SellOnly));
		}
		if (BuyOnly.Count > 0)
		{
			stringBuilder.Compound(formattedName, ".\n\n").Append(value).Append(string.IsNullOrEmpty(faction.BuyDescription) ? " interested in learning about " : faction.BuyDescription)
				.Append(Grammar.MakeAndList(BuyOnly));
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Append('.');
		}
		if (!flag6 && !faction.HasInterestIn("gossip", Buy: true) && !faction.HasInterestIn(faction.GossipSecretString, Buy: true))
		{
			if (stringBuilder.Length == 0)
			{
				stringBuilder.Append(formattedName).Append(value);
			}
			else
			{
				stringBuilder.Append(" They're also");
			}
			if (flag2 && flag3)
			{
				stringBuilder.Append("interested in trading secrets about sultans they admire or despise and hearing gossip that's about them.");
				flag2 = false;
				flag3 = false;
			}
			else if (flag2)
			{
				stringBuilder.Append(" interested in learning about sultans they admire or despise and hearing gossip that's about them.");
				flag2 = false;
			}
			else if (flag3)
			{
				stringBuilder.Append(" interested in sharing secrets about sultans they admire or despise and hearing gossip that's about them.");
				flag3 = false;
			}
			else
			{
				stringBuilder.Append(" interested in hearing gossip that's about them.");
			}
		}
		if (flag2 || flag3)
		{
			if (stringBuilder.Length == 0)
			{
				stringBuilder.Append(formattedName).Append(value);
			}
			else
			{
				stringBuilder.Append(" They're also");
			}
			if (flag2 && flag3)
			{
				stringBuilder.Append("interested in trading secrets about sultans they admire or despise.");
				flag2 = false;
				flag3 = false;
			}
			else if (flag2)
			{
				stringBuilder.Append(" interested in learning about sultans they admire or despise.");
				flag2 = false;
			}
			else if (flag3)
			{
				stringBuilder.Append(" interested in sharing secrets about sultans they admire or despise.");
				flag3 = false;
			}
		}
		return stringBuilder.ToString();
	}

	public static string getRepPageDescription(string factionName)
	{
		return getFeelingDescription(factionName) + "\n\n" + getPreferredSecretDescription(factionName);
	}

	public static string getSultanFactionName(string period)
	{
		if (period == "6")
		{
			return "Resheph";
		}
		return "SultanCult" + period;
	}

	public static string getSultanFactionName(int period)
	{
		return getSultanFactionName(period.ToString());
	}

	public string getFormattedName()
	{
		if (!FormatWithArticle)
		{
			return DisplayName;
		}
		return "the " + DisplayName;
	}

	public string getWaterRitualLiquid(GameObject speaker)
	{
		if (string.IsNullOrEmpty(WaterRitualAltLiquid) || !UseAltBehavior(speaker))
		{
			return WaterRitualLiquid;
		}
		return WaterRitualAltLiquid;
	}

	public bool HasInterestIn(string topics, bool Buy = false, bool Sell = false)
	{
		foreach (FactionInterest interest in Interests)
		{
			if (interest.Includes(topics, Buy, Sell))
			{
				return true;
			}
		}
		return false;
	}

	public int setFactionFeeling(string faction, int faction_feeling)
	{
		FactionFeeling.Set(faction, faction_feeling);
		return faction_feeling;
	}

	public int setPartReputation(string part, int feeling)
	{
		PartReputation.Set(part, feeling);
		return feeling;
	}

	public Faction setParent(string name)
	{
		Faction faction = Factions.get(name);
		Parent = faction.Name;
		FactionFeeling = new Dictionary<string, int>(faction.FactionFeeling);
		InitialPlayerReputation = faction.InitialPlayerReputation;
		return faction;
	}

	public virtual int GetFeelingTowardsObject(GameObject GO)
	{
		bool flag = The.Game.GetIntGameState("DelegationOn") == 1;
		GameObject partyLeader;
		while (GO.pBrain != null && (partyLeader = GO.pBrain.PartyLeader) != null)
		{
			if (HatesPlayer && GO.IsPlayer())
			{
				return -100;
			}
			if (GO.HasTag("Calming") && !GO.IsPlayer())
			{
				return 50;
			}
			if (flag && GO.HasProperty("IsDelegate") && !GO.IsPlayer())
			{
				return 50;
			}
			GO = partyLeader;
		}
		if (HatesPlayer && GO.IsPlayer())
		{
			return -100;
		}
		if (GO.HasTag("Calming") && !GO.IsPlayer())
		{
			return 50;
		}
		if (flag && GO.HasProperty("IsDelegate") && !GO.IsPlayer())
		{
			return 50;
		}
		if (GO.pBrain != null)
		{
			if (GO.pBrain.FactionFeelings.TryGetValue(Name, out var value))
			{
				return value;
			}
			int value2 = 101;
			if (!GO.IsOriginalPlayerBody())
			{
				foreach (KeyValuePair<string, int> item in GO.pBrain.FactionMembership)
				{
					if (item.Value >= 75)
					{
						int feelingTowardsFaction = GetFeelingTowardsFaction(item.Key);
						if (feelingTowardsFaction < value2)
						{
							value2 = feelingTowardsFaction;
						}
					}
				}
			}
			if (value2 != 101)
			{
				return value2;
			}
			if ((GO.IsPlayer() || GO.LeftBehindByPlayer()) && PlayerReputation.any(Name))
			{
				return PlayerReputation.getFeeling(Name);
			}
			if (FactionFeeling.TryGetValue("*", out value2))
			{
				return value2;
			}
		}
		return 0;
	}

	public virtual int GetFeelingTowardsFaction(string Faction)
	{
		int value = 0;
		if (Faction == Name)
		{
			return 100;
		}
		if (FactionFeeling.TryGetValue(Faction, out value))
		{
			return value;
		}
		if (FactionFeeling.TryGetValue("*", out value))
		{
			return value;
		}
		return 0;
	}

	private bool SellRequiresSpecificInterest(IBaseJournalEntry note)
	{
		if (note.Has("onlySellIfTargetedAndInterested"))
		{
			return true;
		}
		return false;
	}

	public bool InterestedIn(IBaseJournalEntry note, bool Buy = false, bool Sell = false)
	{
		if (Buy && note.Has(NoBuySecretString))
		{
			return false;
		}
		if (Sell && note.Has(NoSellSecretString))
		{
			return false;
		}
		if (Buy && note.Has(GossipSecretString))
		{
			return true;
		}
		if (Buy && BuyTargetedSecrets && note.Has(TargetedSecretString))
		{
			bool flag = true;
			foreach (FactionInterest interest in Interests)
			{
				if (interest.Excludes(note, Buy, Sell))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		if (Sell && SellTargetedSecrets && note.Has(TargetedSecretString))
		{
			if (SellRequiresSpecificInterest(note))
			{
				foreach (FactionInterest interest2 in Interests)
				{
					if (interest2.Includes(note, Buy, Sell))
					{
						return true;
					}
				}
			}
			else
			{
				bool flag2 = true;
				foreach (FactionInterest interest3 in Interests)
				{
					if (interest3.Excludes(note, Buy, Sell))
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					return true;
				}
			}
		}
		if (!Sell || !SellRequiresSpecificInterest(note))
		{
			foreach (FactionInterest interest4 in Interests)
			{
				if (interest4.Includes(note, Buy, Sell))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool InterestedIn(IBaseJournalEntry note, ref string becauseOf, bool Buy = false, bool Sell = false)
	{
		if (Buy && note.Has(NoBuySecretString))
		{
			becauseOf = "no buy secret string " + NoBuySecretString + " on note";
			return false;
		}
		if (Sell && note.Has(NoSellSecretString))
		{
			becauseOf = "no sell secret string " + NoSellSecretString + " on note";
			return false;
		}
		if (Buy && note.Has(GossipSecretString))
		{
			becauseOf = "gossip secret string " + GossipSecretString + " on note";
			return true;
		}
		if (Buy && BuyTargetedSecrets && note.Has(TargetedSecretString))
		{
			becauseOf = "targeted secret string " + TargetedSecretString + " on note for buy";
			return true;
		}
		if (Sell && SellTargetedSecrets && note.Has(TargetedSecretString))
		{
			becauseOf = "targeted secret string " + TargetedSecretString + " on note for sell";
			return true;
		}
		foreach (FactionInterest interest in Interests)
		{
			if (interest.Includes(note, Buy, Sell))
			{
				becauseOf = "interest " + interest.DebugName;
				return true;
			}
		}
		return false;
	}

	public bool UseAltBehavior(GameObject speaker)
	{
		if (!string.IsNullOrEmpty(WaterRitualAltBehaviorPart) && speaker.HasPart(WaterRitualAltBehaviorPart))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(WaterRitualAltBehaviorTag) && speaker.HasTagOrProperty(WaterRitualAltBehaviorTag))
		{
			return true;
		}
		return false;
	}

	public bool HasInterestSameAs(FactionInterest interest)
	{
		foreach (FactionInterest interest2 in Interests)
		{
			if (interest2.SameAs(interest))
			{
				return true;
			}
		}
		return false;
	}

	public void AddInterestIfNew(FactionInterest interest)
	{
		if (!HasInterestSameAs(interest))
		{
			Interests.Add(interest);
		}
	}

	public static GameObject GenerateHeirloom(string Type)
	{
		int tier = "5-6".Roll();
		GameObject gameObject = GameObject.create(PopulationManager.RollOneFrom(Type + " " + tier).Blueprint, 0, 1);
		gameObject.SetStringProperty("Mods", "None");
		string type = RelicGenerator.GetType(gameObject);
		string subtype = RelicGenerator.GetSubtype(type);
		string text = RelicGenerator.SelectElement(gameObject) ?? "might";
		int num = 20;
		if (RelicGenerator.ApplyBasicBestowal(gameObject, type, tier, subtype))
		{
			num += 20;
		}
		if (50.in100() && !string.IsNullOrEmpty(text))
		{
			if (RelicGenerator.ApplyElementBestowal(gameObject, text, type, tier, subtype))
			{
				num += 40;
			}
		}
		else if (RelicGenerator.ApplyBasicBestowal(gameObject, type, tier, subtype))
		{
			num += 20;
		}
		string text2 = null;
		if (40.in100())
		{
			List<JournalMapNote> mapNotes = JournalAPI.GetMapNotes((JournalMapNote note) => note.Has("ruins") && !note.Has("historic") && note.text != "some forgotten ruins");
			if (mapNotes.Count > 0)
			{
				text2 = "the " + HistoricStringExpander.ExpandString("<spice.elements." + text + ".adjectives.!random> " + HistoricStringExpander.ExpandString("<spice.itemTypes." + type + ".!random>") + " of " + mapNotes.GetRandomElement().text);
			}
		}
		if (text2 == null)
		{
			GameObject aLegendaryEligibleCreature = EncountersAPI.GetALegendaryEligibleCreature();
			HeroMaker.MakeHero(aLegendaryEligibleCreature);
			Dictionary<string, string> vars = new Dictionary<string, string>
			{
				{ "*element*", text },
				{ "*itemType*", type },
				{
					"*personNounPossessive*",
					Grammar.MakePossessive(HistoricStringExpander.ExpandString("<spice.personNouns.!random>"))
				},
				{
					"*creatureNamePossessive*",
					Grammar.MakePossessive(aLegendaryEligibleCreature.a + aLegendaryEligibleCreature.ShortDisplayNameWithoutEpithetStripped)
				}
			};
			text2 = HistoricStringExpander.ExpandString("<spice.history.relics.names.!random>", null, null, vars);
		}
		gameObject.RequirePart<OriginalItemType>();
		text2 = QudHistoryHelpers.Ansify(Grammar.MakeTitleCase(text2));
		gameObject.pRender.DisplayName = text2;
		gameObject.HasProperName = true;
		gameObject.SetImportant(flag: true);
		if (num != 0 && gameObject.GetPart("Commerce") is Commerce commerce)
		{
			commerce.Value += commerce.Value * (double)num / 100.0;
		}
		return gameObject;
	}

	public GameObject GenerateHeirloom()
	{
		return GenerateHeirloom(Items.ItemTableNames[Heirloom]);
	}

	public void CacheHeirloom()
	{
		HeirloomID = ZoneManager.instance.CacheObject(GenerateHeirloom());
	}

	public void RequireCachedHeirloom()
	{
		if (string.IsNullOrEmpty(HeirloomID))
		{
			HeirloomID = ZoneManager.instance.CacheObject(GenerateHeirloom());
		}
	}

	public GameObject GetHeirloom()
	{
		GameObject gameObject = null;
		string heirloomID = HeirloomID;
		if (!string.IsNullOrEmpty(heirloomID))
		{
			gameObject = ZoneManager.instance.PullCachedObject(heirloomID);
		}
		CacheHeirloom();
		return gameObject ?? GenerateHeirloom();
	}

	public static bool WantsToBuySecret(string Faction, IBaseJournalEntry Note, GameObject Object = null)
	{
		return WantsToBuySecret(Factions.get(Faction), Note, Object);
	}

	public bool WantsToBuySecret(IBaseJournalEntry Note, GameObject Object = null)
	{
		return WantsToBuySecret(this, Note, Object);
	}

	public static bool WantsToBuySecret(Faction Faction, IBaseJournalEntry Note, GameObject Object = null)
	{
		if (!Note.revealed)
		{
			return false;
		}
		if (Note.secretSold)
		{
			return false;
		}
		if (Note is JournalMapNote journalMapNote && Object != null && journalMapNote.zoneid == Object.CurrentZone.ZoneID)
		{
			return false;
		}
		if (Object != null && Object.GetStringProperty("nosecret") == Note.secretid)
		{
			return false;
		}
		if (Object != null && Object.HasPart("Chef") && Note.Has("recipe"))
		{
			return true;
		}
		if (Faction.InterestedIn(Note, Buy: true))
		{
			return true;
		}
		return false;
	}

	public static bool WantsToBuySecret(string faction, IBaseJournalEntry note, GameObject rep, ref string becauseOf)
	{
		if (!note.revealed)
		{
			becauseOf = "note has not been revealed";
			return false;
		}
		if (note.secretSold)
		{
			becauseOf = "note has been sold";
			return false;
		}
		if (note is JournalMapNote journalMapNote && rep != null && journalMapNote.zoneid == rep.CurrentZone.ZoneID)
		{
			becauseOf = "note is about current zone";
			return false;
		}
		if (rep != null && rep.GetStringProperty("nosecret") == note.secretid)
		{
			becauseOf = "secret ID " + note.secretid + " matching nosecret property on speaker";
			return false;
		}
		if (rep != null && rep.HasPart("Chef") && note.Has("recipe"))
		{
			becauseOf = "note is a recipe and speaker is a chef";
			return true;
		}
		if (Factions.get(faction).InterestedIn(note, ref becauseOf, Buy: true))
		{
			return true;
		}
		return false;
	}

	public static bool WantsToSellSecret(string Faction, IBaseJournalEntry Note)
	{
		return WantsToSellSecret(Factions.get(Faction), Note);
	}

	public bool WantsToSellSecret(IBaseJournalEntry Note)
	{
		return WantsToSellSecret(this, Note);
	}

	public static bool WantsToSellSecret(Faction Faction, IBaseJournalEntry Note)
	{
		if (Note.revealed)
		{
			return false;
		}
		if (Note is JournalObservation note && JournalAPI.IsNoteExcluded(note))
		{
			return false;
		}
		if (Faction.InterestedIn(Note, Buy: false, Sell: true))
		{
			return true;
		}
		return false;
	}

	public static bool WantsToSellSecret(string faction, IBaseJournalEntry note, ref string becauseOf)
	{
		if (note.revealed)
		{
			becauseOf = "note has been revealed";
			return false;
		}
		if (note is JournalObservation note2 && JournalAPI.IsNoteExcluded(note2))
		{
			becauseOf = "note is an excluded observation";
			return false;
		}
		if (Factions.get(faction).InterestedIn(note, ref becauseOf, Buy: false, Sell: true))
		{
			return true;
		}
		return false;
	}
}
