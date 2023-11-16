using System;
using HistoryKit;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class VillageSurface : IPart
{
	public string VillageName;

	public string RevealString;

	public string RevealSecret;

	public string RevealKey;

	public bool IsVillageZero;

	public Vector2i RevealLocation;

	public int region = -1;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != ZoneActivatedEvent.ID)
		{
			return ID == ZoneThawedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		IComponent<GameObject>.ThePlayer?.RegisterPartEvent(this, "EnteredCell");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		IComponent<GameObject>.ThePlayer?.RegisterPartEvent(this, "EnteredCell");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		IComponent<GameObject>.ThePlayer?.RegisterPartEvent(this, "EnteredCell");
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		CheckReveal();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (IComponent<GameObject>.ThePlayer != null)
			{
				IComponent<GameObject>.ThePlayer.UnregisterPartEvent(this, "EnteredCell");
			}
			CheckReveal();
		}
		return base.FireEvent(E);
	}

	public void CheckReveal()
	{
		if (IComponent<GameObject>.ThePlayer == null)
		{
			return;
		}
		Zone currentZone = ParentObject.CurrentZone;
		if (!IComponent<GameObject>.ThePlayer.InZone(currentZone))
		{
			return;
		}
		string questID = "Visit " + VillageName;
		string state = "Visited_" + VillageName;
		if (The.Game.HasQuest(questID))
		{
			The.Game.CompleteQuest(questID);
			if (The.Game.GetIntGameState(state) != 1)
			{
				if (!IsVillageZero)
				{
					AchievementManager.IncrementAchievement("ACH_100_VILLAGES");
				}
				The.Game.SetIntGameState(state, 1);
				if (IComponent<GameObject>.ThePlayer != null)
				{
					JournalAPI.AddAccomplishment("You visited the village of " + VillageName + ".", HistoricStringExpander.ExpandString("In the month of " + Calendar.getMonth() + " of " + Calendar.getYear() + " AR, =name= founded the village of " + VillageName + " to <spice.history.gospels.HumblePractice.LateSultanate.!random>."), "general", JournalAccomplishment.MuralCategory.BecomesLoved, JournalAccomplishment.MuralWeight.Medium, null, -1L);
				}
			}
		}
		else if (The.Game.GetIntGameState(state) != 1)
		{
			if (!IsVillageZero && IComponent<GameObject>.ThePlayer != null)
			{
				IComponent<GameObject>.ThePlayer.AwardXP(currentZone.NewTier * 250);
				AchievementManager.IncrementAchievement("ACH_100_VILLAGES");
			}
			The.Game.SetIntGameState(state, 1);
			if (IComponent<GameObject>.ThePlayer != null)
			{
				JournalAPI.AddAccomplishment("You visited the village of " + VillageName + ".", "In the month of " + Calendar.getMonth() + " of " + Calendar.getYear() + " AR, =name= founded the village of " + VillageName + " to <spice.history.gospels.HumblePractice.LateSultanate.!random>.", "general", JournalAccomplishment.MuralCategory.BecomesLoved, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			}
		}
		if (!The.Game.HasIntGameState(RevealKey))
		{
			The.Game.SetIntGameState(RevealKey, 1);
			if (RevealLocation != null && The.Game.ZoneManager.GetZone("JoppaWorld").GetCell(RevealLocation.x, RevealLocation.y).FireEvent("VillageReveal") && RevealString != null && !IsVillageZero && IComponent<GameObject>.ThePlayer != null)
			{
				Popup.Show(RevealString);
				The.Game.Systems.ForEach(delegate(IGameSystem s)
				{
					s.LocationDiscovered(VillageName);
				});
			}
			JournalMapNote mapNote = JournalAPI.GetMapNote(RevealSecret);
			if (mapNote != null && !mapNote.revealed)
			{
				JournalAPI.RevealMapNote(mapNote);
			}
		}
		ParentObject.Obliterate();
	}
}
