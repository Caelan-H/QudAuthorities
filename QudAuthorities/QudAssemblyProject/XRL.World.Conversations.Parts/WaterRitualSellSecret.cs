using System;
using System.Text;
using Qud.API;
using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class WaterRitualSellSecret : IWaterRitualSecretPart
{
	public string Message;

	public int Bonus;

	public bool Secret = true;

	public bool Gossip
	{
		get
		{
			return !Secret;
		}
		set
		{
			Secret = !value;
		}
	}

	public override bool Affordable => WaterRitual.Record.totalFactionAvailable > 0;

	public override bool Filter(IBaseJournalEntry Entry)
	{
		return WaterRitual.RecordFaction.WantsToBuySecret(Entry, The.Speaker);
	}

	public override bool SultanNote(JournalSultanNote Entry)
	{
		return Secret;
	}

	public override bool Observation(JournalObservation Entry)
	{
		return Entry.category != "Gossip" == Secret;
	}

	public override bool MapNote(JournalMapNote Entry)
	{
		return Secret;
	}

	public override bool RecipeNote(JournalRecipeNote Entry)
	{
		return Secret;
	}

	public void Share()
	{
		Random r = new Random(WaterRitual.Record.mySeed);
		GetShuffledSecrets(r, out var Notes, out var Options);
		int num = Popup.ShowOptionList(Secret ? "Choose a secret to share:" : "Choose some gossip to share:", Options.ToArray(), null, 1, null, 60, RespectOptionNewlines: true, AllowEscape: true);
		if (num >= 0)
		{
			IBaseJournalEntry baseJournalEntry = Notes[num];
			baseJournalEntry.secretSold = true;
			baseJournalEntry.AppendHistory(" {{K|-shared with " + WaterRitual.RecordFaction.getFormattedName() + "}}");
			baseJournalEntry.Updated();
			baseJournalEntry.Reveal();
			AwardReputation(Bonus);
		}
	}

	public override void Awake()
	{
		base.Awake();
		Message = null;
		Reputation = (Secret ? 50 : 100);
		Bonus = 0;
		GetWaterRitualSellSecretBehaviorEvent.Send(The.Player, The.Speaker, ref Message, ref Reputation, ref Bonus, Secret, Gossip);
		Reputation = Math.Min(Reputation, WaterRitual.Record.totalFactionAvailable);
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != EnteredElementEvent.ID)
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		if (!Message.IsNullOrEmpty())
		{
			E.Text.Clear();
			E.Text.Append(Message);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (WaterRitual.Record.totalFactionAvailable <= 0)
		{
			Popup.ShowFail(The.Speaker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " can't grant you any more reputation.");
		}
		else
		{
			Share();
			base.Awake();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder().Append("{{").Append(Lowlight)
			.Append("|[{{")
			.Append(Numeric)
			.Append("|+")
			.Append(Reputation);
		if (Bonus != 0)
		{
			if (Affordable)
			{
				stringBuilder.Append("{{c|");
			}
			stringBuilder.Append((Bonus > 0) ? '+' : '-').Append(Bonus);
			if (Affordable)
			{
				stringBuilder.Append("}}");
			}
		}
		E.Tag = stringBuilder.Append("}} reputation]}}").ToString();
		return false;
	}
}
