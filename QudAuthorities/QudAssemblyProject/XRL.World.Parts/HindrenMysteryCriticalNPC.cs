using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class HindrenMysteryCriticalNPC : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != ReplicaCreatedEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		The.Game.SetStringGameState("HindrenMysteryCriticalNPCKilled", "1");
		if (The.Game.HasQuest("Kith and Kin") && !The.Game.FinishedQuest("Kith and Kin"))
		{
			Popup.Show("The death of " + ParentObject.BaseDisplayName + " means that the investigation can go no further.");
			The.Game.FailQuest("Kith and Kin");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (ParentObject.Blueprint.EndsWith("Keh"))
		{
			ParentObject.Blueprint = "Keh";
			if (The.Game.GetStringGameState("HindrenMysteryOutcomeHindriarch", "Keh") == "Keh")
			{
				ParentObject.SetStringProperty("Mayor", "Hindren");
			}
			else
			{
				ParentObject.RemoveStringProperty("Mayor");
				if (The.Game.GetStringGameState("HindrenMysteryOutcomeThief") == "Keh" && !ParentObject.HasPart("HindrenMysteryExile"))
				{
					HindrenQuestOutcome.Exile(ParentObject);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
