using System;
using XRL.Core;

namespace XRL.World;

[Serializable]
public class QuestManager : IPart
{
	public string MyQuestID;

	public ZoneManager zoneManager => XRLCore.Core.Game.ZoneManager;

	public virtual void OnQuestAdded()
	{
	}

	public virtual void OnStepComplete(string StepName)
	{
	}

	public virtual void OnQuestComplete()
	{
	}

	public virtual GameObject GetQuestInfluencer()
	{
		return null;
	}
}
