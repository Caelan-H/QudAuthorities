using System;
using XRL.Rules;

namespace XRL.World;

[Serializable]
public class FindASiteDynamicQuestTemplate : BaseDynamicQuestTemplate
{
	public override void init(DynamicQuestContext context)
	{
		Array values = Enum.GetValues(typeof(QuestStoryType_FindASpecificItem));
		QuestStoryType_FindASite questStoryType_FindASite = (QuestStoryType_FindASite)values.GetValue(Stat.Random(0, values.Length - 1));
		base.zoneManager.AddZonePostBuilderAfterTerrain(context.getQuestGiverZone(), new ZoneBuilderBlueprint("FindASpecificSiteDynamicQuestTemplate_FabricateQuestGiver", "questContext", context, "QST", questStoryType_FindASite, "questGiverFilter", context.getQuestGiverFilter(), "reward", context.getQuestReward()));
	}
}
