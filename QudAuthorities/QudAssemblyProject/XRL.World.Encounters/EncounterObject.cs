using System.Collections.Generic;

namespace XRL.World.Encounters;

public class EncounterObject : EncounterObjectBase
{
	public string Number;

	public string _Blueprint;

	public string Builder;

	public string[] Builders;

	public bool Aquatic;

	public bool LivesOnWalls;

	public string Blueprint
	{
		get
		{
			if (!string.IsNullOrEmpty(_Blueprint) && _Blueprint[0] == '@')
			{
				string text = _Blueprint;
				if (!string.IsNullOrEmpty(text) && _Blueprint.Contains("{zonetier}"))
				{
					text = text.Replace("{zonetier}", ZoneManager.zoneGenerationContextTier.ToString());
				}
				return PopulationManager.RollOneFrom(text.Substring(1), new Dictionary<string, string> { 
				{
					"zonetier",
					ZoneManager.zoneGenerationContextTier.ToString()
				} }, _Blueprint).Blueprint;
			}
			return _Blueprint;
		}
		set
		{
			_Blueprint = value;
		}
	}

	public override string ToString()
	{
		string text = "Blueprint:" + (_Blueprint ?? "NULL") + "/Number:" + Number;
		if (!string.IsNullOrEmpty(Builder))
		{
			text = text + "/Builder:" + Builder;
		}
		if (Builders != null && Builders.Length != 0)
		{
			text = text + "/Builders:" + string.Join(",", Builders);
		}
		if (Aquatic)
		{
			text += "/Aquatic";
		}
		if (LivesOnWalls)
		{
			text += "/LivesOnWalls";
		}
		return text;
	}
}
