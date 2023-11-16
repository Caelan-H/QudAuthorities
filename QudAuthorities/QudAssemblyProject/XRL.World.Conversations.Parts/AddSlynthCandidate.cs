using XRL.Language;
using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class AddSlynthCandidate : IConversationPart
{
	/// <summary>Optional explicit sanctuary name to use instead of zone name.</summary>
	public string Sanctuary;

	public bool Plural;

	public string SactuaryZoneID => The.Speaker?.pBrain?.StartingCell?.ZoneID ?? The.Speaker?.CurrentZone?.ZoneID ?? The.ActiveZone.ZoneID;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (Sanctuary.IsNullOrEmpty())
		{
			Sanctuary = The.ZoneManager.GetZoneDisplayName(SactuaryZoneID, WithIndefiniteArticle: true);
		}
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{W|[confirm " + Sanctuary + " as a sanctuary option]}}";
		return false;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		SlynthQuestSystem system = The.Game.GetSystem<SlynthQuestSystem>();
		Popup.Show(Grammar.InitCap(Sanctuary) + (Plural ? " are" : " is") + " now a sanctuary option for the slynth.");
		system.candidateFactions.Add(The.Speaker?.GetPropertyOrTag("Mayor"));
		system.candidateFactionZones.Add(SactuaryZoneID);
		system.updateQuestStatus();
		return base.HandleEvent(E);
	}
}
