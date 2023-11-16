using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Annals;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Capabilities;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class RandomAltarBaetyl : IPart
{
	public GameObject DemandObject;

	public string DemandBlueprint;

	public bool DemandIsMod;

	public int DemandCount;

	public int RewardType;

	public int RewardTier;

	public string RewardID;

	public int RewardAmount;

	public string RewardFaction;

	public int ItemBonusModChance = 40;

	public int ItemSetModNumber = 1;

	public int ItemMinorBestowalChance = 35;

	public int ItemElementBestowalChance = 15;

	public bool Fulfilled;

	[NonSerialized]
	private Cell FromCell;

	private int lastSparkFrame;

	public string MapNoteID => "Baetyl." + ParentObject.id;

	public RandomAltarBaetyl()
	{
		RewardType = GetRandomRewardTypeInstance();
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != BeginConversationEvent.ID && ID != CanSmartUseEvent.ID && ID != EnteredCellEvent.ID && ID != GetDebugInternalsEvent.ID && ID != GetPointsOfInterestEvent.ID)
		{
			return ID == LeftCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "DemandObject", DemandObject?.DebugName);
		E.AddEntry(this, "DemandBlueprint", DemandBlueprint);
		E.AddEntry(this, "DemandIsMod", DemandIsMod);
		E.AddEntry(this, "DemandCount", DemandCount);
		E.AddEntry(this, "RewardType", RewardType);
		E.AddEntry(this, "RewardTier", RewardTier);
		E.AddEntry(this, "RewardID", RewardID);
		E.AddEntry(this, "Reward", The.ZoneManager.peekCachedObject(RewardID)?.DebugName);
		E.AddEntry(this, "RewardAmount", RewardAmount);
		E.AddEntry(this, "RewardFaction", RewardFaction);
		E.AddEntry(this, "Fulfilled", Fulfilled);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (!Fulfilled && E.StandardChecks(this, E.Actor) && E.Actor.IsPlayer())
		{
			E.Add(ParentObject, ParentObject.BaseDisplayName);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginConversationEvent E)
	{
		if (E.SpeakingWith == ParentObject && E.Conversation.ID == ParentObject.GetBlueprint().GetPartParameter("ConversationScript", "ConversationID") && CanTalk(E.Actor))
		{
			BaetylWantsSacrifice();
			E.RequestInterfaceExit();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (Visible())
		{
			RemoveJournalNote();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && CanTalk(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		FromCell = E.Cell;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		GenerateDemand();
		GenerateReward();
		if (FromCell != null && FromCell.IsVisible())
		{
			JournalMapNote mapNote = JournalAPI.GetMapNote(MapNoteID);
			if (mapNote != null && (E.Cell.ParentZone == null || E.Cell.ParentZone.ZoneID != mapNote.zoneid))
			{
				RemoveJournalNote();
			}
		}
		FromCell = null;
		return base.HandleEvent(E);
	}

	public static int GetRandomRewardTypeInstance()
	{
		return Stat.Random(1, 8);
	}

	public static string GetModDemandName(string part, int num)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(Grammar.Cardinal(num)).Append(' ');
		ModEntry modEntry = ModificationFactory.ModsByPart[part];
		if (modEntry.TinkerDisplayName.Contains("with ") || modEntry.TinkerDisplayName.Contains("of "))
		{
			stringBuilder.Append((num == 1) ? "item" : "items").Append(' ').Append(modEntry.TinkerDisplayName);
		}
		else
		{
			stringBuilder.Append(modEntry.TinkerDisplayName).Append(' ').Append((num == 1) ? "item" : "items");
		}
		return stringBuilder.ToString();
	}

	public string GetDemandName(bool caps = true)
	{
		string text = (DemandIsMod ? GetModDemandName(DemandBlueprint, DemandCount) : DemandObject.GetDemandName(DemandCount));
		if (caps)
		{
			text = ColorUtility.ToUpperExceptFormatting(text);
		}
		return text;
	}

	public void GenerateDemand(Zone Z = null)
	{
		if (DemandBlueprint == null)
		{
			if (30.in100())
			{
				int num = 0;
				ModEntry randomElement;
				while (true)
				{
					randomElement = ModificationFactory.ModList.GetRandomElement();
					if (!randomElement.NoSparkingQuest && randomElement.MinTier <= 8)
					{
						break;
					}
					if (++num > 10000)
					{
						throw new Exception("cannot find any baetyl demands, aborting to avoid infinite loop");
					}
				}
				DemandBlueprint = randomElement.Part;
				DemandCount = Stat.Random(3, 6);
				DemandIsMod = true;
				if (RewardTier == 0)
				{
					RewardTier = randomElement.TinkerTier;
					if (randomElement.TinkerAllowed)
					{
						RewardTier += randomElement.Rarity;
					}
					else
					{
						RewardTier += randomElement.Rarity * randomElement.Rarity;
					}
					if (randomElement.CanAutoTinker)
					{
						RewardTier--;
					}
					if (Z == null && ParentObject != null)
					{
						Z = ParentObject.CurrentZone;
					}
					if (Z != null && Z.NewTier > RewardTier)
					{
						RewardTier = Z.NewTier;
					}
					RewardTier += Stat.Random(1, 2);
					Tier.Constrain(ref RewardTier);
				}
			}
			else
			{
				int num2 = 0;
				GameObjectBlueprint randomElement2;
				while (true)
				{
					randomElement2 = GameObjectFactory.Factory.BlueprintList.GetRandomElement();
					if (!randomElement2.HasPart("Brain") && randomElement2.HasPart("Physics") && randomElement2.HasPart("Render") && !randomElement2.Tags.ContainsKey("NoSparkingQuest") && !randomElement2.Tags.ContainsKey("BaseObject") && !randomElement2.Tags.ContainsKey("ExcludeFromDynamicEncounters") && !randomElement2.ResolvePartParameter("Physics", "Takeable").EqualsNoCase("false") && !randomElement2.ResolvePartParameter("Physics", "IsReal").EqualsNoCase("false") && !randomElement2.ResolvePartParameter("Render", "DisplayName").Contains("[") && (!randomElement2.Props.ContainsKey("SparkingQuestBlueprint") || randomElement2.Name == randomElement2.Props["SparkingQuestBlueprint"]))
					{
						break;
					}
					if (++num2 > 10000)
					{
						throw new Exception("cannot find any baetyl demands, aborting to avoid infinite loop");
					}
				}
				DemandBlueprint = randomElement2.Name;
				DemandCount = Stat.Random(3, 6);
				DemandIsMod = false;
			}
		}
		if (DemandObject != null || DemandIsMod)
		{
			return;
		}
		DemandObject = GameObject.createSample(DemandBlueprint);
		if (RewardTier == 0)
		{
			RewardTier = DemandObject.GetTier();
			if (Z == null && ParentObject != null)
			{
				Z = ParentObject.CurrentZone;
			}
			if (Z != null && Z.NewTier > RewardTier)
			{
				RewardTier = Z.NewTier;
			}
			RewardTier += Stat.Random(1, 2);
			Tier.Constrain(ref RewardTier);
		}
	}

	public void GenerateReward(int Value = 100)
	{
		GenerateRewardItem(Value);
		GenerateRewardAmount(Value);
		GenerateRewardFaction(Value);
	}

	private void GenerateRewardItem(int Value = 100)
	{
		if (!string.IsNullOrEmpty(RewardID))
		{
			if (Value == 100)
			{
				return;
			}
			The.ZoneManager.UncacheObject(RewardID);
			RewardID = null;
		}
		GameObject gameObject = null;
		if (RewardType == 1)
		{
			gameObject = GenerateItem("Melee Weapons", Value);
		}
		else if (RewardType == 2)
		{
			gameObject = GenerateItem("Armor", Value);
		}
		else if (RewardType == 3)
		{
			gameObject = GenerateItem("Missile", Value);
		}
		else if (RewardType == 4)
		{
			gameObject = GenerateItem("Artifact", Value);
		}
		if (gameObject != null)
		{
			RewardID = The.ZoneManager.CacheObject(gameObject);
		}
	}

	private void GenerateRewardAmount(int Value = 100)
	{
		if (RewardAmount != 0 && Value == 100)
		{
			return;
		}
		if (RewardType >= 5 && RewardType <= 7)
		{
			RewardAmount = Stat.Random(1, 3);
			if (Value != 100)
			{
				RewardAmount = Math.Max(1, IncrementalAdjustByValue(RewardAmount, Value));
			}
		}
		else if (RewardType == 8)
		{
			RewardAmount = GivesRep.varyRep(700 + Value);
		}
	}

	private void GenerateRewardFaction(int Value = 100)
	{
		if (!string.IsNullOrEmpty(RewardFaction) || RewardType != 8)
		{
			return;
		}
		List<string> list = new List<string>(64);
		foreach (Faction item in Factions.loop())
		{
			if (item.Visible)
			{
				list.Add(item.Name);
			}
		}
		RewardFaction = list.GetRandomElement();
	}

	private GameObject RetrieveRewardItem()
	{
		return The.ZoneManager.PullCachedObject(RewardID);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BaetylLeaving");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BaetylLeaving" && Visible())
		{
			RemoveJournalNote();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (!Fulfilled && lastSparkFrame != XRLCore.CurrentFrame)
		{
			lastSparkFrame = XRLCore.CurrentFrame;
			if (Stat.RandomCosmetic(1, 120) <= 2)
			{
				for (int i = 0; i < 2; i++)
				{
					ParentObject.ParticleText("&Y" + (char)Stat.RandomCosmetic(191, 198), 0.2f, 20);
				}
				for (int j = 0; j < 2; j++)
				{
					ParentObject.ParticleText("&W\u000f", 0.02f, 10);
				}
				PlayWorldSound("Electric", 0.35f, 0.35f);
			}
		}
		return true;
	}

	public static bool DisplayNameMatches(string B1, string B2)
	{
		GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.Blueprints[B1];
		GameObjectBlueprint gameObjectBlueprint2 = GameObjectFactory.Factory.Blueprints[B2];
		string partParameter = gameObjectBlueprint.GetPartParameter("Render", "DisplayName");
		if (partParameter == null)
		{
			return false;
		}
		string partParameter2 = gameObjectBlueprint2.GetPartParameter("Render", "DisplayName");
		if (partParameter2 == null)
		{
			return false;
		}
		if (partParameter == partParameter2)
		{
			return true;
		}
		if (ColorUtility.StripFormatting(partParameter) == ColorUtility.StripFormatting(partParameter2))
		{
			return true;
		}
		return false;
	}

	public GameObject GenerateItem(string Type, int Value = 100)
	{
		return GenerateItem(Type, Value, RewardTier);
	}

	private int IncrementalAdjustByValue(int Number, int Value)
	{
		if (Value != 100)
		{
			if (Value <= 10)
			{
				Number--;
			}
			if (Value <= 50)
			{
				Number--;
			}
			if (Value >= 150)
			{
				Number++;
			}
			if (Value >= 300)
			{
				Number++;
			}
		}
		return Number;
	}

	public GameObject GenerateItem(string Type, int Value, int TierSpec)
	{
		int tier = Tier.Constrain(IncrementalAdjustByValue(TierSpec, Value));
		string text = Type + " " + tier;
		if (Type != "Missile")
		{
			text += "R";
		}
		int bonusModChance = ItemBonusModChance * Value / 100;
		int setModNumber = IncrementalAdjustByValue(ItemSetModNumber, Value);
		int num = ItemMinorBestowalChance * Value / 100;
		int chance = ItemElementBestowalChance * Value / 100;
		GameObject gameObject = PopulationManager.CreateOneFrom(text, null, bonusModChance, setModNumber);
		bool flag = false;
		bool flag2 = false;
		int num2 = 20;
		string type = RelicGenerator.GetType(gameObject);
		string subtype = RelicGenerator.GetSubtype(type);
		string text2 = null;
		while (num.in100())
		{
			if (RelicGenerator.ApplyBasicBestowal(gameObject, type, tier, subtype))
			{
				flag = true;
				num2 += 20;
			}
			num -= 100;
		}
		if (chance.in100())
		{
			if (text2 == null)
			{
				text2 = RelicGenerator.SelectElement(gameObject);
			}
			if (!string.IsNullOrEmpty(text2) && RelicGenerator.ApplyElementBestowal(gameObject, text2, type, tier, subtype))
			{
				flag2 = true;
				num2 += 40;
			}
		}
		if (flag || flag2)
		{
			gameObject.SetStringProperty("Mods", "None");
			if (text2 == null)
			{
				text2 = RelicGenerator.SelectElement(gameObject) ?? "might";
			}
			string text3 = null;
			if (15.in100())
			{
				List<JournalMapNote> mapNotes = JournalAPI.GetMapNotes((JournalMapNote note) => note.Has("ruins") && !note.Has("historic") && note.text != "some forgotten ruins");
				if (mapNotes.Count > 0)
				{
					text3 = "the " + HistoricStringExpander.ExpandString("<spice.elements." + text2 + ".adjectives.!random> " + HistoricStringExpander.ExpandString("<spice.itemTypes." + type + ".!random>") + " of " + mapNotes.GetRandomElement().text);
				}
			}
			if (text3 == null)
			{
				GameObject aLegendaryEligibleCreature = EncountersAPI.GetALegendaryEligibleCreature();
				HeroMaker.MakeHero(aLegendaryEligibleCreature);
				Dictionary<string, string> vars = new Dictionary<string, string>
				{
					{ "*element*", text2 },
					{ "*itemType*", type },
					{
						"*personNounPossessive*",
						Grammar.MakePossessive(HistoricStringExpander.ExpandString("<spice.personNouns.!random>"))
					},
					{
						"*creatureNamePossessive*",
						Grammar.MakePossessive(aLegendaryEligibleCreature.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: true))
					}
				};
				text3 = HistoricStringExpander.ExpandString("<spice.history.relics.names.!random>", null, null, vars);
			}
			gameObject.RequirePart<OriginalItemType>();
			text3 = QudHistoryHelpers.Ansify(Grammar.MakeTitleCase(text3));
			gameObject.pRender.DisplayName = text3;
			gameObject.HasProperName = true;
			gameObject.SetImportant(flag: true);
		}
		if (num2 != 0 && gameObject.GetPart("Commerce") is Commerce commerce)
		{
			commerce.Value += commerce.Value * (double)num2 / 100.0;
		}
		return gameObject;
	}

	public bool ItemMatchesDemand(GameObject obj)
	{
		if (DemandIsMod)
		{
			return obj.HasPart(DemandBlueprint);
		}
		if (!(obj.Blueprint == DemandBlueprint) && !(obj.GetPropertyOrTag("SparkingQuestBlueprint") == DemandBlueprint))
		{
			return DisplayNameMatches(obj.Blueprint, DemandBlueprint);
		}
		return true;
	}

	public void BaetylWantsSacrifice()
	{
		if (ParentObject.IsHostileTowards(IComponent<GameObject>.ThePlayer))
		{
			return;
		}
		if (Fulfilled)
		{
			Popup.Show("I AM SATED, MORTAL. BEGONE.");
		}
		else
		{
			GenerateDemand();
			int num = 0;
			List<GameObject> list = new List<GameObject>(8);
			foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
			{
				foreach (GameObject item in adjacentCell.GetObjectsInCell())
				{
					if (ItemMatchesDemand(item))
					{
						num += item.Count;
						list.Add(item);
					}
				}
			}
			if (num >= DemandCount)
			{
				int num2 = DemandCount;
				foreach (GameObject item2 in list)
				{
					int count = item2.Count;
					item2.DustPuff();
					if (num2 >= count)
					{
						item2.Obliterate();
						num2 -= count;
					}
					else
					{
						for (int i = 0; i < num2; i++)
						{
							item2.RemoveOne().Obliterate();
							num2--;
						}
					}
					if (num2 <= 0)
					{
						break;
					}
				}
				int value = 100;
				if (Options.SifrahBaetylOfferings)
				{
					int rating = The.Player.StatMod("Intelligence");
					int difficulty = RewardTier + ParentObject.GetIntProperty("BaetylOfferingSifrahDifficultyModifier");
					BaetylOfferingSifrah baetylOfferingSifrah = new BaetylOfferingSifrah(ParentObject, rating, difficulty);
					baetylOfferingSifrah.Play(ParentObject);
					value = baetylOfferingSifrah.Performance;
				}
				ParentObject.pRender.ColorString = "&K^g";
				string text = "";
				GenerateReward(value);
				if (RewardType >= 1 && RewardType <= 4)
				{
					GameObject gameObject = RetrieveRewardItem();
					IComponent<GameObject>.ThePlayer.TakeObject(gameObject, Silent: false, 0);
					text = gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: false);
				}
				else if (RewardType == 5 || (RewardType == 6 && !IComponent<GameObject>.ThePlayer.IsMutant()))
				{
					IComponent<GameObject>.ThePlayer.GetStat("AP").BaseValue += RewardAmount;
					text = RewardAmount + " attribute " + ((RewardAmount == 1) ? "point" : "points");
				}
				else if (RewardType == 6)
				{
					IComponent<GameObject>.ThePlayer.GainMP(RewardAmount);
					text = RewardAmount + " mutation " + ((RewardAmount == 1) ? "point" : "points");
				}
				else if (RewardType == 7)
				{
					int num3 = RewardAmount * (50 + (IComponent<GameObject>.ThePlayer.BaseStat("Intelligence") - 10) * 4);
					IComponent<GameObject>.ThePlayer.GetStat("SP").BaseValue += num3;
					text = num3 + " skill " + ((num3 == 1) ? "point" : "points");
				}
				else if (RewardType == 8)
				{
					The.Game.PlayerReputation.modify(RewardFaction, RewardAmount);
					text = RewardAmount + " reputation with " + Faction.getFormattedName(RewardFaction);
				}
				Fulfilled = true;
				string demandName = GetDemandName(caps: false);
				JournalAPI.AddAccomplishment("You appeased a baetyl with " + demandName + ", and in return received " + text + ".", "While leading a small army through " + Grammar.GetProsaicZoneName(The.Player.CurrentZone) + ", =name= demanded that a local baetyl use its powers to transmute " + demandName + " into " + RewardString().ToLower(), "general", JournalAccomplishment.MuralCategory.AppeasesBaetyl, JournalAccomplishment.MuralWeight.Medium, null, -1L);
				Popup.Show("I ACCEPT YOUR OFFERING!\n\nThe sparking baetyl gives you {{|" + text + "}}!");
			}
			else
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("PETTY MORTAL! PLACE {{|").Append(GetDemandName()).Append("}} BEFORE ME, AND I SHALL REWARD YOU WITH ")
					.Append(RewardString());
				Popup.Show(stringBuilder.ToString());
			}
		}
		UpdateJournalNote();
	}

	public string RewardString()
	{
		string result = "STUFF!";
		if (RewardType == 1)
		{
			result = "A MIGHTY WEAPON.";
		}
		else if (RewardType == 2)
		{
			result = "A SPLENDID VESTMENT.";
		}
		else if (RewardType == 3)
		{
			result = "A BLAZING CANNON.";
		}
		else if (RewardType == 4)
		{
			result = "A PECULIAR CONTRAPTION.";
		}
		else if (RewardType == 5)
		{
			result = "ENHANCED PROWESS.";
		}
		else if (RewardType == 6)
		{
			result = "ENHANCED PROWESS.";
		}
		else if (RewardType == 7)
		{
			result = "HEIGHTENED SKILL.";
		}
		else if (RewardType == 8)
		{
			result = "GREAT RENOWN.";
		}
		return result;
	}

	public bool CanTalk(GameObject Actor = null)
	{
		if (!IsConversationallyResponsiveEvent.Check(ParentObject, Actor ?? The.Player, Physical: true))
		{
			return false;
		}
		if (ParentObject.IsInStasis())
		{
			return false;
		}
		if (ParentObject.HasEffect("Asleep"))
		{
			return false;
		}
		return true;
	}

	public void UpdateJournalNote()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (Fulfilled)
		{
			stringBuilder.Append("A \"SATED\" baetyl");
		}
		else
		{
			GenerateDemand();
			stringBuilder.Append("A baetyl demanding \"{{|").Append(GetDemandName()).Append("}}\" and promising \"")
				.Append(RewardString())
				.Append("\"");
		}
		JournalMapNote mapNote = JournalAPI.GetMapNote(MapNoteID);
		if (mapNote == null)
		{
			JournalAPI.AddMapNote(ParentObject.GetCurrentCell().ParentZone.ZoneID, stringBuilder.ToString(), "Baetyls", null, MapNoteID, revealed: true, sold: false, -1L);
		}
		else
		{
			mapNote.text = stringBuilder.ToString();
		}
	}

	public void RemoveJournalNote(JournalMapNote note)
	{
		if (note != null)
		{
			JournalAPI.DeleteMapNote(note);
		}
	}

	public void RemoveJournalNote()
	{
		RemoveJournalNote(JournalAPI.GetMapNote(MapNoteID));
	}

	[WishCommand(null, null, Command = "baetylrewarditem")]
	public static bool HandleBaetylRewardWish()
	{
		return Stat.Random(1, 4) switch
		{
			1 => HandleBaetylRewardMeleeWish(), 
			2 => HandleBaetylRewardArmorWish(), 
			3 => HandleBaetylRewardMissileWish(), 
			4 => HandleBaetylRewardArtifactWish(), 
			_ => true, 
		};
	}

	[WishCommand(null, null, Command = "baetylrewardmelee")]
	public static bool HandleBaetylRewardMeleeWish()
	{
		return HandleBaetylRewardWish("Melee Weapons");
	}

	[WishCommand(null, null, Command = "baetylrewardarmor")]
	public static bool HandleBaetylRewardArmorWish()
	{
		return HandleBaetylRewardWish("Armor");
	}

	[WishCommand(null, null, Command = "baetylrewardmissile")]
	public static bool HandleBaetylRewardMissileWish()
	{
		return HandleBaetylRewardWish("Missile");
	}

	[WishCommand(null, null, Command = "baetylrewardartifact")]
	public static bool HandleBaetylRewardArtifactWish()
	{
		return HandleBaetylRewardWish("Artifact");
	}

	private static bool HandleBaetylRewardWish(string ItemType)
	{
		RandomAltarBaetyl randomAltarBaetyl = new RandomAltarBaetyl();
		randomAltarBaetyl.GenerateDemand(IComponent<GameObject>.ThePlayer.CurrentZone);
		GameObject gameObject = randomAltarBaetyl.GenerateItem(ItemType);
		Popup.Show("Generated " + gameObject.an() + " as reward for " + randomAltarBaetyl.GetDemandName());
		IComponent<GameObject>.ThePlayer.CurrentCell.AddObject(gameObject);
		return true;
	}
}
