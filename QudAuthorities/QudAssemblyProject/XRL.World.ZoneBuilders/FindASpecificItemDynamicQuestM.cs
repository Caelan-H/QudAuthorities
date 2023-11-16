using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Core;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class FindASpecificItemDynamicQuestManager : QuestManager
{
	[Serializable]
	public class FindASpecificItemDynamicQuestManager_QuestEntry
	{
		public string itemID;

		public string questID;

		public string questStepID;
	}

	public string _itemID;

	public string _questID;

	public string _questStepID;

	public List<FindASpecificItemDynamicQuestManager_QuestEntry> quests;

	public FindASpecificItemDynamicQuestManager()
	{
	}

	public FindASpecificItemDynamicQuestManager(string itemID, string questID, string questStepID)
	{
		_itemID = itemID;
		_questID = questID;
		_questStepID = questStepID;
	}

	public override void OnQuestAdded()
	{
		FindASpecificItemDynamicQuestManager findASpecificItemDynamicQuestManager = The.Player.RequirePart<FindASpecificItemDynamicQuestManager>();
		FindASpecificItemDynamicQuestManager_QuestEntry findASpecificItemDynamicQuestManager_QuestEntry = new FindASpecificItemDynamicQuestManager_QuestEntry();
		findASpecificItemDynamicQuestManager_QuestEntry.itemID = _itemID;
		findASpecificItemDynamicQuestManager_QuestEntry.questID = _questID;
		findASpecificItemDynamicQuestManager_QuestEntry.questStepID = _questStepID;
		if (findASpecificItemDynamicQuestManager.quests == null)
		{
			findASpecificItemDynamicQuestManager.quests = new List<FindASpecificItemDynamicQuestManager_QuestEntry>();
		}
		findASpecificItemDynamicQuestManager.quests.Add(findASpecificItemDynamicQuestManager_QuestEntry);
		XRLCore.Core.Game.Player.Body.RegisterPartEvent(findASpecificItemDynamicQuestManager, "Took");
		XRLCore.Core.Game.Player.Body.RegisterPartEvent(findASpecificItemDynamicQuestManager, "Equipping");
		XRLCore.Core.Game.Player.Body.RegisterPartEvent(findASpecificItemDynamicQuestManager, "EquipperEquipped");
		XRLCore.Core.Game.Player.Body.RegisterPartEvent(findASpecificItemDynamicQuestManager, "InvCommandActivating");
		TestForItem();
	}

	public override void OnQuestComplete()
	{
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.find((GameObject obj) => obj.GetStringProperty("GivesDynamicQuest") == _questID);
	}

	public bool TestForItem()
	{
		while (quests != null)
		{
			foreach (FindASpecificItemDynamicQuestManager_QuestEntry entry in quests)
			{
				bool again = false;
				IComponent<GameObject>.ThePlayer.ForeachInventoryAndEquipment(delegate(GameObject GO)
				{
					if (GO.idmatch(entry.itemID))
					{
						The.Game.FinishQuestStep(entry.questID, entry.questStepID);
						GameObject gameObject = base.zoneManager.peekCachedObject(entry.itemID);
						JournalAPI.AddAccomplishment("You recovered " + gameObject.the + gameObject.ShortDisplayNameSingle + ".", "While exploring " + IComponent<GameObject>.ThePlayer.CurrentZone.DisplayName + ", =name= recovered the fabled artifact called " + gameObject.the + gameObject.DisplayName + ".", "general", JournalAccomplishment.MuralCategory.FindsObject, JournalAccomplishment.MuralWeight.Medium, null, -1L);
						quests.Remove(entry);
						if (quests.Count == 0)
						{
							IComponent<GameObject>.ThePlayer.RemovePart(this);
							again = false;
						}
						else
						{
							again = true;
						}
					}
				});
				if (again)
				{
					goto IL_0000;
				}
			}
			break;
			IL_0000:;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Took" || E.ID == "Equipping" || E.ID == "EquipperEquipped" || E.ID == "InvCommandActivating")
		{
			while (quests != null)
			{
				foreach (FindASpecificItemDynamicQuestManager_QuestEntry quest in quests)
				{
					if (E.GetGameObjectParameter("Object").idmatch(quest.itemID))
					{
						The.Game.FinishQuestStep(quest.questID, quest.questStepID);
						GameObject gameObject = The.ZoneManager.peekCachedObject(quest.itemID);
						JournalAPI.AddAccomplishment("You recovered " + gameObject.the + gameObject.ShortDisplayNameSingle + ".", "While exploring " + IComponent<GameObject>.ThePlayer.CurrentZone.DisplayName + ", =name= recovered the fabled artifact called " + gameObject.the + gameObject.ShortDisplayNameSingle + ".", "general", JournalAccomplishment.MuralCategory.FindsObject, JournalAccomplishment.MuralWeight.Medium, null, -1L);
						quests.Remove(quest);
						if (quests.Count == 0)
						{
							IComponent<GameObject>.ThePlayer.RemovePart(this);
							return true;
						}
						goto IL_004b;
					}
				}
				break;
				IL_004b:;
			}
		}
		return true;
	}
}
