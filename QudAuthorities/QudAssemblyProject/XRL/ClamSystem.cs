using System;
using System.Collections.Generic;
using Qud.API;
using XRL.World;

namespace XRL;

[Serializable]
public class ClamSystem : IGameSystem
{
	public string ClamWorldId = "Tzimtzlum.40.12.1.1.10";

	public List<string> clamJoppaZone = new List<string>();

	public Zone GetClamZone()
	{
		return The.ZoneManager.GetZone(ClamWorldId);
	}

	public Zone GetJoppaZone(int ClamID)
	{
		if (ClamID >= 0 && ClamID < clamJoppaZone.Count)
		{
			return The.ZoneManager.GetZone(clamJoppaZone[ClamID]);
		}
		return GetJoppaZone();
	}

	public Zone GetJoppaZone(string Exclude = null)
	{
		string text = Exclude;
		for (int i = 0; i < 100; i++)
		{
			if (!(text == Exclude))
			{
				break;
			}
			text = clamJoppaZone.GetRandomElement();
		}
		return The.ZoneManager.GetZone(text);
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "GaveDirections" && The.PlayerCell?.ParentZone?.ZoneWorld == "Tzimtzlum")
		{
			JournalAPI.RevealObservation("$Tzimtzlum", onlyIfNotRevealed: true);
		}
		base.FireEvent(E);
	}
}
