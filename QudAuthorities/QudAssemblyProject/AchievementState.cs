using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class AchievementState : IEnumerable<AchievementInfo>, IEnumerable
{
	[JsonProperty("Achievements")]
	public Dictionary<string, AchievementInfo> Achievements = new Dictionary<string, AchievementInfo>();

	[JsonProperty("Stats")]
	public Dictionary<string, StatInfo> Stats = new Dictionary<string, StatInfo>();

	private StatInfo CreateStat(string ID, int MaxValue = 1)
	{
		if (!Stats.TryGetValue(ID, out var value))
		{
			value = (Stats[ID] = new StatInfo(ID, MaxValue));
		}
		else
		{
			value.MaxValue = MaxValue;
		}
		return value;
	}

	public void Add(string ID, string DisplayName)
	{
		Achievements.Add(ID, new AchievementInfo(DisplayName, ID));
	}

	public void Add(string ID, string DisplayName, string StatID, int AchievedAt)
	{
		AchievementInfo achievementInfo = new AchievementInfo(DisplayName, ID, null, null, AchievedAt);
		achievementInfo.Progress = CreateStat(StatID, AchievedAt);
		Achievements.Add(ID, achievementInfo);
	}

	public void Add(string ID, string DisplayName, params object[] Stats)
	{
		AchievementInfo achievementInfo = new AchievementInfo(DisplayName, ID, null, null, (int)Stats[1]);
		StatInfo statInfo = (achievementInfo.Progress = CreateStat((string)Stats[0], achievementInfo.AchievedAt));
		List<StatInfo> list = (achievementInfo.Stats = new List<StatInfo>());
		for (int i = 2; i < Stats.Length; i++)
		{
			if (Stats[i] is string iD)
			{
				list.Add(statInfo = CreateStat(iD));
			}
			else if (Stats[i] is int maxValue)
			{
				statInfo.MaxValue = maxValue;
			}
		}
		list.TrimExcess();
		Achievements.Add(ID, achievementInfo);
	}

	[Obsolete]
	public IEnumerator<AchievementInfo> GetEnumerator()
	{
		throw new NotImplementedException();
	}

	[Obsolete]
	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException();
	}
}
