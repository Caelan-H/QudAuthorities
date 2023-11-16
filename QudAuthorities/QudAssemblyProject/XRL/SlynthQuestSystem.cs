using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genkit;
using HistoryKit;
using Qud.API;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Conversations;
using XRL.World.Parts;

namespace XRL;

[Serializable]
[HasWishCommand]
public class SlynthQuestSystem : IGameSystem
{
	private enum STAGE
	{
		unsettled = 0,
		decided = 50,
		settled = 100
	}

	public static readonly string QUEST_NAME = "Landing Pads";

	public static readonly int REQUIRED_CANDIDATES = 3;

	public int stage;

	public List<string> candidateFactions = new List<string>();

	public List<string> candidateFactionZones = new List<string>();

	public string hydroponZone;

	public string settledZone;

	public string settledFaction;

	public int SlynthLeaveDays = -1;

	public int SlynthArriveDays = -1;

	public int SlynthSettleDays = -1;

	public int CandidateCount = -1;

	[NonSerialized]
	private ConversationNode questNode;

	[NonSerialized]
	private ConversationNode questIntroNode;

	public bool visited;

	public void generateDynamicMayorQuestIntroNode(GameObject speaker)
	{
		string text = speaker?.GetPropertyOrTag("Mayor");
		HistoricEntitySnapshot villageSnapshot = HistoryAPI.GetVillageSnapshot(text);
		if (Factions.getIfExists(text) == null)
		{
			if (Factions.getIfExists(speaker?.GetPrimaryFaction()) == null)
			{
				return;
			}
			text = speaker.GetPrimaryFaction();
		}
		questIntroNode = new ConversationNode();
		questIntroNode.ID = "*slynthmayorquestintronode";
		questIntroNode.Choices = new List<ConversationChoice>();
		visited = true;
		ConversationChoice conversationChoice = new ConversationChoice();
		if (candidateFactions.Contains(text))
		{
			questIntroNode.Text = "Do you have news of the slynth?";
			conversationChoice.GotoID = "Start";
			conversationChoice.Text = "They are not ready to choose yet.";
		}
		else
		{
			questIntroNode.Text = HistoryAPI.ExpandVillageText("Is that so? I imagine you wish to know if =village.name= will host these slynth, to partake in =village.activity= alongside us.", null, villageSnapshot);
			conversationChoice.GotoID = "*slynthmayorquestnode";
			if (The.Game.PlayerReputation.getLevel(text) >= 2)
			{
				conversationChoice.Text = "I am.";
			}
			else
			{
				conversationChoice.Text = "If you would have them, yes.";
			}
		}
		questIntroNode.AddChoice(conversationChoice);
	}

	public void generateIrudadQuestIntroNode(GameObject speaker)
	{
		string text = speaker?.GetPropertyOrTag("Mayor");
		questIntroNode = new ConversationNode();
		questIntroNode.ID = "*slynthmayorquestintronode";
		questIntroNode.Choices = new List<ConversationChoice>();
		visited = true;
		ConversationChoice conversationChoice = new ConversationChoice();
		if (candidateFactions.Contains(text))
		{
			questIntroNode.Text = "Have you spoken to the slynth, =name=? I am curious to hear what destination they choose.";
			conversationChoice.GotoID = "Start";
			conversationChoice.Text = "They are not ready to choose yet.";
		}
		else
		{
			questIntroNode.Text = "Oh? You’re wondering if they might come to Joppa, then?";
			conversationChoice.GotoID = "*slynthmayorquestnode";
			if (The.Game.PlayerReputation.getLevel(text) >= 2)
			{
				conversationChoice.Text = "I am.";
			}
			else
			{
				conversationChoice.Text = "If you would have them, yes.";
			}
		}
		questIntroNode.AddChoice(conversationChoice);
	}

	public void generateShebaQuestIntroNode(GameObject speaker)
	{
		string text = speaker?.GetPropertyOrTag("Mayor");
		questIntroNode = new ConversationNode();
		questIntroNode.ID = "*slynthmayorquestintronode";
		questIntroNode.Choices = new List<ConversationChoice>();
		visited = true;
		ConversationChoice conversationChoice = new ConversationChoice();
		if (candidateFactions.Contains(text))
		{
			questIntroNode.Text = "=name=, bless you. Have you brought tidings of the slynth?";
			conversationChoice.GotoID = "Start";
			conversationChoice.Text = "They are not ready to choose yet.";
		}
		else
		{
			questIntroNode.Text = "You imagine they will thrive here at the Stiltgrounds, among the faithful?";
			conversationChoice.GotoID = "*slynthmayorquestnode";
			if (The.Game.PlayerReputation.getLevel(text) >= 2)
			{
				conversationChoice.Text = "I do.";
			}
			else
			{
				conversationChoice.Text = "If you would have them, yes.";
			}
		}
		questIntroNode.AddChoice(conversationChoice);
	}

	public void generateOthoQuestIntroNode(GameObject speaker)
	{
		string text = speaker?.GetPropertyOrTag("Mayor");
		questIntroNode = new ConversationNode();
		questIntroNode.ID = "*slynthmayorquestintronode";
		questIntroNode.Choices = new List<ConversationChoice>();
		visited = true;
		ConversationChoice conversationChoice = new ConversationChoice();
		if (candidateFactions.Contains(text))
		{
			questIntroNode.Text = "Has this slynth situation been settled yet?";
			conversationChoice.GotoID = "Start";
			conversationChoice.Text = "They are not ready to choose yet.";
		}
		else
		{
			questIntroNode.Text = "Be straightforward, disciple. You mean to ask if we will house them.";
			conversationChoice.GotoID = "*slynthmayorquestnode";
			if (The.Game.PlayerReputation.getLevel(text) >= 2)
			{
				conversationChoice.Text = "I am.";
			}
			else
			{
				conversationChoice.Text = "If you would have them, yes.";
			}
		}
		questIntroNode.AddChoice(conversationChoice);
	}

	public void generateNuntuQuestIntroNode(GameObject speaker)
	{
		string text = speaker?.GetPropertyOrTag("Mayor");
		questIntroNode = new ConversationNode();
		questIntroNode.ID = "*slynthmayorquestintronode";
		questIntroNode.Choices = new List<ConversationChoice>();
		visited = true;
		ConversationChoice conversationChoice = new ConversationChoice();
		if (candidateFactions.Contains(text))
		{
			questIntroNode.Text = "Have the slynth yet to choose a destination, =player.formalAddressTerm=?";
			conversationChoice.GotoID = "Start";
			conversationChoice.Text = "They are not ready to choose yet.";
		}
		else
		{
			questIntroNode.Text = "I don't imagine you brought me this information merely to observe. Are you suggesting that these slynth might take up residence here in my village?";
			conversationChoice.GotoID = "*slynthmayorquestnode";
			if (The.Game.PlayerReputation.getLevel(text) >= 2)
			{
				conversationChoice.Text = "I am.";
			}
			else
			{
				conversationChoice.Text = "If you would have them, yes.";
			}
		}
		questIntroNode.AddChoice(conversationChoice);
	}

