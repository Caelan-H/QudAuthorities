using System;

namespace XRL.World.QuestManagers;

[Serializable]
public class PaxQuestStep
{
	public string Name;

	public string Text;

	public string Target;

	public bool Finished;
}
