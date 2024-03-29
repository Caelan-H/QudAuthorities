using System;
using HistoryKit;
using XRL.Language;
using XRL.Names;

namespace XRL.Annals;

[Serializable]
public class InitializeRegion : HistoricEvent
{
	public HistoricEntitySnapshot SnapRegionParams;

	public string GovernmentType;

	public string OrganizingPrincipleType;

	public string Name;

	public const int normalNameChance = 25;

	public const int normalNameChance_GLOBAL = 15;

	public int period;

	public InitializeRegion(int _period)
	{
		period = _period;
	}

	public override void Generate()
	{
		setEntityProperty("type", "region");
		setEntityProperty("period", period.ToString());
		SnapRegionParams = QudHistoryHelpers.GetRegionalizationParametersSnapshot(history);
		OrganizingPrincipleType = ExpandString("<spice.history.regions.organizingPrinciple." + SnapRegionParams.GetProperty("organizingPrinciple") + ".types.!random>");
		GovernmentType = ExpandString("<spice.history.regions.government." + SnapRegionParams.GetProperty("government") + ".name.!random>");
		setEntityProperty("organizingPrincipleType", OrganizingPrincipleType);
		setEntityProperty("governmentType", GovernmentType);
		addEntityListItem("parameters", OrganizingPrincipleType);
		addEntityListItem("parameters", GovernmentType);
		NameRegion();
		for (int i = 1; i <= 6 - period; i++)
		{
			int num = ((i != 1) ? Random(1, 2) : 2);
			for (int j = 1; j <= num; j++)
			{
				HistoricEntity newEntity = history.GetNewEntity(history.currentYear);
				newEntity.ApplyEvent(new InitializeLocation(Name, period));
				addEntityListItem("locations", newEntity.GetSnapshotAtYear(history.currentYear).GetProperty("name"));
			}
		}
		duration = 0L;
	}