	public void generateHaddasQuestIntroNode(GameObject speaker)
	{
		string text = speaker?.GetPropertyOrTag("Mayor");
		questIntroNode = new ConversationNode();
		questIntroNode.ID = "*slynthmayorquestintronode";
		questIntroNode.Choices = new List<ConversationChoice>();
		visited = true;
		ConversationChoice conversationChoice = new ConversationChoice();
		if (candidateFactions.Contains(text))
		{
			questIntroNode.Text = "Tell me of your seedlings.";
			conversationChoice.GotoID = "Start";
			conversationChoice.Text = "They are not ready to choose yet.";
		}
		else
		{
			questIntroNode.Text = "HA. You would fain plant them here, to joggle their fingers at the salt sky alongside the musa?";
			conversationChoice.GotoID = "*slynthmayorquestnode";
			if (The.Game.PlayerReputation.getLevel(text) >= 2)
			{
				conversationChoice.Text = "I would.";
			}
			else
			{
				conversationChoice.Text = "If you would have them, yes.";
			}
		}
		questIntroNode.AddChoice(conversationChoice);
	}

	public void generateAgyraQuestIntroNode(GameObject speaker)
	{
		string text = speaker?.GetPropertyOrTag("Mayor");
		questIntroNode = new ConversationNode();
		questIntroNode.ID = "*slynthmayorquestintronode";
		questIntroNode.Choices = new List<ConversationChoice>();
		visited = true;
		ConversationChoice conversationChoice = new ConversationChoice();
		if (candidateFactions.Contains(text))
		{
			questIntroNode.Text = "How =ifplayerplural:fare ye:farest thou=, then, and what word of the slynth? \n\nWe shine with curiosity to hear it.";
			conversationChoice.GotoID = "Start";
			conversationChoice.Text = "They are not ready to choose yet.";
		}
		else
		{
			questIntroNode.Text = "=ifplayerplural:Would ye:Wouldst thou= press these slynth into the fissures of historic stone where we reside, that they may observe what we do?";
			conversationChoice.GotoID = "*slynthmayorquestnode";
			if (The.Game.PlayerReputation.getLevel(text) >= 2)
			{
				conversationChoice.Text = "I would.";
			}
			else
			{
				conversationChoice.Text = "If you would have them, yes.";
			}
		}
		questIntroNode.AddChoice(conversationChoice);
	}

	public void generateLulihartQuestIntroNode(GameObject speaker)
	{
		string text = speaker?.GetPropertyOrTag("Mayor");
		questIntroNode = new ConversationNode();
		questIntroNode.ID = "*slynthmayorquestintronode";
		questIntroNode.Choices = new List<ConversationChoice>();
		visited = true;
		ConversationChoice conversationChoice = new ConversationChoice();
		if (candidateFactions.Contains(text))
		{
			questIntroNode.Text = "Still gathering landing pads for your drifting vessels, then, =name=?";
			conversationChoice.GotoID = "Start";
			conversationChoice.Text = "Yes, I am.";
		}
		else
		{
			questIntroNode.Text = "I can't help but wonder why you’re telling me this in particular. I am not a mayor, nor even a citizen. I am a drifter, a pariah, and my 'people' are those who have no home anywhere else.\n\n... and here I wonder a bit less. You're asking me to whisper into the winds on behalf of the slynth, aren't you?";
			conversationChoice.GotoID = "*slynthmayorquestnode";
			if (The.Game.PlayerReputation.getLevel(text) >= 2)
			{
				conversationChoice.Text = "I am.";
			}
			else
			{
				conversationChoice.Text = "If you would have them, yes.";
			}
		}
		questIntroNode.AddChoice(conversationChoice);
	}

	public void generateGoekQuestIntroNode(GameObject speaker)
	{
		string text = speaker?.GetPropertyOrTag("Mayor");
		questIntroNode = new ConversationNode();
		questIntroNode.ID = "*slynthmayorquestintronode";
		questIntroNode.Choices = new List<ConversationChoice>();
		visited = true;
		ConversationChoice conversationChoice = new ConversationChoice();
		if (candidateFactions.Contains(text))
		{
			questIntroNode.Text = "rrk wonder of the glow-hats. Which wind will blow.";
			conversationChoice.GotoID = "Start";
			conversationChoice.Text = "They are not ready to choose yet.";
		}
		else
		{
			questIntroNode.Text = "rrk knows of the glow-hats! Yd-friends see glimpses at night, about the reef.";
			conversationChoice.GotoID = "*slynthmayorquestnode";
			if (The.Game.PlayerReputation.getLevel(text) >= 2)
			{
				conversationChoice.Text = "If you would have them, I ask that you grant them sanctuary.";
			}
			else
			{
				conversationChoice.Text = "If you would have them, I ask that you grant them sanctuary.";
			}
		}
		questIntroNode.AddChoice(conversationChoice);
	}

	public void generateKehQuestIntroNode(GameObject speaker)
	{
		string item = speaker?.GetPropertyOrTag("Mayor");
		questIntroNode = new ConversationNode();
		questIntroNode.ID = "*slynthmayorquestintronode";
		questIntroNode.Choices = new List<ConversationChoice>();
		visited = true;
		ConversationChoice conversationChoice = new ConversationChoice();
		if (candidateFactions.Contains(item))
		{
			questIntroNode.Text = "The slynth? What of them?";
			conversationChoice.GotoID = "Start";
			conversationChoice.Text = "They are not ready to choose yet.";
		}
		else
		{
			questIntroNode.Text = "Don’t be coy, =name=. Emboldened by your incursions into our village, you wish to fill it with your indigent foundlings on the weight of your reputation alone. Am I correct?";
			conversationChoice.GotoID = "KehRejectsSlynth";
			conversationChoice.Text = "I... suppose so, yes.";
		}
		questIntroNode.AddChoice(conversationChoice);
	}

	public void generateEskhindQuestIntroNode(GameObject speaker)
	{
		string text = speaker?.GetPropertyOrTag("Mayor");
		questIntroNode = new ConversationNode();
		questIntroNode.ID = "*slynthmayorquestintronode";
		questIntroNode.Choices = new List<ConversationChoice>();
		visited = true;
		ConversationChoice conversationChoice = new ConversationChoice();
		if (candidateFactions.Contains(text))
		{
			questIntroNode.Text = "We are on the tips of our hooves, =name=! Any word about the slynth?";
			conversationChoice.GotoID = "Start";
			conversationChoice.Text = "They are not ready to choose yet.";
		}
		else
		{
			questIntroNode.Text = "You're just trying to keep dear Esk informed, then? Or could it be that you're wondering if that home may be Bey Lah? ";
			conversationChoice.GotoID = "*slynthmayorquestnode";
			if (The.Game.PlayerReputation.getLevel(text) >= 2)
			{
				conversationChoice.Text = "I am.";
				conversationChoice.GotoID = "EskhindAcceptSlynthStart";
			}
			else
			{
				conversationChoice.Text = "If you would have them, yes.";
			}
		}
		questIntroNode.AddChoice(conversationChoice);
	}

