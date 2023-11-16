using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Annals;
using XRL.Core;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Parts;
using XRL.World.Tinkering;

namespace XRL.World.Capabilities;

[HasWishCommand]
public static class ItemNaming
{
	private const int CHOICE_ENTERED = 0;

	private const int CHOICE_RELIC_STYLE = 1;

	private const int CHOICE_OWN_CULTURE = 2;

	private const int CHOICE_KILL_CULTURE = 3;

	private const int CHOICE_INFLUENCER_CULTURE = 4;

	public static bool Suppress;

	[NonSerialized]
	private static string[] KillBlueprintSources = new string[8] { "LastKillAsMeleeWeaponBlueprint", "LastKillAsMeleeWeaponTurn", "LastKillAsLauncherBlueprint", "LastKillAsLauncherTurn", "LastKillAsProjectileBlueprint", "LastKillAsProjectileTurn", "LastKillAsThrownWeaponBlueprint", "LastKillAsThrownWeaponTurn" };

	public static bool CanBeNamed(GameObject obj, GameObject who)
	{
		if (Suppress)
		{
			return false;
		}
		if (!GameObject.validate(ref obj))
		{
			return false;
		}
		if (obj.HasProperName)
		{
			return false;
		}
		if (!obj.pPhysics.IsReal)
		{
			return false;
		}
		if (obj.pRender == null)
		{
			return false;
		}
		if (obj.IsTemporary)
		{
			return false;
		}
		if (obj.HasPart("NaturalEquipment"))
		{
			return false;
		}
		if (obj.HasTag("AlwaysStack"))
		{
			return false;
		}
		if (obj.HasTag("Creature"))
		{
			return false;
		}
		if (TinkeringHelpers.ConsiderScrap(obj, who))
		{
			return false;
		}
		if (obj.HasTagOrProperty("QuestItem") || obj.GetInventoryCategory() == "Quest Items")
		{
			return false;
		}
		if (!CanBeNamedEvent.Check(who, obj))
		{
			return false;
		}
		return true;
	}

	public static int GetNamingChance(GameObject obj, GameObject who)
	{
		if (!CanBeNamed(obj, who))
		{
			return 0;
		}
		if (obj.Count > 1)
		{
			return 0;
		}
		if (obj.HasPart("Armor") && obj.Equipped == null)
		{
			return 0;
		}
		if (obj.HasPart("Shield") && obj.Equipped == null)
		{
			return 0;
		}
		long num = XRLCore.CurrentTurn - 100;
		int num2 = Brain.WeaponScore(obj, who);
		int num3 = Brain.MissileWeaponScore(obj, who);
		if ((num2 >= 5 || num3 >= 5) && obj.Equipped == null)
		{
			if (who.IsPlayer())
			{
				if (obj.GetIntProperty("LastKillAsMeleeWeaponByPlayerTurn") < num && obj.GetIntProperty("LastKillAsThrownWeaponByPlayerTurn") < num && obj.GetIntProperty("LastKillAsLauncherByPlayerTurn") < num && obj.GetIntProperty("LastKillAsProjectileByPlayerTurn") < num)
				{
					return 0;
				}
			}
			else if (obj.GetIntProperty("LastKillAsMeleeWeaponTurn") < num && obj.GetIntProperty("LastKillAsThrownWeaponTurn") < num && obj.GetIntProperty("LastKillAsLauncherTurn") < num && obj.GetIntProperty("LastKillAsProjectileTurn") < num)
			{
				return 0;
			}
		}
		double num4 = obj.GetTier() - 1 + obj.GetIntProperty("nMods");
		num4 += (double)Math.Min(Brain.ArmorScore(obj, who).DiminishingReturns(0.5), 20);
		num4 += (double)Math.Min(Brain.ShieldScore(obj, who).DiminishingReturns(1.0), 20);
		int num5 = obj.GetIntProperty("KillsAsMeleeWeapon") - obj.GetIntProperty("AccidentalKillsAsMeleeWeapon") / 2 + obj.GetIntProperty("KillsAsThrownWeapon") - obj.GetIntProperty("AccidentalKillsAsThrownWeapon") / 2 + obj.GetIntProperty("KillsAsLauncher") - obj.GetIntProperty("AccidentalKillsAsLauncher") / 2 + obj.GetIntProperty("KillsAsProjectile") - obj.GetIntProperty("AccidentalKillsAsProjectile") / 2;
		if (num5 > 0)
		{
			num4 += (double)Math.Min(num5.DiminishingReturns(1.0), 50);
		}
		if (num5 > 0 || obj.Equipped != null)
		{
			num4 += (double)Math.Min(num2.DiminishingReturns(1.0), 10);
			num4 += (double)Math.Min(num3.DiminishingReturns(1.0), 10);
		}
		int intProperty = obj.GetIntProperty("InventoryActions");
		if (intProperty != 0)
		{
			num4 += (double)Math.Min(intProperty.DiminishingReturns(1.0) / 4, 20);
		}
		int intProperty2 = obj.GetIntProperty("ItemNamingBonus");
		if (intProperty2 != 0)
		{
			num4 += (double)Math.Min(intProperty2.DiminishingReturns(1.0), 10);
		}
		if (obj.GetPart("Description") is Description description && !string.IsNullOrEmpty(description.Mark))
		{
			num4 += 1.0;
		}
		if (num4 <= 0.0)
		{
			if (obj.Equipped != null)
			{
				num4 = 1.0;
			}
			else if (obj.GetIntProperty("LastInventoryActionTurn") > num)
			{
				num4 = 1.0;
			}
		}
		num4 = GetNamingChanceEvent.GetFor(who, obj, num4);
		return Math.Max((int)Math.Round(num4, MidpointRounding.AwayFromZero), 0);
	}

