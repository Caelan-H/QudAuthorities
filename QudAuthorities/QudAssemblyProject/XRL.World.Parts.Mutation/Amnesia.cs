using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Core;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Amnesia : BaseMutation
{
	[NonSerialized]
	public Dictionary<string, VisCell> Cells = new Dictionary<string, VisCell>();

	public string currentZone;

	[NonSerialized]
	private long LastProccedTurn = -1L;

	public Amnesia()
	{
		DisplayName = "Amnesia ({{r|D}})";
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeSecretRevealed");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You forget things and places.\n\nWhenever you learn a new secret, there's a small chance you forget a secret.\nWhenever you return to a map you previously visited, there's a small chance you forget the layout.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public bool DoesSecretTrigger(IBaseJournalEntry Entry)
	{
		if (Entry is JournalSultanNote)
		{
			return true;
		}
		if (Entry is JournalMapNote)
		{
			return true;
		}
		if (Entry is JournalObservation)
		{
			return true;
		}
		if (Entry is JournalRecipeNote)
		{
			return true;
		}
		return false;
	}

	public bool doesAffectSecret(IBaseJournalEntry note)
	{
		if (note == null)
		{
			return false;
		}
		if (!note.Forgettable())
		{
			return false;
		}
		if (!note.Has("gossip") && !note.Has("sultan") && !note.Has("village"))
		{
			return note is JournalMapNote;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeSecretRevealed" && ParentObject.IsPlayer() && LastProccedTurn != The.Game.Turns)
		{
			IBaseJournalEntry parameter = E.GetParameter<IBaseJournalEntry>("Secret");
			if (DoesSecretTrigger(parameter) && 5.in100())
			{
				LastProccedTurn = The.Game.Turns;
				IBaseJournalEntry randomElement = JournalAPI.GetKnownNotes(doesAffectSecret).GetRandomElement();
				if (randomElement != null)
				{
					randomElement.Forget();
					IComponent<GameObject>.AddPlayerMessage("You feel like you forgot something important.");
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (5.in100() && ParentObject.IsPlayer())
		{
			Zone zone = ParentObject.CurrentZone;
			if (zone != null && currentZone != zone.ZoneID && zone.LastPlayerPresence != -1 && zone.LastPlayerPresence < XRLCore.CurrentTurn - 2)
			{
				IComponent<GameObject>.AddPlayerMessage("This place feels vaguely familiar.");
				zone.ClearExploredMap();
				zone.ClearFakeUnexploredMap();
				GenericDeepNotifyEvent.Send(ParentObject, "AmnesiaTriggered", ParentObject, ParentObject);
			}
			currentZone = zone.ZoneID;
		}
		return base.HandleEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}