	public void generateDynamicMayorQuestNode(GameObject speaker)
	{
		string faction = speaker?.GetPropertyOrTag("Mayor");
		HistoricEntitySnapshot snapshot = HistoryAPI.GetVillageSnapshot(faction);
		questNode = new ConversationNode();
		questNode.ID = "*slynthmayorquestnode";
		if (The.Game.PlayerReputation.getLevel(faction) >= 2)
		{
			questNode.Text = HistoryAPI.ExpandVillageText("You have done much for =village.name=, =name=, and your request befits your stature. If these slynth will join us in =village.activity= and if they can come to cherish =village.sacred= as we do, then they are welcome here.", null, snapshot);
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.GotoID = "End";
			conversationChoice.Text = HistoryAPI.ExpandVillageText("You have my thanks, friend. {{W|[confirm =village.name= as a sanctuary option]}}", null, snapshot);
			conversationChoice.Ordinal = 1;
			conversationChoice.onAction = delegate
			{
				Popup.Show(snapshot.Name + " is now a sanctuary option for the slynth.");
				candidateFactions.Add(faction);
				candidateFactionZones.Add(speaker.CurrentZone.ZoneID);
				updateQuestStatus();
				return true;
			};
			ConversationChoice conversationChoice2 = new ConversationChoice();
			conversationChoice2.GotoID = "Start";
			conversationChoice2.Text = "I want to ask something else.";
			conversationChoice2.Ordinal = 1;
			questNode.AddChoice(conversationChoice2);
			questNode.AddChoice(conversationChoice);
		}
		else
		{
			questNode.Text = HistoryAPI.ExpandVillageText("We have not come so far from the founding of =village.name= to allow these strangers in. What if they worship =village.profane=? No, we simply cannot do such a great favor for you.", null, snapshot);
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice3 = new ConversationChoice();
			conversationChoice3.GotoID = "Start";
			conversationChoice3.Text = "I understand.";
			conversationChoice3.Ordinal = 2;
			questNode.AddChoice(conversationChoice3);
		}
	}

	public void generateIrudadQuestNode(GameObject speaker)
	{
		string faction = speaker?.GetPropertyOrTag("Mayor");
		questNode = new ConversationNode();
		questNode.ID = "*slynthmayorquestnode";
		if (The.Game.PlayerReputation.getLevel(faction) >= 2)
		{
			questNode.Text = "I am for you, =name=. Any friend of yours is too a friend of Joppa, and if those friends desire shelter I wouldn’t dare turn them away. I only hope these lilypad-folk have the patience for a bucolic lifestyle. I can't imagine it is as exciting here as it was in the Palladium Reef.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.GotoID = "End";
			conversationChoice.Text = "You have my thanks, Elder. {{W|[confirm Joppa as a sanctuary option]}}";
			conversationChoice.Ordinal = 1;
			conversationChoice.onAction = delegate
			{
				Popup.Show(faction + " is now a sanctuary option for the slynth.");
				candidateFactions.Add(faction);
				candidateFactionZones.Add(speaker.CurrentZone.ZoneID);
				updateQuestStatus();
				return true;
			};
			ConversationChoice conversationChoice2 = new ConversationChoice();
			conversationChoice2.GotoID = "Start";
			conversationChoice2.Text = "I want to ask something else.";
			conversationChoice2.Ordinal = 1;
			questNode.AddChoice(conversationChoice2);
			questNode.AddChoice(conversationChoice);
		}
		else
		{
			questNode.Text = "Blessings upon your generous heart, =name=. I mean no slight to these slynth, but it is too great a favor you ask of the people of Joppa.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice3 = new ConversationChoice();
			conversationChoice3.GotoID = "Start";
			conversationChoice3.Text = "I understand.";
			conversationChoice3.Ordinal = 2;
			questNode.AddChoice(conversationChoice3);
		}
	}

	public void generateShebaQuestNode(GameObject speaker)
	{
		string faction = speaker?.GetPropertyOrTag("Mayor");
		questNode = new ConversationNode();
		questNode.ID = "*slynthmayorquestnode";
		if (The.Game.PlayerReputation.getLevel(faction) >= 2)
		{
			questNode.Text = "This wouldn’t be the first time the Stiltgrounds have hosted refugees, =name=, and make no mistake: it is a burden upon the church, Catechists and Protectors in particular. But your words echo loud in our hearts as our hallways. Some in our number will see the slynth as the chosen of chosen, and if I speak to them first, everyone else will take it as Shekhinah's will. Which, arguably, it would be. Tell the slynth that the Mechanimists will have them.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.GotoID = "End";
			conversationChoice.Text = "You have my thanks, Sheba. {{W|[confirm the Six Day Stilt as a sanctuary option]}}";
			conversationChoice.Ordinal = 1;
			conversationChoice.onAction = delegate
			{
				Popup.Show("The Six Day Stilt are now a sanctuary option for the slynth.");
				candidateFactions.Add(faction);
				candidateFactionZones.Add(speaker.CurrentZone.ZoneID);
				updateQuestStatus();
				return true;
			};
			ConversationChoice conversationChoice2 = new ConversationChoice();
			conversationChoice2.GotoID = "Start";
			conversationChoice2.Text = "I want to ask something else.";
			conversationChoice2.Ordinal = 1;
			questNode.AddChoice(conversationChoice2);
			questNode.AddChoice(conversationChoice);
		}
		else
		{
			questNode.Text = "I mean no slight to these slynth folk, =name=, but the Stiltgrounds have hosted refugees before and it is a substantial burden upon our stewards. Our resources and space are scant, and I must request that you find another home for them. May the Kasaphescence shine upon them, and you, in this endeavor.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice3 = new ConversationChoice();
			conversationChoice3.GotoID = "Start";
			conversationChoice3.Text = "I understand.";
			conversationChoice3.Ordinal = 2;
			questNode.AddChoice(conversationChoice3);
		}
	}

	public void generateOthoQuestNode(GameObject speaker)
	{
		string faction = speaker?.GetPropertyOrTag("Mayor");
		questNode = new ConversationNode();
		questNode.ID = "*slynthmayorquestnode";
		if (The.Game.PlayerReputation.getLevel(faction) >= 2)
		{
			questNode.Text = "I am uncertain that you know the enormity of what you ask me, =name=. Nonetheless, I cannot deny what you have done for our order. These slynth must be tested as you were, but should they show the mettle and drive set by your example, we will accept them as apprentices.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.GotoID = "End";
			conversationChoice.Text = "You have my thanks, Otho. {{W|[confirm Grit Gate as a sanctuary option]}}";
			conversationChoice.Ordinal = 1;
			conversationChoice.onAction = delegate
			{
				Popup.Show("Grit Gate is now a sanctuary option for the slynth.");
				candidateFactions.Add(faction);
				candidateFactionZones.Add(speaker.CurrentZone.ZoneID);
				updateQuestStatus();
				return true;
			};
			ConversationChoice conversationChoice2 = new ConversationChoice();
			conversationChoice2.GotoID = "Start";
			conversationChoice2.Text = "I want to ask something else.";
			conversationChoice2.Ordinal = 1;
			questNode.AddChoice(conversationChoice2);
			questNode.AddChoice(conversationChoice);
		}
		else
		{
			questNode.Text = "You have grown bold beyond your station. What you have asked of us you have not earned. If you must ask again, become worthier first.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice3 = new ConversationChoice();
			conversationChoice3.GotoID = "Start";
			conversationChoice3.Text = "I understand.";
			conversationChoice3.Ordinal = 2;
			questNode.AddChoice(conversationChoice3);
		}
	}

