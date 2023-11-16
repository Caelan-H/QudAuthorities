using System.Collections.Generic;
using HistoryKit;
using XRL.Language;
using XRL.World.Encounters;

namespace XRL.World.Parts;

public class GenerateFriendOrFoe_HEB
{
	private static List<string> _hateReasons;

	private static List<string> _likeReasons;

	static GenerateFriendOrFoe_HEB()
	{
		_hateReasons = new List<string>(30);
		_likeReasons = new List<string>(19);
		_hateReasons.Add("inventing the irrational numbers");
		_hateReasons.Add("destroying the $adjective numbers");
		_hateReasons.Add("dreaming $dimension into being");
		_hateReasons.Add("inventing the concept of $nouns");
		_hateReasons.Add("swapping how $objnouns and $objnoun2s are perceived");
		_hateReasons.Add("warping a pocket of spacetime into a $weirdobj");
		_likeReasons.Add("inventing the irrational numbers");
		_likeReasons.Add("destroying the $adjective numbers");
		_likeReasons.Add("dreaming $dimension into being");
		_likeReasons.Add("inventing the concept of $nouns");
		_likeReasons.Add("swapping how $objnouns and $objnoun2s are perceived");
		_likeReasons.Add("warping a pocket of spacetime into a $weirdobj");
	}

	public static string getHateReason()
	{
		string text = HistoricStringExpander.ExpandString(_hateReasons.GetRandomElement(), null, null, new Dictionary<string, string>
		{
			{
				"$nouns",
				Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.nouns.!random>"))
			},
			{
				"$noun2s",
				Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.nouns.!random>"))
			},
			{
				"$objnouns",
				Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.objectNouns.!random>"))
			},
			{
				"$objnoun2s",
				Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.objectNouns.!random>"))
			},
			{
				"$weirdobj",
				HistoricStringExpander.ExpandString("<spice.history.gospels.EarlySultanate.location.!random>")
			},
			{
				"$adjective",
				HistoricStringExpander.ExpandString("<spice.adjectives.!random>")
			}
		});
		if (text.Contains("$dimension"))
		{
			PsychicManager psychicManager = The.Game.GetObjectGameState("PsychicManager") as PsychicManager;
			if (psychicManager == null || psychicManager.ExtraDimensions == null || psychicManager.ExtraDimensions.Count == 0)
			{
				ExtraDimension randomElement = psychicManager.ExtraDimensions.GetRandomElement();
				string newValue = randomElement.Name.Replace("*dimensionSymbol*", ((char)randomElement.Symbol).ToString());
				text = text.Replace("$dimension", newValue);
			}
			else
			{
				text = text.Replace("$dimension", "an uncharted dimension");
			}
		}
		return text;
	}

	public static string getLikeReason()
	{
		string text = HistoricStringExpander.ExpandString(_likeReasons.GetRandomElement(), null, null, new Dictionary<string, string>
		{
			{
				"$nouns",
				Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.nouns.!random>"))
			},
			{
				"$noun2s",
				Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.nouns.!random>"))
			},
			{
				"$objnouns",
				Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.objectNouns.!random>"))
			},
			{
				"$objnoun2s",
				Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.objectNouns.!random>"))
			},
			{
				"$weirdobj",
				HistoricStringExpander.ExpandString("<spice.history.gospels.EarlySultanate.location.!random>")
			},
			{
				"$adjective",
				HistoricStringExpander.ExpandString("<spice.adjectives.!random>")
			}
		});
		if (text.Contains("$dimension"))
		{
			ExtraDimension randomElement = (The.Game.GetObjectGameState("PsychicManager") as PsychicManager).ExtraDimensions.GetRandomElement();
			string newValue = randomElement.Name.Replace("*dimensionSymbol*", ((char)randomElement.Symbol).ToString());
			text = text.Replace("$dimension", newValue);
		}
		return text;
	}

	public static string getRandomFaction(GameObject parent)
	{
		List<string> list = new List<string>(Factions.getFactionCount());
		foreach (Faction item in Factions.loop())
		{
			if (item.Visible && !parent.pBrain.FactionMembership.ContainsKey(item.Name))
			{
				list.Add(item.Name);
			}
		}
		return list.GetRandomElement();
	}
}
