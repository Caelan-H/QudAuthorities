using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using Qud.API;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AIPilgrim : IPart
{
	public bool FoundTarget;

	public int StiltWx = 5;

	public int StiltWy = 2;

	public int StiltXx = 1;

	public int StiltYx = 1;

	public int StiltZx = 10;

	public string StiltZoneID = "JoppaWorld.5.2.1.1.10";

	public string StiltEntranceZoneID = "JoppaWorld.5.2.1.2.10";

	public string TargetObject = "StiltWell";

	public string MapNoteAttributes;

	public int Chance = 100;

	public bool bIgnore;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIBoredEvent.ID)
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (CheckStartPilgrimage())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("travel", 1);
		return base.HandleEvent(E);
	}

	public override void Initialize()
	{
		if (!If.d100(Chance))
		{
			bIgnore = true;
		}
		base.Initialize();
	}

	public bool CheckStartPilgrimage()
	{
		if (bIgnore)
		{
			return false;
		}
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return false;
		}
		if (ParentObject.PartyLeader != null)
		{
			return false;
		}
		if (ParentObject.HasTagOrProperty("ExcludeFromDynamicEncounters"))
		{
			return false;
		}
		if (FoundTarget)
		{
			return false;
		}
		if (MapNoteAttributes != null)
		{
			if (MapNoteAttributes == "invalid-tag")
			{
				return false;
			}
			if (ParentObject.CurrentZone == null)
			{
				return false;
			}
			IEnumerable<JournalMapNote> mapNotesWithAllAttributes = JournalAPI.GetMapNotesWithAllAttributes(MapNoteAttributes);
			MapNoteAttributes = "invalid-tag";
			if (mapNotesWithAllAttributes.Count() == 0)
			{
				return false;
			}
			JournalMapNote randomElement = mapNotesWithAllAttributes.ToList().GetRandomElement();
			StiltWx = randomElement.wx;
			StiltWy = randomElement.wy;
			StiltXx = randomElement.cx;
			StiltYx = randomElement.cy;
			StiltZx = randomElement.cz;
			StiltZoneID = randomElement.zoneid;
			StiltEntranceZoneID = randomElement.zoneid;
		}
		ParentObject.pBrain.PushGoal(new GoOnAPilgrimage(StiltWx, StiltWy, StiltXx, StiltYx, StiltZx, TargetObject, StiltZoneID, StiltEntranceZoneID));
		return true;
	}
}
