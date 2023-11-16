using System;
using System.Collections.Generic;
using System.Globalization;
using ConsoleLib.Console;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.QuestManagers;

[Serializable]
public class SpreadPax : QuestManager
{
	public List<PaxQuestStep> steps;

	[NonSerialized]
	public static string[] paxPlaces = new string[11]
	{
		"Joppa", "Kyakukya", "Asphalt Mines", "Rusted Archway", "Rustwell", "Red Rock", "Grit Gate", "Six Day Stilt", "Golgotha", "Bethesda Susa",
		"Omonporch"
	};

	[NonSerialized]
	public static string[] paxPlacesExcludeAltStarts = new string[1] { "Joppa" };

	[NonSerialized]
	public static Dictionary<string, string> paxPlacesPreposition = new Dictionary<string, string>
	{
		{ "Rusted Archway", "at" },
		{ "Rustwell", "at" },
		{ "Six Day Stilt", "at" }
	};

	[NonSerialized]
	public static Dictionary<string, string> paxPlacesDisplay = new Dictionary<string, string>
	{
		{ "Asphalt Mines", "the Asphalt Mines" },
		{ "Rusted Archway", "the Rusted Archway" },
		{ "Rustwell", "the Rust Wells" },
		{ "Six Day Stilt", "the Six Day Stilt" }
	};

	[NonSerialized]
	public static Dictionary<string, string> paxPlacesAlias = new Dictionary<string, string> { { "Six Day Stilt", "Stiltgrounds" } };

	[NonSerialized]
	public static string questName = "Spread Klanq around Qud in 4 Ways of Your Choosing";

	private bool hasStepName(string s)
	{
		for (int i = 0; i < steps.Count; i++)
		{
			if (steps[i].Name == s)
			{
				return true;
			}
		}
		return false;
	}

	private void Init()
	{
		if (steps != null)
		{
			return;
		}
		steps = new List<PaxQuestStep>();
		Stat.ReseedFrom("SpreadPax");
		List<string> list = new List<string>(paxPlaces);
		if (!The.Game.GetStringGameState("embark").Contains("Joppa"))
		{
			list.RemoveAll((Predicate<string>)((IEnumerable<string>)paxPlacesExcludeAltStarts).Contains);
		}
		int num = 6;
		while (steps.Count < num)
		{
			PaxQuestStep paxQuestStep = new PaxQuestStep();
			switch (Stat.Random(1, 4))
			{
			case 1:
				paxQuestStep.Name = "Spread Klanq Deep in the Earth";
				paxQuestStep.Target = "Underground:20";
				paxQuestStep.Text = "Puff Klanq spores at a depth of at least 20 levels.";
				break;
			case 2:
			{
				Faction randomFactionWithAtLeastOneMember = Factions.GetRandomFactionWithAtLeastOneMember((Faction f) => !f.Name.Contains("villagers of"));
				TextInfo textInfo2 = new CultureInfo("en-US", useUserOverride: false).TextInfo;
				paxQuestStep.Name = "Spread Klanq to " + textInfo2.ToTitleCase(randomFactionWithAtLeastOneMember.DisplayName);
				paxQuestStep.Target = "Faction:" + randomFactionWithAtLeastOneMember.Name;
				paxQuestStep.Text = "Puff Klanq spores on a sentient member of the " + randomFactionWithAtLeastOneMember.DisplayName + " faction.";
				break;
			}
			case 3:
			{
				string anObjectBlueprint = EncountersAPI.GetAnObjectBlueprint((GameObjectBlueprint ob) => ob.GetPartParameter("Physics", "IsReal", "true").EqualsNoCase("true") && ob.GetPartParameter("Physics", "Takeable", "true").EqualsNoCase("true") && !ob.HasPart("Brain") && !ob.HasPart("Combat") && !ob.HasTag("NoSparkingQuest"));
				GameObject gameObject = GameObject.create(anObjectBlueprint);
				TextInfo textInfo = new CultureInfo("en-US", useUserOverride: false).TextInfo;
				paxQuestStep.Name = "Spread Klanq onto " + gameObject.a + textInfo.ToTitleCase(ColorUtility.StripFormatting(gameObject.pRender.DisplayName));
				paxQuestStep.Target = "Item:" + anObjectBlueprint;
				paxQuestStep.Text = "Puff Klanq spores onto " + gameObject.a + ColorUtility.StripFormatting(gameObject.pRender.DisplayName) + ".";
				break;
			}
			case 4:
			{
				string randomElement = list.GetRandomElement();
				if (!paxPlacesDisplay.TryGetValue(randomElement, out var value))
				{
					value = randomElement;
				}
				if (!paxPlacesPreposition.TryGetValue(randomElement, out var value2))
				{
					value2 = "in";
				}
				paxQuestStep.Name = "Spread Klanq " + value2 + " " + value;
				paxQuestStep.Target = "Place:" + randomElement;
				paxQuestStep.Text = "Puff Klanq spores in the vicinity of " + value + ".";
				break;
			}
			}
			if (!hasStepName(paxQuestStep.Name))
			{
				steps.Add(paxQuestStep);
			}
		}
	}