	public void generateNuntuQuestNode(GameObject speaker)
	{
		string faction = speaker?.GetPropertyOrTag("Mayor");
		questNode = new ConversationNode();
		questNode.ID = "*slynthmayorquestnode";
		if (The.Game.PlayerReputation.getLevel(faction) >= 2)
		{
			questNode.Text = "Coming from anyone else, =name=, I might refuse outright. Housing recently-sentient refugees is no mean favor, I think you'll agree. My people are slow to trust, as well, so I fear that the slynth may feel unwelcome for some time. But then, grafted branches grow the best starapples, do they not? Yes, your lilypad friends are welcome here if they wish.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.GotoID = "End";
			conversationChoice.Text = "You have my thanks, mayor. {{W|[confirm Kyakukya as a sanctuary option]}}";
			conversationChoice.Ordinal = 1;
			conversationChoice.onAction = delegate
			{
				Popup.Show("Kyakukya is now a sanctuary option for the slynth.");
				candidateFactions.Add(faction);
				candidateFactionZones.Add(speaker.CurrentZone.ZoneID);
				updateQuestStatus();
				return true;
			};
			ConversationChoice conversationChoice2 = new ConversationChoice();
			conversationChoice2.GotoID = "Start";
			conversationChoice2.Text = "I want to ask something else.";
			conversationChoice2.Ordinal = 1;
			questNode.AddChoice(conversationChoice2);
			questNode.AddChoice(conversationChoice);
		}
		else
		{
			questNode.Text = "Had I no responsibility to represent my people, I might accept out of the goodness of my own heart alone. Sadly, I do not consider my heart, or even my mind, to be more important than my duty. Perhaps if you were a more renowned figure among my people, I would reconsider.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice3 = new ConversationChoice();
			conversationChoice3.GotoID = "Start";
			conversationChoice3.Text = "I understand.";
			conversationChoice3.Ordinal = 2;
			questNode.AddChoice(conversationChoice3);
		}
	}

	public void generateHaddasQuestNode(GameObject speaker)
	{
		string faction = speaker?.GetPropertyOrTag("Mayor");
		questNode = new ConversationNode();
		questNode.ID = "*slynthmayorquestnode";
		if (The.Game.PlayerReputation.getLevel(faction) >= 2)
		{
			questNode.Text = "Lilypad-folk, you say. The slynth, you say. HA..... HA HA HA! Yes! I never thought to see yet fresher thought-sprouts take seed here in Ezra, but our sun is theirs, and the sky below it, and the thickening air down to the ground. Bring them to hear the songs of our musa-herders, and hope that they survive.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.GotoID = "End";
			conversationChoice.Text = "You have my thanks, mayor. {{W|[confirm Ezra as a sanctuary option]}}";
			conversationChoice.Ordinal = 1;
			conversationChoice.onAction = delegate
			{
				Popup.Show("Ezra is now a sanctuary option for the slynth.");
				candidateFactions.Add(faction);
				candidateFactionZones.Add(speaker.CurrentZone.ZoneID);
				updateQuestStatus();
				return true;
			};
			ConversationChoice conversationChoice2 = new ConversationChoice();
			conversationChoice2.GotoID = "Start";
			conversationChoice2.Text = "I want to ask something else.";
			conversationChoice2.Ordinal = 1;
			questNode.AddChoice(conversationChoice2);
			questNode.AddChoice(conversationChoice);
		}
		else
		{
			questNode.Text = "Rocky soil fits only so many roots, little walker. Perhaps if yours were more anchored here. HA.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice3 = new ConversationChoice();
			conversationChoice3.GotoID = "Start";
			conversationChoice3.Text = "I understand.";
			conversationChoice3.Ordinal = 2;
			questNode.AddChoice(conversationChoice3);
		}
	}

	public void generateAgyraQuestNode(GameObject speaker)
	{
		string faction = speaker?.GetPropertyOrTag("Mayor");
		questNode = new ConversationNode();
		questNode.ID = "*slynthmayorquestnode";
		if (The.Game.PlayerReputation.getLevel(faction) >= 2)
		{
			questNode.Text = "If these slynth wish to become mopango and =ifplayerplural:ye speak:thou speakest= for them, then we shall accept them. Let them gaze into the schematics that wrote history beside us if they so choose.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.GotoID = "End";
			conversationChoice.Text = "You have my thanks, Agyra. {{W|[confirm the mopango hideout as a sanctuary option]}}";
			conversationChoice.Ordinal = 1;
			conversationChoice.onAction = delegate
			{
				Popup.Show("The mopango hideout is now a sanctuary option for the slynth.");
				candidateFactions.Add(faction);
				candidateFactionZones.Add(speaker.CurrentZone.ZoneID);
				updateQuestStatus();
				return true;
			};
			ConversationChoice conversationChoice2 = new ConversationChoice();
			conversationChoice2.GotoID = "Start";
			conversationChoice2.Text = "I want to ask something else.";
			conversationChoice2.Ordinal = 1;
			questNode.AddChoice(conversationChoice2);
			questNode.AddChoice(conversationChoice);
		}
		else
		{
			questNode.Text = "Please understand that we mean no slight to =ifplayerplural:you:thee= in saying so, but what =ifplayerplural:ye ask:thou asketh= of us is too heavy a weight for =ifplayerplural:your names:thy name= to bear. Perhaps this will change, come deed and time.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice3 = new ConversationChoice();
			conversationChoice3.GotoID = "Start";
			conversationChoice3.Text = "I understand.";
			conversationChoice3.Ordinal = 2;
			questNode.AddChoice(conversationChoice3);
		}
	}

