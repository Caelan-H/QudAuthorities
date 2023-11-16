using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.Core;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class FindASiteDynamicQuestManager : QuestManager
{
	[Serializable]
	public class FindASiteDynamicQuestManager_QuestEntry
	{
		public string zoneID;

		public string secretID;

		public string questID;

		public string questStepID;
	}

	public string _zoneID;

	public string _secretID;

	public string _questID;

	public string _questStepID;

	public List<FindASiteDynamicQuestManager_QuestEntry> quests;

	public FindASiteDynamicQuestManager()
	{
	}

	public FindASiteDynamicQuestManager(string zoneID, string secretID, string questID, string questStepID)
		: this()
	{
		_zoneID = zoneID;
		_secretID = secretID;
		_questID = questID;
		_questStepID = questStepID;
	}

	public override void OnQuestAdded()
	{
		FindASiteDynamicQuestManager findASiteDynamicQuestManager = The.Player.RequirePart<FindASiteDynamicQuestManager>();
		FindASiteDynamicQuestManager_QuestEntry findASiteDynamicQuestManager_QuestEntry = new FindASiteDynamicQuestManager_QuestEntry();
		findASiteDynamicQuestManager_QuestEntry.zoneID = _zoneID;
		findASiteDynamicQuestManager_QuestEntry.secretID = _secretID;
		findASiteDynamicQuestManager_QuestEntry.questID = _questID;
		findASiteDynamicQuestManager_QuestEntry.questStepID = _questStepID;
		if (findASiteDynamicQuestManager.quests == null)
		{
			findASiteDynamicQuestManager.quests = new List<FindASiteDynamicQuestManager_QuestEntry>();
		}
		findASiteDynamicQuestManager.quests.Add(findASiteDynamicQuestManager_QuestEntry);
		XRLCore.Core.Game.Player.Body.RegisterPartEvent(findASiteDynamicQuestManager, "EnteredCell");
	}

	public override void OnQuestComplete()
	{
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.find((GameObject obj) => obj.GetStringProperty("GivesDynamicQuest") == _questID);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		base.Register(Object);
		Object.RegisterPartEvent(this, "AfterSecretRevealed");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterSecretRevealed" && E.GetParameter("Secret") is JournalMapNote secret)
		{
			CheckCompleted(null, secret);
		}
		return base.FireEvent(E);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		CheckCompleted(E.Zone);
		return base.HandleEvent(E);
	}

	public void CheckCompleted(Zone Zone = null, JournalMapNote Secret = null)
	{
		for (int num = quests.Count - 1; num >= 0; num--)
		{
			FindASiteDynamicQuestManager_QuestEntry findASiteDynamicQuestManager_QuestEntry = quests[num];
			if (Zone == null)
			{
				Zone = ParentObject.CurrentZone;
			}
			if (Zone?.ZoneID != findASiteDynamicQuestManager_QuestEntry.zoneID)
			{
				continue;
			}
			if (Secret == null)
			{
				Secret = JournalAPI.GetMapNote(findASiteDynamicQuestManager_QuestEntry.secretID);
				if (Secret != null && !Secret.revealed)
				{
					continue;
				}
			}
			else if (Secret.secretid != findASiteDynamicQuestManager_QuestEntry.secretID)
			{
				continue;
			}
			if (!The.Game.HasFinishedQuestStep(findASiteDynamicQuestManager_QuestEntry.questID, findASiteDynamicQuestManager_QuestEntry.questStepID))
			{
				string text = Secret?.text ?? Zone?.DisplayName ?? "a site";
				The.Game.FinishQuestStep(findASiteDynamicQuestManager_QuestEntry.questID, findASiteDynamicQuestManager_QuestEntry.questStepID);
				JournalAPI.AddAccomplishment("You located " + text + ".", "Through the use of " + IComponent<GameObject>.ThePlayer.GetPronounProvider().PossessiveAdjective + " divinely " + HistoricStringExpander.ExpandString("<spice.elements." + IComponent<GameObject>.ThePlayerMythDomain + ".adjectives.!random>") + " eyes, =name= discovered the lost location of " + text + ".", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			}
			quests.RemoveAt(num);
		}
		if (quests.Count == 0)
		{
			The.Player.RemovePart(this);
		}
	}
}
