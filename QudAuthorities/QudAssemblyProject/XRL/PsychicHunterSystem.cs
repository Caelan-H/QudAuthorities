using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;
using XRL.World.Encounters;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL;

[Serializable]
[HasWishCommand]
public class PsychicHunterSystem : IGameSystem
{
	[NonSerialized]
	public Dictionary<string, bool> Visited = new Dictionary<string, bool>();

	public override void ZoneActivated(Zone zone)
	{
		CheckPsychicHunters(zone);
	}

	public override void LoadGame(SerializationReader Reader)
	{
		Visited = Reader.ReadDictionary<string, bool>();
	}

	public override void SaveGame(SerializationWriter Writer)
	{
		Writer.Write(Visited);
	}

	public void CreatePsychicHunter(int numHunters, Zone Z)
	{
		if (The.Player.GetPsychicGlimmer() < PsychicManager.GLIMMER_EXTRADIMENSIONAL_FLOOR)
		{
			CreateSeekerHunters(numHunters, Z);
		}
		else if (numHunters > 1)
		{
			if (50.in100())
			{
				CreateSeekerHunters(numHunters, Z);
			}
			else
			{
				CreateExtradimensionalCultHunters(Z, numHunters);
			}
		}
		else if (numHunters > 0)
		{
			int num = Stat.Random(1, 100);
			if (num <= 30)
			{
				CreateSeekerHunters(numHunters, Z);
			}
			else if (num <= 70)
			{
				CreateExtradimensionalSoloHunters(Z, numHunters);
			}
			else
			{
				CreateExtradimensionalCultHunters(Z, numHunters);
			}
		}
	}