	public void generateLulihartQuestNode(GameObject speaker)
	{
		string faction = speaker?.GetPropertyOrTag("Mayor");
		questNode = new ConversationNode();
		questNode.ID = "*slynthmayorquestnode";
		if (The.Game.PlayerReputation.getLevel(faction) >= 2)
		{
			questNode.Text = "What a strange fate, to pass from new sentience to outcast with no steps in between. Regardless, if it is their will and you vouch for them, that is enough for me. Tell the slynth that should they choose to wander, the wind will blow at their backs.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.GotoID = "End";
			conversationChoice.Text = "You have my thanks, Lulihart. {{W|[confirm pariah caravans as a sanctuary option]}}";
			conversationChoice.Ordinal = 1;
			conversationChoice.onAction = delegate
			{
				Popup.Show("Pariah caravans are now a sanctuary option for the slynth.");
				candidateFactions.Add(faction);
				candidateFactionZones.Add(speaker.CurrentZone.ZoneID);
				updateQuestStatus();
				return true;
			};
			ConversationChoice conversationChoice2 = new ConversationChoice();
			conversationChoice2.GotoID = "Start";
			conversationChoice2.Text = "I want to ask something else.";
			conversationChoice2.Ordinal = 1;
			questNode.AddChoice(conversationChoice2);
			questNode.AddChoice(conversationChoice);
		}
		else
		{
			questNode.Text = "It is hardly so simple, =name=. We drifters hold no hierarchy. I can whisper all I like, but no winds will blow behind the slynth if your name cannot stir more than a breeze. Make yourself known to Pariahs and ask me again.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice3 = new ConversationChoice();
			conversationChoice3.GotoID = "Start";
			conversationChoice3.Text = "I understand.";
			conversationChoice3.Ordinal = 2;
			questNode.AddChoice(conversationChoice3);
		}
	}

	public void generateGoekQuestNode(GameObject speaker)
	{
		string faction = speaker?.GetPropertyOrTag("Mayor");
		questNode = new ConversationNode();
		questNode.ID = "*slynthmayorquestnode";
		if (The.Game.PlayerReputation.getLevel(faction) >= 2)
		{
			questNode.Text = "Boon-friend asks a friend-boon, and what boon is friend! rrk welcome slynth to an Yd, be safe and free as woodsmoke.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.GotoID = "End";
			conversationChoice.Text = "You have my thanks, Goek. {{W|[confirm the Yd Freehold as a sanctuary option]}}";
			conversationChoice.Ordinal = 1;
			conversationChoice.onAction = delegate
			{
				Popup.Show("The Yd Freehold is now a sanctuary option for the slynth.");
				candidateFactions.Add(faction);
				candidateFactionZones.Add(speaker.CurrentZone.ZoneID);
				updateQuestStatus();
				return true;
			};
			ConversationChoice conversationChoice2 = new ConversationChoice();
			conversationChoice2.GotoID = "Start";
			conversationChoice2.Text = "I want to ask something else.";
			conversationChoice2.Ordinal = 1;
			questNode.AddChoice(conversationChoice2);
			questNode.AddChoice(conversationChoice);
		}
		else
		{
			questNode.Text = "Ah! rrk would grant you this boon, rrk would. Were not for the unspoken cries of future-kin, free as woodsmoke but no more safe so longer. rrk would and cannot.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice3 = new ConversationChoice();
			conversationChoice3.GotoID = "Start";
			conversationChoice3.Text = "I understand.";
			conversationChoice3.Ordinal = 2;
			questNode.AddChoice(conversationChoice3);
		}
	}

	public void generateEskhindQuestNode(GameObject speaker)
	{
		string faction = speaker?.GetPropertyOrTag("Mayor");
		questNode = new ConversationNode();
		questNode.ID = "*slynthmayorquestnode";
		if (The.Game.PlayerReputation.getLevel(faction) >= 2)
		{
			questNode.Text = "{{emote|*The Hindriarch returns at long last, expression bright.*}} By majority vote, the slynth are welcome to take up permanent residence here in Bey Lah! I didn't think we were ready, but I suppose my people showed me otherwise. They will effectively be apprentices to the village until they choose a vocation. If they choose to come here, that is to say.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.GotoID = "End";
			conversationChoice.Text = "You have my thanks, Hindriarch. {{W|[confirm Bey Lah as a sanctuary option]}}";
			conversationChoice.Ordinal = 1;
			conversationChoice.onAction = delegate
			{
				Popup.Show("Bey Lah is now a sanctuary option for the slynth.");
				candidateFactions.Add(faction);
				candidateFactionZones.Add(speaker.CurrentZone.ZoneID);
				updateQuestStatus();
				return true;
			};
			ConversationChoice conversationChoice2 = new ConversationChoice();
			conversationChoice2.GotoID = "Start";
			conversationChoice2.Text = "I want to ask something else.";
			conversationChoice2.Ordinal = 1;
			questNode.AddChoice(conversationChoice2);
			questNode.AddChoice(conversationChoice);
		}
		else
		{
			questNode.Text = "I am afeared to even mention this notion to the villagers. Can you imagine my epitaph? \"Here lies the youngest Hindriarch in Bey Lah's history, trampled to death because she suggested we invite in a people most readily described as 'lah with legs'.\" If they trusted you more, maybe we could put it to a vote. But as it is? No. Sorry, =name=.";
			questNode.Choices = new List<ConversationChoice>();
			ConversationChoice conversationChoice3 = new ConversationChoice();
			conversationChoice3.GotoID = "Start";
			conversationChoice3.Text = "I understand.";
			conversationChoice3.Ordinal = 2;
			questNode.AddChoice(conversationChoice3);
		}
	}

	public void updateQuestStatus()
	{
		if (stage != 0)
		{
			return;
		}
		Quest quest = getQuest();
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.AppendLine("The following factions have agreed to provide sanctuary for the slynth.");
		if (candidateFactions.Count > 0)
		{
			stringBuilder.AppendLine();
		}
		foreach (string candidateFaction in candidateFactions)
		{
			stringBuilder.AppendLine();
			if (The.Game.PlayerReputation.getLevel(candidateFaction) >= 2)
			{
				stringBuilder.AppendLine("{{green|û}} {{white|" + Faction.getFormattedName(candidateFaction) + "}}");
			}
		}
		foreach (string candidateFaction2 in candidateFactions)
		{
			if (The.Game.PlayerReputation.getLevel(candidateFaction2) < 2)
			{
				stringBuilder.AppendLine("{{red|X}} {{K|" + Faction.getFormattedName(candidateFaction2) + " [reputation too low]}}");
			}
		}
		quest.StepsByID["Sanctuary Candidates"].Text = stringBuilder.ToString();
		if (candidateFactionsCount() >= REQUIRED_CANDIDATES)
		{
			The.Game.FinishQuestStep(QUEST_NAME, "Consult Settlements");
			return;
		}
		QuestStep questStep = The.Game.Quests[QUEST_NAME].StepsByID["Consult Settlements"];
		if (questStep.Finished)
		{
			questStep.XP = 0;
			questStep.Finished = false;
		}
	}

	public Quest getQuest()
	{
		if (!The.Game.Quests.ContainsKey(QUEST_NAME))
		{
			return null;
		}
		return The.Game.Quests[QUEST_NAME];
	}

	public override void OnAdded()
	{
		hydroponZone = The.Game.GetStringGameState("HydroponZoneID", null);
	}

	public override void PlayerReputationChanged(string faction, int oldValue, int newValue, string because)
	{
		if (stage == 0)
		{
			updateQuestStatus();
		}
	}

	public int candidateFactionsCount()
	{
		return candidateFactions.Count((string f) => The.Game.PlayerReputation.get(f) >= 600);
	}

