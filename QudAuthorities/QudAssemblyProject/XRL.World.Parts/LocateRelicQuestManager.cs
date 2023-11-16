using System;
using HistoryKit;
using Qud.API;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class LocateRelicQuestManager : QuestManager
{
	public string Relic;

	public string QuestID;

	public override void OnQuestAdded()
	{
		IComponent<GameObject>.ThePlayer.AddPart(this);
	}

	public override void OnQuestComplete()
	{
		int i = 0;
		for (int count = IComponent<GameObject>.ThePlayer.PartsList.Count; i < count; i++)
		{
			IPart part = IComponent<GameObject>.ThePlayer.PartsList[i];
			if (part is LocateRelicQuestManager && part.SameAs(this))
			{
				IComponent<GameObject>.ThePlayer.RemovePart(part);
				break;
			}
		}
	}

	public override bool SameAs(IPart p)
	{
		LocateRelicQuestManager locateRelicQuestManager = p as LocateRelicQuestManager;
		if (locateRelicQuestManager.Relic != Relic)
		{
			return false;
		}
		if (locateRelicQuestManager.QuestID != QuestID)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Took");
		Object.RegisterPartEvent(this, "Equipping");
		Object.RegisterPartEvent(this, "EquipperEquipped");
		Object.RegisterPartEvent(this, "InvCommandActivating");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Took" || E.ID == "Equipping" || E.ID == "EquipperEquipped" || E.ID == "InvCommandActivating")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (!string.IsNullOrEmpty(Relic) && gameObjectParameter != null && gameObjectParameter.GetStringProperty("RelicName") == Relic)
			{
				if (XRLCore.Core.Game.HasFinishedQuest(QuestID))
				{
					MetricsManager.LogError(QuestID + " manager for " + Relic + " present after completion, removing");
					IComponent<GameObject>.ThePlayer.RemovePart(this);
				}
				else
				{
					XRLCore.Core.Game.CompleteQuest(QuestID);
					JournalAPI.AddAccomplishment("You recovered the historic relic, " + Relic + ".", HistoricStringExpander.ExpandString("<spice.commonPhrases.intrepid.!random.capitalize> =name= recovered " + Relic + ", a historic relic once thought lost to the sands of time."), "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.High, null, -1L);
				}
			}
		}
		return base.FireEvent(E);
	}
}