	public void NameRegion()
	{
		string text;
		do
		{
			text = NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, "Site");
		}
		while (history.GetEntitiesWherePropertyEquals("name", text).Count > 0);
		setEntityProperty("nameRoot", text);
		string text2 = "";
		if (SnapRegionParams.GetProperty("organizingPrinciple").Equals("religion"))
		{
			text2 = (OrganizingPrincipleType.Equals("sultans") ? "sultan" : (OrganizingPrincipleType.Equals("godless") ? "nothing" : ExpandString("<spice.elements." + OrganizingPrincipleType + ".nouns.!random>")));
			setEntityProperty("orgPrincipleStash", Grammar.Pluralize(text2));
		}
		if (15.in100())
		{
			Name = text;
			setEntityProperty("name", Name);
			setEntityProperty("newName", Name);
		}
		else if (SnapRegionParams.GetProperty("organizingPrinciple").Equals("function"))
		{
			if (25.in100())
			{
				Name = text;
				setEntityProperty("name", Name);
				setEntityProperty("newName", Name);
				return;
			}
			string text3 = ExpandString("<spice.history.regions.organizingPrinciple." + SnapRegionParams.GetProperty("organizingPrinciple") + "." + OrganizingPrincipleType + ".titles.!random>");
			string text4 = ExpandString("<spice.history.regions.government." + SnapRegionParams.GetProperty("government") + ".article>");
			string text5 = ExpandString("<spice.history.regions.government." + SnapRegionParams.GetProperty("government") + ".of>");
			Name = Grammar.MakeTitleCaseWithArticle(text4 + text3 + " " + GovernmentType + " " + text5 + text);
			setEntityProperty("name", Name);
			int num = Random(1, 100);
			if (num <= 25)
			{
				setEntityProperty("newName", text);
			}
			else if (num <= 65)
			{
				setEntityProperty("newName", Grammar.MakeTitleCaseWithArticle("the " + Grammar.Pluralize(text3) + " of " + text));
			}
			else
			{
				setEntityProperty("newName", Grammar.MakeTitleCaseWithArticle(text + " " + text3));
			}
		}
		else if (SnapRegionParams.GetProperty("organizingPrinciple").Equals("wealth"))
		{
			if (25.in100())
			{
				Name = text;
				setEntityProperty("name", Name);
				setEntityProperty("newName", Name);
				return;
			}
			string text6 = ExpandString("<spice.history.regions.organizingPrinciple." + SnapRegionParams.GetProperty("organizingPrinciple") + "." + OrganizingPrincipleType + ".titles.!random>");
			Name = Grammar.MakeTitleCaseWithArticle(ExpandString("<spice.history.regions.organizingPrinciple.wealth." + OrganizingPrincipleType + ".article> ") + text + " " + text6).TrimStart(' ');
			setEntityProperty("name", Name);
			if (50.in100())
			{
				setEntityProperty("newName", text);
			}
			else
			{
				setEntityProperty("newName", Name);
			}
		}
		else if (SnapRegionParams.GetProperty("organizingPrinciple").Equals("profession"))
		{
			if (25.in100())
			{
				Name = text;
				setEntityProperty("name", Name);
				setEntityProperty("newName", Name);
				return;
			}
			string text7 = ExpandString("<spice.professions." + OrganizingPrincipleType + ".plural>");
			Name = Grammar.MakeTitleCaseWithArticle("the " + Grammar.MakePossessive(text7) + " " + GovernmentType + " of " + text);
			setEntityProperty("name", Name);
			int num2 = Random(1, 100);
			if (num2 <= 30)
			{
				setEntityProperty("newName", text);
			}
			else if (num2 <= 70)
			{
				setEntityProperty("newName", Grammar.MakeTitleCaseWithArticle(ExpandString(text + ", <spice.professions." + OrganizingPrincipleType + ".singular><spice.commonPhrases.shortHearth.!random>")));
			}
			else
			{
				setEntityProperty("newName", Grammar.MakeTitleCaseWithArticle(ExpandString(text + ", <spice.commonPhrases.bygone.!random> <spice.commonPhrases.hearth.!random> of " + text7)));
			}
		}
		else
		{
			if (!SnapRegionParams.GetProperty("organizingPrinciple").Equals("religion"))
			{
				return;
			}
			if (25.in100())
			{
				Name = text;
				setEntityProperty("name", Name);
				setEntityProperty("newName", Name);
				return;
			}
			if (OrganizingPrincipleType.Equals("godless"))
			{
				Name = "the godless " + Grammar.MakeTitleCaseWithArticle(GovernmentType + " of " + text);
				setEntityProperty("newName", text);
			}
			else
			{
				Name = "the " + Grammar.GetRandomMeaningfulWord(text2) + "-worshipping " + Grammar.MakeTitleCaseWithArticle(GovernmentType + " of " + text);
				if (25.in100())
				{
					setEntityProperty("newName", text);
				}
				else
				{
					setEntityProperty("newName", Grammar.MakeTitleCaseWithArticle("the " + text + " " + Grammar.GetRandomMeaningfulWord(text2) + "<spice.commonPhrases.yard.!random>"));
				}
			}
			setEntityProperty("name", Name);
		}
	}

	public void NameRegion_Old()
	{
		string text = NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, "Site");
		if (25.in100())
		{
			switch (Random(0, 14))
			{
			case 0:
				text += " Sector";
				break;
			case 1:
				text = "the " + text + " Perimeter";
				break;
			case 2:
				text = "the " + text + " Periphery";
				break;
			case 3:
				text = "the " + text + " Anterior";
				break;
			case 4:
				text = "the " + text + " Posterier";
				break;
			case 5:
				text = "Inner " + text;
				break;
			case 6:
				text = "Outer " + text;
				break;
			case 7:
				text += " Province";
				break;
			case 8:
				text += " District";
				break;
			case 9:
				text += " Quarter";
				break;
			case 10:
				text += " Precinct";
				break;
			case 11:
				text += " Strata";
				break;
			case 12:
				text = "the " + text + " Torus";
				break;
			case 13:
				text = "the " + text + " Sphere";
				break;
			case 14:
				text = "the " + text + " Ellipsoid";
				break;
			}
		}
		if (Random(0, 100) < 10)
		{
			switch (Random(1, 10))
			{
			case 1:
				text += " I";
				break;
			case 2:
				text += " II";
				break;
			case 3:
				text += " III";
				break;
			case 4:
				text += " IV";
				break;
			case 5:
				text += " V";
				break;
			case 6:
				text += " VI";
				break;
			case 7:
				text += " VII";
				break;
			case 8:
				text += " VIII";
				break;
			case 9:
				text += " IX";
				break;
			case 10:
				text += " X";
				break;
			}
		}
		setEntityProperty("name", text);
	}
}