	public static bool Start()
	{
		try
		{
			Quest quest = new Quest();
			quest.Manager = new SpreadPax();
			((SpreadPax)quest.Manager).Init();
			quest.ID = Guid.NewGuid().ToString();
			quest.Name = questName;
			quest.Level = 25;
			quest.Finished = false;
			quest.Accomplishment = "Conspiring with its eponymous mushroom scientist, you spread Klanq throughout Qud.";
			quest.Hagiograph = "Bless the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", when =name= cemented a historic alliance with the godhead Klanq and the two became one! Together the being known as Klanq-=name= puffed the Royal Vapor into every nook and crevice of Qud.";
			quest.HagiographCategory = "DoesSomethingRad";
			quest.StepsByID = new Dictionary<string, QuestStep>();
			quest.Manager.MyQuestID = quest.ID;
			for (int i = 0; i < ((SpreadPax)quest.Manager).steps.Count; i++)
			{
				QuestStep questStep = new QuestStep();
				questStep.ID = Guid.NewGuid().ToString();
				questStep.Name = ((SpreadPax)quest.Manager).steps[i].Name;
				questStep.Finished = false;
				questStep.Text = ((SpreadPax)quest.Manager).steps[i].Text;
				questStep.XP = 1500;
				quest.StepsByID.Add(questStep.Name, questStep);
			}
			IComponent<GameObject>.TheGame.StartQuest(quest);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("SpreadPax.Start", x);
		}
		return true;
	}

	public override void OnQuestAdded()
	{
		Init();
		IComponent<GameObject>.ThePlayer.AddPart(this);
		IComponent<GameObject>.ThePlayer.RegisterPartEvent(this, "ActivatePaxInfection");
	}

	public override void OnStepComplete(string StepName)
	{
		Init();
	}

