using System;
using System.Collections.Generic;

namespace XRL.World;

[Serializable]
public class DynamicQuestReward
{
	public string special;

	public int StepXP;

	public List<DynamicQuestRewardElement> rewards = new List<DynamicQuestRewardElement>();

	public List<DynamicQuestRewardElement> postrewards = new List<DynamicQuestRewardElement>();

	public void award()
	{
		foreach (DynamicQuestRewardElement reward in rewards)
		{
			reward.award();
		}
	}

	public void postaward()
	{
		foreach (DynamicQuestRewardElement postreward in postrewards)
		{
			postreward.award();
		}
	}

	public string getRewardConversationType()
	{
		foreach (DynamicQuestRewardElement reward in rewards)
		{
			string rewardConversationType = reward.getRewardConversationType();
			if (!string.IsNullOrEmpty(rewardConversationType))
			{
				return rewardConversationType;
			}
		}
		return "Choice";
	}

	public string getRewardAcceptQuestText()
	{
		foreach (DynamicQuestRewardElement reward in rewards)
		{
			string rewardAcceptQuestText = reward.getRewardAcceptQuestText();
			if (!string.IsNullOrEmpty(rewardAcceptQuestText))
			{
				return rewardAcceptQuestText;
			}
		}
		return null;
	}
}
