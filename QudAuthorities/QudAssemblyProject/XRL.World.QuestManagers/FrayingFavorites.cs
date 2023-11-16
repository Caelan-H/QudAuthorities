using System;

namespace XRL.World.QuestManagers;

[Serializable]
public class FrayingFavorites : QuestManager
{
	public override void OnQuestAdded()
	{
		IComponent<GameObject>.ThePlayer.RegisterPartEvent(this, "PlayerAfterConversation");
		CheckChatFlags();
		if (The.Game.GetIntGameState("FinishedFrayingFavorites") == 1)
		{
			The.Game.FinishQuest("Fraying Favorites");
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterConversationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterConversationEvent E)
	{
		CheckChatFlags();
		return base.HandleEvent(E);
	}

	public void CheckChatFlags()
	{
		if (IComponent<GameObject>.ThePlayer.HasProperty("DoyobaChat") && IComponent<GameObject>.ThePlayer.HasProperty("DadogomChat") && IComponent<GameObject>.ThePlayer.HasProperty("GyamyoChat") && IComponent<GameObject>.ThePlayer.HasProperty("YonaChat"))
		{
			The.Game.FinishQuestStep("Fraying Favorites", "Optional: Speak with the Watchers");
		}
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.findByBlueprint("Lebah");
	}
}
