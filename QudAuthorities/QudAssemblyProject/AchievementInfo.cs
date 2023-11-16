using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class AchievementInfo
{
	public string GameCenterID;

	public string iOSID;

	public string SteamID;

	public string DisplayName;

	[JsonProperty]
	public bool Achieved;

	public int AchievedAt;

	public StatInfo Progress;

	public List<StatInfo> Stats;

	public AchievementInfo()
	{
	}

	public AchievementInfo(string DisplayName, string SteamID, string GameCenterID = null, string iOSID = null, int AchievedAt = 0)
	{
		this.GameCenterID = GameCenterID;
		this.iOSID = iOSID;
		this.SteamID = SteamID;
		this.DisplayName = DisplayName;
		this.AchievedAt = AchievedAt;
	}
}
