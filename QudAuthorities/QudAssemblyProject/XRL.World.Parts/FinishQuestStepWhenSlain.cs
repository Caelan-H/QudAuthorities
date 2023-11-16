using System;

namespace XRL.World.Parts;

[Serializable]
public class FinishQuestStepWhenSlain : IPart
{
	public string Quest;

	public string Step;

	public string GameState;

	[FieldSaveVersion(242)]
	public bool RequireQuest;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeDie");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDie")
		{
			if (GameState != null)
			{
				The.Game.SetIntGameState(GameState, 1);
			}
			if (!The.Game.HasQuest(Quest))
			{
				if (RequireQuest)
				{
					return true;
				}
				The.Game.StartQuest(Quest);
			}
			The.Game.FinishQuestStep(Quest, Step);
		}
		return base.FireEvent(E);
	}
}
