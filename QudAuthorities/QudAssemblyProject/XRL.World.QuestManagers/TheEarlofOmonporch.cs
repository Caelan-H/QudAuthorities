using System;

namespace XRL.World.QuestManagers;

[Serializable]
public class TheEarlofOmonporch : QuestManager
{
	public override void OnQuestAdded()
	{
		The.Player.AddPart(this);
		if (The.Game.GetIntGameState("AsphodelSlain") == 1)
		{
			The.Game.FinishQuestStep("The Earl of Omonporch", "Travel to Omonporch");
			The.Game.FinishQuestStep("The Earl of Omonporch", "Secure the Spindle");
		}
	}

	public override void OnQuestComplete()
	{
		The.Player.RemovePart("TheEarlofOmonporch");
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.findByBlueprint("Otho");
	}
}
