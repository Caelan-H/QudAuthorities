using System;
using HistoryKit;
using XRL.Core;

namespace XRL.Annals;

[Serializable]
public class LocationConstructed : HistoricEvent
{
	public override void Generate()
	{
		string[] array = new string[4] { "Tomb", "Palace", "Library", "Abbey" };
		setEntityProperty("name", XRLCore.Core.GenerateRandomPlayerName() + "'s " + array[Random(0, 3)]);
		addEntityListItem("spice", "glass");
		duration = Random(1, 120);
		setEventProperty("gospel", "construction begain in " + year + " and ended in " + (year + duration));
	}
}