	public override void QuestCompleted(Quest Quest)
	{
		if (!(Quest.ID != QUEST_NAME))
		{
			RollResult();
			stage = 100;
			SlynthLeaveDays = 1;
			SlynthArriveDays = Stat.Random(5, 7);
			SlynthSettleDays = SlynthArriveDays + 7;
			CandidateCount = candidateFactionsCount();
			The.Game.PlayerReputation.modify(settledFaction, -600);
		}
	}

	public void RollResult()
	{
		if (stage < 50)
		{
			stage = 50;
		}
		if (settledFaction == null)
		{
			int index;
			do
			{
				index = Stat.Rand.Next(0, candidateFactions.Count);
			}
			while (The.Game.PlayerReputation.get(candidateFactions[index]) < 600);
			The.Game.SetStringGameState("SlynthSettlementFaction", settledFaction = candidateFactions[index]);
			The.Game.SetStringGameState("SlynthSettlementZone", settledZone = candidateFactionZones[index]);
		}
	}

	public long GetQuestFinishDays()
	{
		long questFinishTime = The.Game.GetQuestFinishTime(QUEST_NAME);
		if (questFinishTime < 0)
		{
			return -1L;
		}
		return (The.Game.TimeTicks - questFinishTime) / 1200;
	}

	public override void ZoneActivated(Zone Z)
	{
		if (SlynthLeaveDays >= 0 && Z.ZoneID == hydroponZone)
		{
			HydroponActivated(Z);
		}
		else if (SlynthSettleDays >= 0 && Z.ZoneID == settledZone)
		{
			SettlementActivated(Z);
		}
		base.ZoneActivated(Z);
	}

	public void HydroponActivated(Zone Z)
	{
		if (SlynthLeaveDays < 0 || GetQuestFinishDays() < SlynthLeaveDays || The.Game.HasIntGameState("LandingPadsSlynthLeft"))
		{
			return;
		}
		SlynthLeaveDays = -1;
		The.Game.SetIntGameState("LandingPadsSlynthLeft", 1);
		List<GameObject> objects = Z.GetObjects("BaseSlynth");
		int num = Stat.Random(8, 10);
		using List<GameObject>.Enumerator enumerator = objects.GetEnumerator();
		while (enumerator.MoveNext() && (!enumerator.Current.Destroy(null, Silent: true) || --num > 0))
		{
		}
	}

	public void GetSlynthCells(Zone Z, out List<Cell> Cells, out int Amount, out bool Wanders)
	{
		if (settledFaction == "Pariahs")
		{
			Cells = Z.GetEmptyReachableCells(new Rect2D(43, 18, 46, 22));
			Amount = 2;
			Wanders = false;
		}
		else if (settledFaction == "Mechanimists")
		{
			Cells = new List<Cell>(Stat.Random(5, 9));
			Wanders = true;
			for (int i = 0; i < Cells.Capacity; i++)
			{
				int num = Stat.Random(0, Directions.DirectionList.Length);
				string direction = ".";
				if (num < Directions.DirectionList.Length)
				{
					direction = Directions.DirectionList[num];
				}
				string zoneFromIDAndDirection = The.ZoneManager.GetZoneFromIDAndDirection(Z.ZoneID, direction);
				Zone zone = The.ZoneManager.GetZone(zoneFromIDAndDirection);
				for (int j = 0; j < 100; j++)
				{
					Cell randomCell = zone.GetRandomCell(1);
					if (randomCell.IsReachable() && randomCell.IsEmpty() && !Cells.Contains(randomCell))
					{
						Cells.Add(randomCell);
						break;
					}
				}
			}
			Amount = Cells.Count;
		}
		else if (settledFaction == "YdFreehold")
		{
			int z = ((Z.Z == 10) ? 11 : 10);
			Cells = The.ZoneManager.GetZone(Z.ZoneWorld, Z.wX, Z.wY, Z.X, Z.Y, z).GetEmptyReachableCells();
			Cells.AddRange(Z.GetEmptyReachableCells());
			Amount = Math.Min(Stat.Random(5, 9), Cells.Count);
			Wanders = true;
		}
		else if (settledFaction == "Mopango")
		{
			Cells = Z.GetCells((Cell C) => C.IsEmpty() && C.IsReachable() && C.HasObject("MopangoHideoutTile"));
			Amount = Math.Min(Stat.Random(5, 9), Cells.Count);
			Wanders = true;
			if (Cells.Count < Amount)
			{
				Cells.AddRange(Z.GetEmptyReachableCells());
			}
		}
		else
		{
			Cells = Z.GetEmptyReachableCells();
			Amount = Math.Min(Stat.Random(5, 9), Cells.Count);
			Wanders = true;
		}
	}

	public void SettlementActivated(Zone Z)
	{
		if (SlynthArriveDays >= 0 && GetQuestFinishDays() >= SlynthArriveDays && !The.Game.HasIntGameState("LandingPadsSlynthArrived"))
		{
			SlynthArriveDays = -1;
			The.Game.SetIntGameState("LandingPadsSlynthArrived", 1);
			GetSlynthCells(Z, out var Cells, out var Amount, out var Wanders);
			string factions = settledFaction + "-100";
			for (int i = 0; i < Amount; i++)
			{
				Cell randomElement = Cells.GetRandomElement();
				GameObject gameObject = randomElement?.AddObject("BaseSlynth");
				if (gameObject != null)
				{
					gameObject.RequirePart<ConversationScript>().ConversationID = "SlynthSettler";
					gameObject.pBrain.Calm = true;
					gameObject.pBrain.Wanders = Wanders;
					gameObject.pBrain.Factions = factions;
					gameObject.pBrain.InitFromFactions();
					gameObject.MakeActive();
					Cells.Remove(randomElement);
				}
			}
		}
		if (SlynthSettleDays >= 0 && GetQuestFinishDays() >= SlynthSettleDays && !The.Game.HasIntGameState("LandingPadsSlynthSettled"))
		{
			SlynthSettleDays = -1;
			The.Game.SetIntGameState("LandingPadsSlynthSettled", 1);
		}
	}

