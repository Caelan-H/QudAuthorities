using System;
using HistoryKit;
using XRL.Names;

namespace XRL.Annals;

[Serializable]
public class Regionalize : HistoricEvent
{
	public override void Generate()
	{
		setEntityProperty("name", "regionalizationParameters");
		setEntityProperty("government", ExpandString("<spice.history.regions.government.types.!random>"));
		setEntityProperty("topology", ExpandString("<spice.history.regions.topology.types.!random>"));
		setEntityProperty("organizingPrinciple", ExpandString("<spice.history.regions.organizingPrinciple.types.!random>"));
		setEntityProperty("successorChance", Random(0, 100).ToString());
		setEntityProperty("siteName1", NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, "Site"));
		setEntityProperty("siteName2", NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, "Site"));
		setEntityProperty("siteName3", NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, "Site"));
		setEntityProperty("siteTopologyTheChance", ((Random(0, 1) == 0) ? 20 : 80).ToString());
		string text = ExpandString("<spice.siteModifiers1.!random>");
		setEntityProperty("siteTopology1", text);
		string text2;
		do
		{
			text2 = ExpandString("<spice.siteModifiers2.!random>");
		}
		while (text2 == text);
		setEntityProperty("siteTopology2", text2);
		duration = 0L;
	}
}
