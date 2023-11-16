using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class StatInfo
{
	public string GameCenterID;

	public string iOSID;

	public string SteamID;

	[JsonProperty]
	public int Value;

	private int _MaxValue;

	public int MaxValue
	{
		get
		{
			return _MaxValue;
		}
		set
		{
			if (value > _MaxValue)
			{
				_MaxValue = value;
			}
		}
	}

	public StatInfo()
	{
	}

	public StatInfo(string SteamID, int MaxValue = 0, string GameCenterID = null, string iOSID = null)
	{
		this.GameCenterID = GameCenterID;
		this.iOSID = iOSID;
		this.SteamID = SteamID;
		this.MaxValue = MaxValue;
	}
}
