using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qud.API;
using XRL.Core;
using XRL.UI;
using XRL.World.Conversations;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class Neelahind : IPart
{
	[NonSerialized]
	public List<JournalObservation> circumstances;

	[NonSerialized]
	public JournalObservation selectedCircumstance;

	[NonSerialized]
	public List<JournalObservation> motives;

	[NonSerialized]
	public JournalObservation selectedMotive;

	public List<string> eliminatedSuspects = new List<string>();

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ShowConversationChoices");
		Object.RegisterPartEvent(this, "GetConversationNode");
		Object.RegisterPartEvent(this, "VisitConversationNode");
		base.Register(Object);
	}

	public ConversationNode generateCircumstanceNode()
	{
		ConversationNode conversationNode = new ConversationNode();
		conversationNode.ID = "*circumstance";
		conversationNode.Text = "I see. What is it?";
		conversationNode.Choices = new List<ConversationChoice>();
		circumstances = HindrenMysteryGamestate.instance.getKnownFreeClues();
		int num = 0;
		foreach (JournalObservation circumstance in circumstances)
		{
			conversationNode.AddChoice(circumstance.text, "*selectCircumstance:" + num);
			num++;
		}
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.GotoID = "End";
		conversationChoice.Text = "I'm still not completely certain. Let me ponder it more.";
		conversationNode.Choices.Add(conversationChoice);
		return conversationNode;
	}

	public ConversationNode generateMotiveNode()
	{
		string text = selectedCircumstance.attributes.Where((string o) => o.StartsWith("influence:")).First().Split(':')[1];
		ConversationNode conversationNode = new ConversationNode();
		conversationNode.ID = "*motive";
		conversationNode.Text = "Signs of " + text + ", seemingly hidden from the village. But that can't be your only evidence, can it? There's no culprit!";
		conversationNode.Choices = new List<ConversationChoice>();
		motives = HindrenMysteryGamestate.instance.getKnownMotiveClues();
		int num = 0;
		foreach (JournalObservation motive in motives)
		{
			conversationNode.AddChoice(motive.text, "*selectMotive:" + num);
			num++;
		}
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.GotoID = "End";
		conversationChoice.Text = "I'm still not completely certain. Let me ponder it more.";
		conversationNode.Choices.Add(conversationChoice);
		return conversationNode;
	}

	public ConversationNode getExclusionNode(JournalObservation circumstance, string thief)
	{
		if (eliminatedSuspects.Count((string o) => o == thief) >= 1)
		{
			eliminatedSuspects.Add(thief);
			ConversationNode conversationNode = new ConversationNode();
			conversationNode.ID = "*exclusion";
			conversationNode.Text = "I'm afraid that well is poisoned due to your false accusation: " + GetThiefDisplayName(thief) + " isn't the culprit.";
			conversationNode.Choices = new List<ConversationChoice>();
			ConversationChoice item = new ConversationChoice
			{
				GotoID = "End",
				Text = "Ah."
			};
			conversationNode.Choices.Add(item);
			return conversationNode;
		}
		string text = null;
		if (thief == "kendren")
		{
			if (circumstance.id == "Clue_BloodyPatchOfFur")
			{
				text = "Hmm. But the only fur there was from a hindren's pelt, and kendren warriors fight to the death. With no corpse and no other fur, it seems unlikely that a hindren fought an outsider.";
			}
			if (circumstance.id == "Clue_SawATrader")
			{
				text = "I'm afraid that makes no sense. Why would an outsider invite a trader to our village, when they could simply meet one outside of Bey Lah?";
			}
			if (circumstance.id == "Clue_SoreThroats")
			{
				text = "But weren't the rumors of sore throats in the village? That wouldn't have anything to do with kendren.";
			}
			if (circumstance.id == "Clue_LeatherGoneMissing")
			{
				text = "Why wouldn't an outsider bring their own leather? It would better disguise Kindrish. It seems a lot of work to find and raid our leather stores.";
			}
			if (circumstance.id == "Clue_WornBracer")
			{
				text = "Bracers are a Bey Lah craft! It would make no sense for a kendren to have one, then throw it away in the village.";
			}
		}
		else if (thief == "kese")
		{
			if (circumstance.id == "Clue_SplinteredWeaponHaft")
			{
				text = "Kesehind wields Ari, a battleaxe handed down through generations. It cannot break, and if it broke a weapon there would be scorch marks on it. This evidence does not hold up.";
			}
			if (circumstance.id == "Clue_TalkingLateAtNight")
			{
				text = "There are few voices as low as Kesehind's in our village. Witnesses report a higher-pitched voice, don't they? They would know if Kesehind were speaking.";
			}
			if (circumstance.id == "Clue_YuckwheatChaff")
			{
				text = "Oh, no, that wouldn't be Kesehind. He'd sooner die of fever than choke down a single stem of yuckwheat.";
			}
			if (circumstance.id == "Clue_YuckwheatStench")
			{
				text = "Oh, no, that wouldn't be Kesehind. He'd sooner die of fever than choke down a single stem of yuckwheat.";
			}
			if (circumstance.id == "Clue_SoundOfToolsAtNight")
			{
				text = "That would be nigh unto impossible for Kesehind. He cannot perform fine tasks by torchlight; his night vision is too poor.";
			}
		}
		else if (thief == "esk")
		{
			if (circumstance.id == "Clue_BloodyWatervine")
			{
				text = "No, that can't be it. Eskhind hates everything about watervine and keeps a wide berth from it. If her blood was spilled, it would have happened nearer to the path.";
			}
			if (circumstance.id == "Clue_BulgingWaterskin")
			{
				text = "No, no. Eskhind makes the most of what few possessions she keeps, and if she traded a treasure for so much water, she'd never have lost track of any.";
			}
			if (circumstance.id == "Clue_GreatSaltbackPrint")
			{
				text = "Oh, no, Eskhind is deathly afraid of saltbacks. She would never get close enough to trade with a dromad merchent.";
			}
			if (circumstance.id == "Clue_PoolOfPutrescence")
			{
				text = "I doubt it. If Eskhind were sick, the last place she would stumble to evacuate her guts would be a watervine field.";
			}
			if (circumstance.id == "Clue_ForeignLeatherworkingTools")
			{
				text = "I'm afraid that Eskhind has no idea how to use proper leatherworking tools, much less pay for a set so fine. This surely isn't hers.";
			}
		}
		else if (thief == "keh")
		{
			if (circumstance.id == "Clue_HeardFighting")
			{
				text = "I know the rumor you mean, but villagers heard the clash of weapons. Keh is too old for a prolonged fight like that, so I do not believe it was her.";
			}
			if (circumstance.id == "Clue_HeardLoudSplashing")
			{
				text = "Grand-Doe cannot swim, and panics even near the shallow pools of our paddies. I cannot imagine she would make splashing noises without adding screaming noises to them.";
			}
			if (circumstance.id == "Clue_CopperNugget")
			{
				text = "Copper causes Grand-Doe to break out in great welts. She would not have traded for it, and if she saw a trader drop it, surely she would have told them to pick it up. She hates mess.";
			}
			if (circumstance.id == "Clue_SeveredTongue")
			{
				text = "I cannot bring myself to believe that Grand-Doe would allow anyone to stumble upon her own severed tongue. She would surely burn it as soon as she saw it.";
			}
			if (circumstance.id == "Clue_LeatherScraps")
			{
				text = "No, Grand-Doe is absolutely meticulous about work clutter. She wouldn't leave scraps lying about.";
			}
		}
		if (text != null)
		{
			ConversationNode conversationNode2 = new ConversationNode();
			conversationNode2.ID = "*exclusion";
			conversationNode2.Text = text;
			conversationNode2.Choices = new List<ConversationChoice>();
			ConversationChoice item2 = new ConversationChoice
			{
				GotoID = "End",
				Text = "Ah.",
				onAction = delegate
				{
					eliminatedSuspects.Add(thief);
					return true;
				}
			};
			conversationNode2.Choices.Add(item2);
			return conversationNode2;
		}
		return null;
	}

	public ConversationNode generateAccusationNode()
	{
		string influence = selectedCircumstance.attributes.Where((string o) => o.StartsWith("influence:")).First().Split(':')[1];
		string text = selectedMotive.attributes.Where((string o) => o.StartsWith("influence:")).First().Split(':')[1];
		string thief = selectedMotive.attributes.Where((string o) => o.StartsWith("motive:")).First().Split(':')[1];
		ConversationNode exclusionNode = getExclusionNode(selectedCircumstance, thief);
		if (exclusionNode != null)
		{
			return exclusionNode;
		}
		ConversationNode conversationNode = new ConversationNode();
		conversationNode.ID = "*accusation";
		conversationNode.Text = "Oh. Yes, this does indicate that some kind of " + text + " transpired, and " + GetThiefDisplayName(thief) + " was trying to hide it.\n\nSo what you're saying is....";
		conversationNode.Choices = new List<ConversationChoice>();
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.GotoID = "*makeAccusation";
		conversationChoice.Text = "That's right. By concealing " + collapseInfluence(influence, text) + ", it was " + GetThiefDisplayName(thief) + " who stole Kindrish!";
		conversationNode.Choices.Add(conversationChoice);
		ConversationChoice conversationChoice2 = new ConversationChoice();
		conversationChoice2.GotoID = "End";
		conversationChoice2.Text = "I'm still not completely certain. Let me ponder it more.";
		conversationNode.Choices.Add(conversationChoice2);
		return conversationNode;
	}

	public string collapseInfluence(string influence1, string influence2)
	{
		if (string.Equals(influence1, influence2))
		{
			return influence1;
		}
		return influence1 + " and " + influence2;
	}

	public ConversationNode generateFinaleNode()
	{
		string circumstanceInfluence = selectedCircumstance.attributes.Where((string o) => o.StartsWith("influence:")).First().Split(':')[1];
		string motiveInfluence = selectedMotive.attributes.Where((string o) => o.StartsWith("influence:")).First().Split(':')[1];
		string thief = selectedMotive.attributes.Where((string o) => o.StartsWith("motive:")).First().Split(':')[1];
		HindrenQuestOutcome newOutcome = new HindrenQuestOutcome();
		newOutcome.loveState = ((The.Game.GetQuestFinishTime("Love and Fear") > 0) ? "love" : "nolove");
		newOutcome.thief = thief;
		newOutcome.circumstance = circumstanceInfluence;
		newOutcome.motive = motiveInfluence;
		List<string> list = new List<string> { "trade", "craft" };
		List<string> list2 = new List<string> { "illness", "violence" };
		if (list.Contains(circumstanceInfluence) && list.Contains(motiveInfluence))
		{
			newOutcome.climate = "prosperous";
		}
		else if (list2.Contains(circumstanceInfluence) && list2.Contains(motiveInfluence))
		{
			newOutcome.climate = "tumultuous";
		}
		else
		{
			newOutcome.climate = "mixed";
		}
		GameObject outcomeGameObject = GameObject.create("Widget");
		outcomeGameObject.AddPart(newOutcome);
		ConversationNode value = null;
		Func<bool> onAction = delegate
		{
			HindrenMysteryGamestate.instance.getBeyLahZone().GetCell(0, 0).AddObject(outcomeGameObject);
			XRLCore.Core.Game.SetStringGameState("HindrenMysteryOutcomeClimate", newOutcome.climate);
			XRLCore.Core.Game.SetStringGameState("HindrenMysteryOutcomeCircumstance", newOutcome.circumstance);
			XRLCore.Core.Game.SetStringGameState("HindrenMysteryOutcomeThief", thief);
			XRLCore.Core.Game.SetStringGameState("HindrenMysteryOutcomeHindriarch", newOutcome.CurrentHindriarch());
			XRLCore.Core.Game.SetStringGameState("HindrenMysteryOutcomeClimate" + newOutcome.climate, "1");
			XRLCore.Core.Game.SetStringGameState("HindrenMysteryOutcomeThief" + thief, "1");
			XRLCore.Core.Game.SetStringGameState("HindrenMysteryOutcomeCircumstance" + newOutcome.circumstance, "1");
			XRLCore.Core.Game.SetStringGameState("HindrenMysteryOutcomeHindriarch" + newOutcome.CurrentHindriarch(), "1");
			giveAward(newOutcome.climate, motiveInfluence, circumstanceInfluence, thief);
			The.Game.FinishQuest("Kith and Kin");
			The.Game.FinishQuest("Find Eskhind");
			newOutcome.ApplyPrologue();
			return true;
		};
		if (ConversationLoader.Loader.ConversationsByID["Neelahind"].NodesByID.TryGetValue(newOutcome.outcome, out value))
		{
			ConversationNode conversationNode = new ConversationNode();
			conversationNode.Copy(value);
			conversationNode.Choices[0].onAction = onAction;
			return conversationNode;
		}
		ConversationNode conversationNode2 = new ConversationNode();
		conversationNode2.ID = "*accusation";
		conversationNode2.Text = "[Missing outcome conversation node; " + newOutcome.outcome + "]";
		conversationNode2.Choices = new List<ConversationChoice>();
		ConversationChoice item = new ConversationChoice
		{
			GotoID = "End",
			Text = "Live and drink, Neelahind.",
			onAction = onAction
		};
		conversationNode2.Choices.Add(item);
		return conversationNode2;
	}

	public void giveAward(string climate, string motiveInfluence, string circumstanceInfluence, string thief)
	{
		List<GameObject> list = new List<GameObject>();
		List<string> list2 = new List<string>();
		bool flag = false;
		switch (thief)
		{
		case "keh":
			switch (motiveInfluence)
			{
			case "violence":
				if (circumstanceInfluence == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Dagger3"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Dagger3"));
				}
				break;
			case "illness":
			{
				int num4 = 4;
				if (circumstanceInfluence == "violence")
				{
					num4 = 8;
				}
				for (int l = 0; l < num4; l++)
				{
					list.Add(GameObject.create(PopulationManager.RollOneFrom("DynamicObjectsTable:Tonics_NonRare").Blueprint));
				}
				break;
			}
			case "craft":
				if (circumstanceInfluence == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Homoelectric Wrist Warmer"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Homoelectric Wrist Warmer"));
				}
				break;
			case "trade":
			{
				int num3 = 3;
				if (circumstanceInfluence == "violence")
				{
					num3 = 8;
				}
				if (circumstanceInfluence == "trade")
				{
					num3 = 6;
					flag = true;
				}
				for (int k = 0; k < num3; k++)
				{
					list.Add(GameObject.create(PopulationManager.RollOneFrom("BooksAndRandomBooks").Blueprint));
				}
				break;
			}
			}
			break;
		case "esk":
			switch (motiveInfluence)
			{
			case "violence":
				if (circumstanceInfluence == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Chain Pistol"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Chain Pistol"));
				}
				break;
			case "illness":
				list.Add(GameObjectFactory.Factory.CreateObject("FluxPhial"));
				if (circumstanceInfluence == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject("FluxPhial"));
				}
				break;
			case "craft":
				if (circumstanceInfluence == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Magnetized Boots"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Magnetized Boots"));
				}
				break;
			case "trade":
			{
				int num2 = 3;
				for (int j = 0; j < num2; j++)
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints[PopulationManager.RollOneFrom("DynamicObjectsTable:EnergyCells:Tier0-4").Blueprint], 0, 2));
				}
				break;
			}
			}
			break;
		case "kese":
			switch (motiveInfluence)
			{
			case "violence":
				if (circumstanceInfluence == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Ari"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Ari"));
				}
				if (circumstanceInfluence == "craft")
				{
					list2.Add("mod:ModFlaming");
				}
				break;
			case "illness":
				BodyPart.MakeSeveredBodyParts((circumstanceInfluence == "violence") ? 4 : 2, null, "Face", null, null, null, null, list);
				break;
			case "craft":
				if (circumstanceInfluence == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Carbide Plate Armor", 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Carbide Plate Armor"));
				}
				break;
			case "trade":
			{
				int num = ((circumstanceInfluence == "violence") ? 8 : 4);
				for (int i = 0; i < num; i++)
				{
					list.Add(GameObject.create(PopulationManager.RollOneFrom("DynamicObjectsTable:Grenades").Blueprint));
				}
				break;
			}
			}
			break;
		case "kendren":
			switch (motiveInfluence)
			{
			case "violence":
			{
				List<GameObject> list4 = new List<GameObject>();
				while (list4.Count < 3)
				{
					string blueprint = EncountersAPI.GetAnItemBlueprint((GameObjectBlueprint b) => b.Tier == 3 && b.DescendsFrom("MeleeWeapon") && EncountersAPI.IsEligibleForDynamicEncounters(b));
					if (!list4.Any((GameObject o) => o.Blueprint == blueprint))
					{
						if (circumstanceInfluence == "violence")
						{
							list4.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints[blueprint], 0, 2));
						}
						else
						{
							list4.Add(GameObjectFactory.Factory.Blueprints[blueprint].createOne());
						}
					}
				}
				list4.ForEach(delegate(GameObject o)
				{
					o.MakeUnderstood();
				});
				list.Add(ConversationsAPI.chooseOneItem(list4, "Choose an item", allowEscape: false));
				break;
			}
			case "illness":
				if (circumstanceInfluence == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Bio-Scanning Bracelet"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Bio-Scanning Bracelet"));
				}
				break;
			case "craft":
			{
				List<GameObject> list3 = new List<GameObject>();
				while (list3.Count < 3)
				{
					string blueprint2 = EncountersAPI.GetAnItemBlueprint((GameObjectBlueprint b) => b.Tier == 3 && b.DescendsFrom("Armor") && EncountersAPI.IsEligibleForDynamicEncounters(b));
					if (!list3.Any((GameObject o) => o.Blueprint == blueprint2))
					{
						if (circumstanceInfluence == "violence")
						{
							list3.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints[blueprint2], 0, 2));
						}
						else
						{
							list3.Add(GameObjectFactory.Factory.Blueprints[blueprint2].createOne());
						}
					}
				}
				list3.ForEach(delegate(GameObject o)
				{
					o.MakeUnderstood();
				});
				list.Add(ConversationsAPI.chooseOneItem(list3, "Choose an item", allowEscape: false));
				break;
			}
			case "trade":
				list.Add(GameObjectFactory.Factory.CreateObject("Hoversled"));
				if (circumstanceInfluence == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Hoversled"));
				}
				break;
			}
			break;
		}
		if (circumstanceInfluence == "illness")
		{
			for (int m = 0; m < 4; m++)
			{
				list.Add(GameObject.create(PopulationManager.RollOneFrom("DynamicObjectsTable:Tonics_NonRare").Blueprint));
			}
		}
		if (circumstanceInfluence == "trade" && !flag)
		{
			List<GameObject> list5 = new List<GameObject>();
			foreach (GameObject item in list)
			{
				list5.Add(item.DeepCopy(CopyEffects: false, CopyID: true));
			}
			list.AddRange(list5);
		}
		list.ForEach(delegate(GameObject o)
		{
			o.MakeUnderstood();
		});
		list.Sort((GameObject o1, GameObject o2) => o1.DisplayNameOnlyDirect.CompareTo(o2.DisplayNameOnlyDirect));
		StringBuilder stringBuilder = new StringBuilder();
		int num5 = 0;
		stringBuilder.AppendLine("In return for your service, you receive:\n");
		foreach (GameObject item2 in list)
		{
			stringBuilder.AppendLine(item2.DisplayName);
			IComponent<GameObject>.ThePlayer.ReceiveObject(item2);
			if (circumstanceInfluence == "craft" && (!(thief == "kese") || !(motiveInfluence == "violence")))
			{
				if (item2.HasPart("TinkerItem") && item2.GetPart<TinkerItem>().CanBuild)
				{
					list2.Add(item2.Blueprint);
				}
				else
				{
					num5 = 1;
				}
			}
		}
		if (num5 > 0)
		{
			stringBuilder.AppendLine("failed energy relay x" + num5);
			stringBuilder.AppendLine("fried processing core x" + num5);
			stringBuilder.AppendLine("cracked robotics housing x" + num5 * 2);
			for (int n = 0; n < num5; n++)
			{
				IComponent<GameObject>.ThePlayer.ReceiveObject("Scrap 1");
			}
			for (int num6 = 0; num6 < num5; num6++)
			{
				IComponent<GameObject>.ThePlayer.ReceiveObject("Scrap 2");
			}
			for (int num7 = 0; num7 < num5 * 2; num7++)
			{
				IComponent<GameObject>.ThePlayer.ReceiveObject("Scrap 2");
			}
		}
		if (list2.Count > 0)
		{
			foreach (string item3 in list2)
			{
				GameObject gameObject = TinkerData.createDataDisk(item3);
				gameObject.MakeUnderstood();
				IComponent<GameObject>.ThePlayer.TakeObject(gameObject, Silent: true, 0);
				stringBuilder.AppendLine(gameObject.DisplayName);
			}
		}
		stringBuilder.Append("\n\n");
		stringBuilder.Append("[{{C|" + GetThiefDisplayName(thief) + "}}, {{C|" + motiveInfluence + "}}, {{C|" + circumstanceInfluence + "}}]");
		Popup.Show(stringBuilder.ToString());
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GetConversationNode")
		{
			string stringParameter = E.GetStringParameter("GotoID");
			if (stringParameter == "*circumstance")
			{
				E.SetParameter("ConversationNode", generateCircumstanceNode());
			}
			else if (stringParameter.StartsWith("*selectCircumstance:"))
			{
				int index = Convert.ToInt32(stringParameter.Split(':')[1]);
				selectedCircumstance = circumstances[index];
				E.SetParameter("ConversationNode", generateMotiveNode());
			}
			else if (stringParameter.StartsWith("*selectMotive:"))
			{
				int index2 = Convert.ToInt32(stringParameter.Split(':')[1]);
				selectedMotive = motives[index2];
				E.SetParameter("ConversationNode", generateAccusationNode());
			}
			else if (stringParameter == "*makeAccusation")
			{
				E.SetParameter("ConversationNode", generateFinaleNode());
			}
		}
		return base.FireEvent(E);
	}

	public static string GetThiefDisplayName(string thief)
	{
		return thief switch
		{
			"kendren" => "a kendren", 
			"esk" => "Eskhind", 
			"kese" => "Kesehind", 
			_ => "Keh", 
		};
	}
}
