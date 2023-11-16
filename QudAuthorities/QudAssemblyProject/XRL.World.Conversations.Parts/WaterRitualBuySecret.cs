using System;
using System.Linq;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class WaterRitualBuySecret : IWaterRitualSecretPart
{
	public override bool Available => WaterRitual.Record.secretsRemaining > 0;

	public override bool Filter(IBaseJournalEntry Entry)
	{
		return WaterRitual.RecordFaction.WantsToSellSecret(Entry);
	}

	public override void Awake()
	{
		base.Awake();
		Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "Secret", 50);
	}

	public void Share()
	{
		Random r = new Random(WaterRitual.Record.mySeed);
		GetShuffledSecrets(r, out var Notes, out var _);
		RevealEntry(Notes.GetRandomElement(r));
	}

	public void RevealEntry(IBaseJournalEntry entry)
	{
		bool flag = true;
		entry.attributes.Add(WaterRitual.RecordFaction.NoBuySecretString);
		WaterRitual.Record.secretsRemaining--;
		if (!(entry is JournalSultanNote journalSultanNote))
		{
			if (!(entry is JournalMapNote journalMapNote))
			{
				if (!(entry is JournalObservation journalObservation))
				{
					if (entry is JournalRecipeNote)
					{
						Popup.Show(The.Speaker.Does("share", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " a recipe with you.");
					}
				}
				else
				{
					string text = The.Speaker.Does("share", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " some gossip with you.";
					if (entry.Has("gossip"))
					{
						text += "\n\n\"";
						string text2 = HistoricStringExpander.ExpandString("<spice.gossip.leadIns.!random>", null, The.Game.sultanHistory);
						text = ((!text2.Contains('?') && !text2.Contains('.') && !journalObservation.initCapAsFragment) ? (text + text2 + " " + Grammar.InitLower(journalObservation.text)) : (text + text2 + " " + journalObservation.text));
						text += "\"";
					}
					Popup.Show(text);
				}
			}
			else
			{
				Popup.Show(The.Speaker.Does("share", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " the location of " + Markup.Wrap(Grammar.LowerArticles(journalMapNote.text)) + ".");
			}
		}
		else
		{
			HistoricEvent @event = HistoryAPI.GetEvent(journalSultanNote.eventId);
			Popup.Show(The.Speaker.Does("share", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " an event from the life of a sultan with you.\n\n\"" + @event.GetEventProperty("gospel") + "\"");
			@event.Reveal();
			flag = false;
		}
		if (WaterRitual.RecordFaction.Visible)
		{
			entry.AppendHistory(" {{K|-learned from " + WaterRitual.RecordFaction.getFormattedName() + "}}");
		}
		if (flag || !entry._revealed)
		{
			entry.Reveal();
		}
		entry.Updated();
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (WaterRitual.Record.secretsRemaining <= 0)
		{
			Popup.ShowFail(The.Speaker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " has no more secrets to share.");
		}
		else if (UseReputation())
		{
			Share();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[{{" + Numeric + "|-" + Reputation + "}} reputation]}}";
		return false;
	}
}
