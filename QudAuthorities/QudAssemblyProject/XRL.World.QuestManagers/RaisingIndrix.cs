using System;

namespace XRL.World.QuestManagers;

[Serializable]
public class RaisingIndrix : QuestManager
{
	public int nRaidersKilled;

	public override void OnQuestAdded()
	{
	}

	public override void OnQuestComplete()
	{
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.findByBlueprint("Warden Indrix");
	}
}
