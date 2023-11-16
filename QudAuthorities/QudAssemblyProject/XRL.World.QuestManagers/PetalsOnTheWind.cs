using System;

namespace XRL.World.QuestManagers;

[Serializable]
public class PetalsOnTheWind : QuestManager
{
	public override void OnQuestAdded()
	{
		ZoneManager.instance.GetZone("JoppaWorld").BroadcastEvent("BeyLahReveal");
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.findByBlueprint("Lulihart");
	}
}
