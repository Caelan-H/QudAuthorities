using System;
using XRL.Core;

namespace XRL.World.QuestManagers;

[Serializable]
public class SlayTheRaiders : QuestManager
{
	public int nRaidersKilled;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Killed");
		base.Register(Object);
	}

	public override void OnQuestAdded()
	{
		IComponent<GameObject>.ThePlayer.AddPart(this);
	}

	public override void OnQuestComplete()
	{
		IComponent<GameObject>.ThePlayer.RemovePart("SlayTheRaiders");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Killed" && E.GetGameObjectParameter("Object").Blueprint.Contains("Desert Raider"))
		{
			nRaidersKilled++;
			XRLCore.Core.Game.Quests["Slay the raiders"].StepsByID["Slay the raiders"].Text = "Slay thirty raiders. " + nRaidersKilled + "/30 killed";
			if (nRaidersKilled >= 30)
			{
				XRLCore.Core.Game.FinishQuestStep("Slay the raiders", "Slay the raiders");
			}
		}
		return true;
	}
}