	public void LandingPadsCommentary(GameObject Speaker, ConversationNode CurrentNode, List<ConversationChoice> Choices)
	{
		Dictionary<string, ConversationNode> nodesByID = CurrentNode.ParentConversation.NodesByID;
		string text = Speaker?.GetStringProperty(CurrentNode.ID);
		if (!string.IsNullOrEmpty(text))
		{
			string oldText = CurrentNode.Text;
			CurrentNode.OnLeaveNode = delegate
			{
				CurrentNode.Text = oldText;
			};
			if (nodesByID.TryGetValue("Commentary" + text, out var value))
			{
				CurrentNode.Text = value.Text;
			}
			else if (nodesByID.TryGetValue("CommentaryDynamic", out value))
			{
				CurrentNode.Text = HistoryAPI.ExpandVillageText(value.Text, text);
			}
		}
		if (CurrentNode.Choices.Any((ConversationChoice x) => x.GotoID == CurrentNode.ID))
		{
			return;
		}
		for (int i = 0; i < candidateFactions.Count; i++)
		{
			bool flag = false;
			string candidate = candidateFactions[i];
			if (The.Game.PlayerReputation.get(candidate) < 600)
			{
				continue;
			}
			if (!nodesByID.TryGetValue("Commentary" + candidate, out var value2))
			{
				if (!nodesByID.TryGetValue("CommentaryDynamic", out value2))
				{
					continue;
				}
				flag = true;
			}
			foreach (ConversationChoice choice in value2.Choices)
			{
				ConversationChoice item = new ConversationChoice
				{
					ID = candidate,
					Text = (flag ? HistoryAPI.ExpandVillageText(choice.Text, candidate) : choice.Text),
					GotoID = CurrentNode.ID,
					ParentNode = CurrentNode,
					Ordinal = i + 1,
					onAction = delegate
					{
						Speaker?.SetStringProperty(CurrentNode.ID, candidate);
						return true;
					},
					IfDelegate = delegate
					{
						if (Speaker?.GetStringProperty(CurrentNode.ID) != candidate)
						{
							return true;
						}
						Speaker.RemoveProperty(CurrentNode.ID);
						return false;
					}
				};
				CurrentNode.Choices.Add(item);
			}
		}
		CurrentNode.Choices.Sort();
	}

	public void SlynthDynamicVillageNodes(GameObject Speaker, ConversationNode CurrentNode, List<ConversationChoice> Choices)
	{
		string text = Speaker?.GetPropertyOrTag("Mayor");
		if (text == null || settledFaction != text || CurrentNode.ID != "Start" || CurrentNode.ParentConversation.NodesByID.ContainsKey("SlynthArrived") || CurrentNode.ParentConversation.NodesByID.ContainsKey("SlynthSettled") || !Dialogue.Blueprints.TryGetValue("DynamicVillageMayor", out var value))
		{
			return;
		}
		Conversation parentConversation = CurrentNode.ParentConversation;
		HistoricEntitySnapshot villageSnapshot = HistoryAPI.GetVillageSnapshot(text);
		if (villageSnapshot != null)
		{
			if (The.Game.HasGameState("LandingPadsSlynthSettled"))
			{
				CurrentNode.AddChoice(new ConversationChoice
				{
					ID = "SlynthSettledChoice",
					GotoID = "SlynthSettled",
					Text = HistoryAPI.ExpandVillageText(value.GetChild("Welcome", "SlynthSettledChoice", "Text").Text, null, villageSnapshot)
				});
				parentConversation.AddNode(new ConversationNode
				{
					ID = "SlynthSettled",
					Text = HistoryAPI.ExpandVillageText(value.GetChild("SlynthSettled", "Text").Text, null, villageSnapshot)
				}).AddChoice(new ConversationChoice
				{
					ID = "StartChoice",
					GotoID = "Start",
					Text = HistoryAPI.ExpandVillageText(value.GetChild("SlynthSettled", "StartChoice", "Text").Text, null, villageSnapshot)
				});
			}
			else if (The.Game.HasGameState("LandingPadsSlynthArrived"))
			{
				CurrentNode.AddChoice(new ConversationChoice
				{
					ID = "SlynthArrivedChoice",
					GotoID = "SlynthArrived",
					Text = HistoryAPI.ExpandVillageText(value.GetChild("Welcome", "SlynthArrivedChoice", "Text").Text, null, villageSnapshot)
				});
				parentConversation.AddNode(new ConversationNode
				{
					ID = "SlynthArrived",
					Text = HistoryAPI.ExpandVillageText(value.GetChild("SlynthArrived", "Text").Text, null, villageSnapshot)
				}).AddChoice(new ConversationChoice
				{
					ID = "StartChoice",
					GotoID = "Start",
					Text = HistoryAPI.ExpandVillageText(value.GetChild("SlynthArrived", "StartChoice", "Text").Text, null, villageSnapshot)
				});
			}
		}
	}

	public override void FireEvent(Event e)
	{
		if (e.ID == "ShowConversationChoices")
		{
			if (stage == 0 && !The.Game.HasIntGameState("SlynthCandidatesReady"))
			{
				e.GetParameter<ConversationNode>("FirstNode");
				ConversationNode parameter = e.GetParameter<ConversationNode>("CurrentNode");
				List<ConversationChoice> parameter2 = e.GetParameter<List<ConversationChoice>>("Choices");
				GameObject parameter3 = e.GetParameter<GameObject>("Speaker");
				string text = parameter3?.GetPropertyOrTag("Mayor");
				if (parameter.ID == "Start")
				{
					if (text != null && !parameter2.Any((ConversationChoice c) => c.GotoID == "*slynthmayorquestintronode"))
					{
						ConversationChoice conversationChoice = new ConversationChoice();
						if (candidateFactions.Contains(text))
						{
							conversationChoice.Text = "About the slynth...";
						}
						else
						{
							conversationChoice.Text = "In my travels I encountered a people, the slynth, seeking a new home.";
						}
						conversationChoice.GotoID = "*slynthmayorquestintronode";
						parameter2.Insert(0, conversationChoice);
					}
					if (!parameter3.HasTagOrProperty("SlynthQuestGiver"))
					{
					}
				}
				else if (!(parameter.ID == "LandingPadsCommentary"))
				{
				}
			}
			else if (stage >= 100)
			{
				SlynthDynamicVillageNodes(e.GetGameObjectParameter("Speaker"), e.GetParameter<ConversationNode>("CurrentNode"), e.GetParameter<List<ConversationChoice>>("Choices"));
			}
		}
		if (!(e.ID == "GetConversationNode"))
		{
			return;
		}
		if (e.GetStringParameter("GotoID") == "*slynthmayorquestintronode")
		{
			GameObject parameter4 = e.GetParameter<GameObject>("Speaker");
			string propertyOrTag = parameter4.GetPropertyOrTag("Mayor");
			switch (propertyOrTag)
			{
			case "Joppa":
				generateIrudadQuestIntroNode(parameter4);
				break;
			case "Mechanimists":
				generateShebaQuestIntroNode(parameter4);
				break;
			case "Barathrumites":
				generateOthoQuestIntroNode(parameter4);
				break;
			case "Kyakukya":
				generateNuntuQuestIntroNode(parameter4);
				break;
			case "Ezra":
				generateHaddasQuestIntroNode(parameter4);
				break;
			case "Mopango":
				generateAgyraQuestIntroNode(parameter4);
				break;
			case "Pariahs":
				generateLulihartQuestIntroNode(parameter4);
				break;
			case "YdFreehold":
				generateGoekQuestIntroNode(parameter4);
				break;
			case "Hindren":
				if (string.Equals(parameter4.Blueprint, "Keh"))
				{
					generateKehQuestIntroNode(parameter4);
					break;
				}
				goto default;
			default:
				if (propertyOrTag == "Hindren")
				{
					generateEskhindQuestIntroNode(parameter4);
				}
				else
				{
					generateDynamicMayorQuestIntroNode(e.GetParameter<GameObject>("Speaker"));
				}
				break;
			}
			questIntroNode.alwaysShowAsNotVisted = !visited;
			e.SetParameter("ConversationNode", questIntroNode);
		}
		if (e.GetStringParameter("GotoID") == "*slynthmayorquestnode")
		{
			GameObject parameter5 = e.GetParameter<GameObject>("Speaker");
			switch (parameter5.GetPropertyOrTag("Mayor"))
			{
			case "Joppa":
				generateIrudadQuestNode(parameter5);
				break;
			case "Mechanimists":
				generateShebaQuestNode(parameter5);
				break;
			case "Barathrumites":
				generateOthoQuestNode(parameter5);
				break;
			case "Kyakukya":
				generateNuntuQuestNode(parameter5);
				break;
			case "Ezra":
				generateHaddasQuestNode(parameter5);
				break;
			case "Mopango":
				generateAgyraQuestNode(parameter5);
				break;
			case "Pariahs":
				generateLulihartQuestNode(parameter5);
				break;
			case "YdFreehold":
				generateGoekQuestNode(parameter5);
				break;
			case "Hindren":
				generateEskhindQuestNode(parameter5);
				break;
			default:
				generateDynamicMayorQuestNode(parameter5);
				break;
			}
			questNode.alwaysShowAsNotVisted = !visited;
			e.SetParameter("ConversationNode", questNode);
		}
	}