	public static int GetNamingBestowalChance(GameObject obj, GameObject who)
	{
		return Math.Max(GetNamingBestowalChanceEvent.GetFor(who, obj, 25), 0);
	}

	public static Dictionary<GameObject, int> GetNamingChances(GameObject who)
	{
		List<GameObject> inventoryAndEquipment = who.GetInventoryAndEquipment();
		Dictionary<GameObject, int> dictionary = null;
		int i = 0;
		for (int count = inventoryAndEquipment.Count; i < count; i++)
		{
			int namingChance = GetNamingChance(inventoryAndEquipment[i], who);
			if (namingChance > 0)
			{
				if (dictionary == null)
				{
					dictionary = new Dictionary<GameObject, int>((inventoryAndEquipment.Count >= 32) ? 16 : 8);
				}
				dictionary.Add(inventoryAndEquipment[i], namingChance);
			}
		}
		return dictionary;
	}

	public static bool Opportunity(GameObject who, GameObject kill = null, GameObject influencedBy = null, string opportunityType = "General", int suppressedByAnyTypeForLevels = 0, int suppressedBySameTypeForLevels = 0, int suppressedBySameTypeOnlyIfAtLeast = 0, int chanceToBypassSuppression = 0, bool force = false)
	{
		if (Popup.bSuppressPopups)
		{
			return false;
		}
		bool flag = false;
		if (suppressedByAnyTypeForLevels > 0)
		{
			int intProperty = who.GetIntProperty("LastItemNamingDoneAtLevel");
			if (intProperty > 0 && who.Stat("Level") < intProperty + suppressedByAnyTypeForLevels)
			{
				flag = true;
			}
		}
		if (!flag && suppressedBySameTypeForLevels > 0)
		{
			int intProperty2 = who.GetIntProperty("Last" + opportunityType + "ItemNamingDoneAtLevel");
			if (intProperty2 > 0 && who.Stat("Level") < intProperty2 + suppressedBySameTypeForLevels && (suppressedBySameTypeOnlyIfAtLeast == 0 || who.GetIntProperty(opportunityType + "ItemNamingDoneAtLevel") < suppressedBySameTypeOnlyIfAtLeast))
			{
				flag = true;
			}
		}
		if (flag && !chanceToBypassSuppression.in100())
		{
			return false;
		}
		if (!GameObject.validate(ref who))
		{
			return false;
		}
		Dictionary<GameObject, int> namingChances = GetNamingChances(who);
		if (namingChances == null || namingChances.Count <= 0)
		{
			return false;
		}
		List<GameObject> list = Event.NewGameObjectList();
		int num = 0;
		while (true)
		{
			foreach (KeyValuePair<GameObject, int> item in namingChances)
			{
				if (who.IsPlayer() ? item.Value.in1000(Stat.NamingRnd) : item.Value.in1000())
				{
					list.Add(item.Key);
				}
			}
			if (list.Count > 0)
			{
				break;
			}
			if (!force || ++num >= 1000)
			{
				return false;
			}
		}
		bool? flag2;
		do
		{
			GameObject obj = null;
			if (list.Count > 1)
			{
				if (who.IsPlayer())
				{
					List<string> list2 = new List<string>(list.Count);
					List<object> list3 = new List<object>(list.Count);
					List<char> list4 = new List<char>(list.Count);
					list2.Add("nothing");
					list3.Add(null);
					list4.Add('-');
					char c = 'a';
					int i = 0;
					for (int count = list.Count; i < count; i++)
					{
						list2.Add(list[i].DisplayName);
						list3.Add(list[i]);
						list4.Add(c);
						c = (char)(c + 1);
					}
					int num2 = Popup.ShowOptionList("You swell with the inspiration to name an item.", list2.ToArray(), list4.ToArray(), 0, "What would you like to name?");
					if (num2 >= 0)
					{
						obj = list3[num2] as GameObject;
					}
				}
				else
				{
					Dictionary<GameObject, int> dictionary = new Dictionary<GameObject, int>(list.Count);
					int j = 0;
					for (int count2 = list.Count; j < count2; j++)
					{
						dictionary.Add(list[j], namingChances[list[j]]);
					}
					obj = dictionary.GetRandomElement();
				}
			}
			else if (who.IsPlayer())
			{
				if (Popup.ShowYesNo("You swell with the inspiration to name your " + list[0].DisplayNameOnly + ". Do you wish to?", AllowEscape: false) == DialogResult.Yes)
				{
					obj = list[0];
				}
			}
			else
			{
				obj = list[0];
			}
			flag2 = NameItem(obj, who, kill, influencedBy, opportunityType);
		}
		while (!flag2.HasValue);
		return flag2.GetValueOrDefault();
	}

