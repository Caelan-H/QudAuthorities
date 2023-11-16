using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class CherubimSpawner : IPart
{
	public string Group = "A";

	public int Period = 1;

	public bool bDynamic = true;

	private static string cherubDescription = "Gallium veins press against the underside of =pronouns.possessive= crystalline *skin* and gleam warmly. =pronouns.Possessive= body is perfect, and the whole of it is wet with amniotic slick; could =pronouns.subjective= have just now peeled =pronouns.reflexive= off an oil canvas? =verb:Were:afterpronoun= =pronouns.subjective= cast into the material realm by a dreaming, dripping brain? Whatever the embryo, =pronouns.subjective= =verb:are:afterpronoun= now the archetypal *creatureType*; it's all there in impeccable simulacrum: *features*. Perfection is realized.";

	private static string mechanicCherubDescription = "Dials tick and vacuum tubes mantle under synthetic *skin* and inside plastic joints. *features* are wrought from a vast and furcate machinery into the ideal form of the *creatureType*. By the artistry of =pronouns.possessive= construction, =pronouns.subjective= closely =verb:resemble:afterpronoun= =pronouns.possessive= referent, but an exposed cog here and an exhaust valve there betray the truth of =pronouns.possessive= nature. =pronouns.Possessive= movements are short and mimetic; =pronouns.subjective= =verb:inhabit:afterpronoun= the valley between the mountains of life and imagination.";

	private static List<string> randomCherubimFaction = new List<string>
	{
		"Prey", "Baboons", "Apes", "Crabs", "Bears", "Winged Mammals", "Birds", "Fish", "Insects", "Swine",
		"Reptiles", "Cannibals", "Arachnids", "Cats", "Dogs", "Mollusks", "Tortoises", "Robots", "Baetyls", "Antelopes",
		"Worms", "Oozes", "Equines", "Newly Sentient Beings", "Hermits", "Frogs", "Strangers", "Flowers", "Roots", "Fungi",
		"Vines", "Urchins", "Succulents", "Trees"
	};

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		string text = "";
		if (Period >= 4)
		{
			text = "Mechanical ";
		}
		HistoricEntitySnapshot sultan = HistoryAPI.GetSultanForPeriod(Period);
		string AStateName = "cherubim" + Period + "Afaction";
		string AFaction = The.Game.RequireGameState(AStateName, delegate
		{
			string text7 = null;
			if (sultan == null)
			{
				MetricsManager.LogError("no sultan found for period " + Period + " generating " + AStateName);
			}
			else
			{
				List<string> list2 = sultan.GetList("likedFactions");
				if (list2 == null || list2.Count == 0)
				{
					MetricsManager.LogError("no liked factions found for period " + Period + " sultan generating " + AStateName);
				}
				else
				{
					IEnumerable<string> enumerable2 = list2.Where((string f) => randomCherubimFaction.Contains(f));
					if (enumerable2 == null)
					{
						MetricsManager.LogError("no eligible factions found for period " + Period + " sultan generating " + AStateName);
					}
					else
					{
						text7 = enumerable2.GetRandomElement();
					}
				}
			}
			if (text7 == null)
			{
				text7 = randomCherubimFaction.GetRandomElement();
			}
			return text7;
		});
		string BStateName = "cherubim:" + Period + "Bfaction";
		string text2 = The.Game.RequireGameState(BStateName, delegate
		{
			string text6 = null;
			if (sultan == null)
			{
				MetricsManager.LogError("no sultan found for period " + Period + " generating " + BStateName);
			}
			else
			{
				List<string> list = sultan.GetList("likedFactions");
				if (list == null || list.Count == 0)
				{
					MetricsManager.LogError("no liked factions found for period " + Period + " sultan generating " + BStateName);
				}
				else
				{
					IEnumerable<string> enumerable = list.Where((string f) => randomCherubimFaction.Contains(f) && f != AFaction);
					if (enumerable == null)
					{
						MetricsManager.LogError("no eligible factions found for period " + Period + " sultan generating " + BStateName);
					}
					else
					{
						text6 = enumerable.GetRandomElement();
					}
				}
			}
			if (text6 == null)
			{
				text6 = AFaction;
			}
			return text6;
		});
		string AElement = The.Game.RequireGameState("cherubim" + Period + "Aelement", () => sultan?.GetList("elements")?.GetRandomElement() ?? randomCherubimFaction.GetRandomElement());
		string text3 = The.Game.RequireGameState("cherubim:" + Period + "Belement", () => sultan?.GetList("elements")?.Where((string e) => e != AElement)?.GetRandomElement() ?? AElement);
		string text4 = ((Group == "A") ? AFaction : text2);
		string text5 = ((Group == "A") ? AElement : text3);
		GameObject gameObject = GameObject.create(text + text4 + " Cherub");
		gameObject.SetStringProperty("SpawnedFrom", ParentObject.Blueprint);
		gameObject.pRender.RenderString = "\u008f";
		if (text == "Mechanical ")
		{
			gameObject.pRender.RenderString = "\u008e";
		}
		gameObject.pRender.DetailColor = "W";
		if (text == "Mechanical ")
		{
			gameObject.pRender.DisplayName = gameObject.pRender.DisplayName.Replace("mechanical ", "");
		}
		if (text == "Mechanical ")
		{
			string newValue = (gameObject.HasTag("AlternateCreatureType") ? gameObject.GetTag("AlternateCreatureType") : gameObject.pRender.DisplayName.Substring(0, gameObject.pRender.DisplayName.IndexOf(' ')));
			string newValue2 = Grammar.InitCap(Grammar.MakeAndList(gameObject.GetxTag("TextFragments", "PoeticFeatures").Split(',').ToList()));
			(gameObject.GetPart("Description") as Description)._Short = mechanicCherubDescription.Replace("*skin*", gameObject.GetxTag("TextFragments", "Skin")).Replace("*creatureType*", newValue).Replace("*features*", newValue2);
		}
		else
		{
			string newValue3 = (gameObject.HasTag("AlternateCreatureType") ? gameObject.GetTag("AlternateCreatureType") : gameObject.pRender.DisplayName.Substring(0, gameObject.pRender.DisplayName.IndexOf(' ')));
			string newValue4 = "the " + string.Join(", the ", gameObject.GetxTag("TextFragments", "PoeticFeatures").Split(','));
			(gameObject.GetPart("Description") as Description)._Short = cherubDescription.Replace("*skin*", gameObject.GetxTag("TextFragments", "Skin")).Replace("*creatureType*", newValue3).Replace("*features*", newValue4);
		}
		if (text == "Mechanical ")
		{
			gameObject.RequirePart<HeatSelfOnFreeze>().HeatAmount = "50";
			gameObject.RequirePart<ReflectProjectiles>().Chance = 50;
		}
		else
		{
			gameObject.RequirePart<HeatSelfOnFreeze>();
			gameObject.RequirePart<ReflectProjectiles>();
		}
		if (text == "Mechanical ")
		{
			gameObject.RequirePart<Robot>().EMPable = false;
			gameObject.RequirePart<MentalShield>();
			gameObject.RequirePart<Metal>();
			if (!gameObject.HasPart("NightVision"))
			{
				gameObject.GetPart<Mutations>().AddMutation(new DarkVision(), 12);
			}
			if (gameObject.GetPart("Corpse") is Corpse corpse)
			{
				corpse.CorpseChance = 0;
				corpse.BurntCorpseChance = 0;
				corpse.VaporizedCorpseChance = 0;
			}
			gameObject.SetIntProperty("Inorganic", 1);
			gameObject.SetIntProperty("Bleeds", 1);
			gameObject.SetStringProperty("SeveredLimbBlueprint", "RobotLimb");
			gameObject.SetStringProperty("SeveredHeadBlueprint", "RobotHead7");
			gameObject.SetStringProperty("SeveredFaceBlueprint", "RobotFace");
			gameObject.SetStringProperty("SeveredArmBlueprint", "RobotArm");
			gameObject.SetStringProperty("SeveredHandBlueprint", "RobotHand");
			gameObject.SetStringProperty("SeveredLegBlueprint", "RobotLeg");
			gameObject.SetStringProperty("SeveredFootBlueprint", "RobotFoot");
			gameObject.SetStringProperty("SeveredFeetBlueprint", "RobotFeet");
			gameObject.SetStringProperty("SeveredTailBlueprint", "RobotTail");
			gameObject.SetStringProperty("SeveredRootsBlueprint", "RobotRoots");
			gameObject.SetStringProperty("SeveredFinBlueprint", "RobotFin");
			gameObject.SetStringProperty("BleedLiquid", "oil-1000");
			gameObject.SetStringProperty("BleedColor", "&K");
			gameObject.SetStringProperty("BleedPrefix", "&Koily");
			gameObject.Body?.CategorizeAll(7);
		}
		if (!bDynamic)
		{
			gameObject.pBrain.Wanders = false;
			gameObject.pBrain.WandersRandomly = false;
			gameObject.pBrain.FactionMembership.Clear();
			gameObject.pBrain.FactionMembership.Add("Cherubim", 100);
			gameObject.SetIntProperty("HelpModifier", 500);
			gameObject.SetIntProperty("CherubimLock", 1);
			gameObject.SetIntProperty("MarkOfDeathGuardian", 1);
			gameObject.SetIntProperty("StaysOnZLevel", 1);
		}
		switch (text5)
		{
		case "glass":
			gameObject.AddPart<ReflectDamage>().ReflectPercentage = 25;
			gameObject.AddPart<ModGlazed>().Chance = 10;
			gameObject.pRender.DisplayName = "glass " + gameObject.pRender.DisplayName;
			gameObject.pRender.ColorString = "&K";
			gameObject.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of glass cherubim.\n• Attacks have a 10% chance to dismember.\n• Reflects 25% damage back at attackers.";
			break;
		case "jewels":
			gameObject.Statistics["Ego"].BaseValue += 10;
			gameObject.AddPart<ModTransmuteOnHit>().ChancePerThousand = 50;
			gameObject.pRender.DisplayName = "jeweled " + gameObject.pRender.DisplayName;
			gameObject.pRender.ColorString = "&M";
			gameObject.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of jeweled cherubim.\n• +10 Ego.\n• Attacks have a small chance to transmute opponents into gemstones.";
			break;
		case "stars":
			gameObject.GetPart<Mutations>().AddMutation(new LightManipulation(), 10);
			gameObject.pRender.DisplayName = "star " + gameObject.pRender.DisplayName;
			gameObject.pRender.ColorString = "&Y";
			gameObject.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of star cherubim.\n• Light Manipulation 10";
			break;
		case "time":
			gameObject.GetPart<Mutations>().AddMutation(new TemporalFugue(), 10);
			gameObject.pRender.DisplayName = "time " + gameObject.pRender.DisplayName;
			gameObject.pRender.ColorString = "&b";
			gameObject.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of time cherubim.\n• Temporal Fugue 10";
			break;
		case "salt":
			gameObject.Statistics["Willpower"].BaseValue += 10;
			gameObject.Statistics["Hitpoints"].BaseValue *= 2;
			gameObject.pRender.DisplayName = "salt " + gameObject.pRender.DisplayName;
			gameObject.pRender.ColorString = "&y";
			gameObject.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of salt cherubim.\n• +10 Willpower\n• +100% HP";
			break;
		case "ice":
			gameObject.Statistics["ColdResistance"].BaseValue += 100;
			gameObject.GetPart<Mutations>().AddMutation(new IceBreather(), 10);
			gameObject.pRender.DisplayName = "ice " + gameObject.pRender.DisplayName;
			gameObject.pRender.ColorString = "&C";
			gameObject.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of ice cherubim.\n• +100 Cold Resist\n• Ice Breath 10";
			break;
		case "scholarship":
			gameObject.Statistics["Intelligence"].BaseValue += 10;
			gameObject.AddPart<ModBeetlehost>().Chance = 100;
			gameObject.pRender.DisplayName = "learned " + gameObject.pRender.DisplayName;
			gameObject.pRender.ColorString = "&B";
			gameObject.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of learned cherubim.\n• +10 Intelligence\n• Attacks discharge clockwork beetles.";
			break;
		case "might":
			gameObject.Statistics["Strength"].BaseValue += 20;
			gameObject.pRender.DisplayName = "mighty " + gameObject.pRender.DisplayName;
			gameObject.pRender.ColorString = "&r";
			gameObject.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of mighty cherubim.\n• +20 Strength";
			break;
		case "chance":
		{
			ModBlinkEscape modBlinkEscape = gameObject.RequirePart<ModBlinkEscape>();
			modBlinkEscape.WorksOnEquipper = false;
			modBlinkEscape.WorksOnSelf = true;
			gameObject.RegisterPartEvent(modBlinkEscape, "BeforeApplyDamage");
			modBlinkEscape.Tier = 20;
			gameObject.AddPart<ModFatecaller>().Chance = 20;
			gameObject.pRender.DisplayName = "chaotic " + gameObject.pRender.DisplayName;
			gameObject.pRender.ColorString = "&m";
			gameObject.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of chaotic cherubim.\n• Whenever this creature is about to take damage, there's a 25% chance they blink away instead.\n• Whenever this creature attacks, 50% of the time the Fates have their way.";
			break;
		}
		case "circuitry":
			gameObject.Statistics["ElectricResistance"].BaseValue += 100;
			gameObject.GetPart<Mutations>().AddMutation(new ElectricalGeneration(), 10);
			gameObject.pRender.DisplayName = "electric " + gameObject.pRender.DisplayName;
			gameObject.pRender.ColorString = "&W";
			gameObject.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of electric cherubim.\n• +100 Electrical Resist\n• Electrical Generation 10";
			break;
		case "travel":
			gameObject.Statistics["Speed"].BaseValue += 5;
			gameObject.GetPart<Mutations>().AddMutation(new Teleportation(), 10);
			gameObject.pRender.DisplayName = "quickened " + gameObject.pRender.DisplayName;
			gameObject.pRender.ColorString = "&g";
			gameObject.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of quickened cherubim.\n• +5 Quickness\n• Teleportation 10";
			break;
		}
		if (text == "Mechanical ")
		{
			gameObject.pRender.DisplayName = "mechanical " + gameObject.pRender.DisplayName;
		}
		E.ReplacementObject = gameObject;
		return base.HandleEvent(E);
	}
}
