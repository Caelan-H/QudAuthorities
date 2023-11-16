using System;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
public class FindASpecificItemDynamicQuestTemplate : BaseDynamicQuestTemplate
{
	public override void init(DynamicQuestContext context)
	{
		Array values = Enum.GetValues(typeof(QuestStoryType_FindASpecificItem));
		QuestStoryType_FindASpecificItem questStoryType_FindASpecificItem = (QuestStoryType_FindASpecificItem)values.GetValue(Stat.Random(0, values.Length - 1));
		GameObject questDeliveryItem = context.getQuestDeliveryItem();
		questDeliveryItem.AddPart(new DynamicQuestTarget());
		questDeliveryItem.pRender.DisplayName = context.getQuestItemNameMutation(questDeliveryItem.DisplayNameOnlyDirectAndStripped);
		questDeliveryItem.SetStringProperty("HasPregeneratedName", "yes");
		string text = base.zoneManager.CacheObject(questDeliveryItem);
		DynamicQuestDeliveryTarget questDeliveryTarget = context.getQuestDeliveryTarget();
		base.zoneManager.AddZonePostBuilderAfterTerrain(context.getQuestGiverZone(), new ZoneBuilderBlueprint("FindASpecificItemDynamicQuestTemplate_FabricateQuestGiver", "questContext", context, "QST", questStoryType_FindASpecificItem, "questGiverFilter", context.getQuestGiverFilter(), "deliveryItemID", text, "deliveryTarget", questDeliveryTarget, "reward", context.getQuestReward()));
		base.zoneManager.AddZonePostBuilderAfterTerrain(questDeliveryTarget.zoneId, new ZoneBuilderBlueprint("FindASpecificItemDynamicQuestTemplate_FabricateQuestItem", "deliveryItemID", text));
	}
}