	public static void CreateSeekerHunters(int numHunters, Zone Z)
	{
		bool flag = false;
		if (The.Game.PlayerReputation.get("Seekers") >= 250)
		{
			return;
		}
		XRL.World.GameObject gameObject = The.Player;
		for (int i = 1; i <= numHunters; i++)
		{
			XRL.World.GameObject gameObject2 = XRL.World.GameObject.create("PsychicSeekerHunter");
			gameObject2.pRender.SetForegroundColor("M");
			gameObject2.pBrain.Hostile = true;
			gameObject2.pBrain.Hibernating = false;
			gameObject2.pBrain.SetFeeling(gameObject, -100);
			gameObject2.Body?.CategorizeAllExcept(18, 6);
			gameObject2.AwardXP(gameObject.Stat("XP"));
			gameObject2.Statistics["Ego"].BaseValue = gameObject.Statistics["Ego"].Value;
			if (gameObject2.Statistics.ContainsKey("MP"))
			{
				gameObject2.Statistics["MP"].BaseValue = 0;
			}
			Mutations obj = gameObject.GetPart("Mutations") as Mutations;
			Mutations mutations = gameObject2.GetPart("Mutations") as Mutations;
			foreach (BaseMutation mutation in obj.MutationList)
			{
				MutationEntry mutationEntry = mutation.GetMutationEntry();
				if (mutationEntry?.Category == null || !(mutationEntry.Category.Name == "Mental") || mutationEntry.Cost <= 1)
				{
					continue;
				}
				List<MutationEntry> list = new List<MutationEntry>(gameObject2.GetPart<Mutations>().GetMutatePool());
				list.ShuffleInPlace();
				MutationEntry mutationEntry2 = null;
				foreach (MutationEntry item in list)
				{
					if (item.Category != null && item.Category.Name == "Mental" && !gameObject2.HasPart(item.Class) && item.Cost > 1)
					{
						mutationEntry2 = item;
						break;
					}
				}
				if (mutationEntry2 != null)
				{
					mutations.AddMutation(mutationEntry2.Class, mutation.BaseLevel);
				}
			}
			gameObject2.pBrain.PushGoal(new Kill(gameObject));
			string newValue = ((gameObject.GetPsychicGlimmer() < PsychicManager.GLIMMER_FLOOR + 15) ? "Osprey" : ((gameObject.GetPsychicGlimmer() < PsychicManager.GLIMMER_FLOOR + 30) ? "Harrier" : ((gameObject.GetPsychicGlimmer() < PsychicManager.GLIMMER_FLOOR + 45) ? "Owl" : ((gameObject.GetPsychicGlimmer() < PsychicManager.GLIMMER_FLOOR + 60) ? "Condor" : ((gameObject.GetPsychicGlimmer() < PsychicManager.GLIMMER_FLOOR + 75) ? "Strix" : ((gameObject.GetPsychicGlimmer() >= PsychicManager.GLIMMER_FLOOR + 90) ? "Rukh" : "Eagle"))))));
			string value = "Ptoh's " + Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.seekers.title.!random>").Replace("*rank*", newValue));
			string text = NameMaker.MakeName(gameObject2, null, null, null, null, null, null, null, null, "PsychicHunter", new Dictionary<string, string> { { "*Title*", value } }, FailureOkay: false, SpecialFaildown: true);
			gameObject2.DisplayName = "{{M|" + text + "}}";
			gameObject2.HasProperName = true;
			gameObject2.RequirePart<AbsorbablePsyche>();
			gameObject2.AddPart(new HasThralls(GetThrallRoll(gameObject.GetPsychicGlimmer()), stripGear: true));
			gameObject2.RemovePart("MentalShield");
			gameObject2.RemovePart("DroidScramblerWeakness");
			int num = 1000;
			while (num > 0)
			{
				num--;
				int x = Stat.Random(0, Z.Width - 1);
				int y = Stat.Random(0, Z.Height - 1);
				Cell cell = Z.GetCell(x, y);
				if (cell.IsEmpty())
				{
					cell.AddObject(gameObject2);
					gameObject2.MakeActive();
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			if (numHunters > 1)
			{
				Popup.Show("{{c|You sense the animus of a vast mind. They are near.}}");
			}
			else
			{
				Popup.Show("{{c|You sense the animus of a vast mind. Someone is near.}}");
			}
		}
	}

	public static void CreateExtradimensionalSoloHunters(Zone Z, int Number = 1, List<XRL.World.GameObject> ObjectList = null, bool Place = true, bool TeleportSwirl = false, bool UseMessage = true, bool UsePopup = true)
	{
		XRL.World.GameObject gameObject = The.Player;
		ExtraDimension randomElement = (The.Game.GetObjectGameState("PsychicManager") as PsychicManager).ExtraDimensions.GetRandomElement();
		int num = 0;
		for (int i = 0; i < Number; i++)
		{
			XRL.World.GameObject nonLegendaryCreatureAroundPlayerLevel = EncountersAPI.GetNonLegendaryCreatureAroundPlayerLevel();
			nonLegendaryCreatureAroundPlayerLevel.pRender.SetForegroundColor(randomElement.mainColor);
			nonLegendaryCreatureAroundPlayerLevel.pRender.DetailColor = "O";
			if (!nonLegendaryCreatureAroundPlayerLevel.HasProperty("PsychicHunter"))
			{
				nonLegendaryCreatureAroundPlayerLevel.SetStringProperty("PsychicHunter", "true");
			}
			nonLegendaryCreatureAroundPlayerLevel.pBrain.Hostile = true;
			nonLegendaryCreatureAroundPlayerLevel.pBrain.Calm = false;
			nonLegendaryCreatureAroundPlayerLevel.pBrain.Hibernating = false;
			nonLegendaryCreatureAroundPlayerLevel.pBrain.Aquatic = false;
			nonLegendaryCreatureAroundPlayerLevel.pBrain.Mobile = true;
			nonLegendaryCreatureAroundPlayerLevel.pBrain.SetFeeling(gameObject, -100);
			nonLegendaryCreatureAroundPlayerLevel.pBrain.FactionMembership.Clear();
			nonLegendaryCreatureAroundPlayerLevel.pBrain.FactionMembership.Add("Playerhater", 100);
			nonLegendaryCreatureAroundPlayerLevel.RequirePart<Combat>();
			if (nonLegendaryCreatureAroundPlayerLevel.GetPart("ConversationScript") is ConversationScript conversationScript)
			{
				conversationScript.Filter = "Weird";
				conversationScript.FilterExtras = randomElement.Name;
			}
			nonLegendaryCreatureAroundPlayerLevel.Body?.CategorizeAllExcept(18, 6);
			nonLegendaryCreatureAroundPlayerLevel.AwardXP(gameObject.Stat("XP"));
			nonLegendaryCreatureAroundPlayerLevel.Statistics["Ego"].BaseValue = gameObject.Statistics["Ego"].Value;
			if (nonLegendaryCreatureAroundPlayerLevel.Statistics.ContainsKey("MP"))
			{
				nonLegendaryCreatureAroundPlayerLevel.Statistics["MP"].BaseValue = 0;
			}
			Mutations obj = gameObject.GetPart("Mutations") as Mutations;
			Mutations mutations = nonLegendaryCreatureAroundPlayerLevel.GetPart("Mutations") as Mutations;
			foreach (BaseMutation mutation in obj.MutationList)
			{
				MutationEntry mutationEntry = mutation.GetMutationEntry();
				if (mutationEntry?.Category == null || !(mutationEntry.Category.Name == "Mental") || mutationEntry.Cost <= 1)
				{
					continue;
				}
				List<MutationEntry> list = new List<MutationEntry>(nonLegendaryCreatureAroundPlayerLevel.GetPart<Mutations>().GetMutatePool());
				list.ShuffleInPlace();
				MutationEntry mutationEntry2 = null;
				foreach (MutationEntry item in list)
				{
					if (item.Category != null && item.Category.Name == "Mental" && !nonLegendaryCreatureAroundPlayerLevel.HasPart(item.Class) && item.Cost > 1)
					{
						mutationEntry2 = item;
						break;
					}
				}
				if (mutationEntry2 != null)
				{
					mutations.AddMutation(mutationEntry2.Class, mutation.BaseLevel);
				}
			}
			string displayName = nonLegendaryCreatureAroundPlayerLevel.DisplayName;
			string text = NameMaker.MakeName(nonLegendaryCreatureAroundPlayerLevel, null, null, null, null, null, null, null, null, "PsychicHunter", null, FailureOkay: false, SpecialFaildown: true);
			text = randomElement.Weirdify(text);
			nonLegendaryCreatureAroundPlayerLevel.DisplayName = XRL.World.Event.NewStringBuilder().Append("{{O|").Append(text)
				.Append(", extradimensional ")
				.Append(displayName)
				.Append(" and esper ")
				.Append(HistoricStringExpander.ExpandString("<spice.commonPhrases.hunter.!random>"))
				.Append("}}")
				.ToString();
			nonLegendaryCreatureAroundPlayerLevel.HasProperName = true;
			nonLegendaryCreatureAroundPlayerLevel.pBrain.PushGoal(new Kill(gameObject));
			XRL.World.Parts.Temporary.AddHierarchically(nonLegendaryCreatureAroundPlayerLevel);
			nonLegendaryCreatureAroundPlayerLevel.RequirePart<AbsorbablePsyche>();
			nonLegendaryCreatureAroundPlayerLevel.AddPart(new Extradimensional("{{O|" + randomElement.Name.Replace("*dimensionSymbol*", ((char)randomElement.Symbol).ToString()) + "}}", randomElement.WeaponIndex, randomElement.MissileWeaponIndex, randomElement.ArmorIndex, randomElement.ShieldIndex, randomElement.MiscIndex, randomElement.Training, randomElement.SecretID));
			nonLegendaryCreatureAroundPlayerLevel.RequirePart<ExtradimensionalLoot>();
			nonLegendaryCreatureAroundPlayerLevel.AddPart(new RevealObservationOnLook(randomElement.SecretID));
			nonLegendaryCreatureAroundPlayerLevel.RemovePart("MentalShield");
			nonLegendaryCreatureAroundPlayerLevel.RemovePart("DroidScramblerWeakness");
			ObjectList?.Add(nonLegendaryCreatureAroundPlayerLevel);
			if (Place && PlaceHunter(Z, nonLegendaryCreatureAroundPlayerLevel, null, TeleportSwirl))
			{
				num++;
			}
		}
		if (UseMessage && num > 0)
		{
			PsychicPresenceMessage(num, UsePopup);
		}
	}

	public static void CreateExtradimensionalSoloDeviant(Zone Z)
	{
		XRL.World.GameObject gameObject = The.Player;
		ExtraDimension randomElement = (The.Game.GetObjectGameState("PsychicManager") as PsychicManager).ExtraDimensions.GetRandomElement();
		XRL.World.GameObject nonLegendaryCreatureAroundPlayerLevel = EncountersAPI.GetNonLegendaryCreatureAroundPlayerLevel();
		nonLegendaryCreatureAroundPlayerLevel.pRender.SetForegroundColor(randomElement.mainColor);
		nonLegendaryCreatureAroundPlayerLevel.pRender.DetailColor = "O";
		if (!nonLegendaryCreatureAroundPlayerLevel.HasProperty("PsychicHunter"))
		{
			nonLegendaryCreatureAroundPlayerLevel.SetStringProperty("PsychicHunter", "true");
		}
		nonLegendaryCreatureAroundPlayerLevel.pBrain.Hibernating = false;
		nonLegendaryCreatureAroundPlayerLevel.pBrain.Aquatic = false;
		nonLegendaryCreatureAroundPlayerLevel.pBrain.Mobile = true;
		nonLegendaryCreatureAroundPlayerLevel.pBrain.FactionMembership.Clear();
		nonLegendaryCreatureAroundPlayerLevel.pBrain.FactionMembership.Add("highly entropic beings", 100);
		nonLegendaryCreatureAroundPlayerLevel.RequirePart<Combat>();
		if (nonLegendaryCreatureAroundPlayerLevel.GetPart("ConversationScript") is ConversationScript conversationScript)
		{
			conversationScript.Filter = "Weird";
			conversationScript.FilterExtras = randomElement.Name;
		}
		nonLegendaryCreatureAroundPlayerLevel.Body?.CategorizeAllExcept(18, 6);
		nonLegendaryCreatureAroundPlayerLevel.AwardXP(gameObject.Stat("XP"));
		nonLegendaryCreatureAroundPlayerLevel.Statistics["Ego"].BaseValue = gameObject.Statistics["Ego"].Value;
		if (nonLegendaryCreatureAroundPlayerLevel.Statistics.ContainsKey("MP"))
		{
			nonLegendaryCreatureAroundPlayerLevel.Statistics["MP"].BaseValue = 0;
		}
		Mutations obj = gameObject.GetPart("Mutations") as Mutations;
		Mutations mutations = nonLegendaryCreatureAroundPlayerLevel.GetPart("Mutations") as Mutations;
		foreach (BaseMutation mutation in obj.MutationList)
		{
			MutationEntry mutationEntry = mutation.GetMutationEntry();
			if (mutationEntry?.Category == null || !(mutationEntry.Category.Name == "Mental") || mutationEntry.Cost <= 1)
			{
				continue;
			}
			List<MutationEntry> list = new List<MutationEntry>(nonLegendaryCreatureAroundPlayerLevel.GetPart<Mutations>().GetMutatePool());
			list.ShuffleInPlace();
			MutationEntry mutationEntry2 = null;
			foreach (MutationEntry item in list)
			{
				if (item.Category != null && item.Category.Name == "Mental" && !nonLegendaryCreatureAroundPlayerLevel.HasPart(item.Class) && item.Cost > 1)
				{
					mutationEntry2 = item;
					break;
				}
			}
			if (mutationEntry2 != null)
			{
				mutations.AddMutation(mutationEntry2.Class, mutation.BaseLevel);
			}
		}
		string displayName = nonLegendaryCreatureAroundPlayerLevel.DisplayName;
		string text = NameMaker.MakeName(nonLegendaryCreatureAroundPlayerLevel, null, null, null, null, null, null, null, null, "PsychicHunter", null, FailureOkay: false, SpecialFaildown: true);
		text = randomElement.Weirdify(text);
		nonLegendaryCreatureAroundPlayerLevel.DisplayName = XRL.World.Event.NewStringBuilder().Append("{{O|").Append(text)
			.Append(", ")
			.Append(displayName)
			.Append(" and transdimensional ")
			.Append(HistoricStringExpander.ExpandString("<spice.commonPhrases.entropist.!random>"))
			.Append("}}")
			.ToString();
		nonLegendaryCreatureAroundPlayerLevel.HasProperName = true;
		XRL.World.Parts.Temporary.AddHierarchically(nonLegendaryCreatureAroundPlayerLevel);
		nonLegendaryCreatureAroundPlayerLevel.RequirePart<AbsorbablePsyche>();
		nonLegendaryCreatureAroundPlayerLevel.AddPart(new Extradimensional("{{O|" + randomElement.Name.Replace("*dimensionSymbol*", ((char)randomElement.Symbol).ToString()) + "}}", randomElement.WeaponIndex, randomElement.MissileWeaponIndex, randomElement.ArmorIndex, randomElement.ShieldIndex, randomElement.MiscIndex, randomElement.Training, randomElement.SecretID));
		nonLegendaryCreatureAroundPlayerLevel.RequirePart<ExtradimensionalLoot>();
		nonLegendaryCreatureAroundPlayerLevel.AddPart(new RevealObservationOnLook(randomElement.SecretID));
		nonLegendaryCreatureAroundPlayerLevel.RemovePart("MentalShield");
		nonLegendaryCreatureAroundPlayerLevel.RemovePart("DroidScramblerWeakness");
		Debug.Log(nonLegendaryCreatureAroundPlayerLevel.DisplayName);
		Debug.Log((nonLegendaryCreatureAroundPlayerLevel.GetPart("Description") as Description).Short);
		int num = 1000;
		while (num > 0)
		{
			num--;
			int x = Stat.Random(0, Z.Width - 1);
			int y = Stat.Random(0, Z.Height - 1);
			Cell cell = Z.GetCell(x, y);
			if (cell.IsEmpty())
			{
				cell.AddObject(nonLegendaryCreatureAroundPlayerLevel);
				nonLegendaryCreatureAroundPlayerLevel.MakeActive();
				Popup.Show("{{c|You sense a psychic presence foreign to this place and time.}}");
				break;
			}
		}
	}

	public static void CreateExtradimensionalCultHunters(Zone Z, int Number = 1, List<XRL.World.GameObject> ObjectList = null, bool Place = true, bool TeleportSwirl = false, bool UseMessage = true, bool UsePopup = true)
	{
		XRL.World.GameObject gameObject = The.Player;
		PsychicFaction randomElement = (The.Game.GetObjectGameState("PsychicManager") as PsychicManager).PsychicFactions.GetRandomElement();
		string factionName = randomElement.factionName;
		int num = 0;
		for (int i = 0; i < Number; i++)
		{
			List<GameObjectBlueprint> factionMembers = GameObjectFactory.Factory.GetFactionMembers(factionName);
			factionMembers.ShuffleInPlace();
			GameObjectBlueprint gameObjectBlueprint = null;
			GameObjectBlueprint gameObjectBlueprint2 = null;
			int num2 = int.MaxValue;
			int num3 = gameObject.Stat("Level");
			int num4 = 0;
			foreach (GameObjectBlueprint item in factionMembers)
			{
				if (item.HasStat("Level") && EncountersAPI.IsEligibleForDynamicEncounters(item) && EncountersAPI.IsLegendaryEligible(item))
				{
					int value = item.GetStat("Level").Value;
					if (gameObjectBlueprint == null && value < num3)
					{
						gameObjectBlueprint = item;
						num4 = value;
					}
					else if (value < num3 && value > num4)
					{
						gameObjectBlueprint = item;
						num4 = value;
					}
					else if (value < num3 && value == num4 && 50.in100())
					{
						gameObjectBlueprint = item;
						num4 = value;
					}
					if (gameObjectBlueprint2 == null)
					{
						gameObjectBlueprint2 = item;
						num2 = value;
					}
					else if (value < num2)
					{
						gameObjectBlueprint2 = item;
						num2 = value;
					}
					else if (value == num2 && 50.in100())
					{
						gameObjectBlueprint2 = item;
						num2 = value;
					}
				}
			}
			GameObjectBlueprint gameObjectBlueprint3 = gameObjectBlueprint ?? gameObjectBlueprint2;
			if (gameObjectBlueprint3 == null)
			{
				Debug.Log("No member found from faction:" + factionName);
				return;
			}
			string name = gameObjectBlueprint3.Name;
			string preferredMutation = randomElement.preferredMutation;
			XRL.World.GameObject gameObject2 = XRL.World.GameObject.create(name);
			if (gameObject2.Stat("Level") - gameObject.Stat("Level") >= 10)
			{
				continue;
			}
			gameObject2.pRender.SetForegroundColor(randomElement.mainColor);
			gameObject2.pRender.DetailColor = "O";
			if (!gameObject2.HasProperty("PsychicHunter"))
			{
				gameObject2.SetStringProperty("PsychicHunter", "true");
			}
			gameObject2.pBrain.Hostile = true;
			gameObject2.pBrain.Calm = false;
			gameObject2.pBrain.Hibernating = false;
			gameObject2.pBrain.Aquatic = false;
			gameObject2.pBrain.Mobile = true;
			gameObject2.pBrain.SetFeeling(gameObject, -100);
			gameObject2.pBrain.FactionMembership.Clear();
			gameObject2.pBrain.FactionMembership.Add("Playerhater", 100);
			gameObject2.RequirePart<Combat>();
			if (gameObject2.GetPart("ConversationScript") is ConversationScript conversationScript)
			{
				conversationScript.Filter = "Weird";
				conversationScript.FilterExtras = randomElement.factionName;
			}
			gameObject2.Body?.CategorizeAllExcept(18, 6);
			gameObject2.AwardXP(gameObject.Stat("XP"));
			gameObject2.Statistics["Ego"].BaseValue = gameObject.Stat("Ego");
			if (gameObject2.HasStat("MP"))
			{
				gameObject2.GetStat("MP").BaseValue = 0;
			}
			Mutations obj = gameObject.GetPart("Mutations") as Mutations;
			Mutations mutations = gameObject2.GetPart("Mutations") as Mutations;
			bool flag = false;
			if (preferredMutation != "none")
			{
				mutations.AddMutation(preferredMutation, gameObject2.Stat("Level") / 4);
			}
			else
			{
				flag = true;
			}
			foreach (BaseMutation mutation in obj.MutationList)
			{
				if (!flag)
				{
					flag = true;
					continue;
				}
				MutationEntry mutationEntry = mutation.GetMutationEntry();
				if (mutationEntry?.Category == null || !(mutationEntry.Category.Name == "Mental") || mutationEntry.Cost <= 1)
				{
					continue;
				}
				List<MutationEntry> list = new List<MutationEntry>(gameObject2.GetPart<Mutations>().GetMutatePool());
				list.ShuffleInPlace();
				MutationEntry mutationEntry2 = null;
				foreach (MutationEntry item2 in list)
				{
					if (item2.Category != null && item2.Category.Name == "Mental" && !gameObject2.HasPart(item2.Class) && item2.Cost > 1)
					{
						mutationEntry2 = item2;
						break;
					}
				}
				if (mutationEntry2 != null)
				{
					mutations.AddMutation(mutationEntry2.Class, mutation.BaseLevel);
				}
			}
			string displayName = gameObject2.DisplayName;
			string text = NameMaker.MakeName(gameObject2, null, null, null, null, null, null, null, null, "PsychicHunter", null, FailureOkay: false, SpecialFaildown: true);
			text = randomElement.Weirdify(text);
			gameObject2.DisplayName = XRL.World.Event.NewStringBuilder().Append("{{O|").Append(text)
				.Append(", extradimensional ")
				.Append(displayName)
				.Append(" and esper from the ")
				.Append(randomElement.cultForm.Replace("*cultSymbol*", ((char)randomElement.cultSymbol).ToString()))
				.Append("}}")
				.ToString();
			gameObject2.HasProperName = true;
			gameObject2.pBrain.PushGoal(new Kill(gameObject));
			XRL.World.Parts.Temporary.AddHierarchically(gameObject2);
			gameObject2.RequirePart<AbsorbablePsyche>();
			gameObject2.AddPart(new Extradimensional("{{O|" + randomElement.dimensionName.Replace("*dimensionSymbol*", ((char)randomElement.dimensionSymbol).ToString()) + "}}", randomElement.dimensionalWeaponIndex, randomElement.dimensionalMissileWeaponIndex, randomElement.dimensionalArmorIndex, randomElement.dimensionalShieldIndex, randomElement.dimensionalMiscIndex, randomElement.dimensionalTraining, randomElement.dimensionSecretID));
			gameObject2.RequirePart<ExtradimensionalLoot>();
			gameObject2.AddPart(new RevealObservationOnLook(randomElement.dimensionSecretID));
			gameObject2.RemovePart("MentalShield");
			gameObject2.RemovePart("DroidScramblerWeakness");
			ObjectList?.Add(gameObject2);
			if (Place && PlaceHunter(Z, gameObject2, null, TeleportSwirl))
			{
				num++;
			}
		}
		if (UseMessage && num > 0)
		{
			PsychicPresenceMessage(num, UsePopup);
		}
	}

	public static bool PlaceHunter(Zone Zone, XRL.World.GameObject Hunter, Cell Cell = null, bool TeleportSwirl = false, string TeleportColor = "&C")
	{
		for (int i = 0; i < 1000; i++)
		{
			if (Cell != null)
			{
				break;
			}
			int x = Stat.Random(0, Zone.Width - 1);
			int y = Stat.Random(0, Zone.Height - 1);
			Cell cell = Zone.GetCell(x, y);
			if (cell.IsEmpty())
			{
				Cell = cell;
			}
		}
		if (Cell != null)
		{
			Cell.AddObject(Hunter);
			Hunter.MakeActive();
			if (TeleportSwirl)
			{
				Hunter.SmallTeleportSwirl(Cell, TeleportColor);
			}
			return true;
		}
		return false;
	}

	public static void PsychicPresenceMessage(int Number = 1, bool UsePopup = true)
	{
		string text = ((Number > 1) ? "psychic presences" : "a psychic presence");
		Messaging.XDidY(The.Player, "sense", text + " foreign to this place and time", null, "c", null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup);
	}

	public static int GetNumPsychicHunters(int glimmer, bool stableZone = false)
	{
		int num = Stat.Random(1, 1000);
		int num2 = 0;
		if (glimmer < 20)
		{
			return 0;
		}
		if (glimmer >= 20 && glimmer <= 49)
		{
			if (num >= (glimmer - 20 + 15) * 2)
			{
				return 0;
			}
			num2++;
		}
		else
		{
			if (!((double)num < (double)(glimmer - 40 + 105) * 0.666))
			{
				return 0;
			}
			num2++;
		}
		if (glimmer >= 35 && num2 > 0)
		{
			if (Stat.Random(1, 1000) >= (glimmer - 20) * 5)
			{
				return num2;
			}
			num2++;
		}
		if (glimmer >= 50)
		{
			if (Stat.Random(1, 1000) >= (glimmer - 20) * 5)
			{
				return num2;
			}
			num2++;
			if (Stat.Random(1, 1000) >= (glimmer - 20) * 5)
			{
				return num2;
			}
			num2++;
			if (glimmer >= 80)
			{
				if (Stat.Random(1, 1000) >= (glimmer - 50) * 5)
				{
					return num2;
				}
				num2++;
			}
		}
		return num2;
	}

	public static string GetThrallRoll(int glimmer)
	{
		string text = "0";
		if (glimmer <= PsychicManager.GLIMMER_FLOOR + 15)
		{
			return "0-1";
		}
		if (glimmer <= PsychicManager.GLIMMER_FLOOR + 30)
		{
			return "0-2";
		}
		if (glimmer <= PsychicManager.GLIMMER_FLOOR + 45)
		{
			return "1-2";
		}
		if (glimmer <= PsychicManager.GLIMMER_FLOOR + 60)
		{
			return "1-3";
		}
		if (glimmer <= PsychicManager.GLIMMER_FLOOR + 75)
		{
			return "2-3";
		}
		if (glimmer <= PsychicManager.GLIMMER_FLOOR + 90)
		{
			return "2-4";
		}
		return "3-4";
	}

	public static string GetPsychicGlimmerDescription(int glimmer)
	{
		string text = "";
		string text2 = ((glimmer <= PsychicManager.GLIMMER_FLOOR + 15) ? "Currently, you are being watched and pursued by ospreys, Ptoh's servants and birds of psychic prey who pluck larval espers from their egg sacs." : ((glimmer <= PsychicManager.GLIMMER_FLOOR + 30) ? "Currently, you are being watched and pursued by harriers, Ptoh's servants and birds of psychic prey who pluck fledgling espers from the shallows." : ((glimmer <= PsychicManager.GLIMMER_FLOOR + 45) ? "Currently, you are being watched and pursued by owls, Ptoh's servants and birds of psychic prey who snatch espers from the nighted weald." : ((glimmer <= PsychicManager.GLIMMER_FLOOR + 60) ? "Currently, you are being watched and pursued by condors, Ptoh's servants and birds of psychic prey who snatch thriving espers from the vast wood." : ((glimmer <= PsychicManager.GLIMMER_FLOOR + 75) ? "Currently, you are being watched and pursued by strixes, Ptoh's servants and birds of psychic prey who drink the blood of mature espers." : ((glimmer > PsychicManager.GLIMMER_FLOOR + 90) ? "Currently, you are being watched and pursued by rukhs, Ptoh's most powerful servants and birds of psychic prey who seize masterful espers from their belfries." : "Currently, you are being watched and pursued by eagles, Ptoh's servants and birds of psychic prey who seize powerful espers from their roosts."))))));
		if (glimmer >= PsychicManager.GLIMMER_EXTRADIMENSIONAL_FLOOR)
		{
			text = "\n\nYou are also visible to psychic beings from other dimensions.";
		}
		return "Your psychic glimmer represents how noticeable you are in the vast psychic aether. As your mental mutations increase in level, so does your psychic glimmer and the frequency, strength, and number of those who desire to absorb your mind." + "\n\n" + text2 + text;
	}

	public void CheckPsychicHunters(Zone Z)
	{
		if (Z.IsWorldMap())
		{
			return;
		}
		XRL.World.GameObject gameObject = The.Player;
		if (Visited.ContainsKey(Z.ZoneID))
		{
			return;
		}
		Visited.Add(Z.ZoneID, value: true);
		bool flag = gameObject.HasEffect("AmbientRealityStabilized") || Z.GetCell(0, 0).HasObject("AmbientStabilization");
		int numPsychicHunters = GetNumPsychicHunters(gameObject.GetPsychicGlimmer(), flag);
		if (numPsychicHunters > 0)
		{
			if (flag && 90.in100())
			{
				Messaging.EmitMessage(gameObject, "Some dimensional interlopers attempt to enter this region of spacetime, but the ambient normality field keeps them at bay.");
			}
			else
			{
				CreatePsychicHunter(numPsychicHunters, Z);
			}
		}
	}

	[WishCommand("extraculthunter", null)]
	public static void CultWish()
	{
		CreateExtradimensionalCultHunters(The.ActiveZone, 1, null, Place: true, TeleportSwirl: true);
	}

	[WishCommand("extraculthunter", null)]
	public static void CultWish(string Number)
	{
		if (int.TryParse(Number, out var result))
		{
			CreateExtradimensionalCultHunters(The.ActiveZone, result, null, Place: true, TeleportSwirl: true);
		}
	}

	[WishCommand("extrasolohunter", null)]
	public static void SoloWish()
	{
		CreateExtradimensionalSoloHunters(The.ActiveZone, 1, null, Place: true, TeleportSwirl: true);
	}

	[WishCommand("extrasolohunter", null)]
	public static void SoloWish(string Number)
	{
		if (int.TryParse(Number, out var result))
		{
			CreateExtradimensionalSoloHunters(The.ActiveZone, result, null, Place: true, TeleportSwirl: true);
		}
	}
}