	public static void RevealHydropon()
	{
		JournalAPI.RevealMapNote(JournalAPI.GetMapNote("$hydropon"));
	}

	public static void SlynthQuestWish(Faction Faction = null, bool Complete = false)
	{
		The.Game.StartQuest(QUEST_NAME);
		SlynthQuestSystem system = The.Game.GetSystem<SlynthQuestSystem>();
		if (system.candidateFactions.Count < REQUIRED_CANDIDATES && The.Game.HasUnfinishedQuest(QUEST_NAME))
		{
			SlynthQuestCandidates();
		}
		if (Faction != null)
		{
			int num = system.candidateFactions.IndexOf(Faction.Name);
			system.settledFaction = ((num >= 0) ? system.candidateFactions[num] : null);
			system.settledZone = ((num >= 0) ? system.candidateFactionZones[num] : null);
		}
		if (Complete || Faction != null)
		{
			The.Game.CompleteQuest(QUEST_NAME);
		}
	}

	public static void SlynthQuestCandidates()
	{
		SlynthQuestSystem system = The.Game.GetSystem<SlynthQuestSystem>();
		system.candidateFactions.Clear();
		system.candidateFactionZones.Clear();
		Zone zone = The.ZoneManager.GetZone("JoppaWorld");
		List<VillageTerrain> list = new List<VillageTerrain>();
		for (int i = 0; i < 80; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				GameObject firstObjectWithPart = zone.GetCell(i, j).GetFirstObjectWithPart("VillageTerrain");
				if (firstObjectWithPart != null)
				{
					list.Add(firstObjectWithPart.GetPart<VillageTerrain>());
				}
			}
		}
		list.ShuffleInPlace();
		for (int k = 0; k < 3; k++)
		{
			VillageTerrain villageTerrain = list[k];
			Cell currentCell = villageTerrain.ParentObject.CurrentCell;
			HistoricEntitySnapshot currentSnapshot = villageTerrain.village.GetCurrentSnapshot();
			villageTerrain.FireEvent(Event.New("VillageReveal"));
			system.candidateFactions.Add("villagers of " + currentSnapshot.Name);
			system.candidateFactionZones.Add(ZoneID.Assemble("JoppaWorld", currentCell.X, currentCell.Y, 1, 1, 10));
		}
		int num = -The.Player.GetIntProperty("AllVisibleRepModifier");
		foreach (string candidateFaction in system.candidateFactions)
		{
			The.Game.PlayerReputation.set(candidateFaction, 600 + num);
		}
		system.updateQuestStatus();
	}

	[WishCommand("slynthquest", null)]
	public static void SlynthQuestWish(string Value)
	{
		Popup.bSuppressPopups = true;
		ItemNaming.Suppress = true;
		SlynthQuestWish();
		SlynthQuestSystem system = The.Game.GetSystem<SlynthQuestSystem>();
		if (!Value.Contains("start"))
		{
			if (Value.Contains("complete"))
			{
				SlynthQuestWish(null, Complete: true);
			}
			else if (Value.Contains("reset"))
			{
				ResetSlynthQuest(system);
			}
			else if (Value.Contains("leave"))
			{
				SlynthQuestWish(null, Complete: true);
				system.SlynthLeaveDays = 0;
				if (The.Player.InZone(The.Game.GetStringGameState("HydroponZoneID")))
				{
					system.HydroponActivated(The.ActiveZone);
				}
				else
				{
					The.Player.ZoneTeleport(The.Game.GetStringGameState("HydroponZoneID"));
				}
			}
			else if (Value.Contains("arrive"))
			{
				SlynthQuestWish(null, Complete: true);
				system.SlynthLeaveDays = 0;
				system.SlynthArriveDays = 0;
				if (The.Player.InZone(system.settledZone))
				{
					system.SettlementActivated(The.ActiveZone);
				}
				else
				{
					The.Player.ZoneTeleport(system.settledZone);
				}
			}
			else if (Value.Contains("settle"))
			{
				SlynthQuestWish(null, Complete: true);
				system.SlynthLeaveDays = 0;
				system.SlynthArriveDays = 0;
				system.SlynthSettleDays = 0;
				if (The.Player.InZone(system.settledZone))
				{
					system.SettlementActivated(The.ActiveZone);
				}
				else
				{
					The.Player.ZoneTeleport(system.settledZone);
				}
			}
			else
			{
				Faction faction = Factions.loop().FirstOrDefault((Faction f) => f.Name.EqualsNoCase(Value));
				if (faction == null)
				{
					MessageQueue.AddPlayerMessage("No faction found by that name.");
					return;
				}
				SlynthQuestWish(faction);
			}
		}
		Popup.bSuppressPopups = false;
		ItemNaming.Suppress = false;
	}

	public static void ResetSlynthQuest(SlynthQuestSystem System)
	{
		The.Game.Quests.Remove(QUEST_NAME);
		The.Game.FinishedQuests.Remove(QUEST_NAME);
		The.Game.RemoveInt64GameState("QuestFinishedTime_" + QUEST_NAME);
		System.candidateFactions.Clear();
		System.candidateFactionZones.Clear();
		System.stage = 0;
		System.settledZone = null;
		System.settledFaction = null;
		System.SlynthLeaveDays = -1;
		System.SlynthArriveDays = -1;
		System.SlynthSettleDays = -1;
		if (!The.Player.InZone(System.hydroponZone))
		{
			The.Player.ZoneTeleport(System.hydroponZone);
		}
		Zone activeZone = The.ActiveZone;
		List<Cell> reachableCells = activeZone.GetReachableCells();
		reachableCells.ShuffleInPlace();
		int i = activeZone.CountObjects("BaseSlynth");
		int num = 0;
		for (; i <= 14; i++)
		{
			reachableCells[num++].AddObject("BaseSlynth").MakeActive();
		}
	}
}