	private static string FindKillBlueprint(GameObject obj)
	{
		string result = null;
		long num = 0L;
		int i = 0;
		for (int num2 = KillBlueprintSources.Length; i < num2; i += 2)
		{
			string name = KillBlueprintSources[i];
			string name2 = KillBlueprintSources[i + 1];
			string stringProperty = obj.GetStringProperty(name);
			if (!string.IsNullOrEmpty(stringProperty))
			{
				long longProperty = obj.GetLongProperty(name2);
				if (longProperty > num)
				{
					result = stringProperty;
					num = longProperty;
				}
			}
		}
		return result;
	}

	private static string FindZoneName(GameObject who)
	{
		Zone currentZone = who.CurrentZone;
		if (currentZone == null)
		{
			return null;
		}
		return (currentZone.HasProperName ? currentZone.BaseDisplayName : null) ?? currentZone.NameContext;
	}

	private static string GenerateRelicStyleName(GameObject obj, GameObject who, GameObject kill, GameObject influencedBy, ref string element, ref string type)
	{
		if (element == null)
		{
			element = RelicGenerator.SelectElement(obj, who, kill, influencedBy);
		}
		if (type == null)
		{
			type = RelicGenerator.GetType(obj);
		}
		string text = FindZoneName(who);
		string phrase;
		if (!string.IsNullOrEmpty(text) && 50.in100())
		{
			phrase = "the " + HistoricStringExpander.ExpandString("<spice.elements." + element + ".adjectives.!random> " + HistoricStringExpander.ExpandString("<spice.itemTypes." + type + ".!random>") + " of " + text);
			return Grammar.MakeTitleCase(phrase);
		}
		string text2 = ((kill != null && kill.HasProperName && 30.in100()) ? (kill.a + kill.DisplayNameOnlyStripped) : ((influencedBy != null && influencedBy.HasProperName && 50.in100()) ? (influencedBy.a + influencedBy.DisplayNameOnlyStripped) : (who.a + who.DisplayNameOnlyStripped)));
		Dictionary<string, string> vars = new Dictionary<string, string>
		{
			{ "*element*", element },
			{ "*itemType*", type },
			{
				"*personNounPossessive*",
				Grammar.MakePossessive(HistoricStringExpander.ExpandString("<spice.personNouns.!random>"))
			},
			{
				"*creatureNamePossessive*",
				Grammar.MakePossessive(text2)
			}
		};
		phrase = HistoricStringExpander.ExpandString("<spice.history.relics.names.!random>", null, null, vars);
		if (phrase.Contains(text2))
		{
			phrase = phrase.Replace(text2, "------------");
			phrase = Grammar.MakeTitleCase(phrase);
			return phrase.Replace("------------", text2);
		}
		return Grammar.MakeTitleCase(phrase);
	}

