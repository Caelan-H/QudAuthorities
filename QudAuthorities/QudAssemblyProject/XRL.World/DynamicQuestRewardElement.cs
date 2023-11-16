using System;

namespace XRL.World;

[Serializable]
public abstract class DynamicQuestRewardElement
{
	public abstract void award();

	public abstract string getRewardConversationType();

	public abstract string getRewardAcceptQuestText();
}