	public override void OnQuestComplete()
	{
		Init();
		int i = 0;
		for (int count = IComponent<GameObject>.ThePlayer.PartsList.Count; i < count; i++)
		{
			IPart part = IComponent<GameObject>.ThePlayer.PartsList[i];
			if (part is SpreadPax && part.SameAs(this))
			{
				IComponent<GameObject>.ThePlayer.RemovePart(part);
				break;
			}
		}
		Body body = IComponent<GameObject>.ThePlayer.Body;
		if (body == null)
		{
			return;
		}
		foreach (BodyPart item in body.LoopParts())
		{
			if (!(item.Equipped?.Blueprint != "PaxInfection") && item.Equipped.Destroy(null, Silent: true))
			{
				string ordinalName = item.GetOrdinalName();
				string possessiveAdjective = IComponent<GameObject>.ThePlayer.GetPronounProvider().PossessiveAdjective;
				Popup.Show("The infected crust of skin on your " + ordinalName + " loosens and breaks away.");
				JournalAPI.AddAccomplishment("To the dismay of fungi everywhere, you cured the Pax Klanq infection on your " + ordinalName + ".", "Bless the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", when =name= dissolved a sham alliance with the treacherous fungus Klanq by eradicating it from " + possessiveAdjective + " " + ordinalName + "!", "general", JournalAccomplishment.MuralCategory.BodyExperienceNeutral, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			}
		}
	}

	public override bool SameAs(IPart p)
	{
		SpreadPax spreadPax = p as SpreadPax;
		if (spreadPax.steps == null != (steps == null))
		{
			return false;
		}
		if (steps != null)
		{
			if (spreadPax.steps.Count != steps.Count)
			{
				return false;
			}
			int i = 0;
			for (int count = steps.Count; i < count; i++)
			{
				if (spreadPax.steps[i].Name == steps[i].Name && spreadPax.steps[i].Text == steps[i].Text && spreadPax.steps[i].Target == steps[i].Target)
				{
					return false;
				}
			}
		}
		return base.SameAs(p);
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.findByBlueprint("Pax Klanq");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ActivatePaxInfection")
		{
			if (ParentObject.CurrentCell == null)
			{
				return true;
			}
			Init();
			for (int i = 0; i < steps.Count; i++)
			{
				if (steps[i].Finished)
				{
					continue;
				}
				if (steps[i].Target.StartsWith("Underground:"))
				{
					int num = int.Parse(steps[i].Target.Split(':')[1]) + 10;
					Zone currentZone = ParentObject.CurrentZone;
					if (currentZone != null && currentZone.Z >= num)
					{
						IComponent<GameObject>.TheGame.FinishQuestStep(MyQuestID, steps[i].Name);
						steps[i].Finished = true;
					}
				}
				else if (steps[i].Target.StartsWith("Faction:"))
				{
					string key = steps[i].Target.Split(':')[1];
					foreach (Cell localAdjacentCell in ParentObject.CurrentCell.GetLocalAdjacentCells(3, includeSelf: true))
					{
						for (int j = 0; j < localAdjacentCell.Objects.Count; j++)
						{
							if (localAdjacentCell.Objects[j].HasPart("Brain") && localAdjacentCell.Objects[j].pBrain.FactionMembership.ContainsKey(key) && localAdjacentCell.Objects[j].pBrain.FactionMembership[key] > 0)
							{
								IComponent<GameObject>.TheGame.FinishQuestStep(MyQuestID, steps[i].Name);
								steps[i].Finished = true;
								goto end_IL_020c;
							}
						}
						continue;
						end_IL_020c:
						break;
					}
				}
				else if (steps[i].Target.StartsWith("Item:"))
				{
					string text = steps[i].Target.Split(':')[1];
					foreach (Cell localAdjacentCell2 in ParentObject.CurrentCell.GetLocalAdjacentCells(3, includeSelf: true))
					{
						for (int k = 0; k < localAdjacentCell2.Objects.Count; k++)
						{
							if (localAdjacentCell2.Objects[k].Blueprint == text || RandomAltarBaetyl.DisplayNameMatches(localAdjacentCell2.Objects[k].Blueprint, text))
							{
								IComponent<GameObject>.TheGame.FinishQuestStep(MyQuestID, steps[i].Name);
								steps[i].Finished = true;
								goto end_IL_0325;
							}
						}
						continue;
						end_IL_0325:
						break;
					}
				}
				else if (steps[i].Target.StartsWith("Place:") && ParentObject.CurrentZone != null)
				{
					string text2 = steps[i].Target.Split(':')[1];
					string text3 = WorldFactory.Factory.ZoneDisplayName(ParentObject.CurrentZone.ZoneID);
					if (text3.Contains(text2, CompareOptions.IgnoreCase) || (paxPlacesAlias.TryGetValue(text2, out var value) && text3.Contains(value, CompareOptions.IgnoreCase)))
					{
						IComponent<GameObject>.TheGame.FinishQuestStep(MyQuestID, steps[i].Name);
						steps[i].Finished = true;
					}
				}
			}
			int num2 = 0;
			for (int l = 0; l < steps.Count; l++)
			{
				if (steps[l].Finished)
				{
					num2++;
				}
			}
			if (num2 >= 4)
			{
				IComponent<GameObject>.TheGame.FinishQuest(MyQuestID);
			}
		}
		return base.FireEvent(E);
	}
}