	public static bool? NameItem(GameObject obj, GameObject who, GameObject kill = null, GameObject influencedBy = null, string opportunityType = "General")
	{
		if (!GameObject.validate(ref obj))
		{
			return false;
		}
		if (kill == null)
		{
			string text = FindKillBlueprint(obj);
			kill = ((text != null) ? GameObject.createSample(text) : null);
			if (obj.DisplayNameOnly.Contains("["))
			{
				kill = null;
			}
		}
		string text2 = null;
		string text3 = null;
		string element = null;
		string type = null;
		bool useAnsify = false;
		if (who.IsPlayer())
		{
			List<string> list = new List<string>();
			List<char> list2 = new List<char>();
			List<int> list3 = new List<int>();
			char c = 'a';
			list3.Add(0);
			list2.Add(c++);
			list.Add("Enter a name.");
			list3.Add(1);
			list2.Add(c++);
			list.Add("Name " + obj.them + " based on " + obj.its + " qualities.");
			list3.Add(2);
			list2.Add(c++);
			list.Add("Choose a random name from your own culture.");
			if (kill != null)
			{
				list3.Add(3);
				list2.Add(c++);
				list.Add("Choose a random name from " + kill.poss("culture") + ".");
			}
			if (GameObject.validate(ref influencedBy) && influencedBy.HasTag("Creature"))
			{
				list3.Add(4);
				list2.Add(c++);
				list.Add("Choose a random name from " + influencedBy.poss("culture") + ".");
			}
			int num = Popup.ShowOptionList("", list.ToArray(), list2.ToArray(), 1, "Rename " + obj.t() + ".", 60, RespectOptionNewlines: false, AllowEscape: true);
			if (num < 0)
			{
				return null;
			}
			switch (list3[num])
			{
			case 0:
				text2 = Popup.AskString("Enter a new name for " + obj.t() + ".", "", 30, 0, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -+/#()!@$%*<>'", ReturnNullForEscape: true);
				if (string.IsNullOrEmpty(text2))
				{
					return null;
				}
				break;
			case 1:
				text2 = GenerateRelicStyleName(obj, who, kill, influencedBy, ref element, ref type);
				break;
			case 2:
				text2 = NameMaker.MakeName(who, null, null, null, null, null, null, null, null, "Item", null, FailureOkay: false, SpecialFaildown: true);
				break;
			case 3:
				text2 = NameMaker.MakeName(kill, null, null, null, null, null, null, null, null, "Item", null, FailureOkay: false, SpecialFaildown: true);
				break;
			case 4:
				text2 = NameMaker.MakeName(influencedBy, null, null, null, null, null, null, null, null, "Item", null, FailureOkay: false, SpecialFaildown: true);
				break;
			}
			text3 = Popup.ShowColorPicker("", 0, "You select the name '" + text2 + "' for " + obj.t() + ". Choose a color for " + obj.them + ".", 60, RespectOptionNewlines: false, AllowEscape: true, 0, "", includeNone: true, includePatterns: true);
			if (text3 == null)
			{
				return null;
			}
		}
		else
		{
			List<int> list4 = new List<int>(8);
			list4.Add(0);
			list4.Add(1);
			list4.Add(1);
			list4.Add(1);
			list4.Add(2);
			if (kill != null)
			{
				list4.Add(3);
			}
			if (influencedBy != null)
			{
				list4.Add(4);
			}
			switch (list4.GetRandomElement())
			{
			case 0:
			case 2:
				text2 = NameMaker.MakeName(who, null, null, null, null, null, null, null, null, "Item", null, FailureOkay: false, SpecialFaildown: true);
				break;
			case 1:
				text2 = GenerateRelicStyleName(obj, who, kill, influencedBy, ref element, ref type);
				break;
			case 3:
				text2 = NameMaker.MakeName(kill, null, null, null, null, null, null, null, null, "Item", null, FailureOkay: false, SpecialFaildown: true);
				break;
			case 4:
				text2 = NameMaker.MakeName(influencedBy, null, null, null, null, null, null, null, null, "Item", null, FailureOkay: false, SpecialFaildown: true);
				break;
			}
			useAnsify = true;
		}
		return NameItem(obj, who, text2, text3, element, type, useAnsify, canBestow: true, kill, influencedBy, opportunityType);
	}

	public static bool NameItem(GameObject obj, GameObject who, string name, string color = null, string element = null, string type = null, bool useAnsify = false, bool canBestow = false, GameObject kill = null, GameObject influencedBy = null, string opportunityType = "General")
	{
		if (!CanBeNamed(obj, who))
		{
			return false;
		}
		string value = null;
		if (name.StartsWith("the ") || name.StartsWith("The "))
		{
			value = "the";
			name = name.Substring(4);
		}
		string text = name;
		if (useAnsify)
		{
			name = QudHistoryHelpers.Ansify(name);
		}
		else if (!string.IsNullOrEmpty(color))
		{
			name = "{{" + color + "|" + name + "}}";
		}
		GameObject gameObject = obj.DeepCopy();
		The.ZoneManager.CacheObject(gameObject);
		obj.SetStringProperty("PreNamingState", gameObject.id);
		string shortDisplayName = obj.ShortDisplayName;
		string text2 = obj.a + shortDisplayName;
		if (who.IsPlayer())
		{
			Popup.Show("You name " + obj.t() + " '" + name + "'.");
		}
		obj.RequirePart<OriginalItemType>();
		obj.SplitStack(1, who);
		obj.pRender.DisplayName = name;
		obj.pRender.SetForegroundColor(ColorUtility.GetMainForegroundColor(name));
		obj.SetImportant(flag: true, force: true);
		obj.HasProperName = true;
		if (!string.IsNullOrEmpty(value))
		{
			obj.SetStringProperty("IndefiniteArticle", value);
			obj.SetStringProperty("DefiniteArticle", value);
		}
		obj.SetIntProperty("Renamed", 1);
		int num = who.Stat("Level");
		int intProperty = who.GetIntProperty("LastItemNamingDoneAtLevel");
		int intProperty2 = who.GetIntProperty("Last" + opportunityType + "ItemNamingDoneAtLevel");
		who.ModIntProperty("ItemNamingDone", 1);
		if (who.IsPlayer())
		{
			The.Game.ModIntGameState("PlayerItemNamingDone", 1);
		}
		who.SetIntProperty("LastItemNamingDoneAtLevel", num);
		who.SetIntProperty("Last" + opportunityType + "ItemNamingDoneAtLevel", num);
		if (intProperty == num)
		{
			who.ModIntProperty("ItemNamingDoneAtLevel", 1);
		}
		else
		{
			who.SetIntProperty("ItemNamingDoneAtLevel", 1);
		}
		if (intProperty2 == num || intProperty2 < intProperty)
		{
			who.ModIntProperty(opportunityType + "ItemNamingDoneAtLevel", 1);
		}
		else
		{
			who.SetIntProperty(opportunityType + "ItemNamingDoneAtLevel", 1);
		}
		int num2 = 0;
		bool flag = false;
		if (canBestow)
		{
			int num3 = 0;
			bool flag2 = false;
			if (Options.SifrahItemNaming && who.IsPlayer())
			{
				int rating = who.StatMod("Ego") + who.StatMod("Willpower");
				int difficulty = Tier.Constrain(obj.GetTier());
				ItemNamingSifrah itemNamingSifrah = new ItemNamingSifrah(obj, rating, difficulty);
				itemNamingSifrah.Play(obj);
				num3 = itemNamingSifrah.BasicBestowals;
				flag2 = itemNamingSifrah.ElementBestowal;
			}
			else
			{
				int @for = GetNamingBestowalChanceEvent.GetFor(who, obj, GlobalConfig.GetIntSetting("ItemNamingBestowalBaseChance"));
				if (who.IsPlayer() ? @for.in100(Stat.NamingRnd) : @for.in100())
				{
					num3++;
					if (who.IsPlayer() ? @for.in100(Stat.NamingRnd) : @for.in100())
					{
						flag2 = true;
					}
				}
			}
			int num4 = -1;
			string text3 = null;
			if (num3 > 0 || flag2)
			{
				if (type == null)
				{
					type = RelicGenerator.GetType(obj);
				}
				if (text3 == null)
				{
					text3 = RelicGenerator.GetSubtype(type);
				}
				if (num4 == -1)
				{
					num4 = Tier.Constrain(Stat.Random((num < 20) ? 1 : 2, 1 + num / 5));
				}
				if (type != null && text3 != null)
				{
					BodyPart bodyPart = null;
					GameObject obj2 = obj.Equipped;
					bool flag3 = false;
					bool flag4 = false;
					if (obj2 != null)
					{
						bodyPart = obj2.FindEquippedObject(obj);
						if (!flag3 && !flag4)
						{
							obj.SetIntProperty("NeverStack", 1);
							flag4 = true;
						}
					}
					bool flag5 = false;
					if (obj2 != null)
					{
						flag5 = obj2.FireEvent(Event.New("CommandUnequipObject", "BodyPart", bodyPart));
					}
					try
					{
						for (int i = 0; i < num3; i++)
						{
							if (RelicGenerator.ApplyBasicBestowal(obj, type, num4, text3))
							{
								num2++;
							}
						}
						if (num2 == 0)
						{
							flag2 = true;
						}
						if (flag2)
						{
							if (element == null)
							{
								element = RelicGenerator.SelectElement(obj, who, kill, influencedBy);
							}
							if (!string.IsNullOrEmpty(element))
							{
								flag = RelicGenerator.ApplyElementBestowal(obj, element, type, num4, text3);
							}
						}
						if (num2 > 0 || flag)
						{
							if (flag)
							{
								obj.SetStringProperty("Mods", "None");
							}
							if (who.IsPlayer())
							{
								Popup.Show(ColorUtility.CapitalizeExceptFormatting(obj.T()) + obj.GetVerb("seem") + " to have taken on new qualities.");
							}
						}
					}
					finally
					{
						if (GameObject.validate(ref obj))
						{
							if (bodyPart != null && bodyPart.Equipped == null && flag5 && GameObject.validate(ref obj2))
							{
								obj2.FireEvent(Event.New("CommandEquipObject", "Object", obj, "BodyPart", bodyPart));
							}
							if (flag4)
							{
								obj.RemoveIntProperty("NeverStack");
							}
						}
						if (obj.Equipped == null && obj.InInventory == null && obj.CurrentCell == null)
						{
							who.ReceiveObject(obj);
						}
					}
				}
			}
		}
		if (who.IsPlayer())
		{
			JournalAPI.AddAccomplishment("You named " + text2 + " '" + name + "'.", "Blessed by divine beings, =name= discovered the legendary " + shortDisplayName + " known as '" + text + "'.", "general", JournalAccomplishment.MuralCategory.DoesSomethingRad, (flag || num2 > 0) ? JournalAccomplishment.MuralWeight.High : JournalAccomplishment.MuralWeight.Medium, null, -1L);
		}
		return true;
	}

	public static GameObject GetPreNamingVersion(GameObject obj)
	{
		string stringProperty = obj.GetStringProperty("PreNamingState");
		if (!string.IsNullOrEmpty(stringProperty))
		{
			GameObject gameObject = The.ZoneManager.peekCachedObject(stringProperty);
			if (gameObject != null)
			{
				return gameObject.DeepCopy();
			}
		}
		return null;
	}

	[WishCommand(null, null, Regex = "^itemnaming(?::([^:]*?)\\s*(?::\\s*([^:]*?))?\\s*)?$")]
	public static bool HandleItemNamingWish(Match match)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		string value = match.Groups[1].Value;
		string value2 = match.Groups[2].Value;
		if (!string.IsNullOrEmpty(value))
		{
			gameObject = GameObject.create(value);
			stringBuilder.Append("[Debug: Created " + gameObject.DebugName + " as kill.]\n");
		}
		if (!string.IsNullOrEmpty(value2))
		{
			gameObject2 = GameObject.create(value2);
			stringBuilder.Append("[Debug: Created " + gameObject2.DebugName + " as influencedBy.]\n");
		}
		if (stringBuilder.Length > 0)
		{
			Popup.Show(stringBuilder.ToString());
		}
		if (!Opportunity(The.Player, gameObject, gameObject2, "Wish", 0, 0, 0, 0, force: true))
		{
			Popup.Show("[Debug: Naming failed.]");
		}
		return true;
	}
}
