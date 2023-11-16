using System;
using HistoryKit;
using XRL.Language;

namespace XRL.Annals;

[Serializable]
public class InitializeResheph : HistoricEvent
{
	public int period;

	public InitializeResheph(int _period)
	{
		period = _period;
	}

	public override void Generate()
	{
		setEntityProperty("type", "sultan");
		setEntityProperty("Resheph", "true");
		setEntityProperty("birthYear", year.ToString());
		addEntityListItem("cognomen", "the Above");
		setEntityProperty("cultName", "Cult of the Coiled Lamb");
		string value = "Resheph";
		setEntityProperty("nameRoot", value);
		setEntityProperty("suffix", "0");
		setEntityProperty("name", value);
		setEntityProperty("period", period.ToString());
		setEntityProperty("isAlive", "true");
		string text = "he";
		setEntityProperty("subjectPronoun", text);
		setEntityProperty("possessivePronoun", Grammar.PossessivePronoun(text));
		setEntityProperty("objectPronoun", Grammar.ObjectPronoun(text));
		setEntityProperty("region", "null");
		duration = 0L;
	}
}
