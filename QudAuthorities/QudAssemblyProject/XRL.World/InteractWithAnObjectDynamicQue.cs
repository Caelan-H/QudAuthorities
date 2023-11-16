using System;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
public class InteractWithAnObjectDynamicQuestTemplate : BaseDynamicQuestTemplate
{
	public override void init(DynamicQuestContext context)
	{
		Array values = Enum.GetValues(typeof(QuestStoryType_InteractWithAnObject));
		QuestStoryType_InteractWithAnObject questStoryType_InteractWithAnObject = (QuestStoryType_InteractWithAnObject)values.GetValue(Stat.Random(0, values.Length - 1));
		GameObject gameObject = ((questStoryType_InteractWithAnObject != QuestStoryType_InteractWithAnObject.StrangePlan) ? context.getQuestRemoteInteractable() : context.getQuestGenericRemoteInteractable());
		gameObject.AddPart(new DynamicQuestTarget());
		gameObject.SetIntProperty("NoAIEquip", 1);
		if (!gameObject.HasPart("Shrine"))
		{
			gameObject.pRender.DisplayName = context.getQuestItemNameMutation(gameObject.pRender.DisplayName);
			gameObject.SetStringProperty("HasPregeneratedName", "yes");
			gameObject.HasProperName = true;
			gameObject.SetStringProperty("DefiniteArticle", "the");
			gameObject.SetStringProperty("IndefiniteArticle", "the");
		}
		gameObject.RequirePart<Interesting>().Radius = 1;
		if (gameObject.IsTakeable())
		{
			gameObject.RequirePart<RemoveInterestingOnTake>();
		}
		gameObject.FireEvent("SpecialInit");
		string text = base.zoneManager.CacheObject(gameObject);
		DynamicQuestDeliveryTarget questDeliveryTarget = context.getQuestDeliveryTarget();
		base.zoneManager.AddZonePostBuilderAfterTerrain(context.getQuestGiverZone(), new ZoneBuilderBlueprint("InteractWithAnObjectDynamicQuestTemplate_FabricateQuestGiver", "questContext", context, "questGiverFilter", context.getQuestGiverFilter(), "QST", questStoryType_InteractWithAnObject, "deliveryItemID", text, "deliveryTarget", questDeliveryTarget, "reward", context.getQuestReward()));
		base.zoneManager.AddZonePostBuilderAfterTerrain(questDeliveryTarget.zoneId, new ZoneBuilderBlueprint("InteractWithAnObjectDynamicQuestTemplate_FabricateQuestItem", "deliveryItemID", text));
	}
}
