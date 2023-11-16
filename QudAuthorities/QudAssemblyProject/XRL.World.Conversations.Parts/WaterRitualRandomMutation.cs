using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class WaterRitualRandomMutation : IWaterRitualPart
{
	public MutationEntry Entry;

	public string Category = "Physical";

	public string Message;

	public bool Mutant;

	public bool Incompatible;

	public override bool Available
	{
		get
		{
			if (Mutant)
			{
				return !Incompatible;
			}
			return false;
		}
	}

	public override void Awake()
	{
		if (WaterRitual.Alternative || WaterRitual.RecordFaction.WaterRitualRandomMentalMutation <= 0 || WaterRitual.Record.Has("SoldRandom" + Category + "Mutation"))
		{
			return;
		}
		string text = "Random" + Category + "Mutation:";
		if (WaterRitual.Record.TryGetAttribute(text, out var Value))
		{
			Entry = MutationFactory.GetMutationEntryByName(Value);
			if (Entry == null)
			{
				return;
			}
		}
		else
		{
			List<MutationEntry> list = new List<MutationEntry>(from e in The.Player.GetPart<Mutations>().GetMutatePool()
				where e.Category.Name == Category
				select e);
			if (list.IsNullOrEmpty())
			{
				return;
			}
			Entry = list.GetRandomElement();
			WaterRitual.Record.attributes.Add(text + Entry.DisplayName);
		}
		if (The.Player.Property.TryGetValue("MutationLevel", out Value))
		{
			MutationEntry mutationEntryByName = MutationFactory.GetMutationEntryByName(Value);
			Incompatible = mutationEntryByName != null && !mutationEntryByName.OkWith(Entry);
		}
		Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "Mutation", WaterRitual.RecordFaction.WaterRitualRandomMentalMutation * Entry.Cost);
		Mutant = The.Player.IsMutant();
		Visible = true;
	}

	public override void LoadText(string Text)
	{
		Message = Text;
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
		if (!Mutant)
		{
			Popup.ShowFail("You can't be mutated.");
		}
		else if (Incompatible)
		{
			Popup.ShowFail("You can't gain " + Category.ToLowerInvariant() + " mutations.");
		}
		else if (UseReputation())
		{
			if (!Message.IsNullOrEmpty())
			{
				StringBuilder stringBuilder = Event.NewStringBuilder(Message);
				stringBuilder.Replace("=subject.T=", The.Speaker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: true)).Replace("=subject.t=", The.Speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: true)).Replace("=mutation.name=", "{{M|" + Entry.DisplayName + "}}");
				Popup.Show(GameText.VariableReplace(stringBuilder, The.Speaker));
			}
			The.Player.GetPart<Mutations>().AddMutation(Entry.CreateInstance(), 1);
			WaterRitual.Record.attributes.Add("SoldRandom" + Category + "Mutation");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[gain {{M|" + Entry.DisplayName + "}}: {{" + Numeric + "|-" + Reputation + "}} reputation]}}";
		return false;
	}
}
