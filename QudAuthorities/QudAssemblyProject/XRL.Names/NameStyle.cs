using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Names;

public class NameStyle
{
	public string Name;

	public int HyphenationChance;

	public int TwoNameChance;

	public string Base;

	public string Format = "TitleCase";

	public List<NamePrefix> Prefixes = new List<NamePrefix>();

	public string PrefixAmount = "0";

	public List<NameInfix> Infixes = new List<NameInfix>();

	public string InfixAmount = "0";

	public List<NamePostfix> Postfixes = new List<NamePostfix>();

	public string PostfixAmount = "0";

	public List<NameScope> Scopes = new List<NameScope>();

	public List<NameTemplate> TitleTemplates = new List<NameTemplate>();

	public Dictionary<string, List<NameValue>> TemplateVars = new Dictionary<string, List<NameValue>>();

	private static StringBuilder SB = new StringBuilder();

	private static int NameGenerationBadWordsFailures;

	public string Generate(GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, Dictionary<string, string> TitleContext = null, bool FailureOkay = false, bool SpecialFaildown = false, NameStyle Skip = null, List<NameStyle> SkipList = null)
	{
		int num = 0;
		string text;
		while (true)
		{
			if (!string.IsNullOrEmpty(Base))
			{
				if (SkipList != null)
				{
					SkipList = new List<NameStyle>(SkipList);
					SkipList.Add(this);
				}
				else if (Skip != null)
				{
					SkipList = new List<NameStyle> { Skip, this };
					Skip = null;
				}
				else
				{
					Skip = this;
				}
				if (Base == "*")
				{
					text = NameStyles.Generate(For, Genotype, Subtype, Species, Culture, Faction, Gender, Mutations, Tag, null, TitleContext, FailureOkay, SpecialFaildown: false, Skip, SkipList, ForProcessed: true);
				}
				else
				{
					if (!NameStyles.NameStyleTable.TryGetValue(Base, out var value))
					{
						return "InvalidBase:" + Base;
					}
					text = value.Generate(For, Genotype, Subtype, Species, Culture, Faction, Gender, Mutations, Tag, Special, TitleContext, FailureOkay, SpecialFaildown, Skip, SkipList);
				}
				if (text == null)
				{
					return null;
				}
			}
			else
			{
				SB.Clear();
				int num2 = ((!TwoNameChance.in100()) ? 1 : 2);
				for (int i = 0; i < num2; i++)
				{
					int num3 = PrefixAmount.RollCached();
					int num4 = InfixAmount.RollCached();
					int num5 = PostfixAmount.RollCached();
					for (int j = 0; j < num3; j++)
					{
						SB.Append(Prefixes.GetRandomNameElement());
						if (HyphenationChance.in100() && (num4 > 0 || num5 > 0 || j < num3 - 1))
						{
							SB.Append('-');
						}
					}
					for (int k = 0; k < num4; k++)
					{
						SB.Append(Infixes.GetRandomNameElement());
						if (HyphenationChance.in100() && (num5 > 0 || k < num4 - 1))
						{
							SB.Append('-');
						}
					}
					for (int l = 0; l < num5; l++)
					{
						SB.Append(Postfixes.GetRandomNameElement());
						if (HyphenationChance.in100() && l < num5 - 1)
						{
							SB.Append('-');
						}
					}
					if (i < num2 - 1)
					{
						SB.Append(' ');
					}
				}
				text = SB.ToString().Trim();
			}
			string randomNameElement = TitleTemplates.GetRandomNameElement();
			if (!string.IsNullOrEmpty(randomNameElement))
			{
				string text2 = randomNameElement;
				if (text2.Contains(";"))
				{
					text2 = text2.Split(';').GetRandomElement();
				}
				Dictionary<string, string> dictionary = new Dictionary<string, string>(8);
				if (TitleContext != null)
				{
					foreach (KeyValuePair<string, string> item in TitleContext)
					{
						dictionary[item.Key] = item.Value.Split(',').GetRandomElement();
					}
				}
				if (!dictionary.ContainsKey("*Name*"))
				{
					dictionary["*Name*"] = text;
				}
				if (text2.Contains("*AltName*") && !dictionary.ContainsKey("*AltName*"))
				{
					dictionary["*AltName*"] = NameStyles.Generate(For, Genotype, Subtype, Species, Culture, Faction, Gender, Mutations, Tag, null, TitleContext, FailureOkay: false, SpecialFaildown: false, Skip, SkipList, ForProcessed: true);
				}
				if (For != null)
				{
					if (text2.Contains("*CreatureType*") && !dictionary.ContainsKey("*CreatureType*"))
					{
						if (For.HasTagOrProperty("UseFullDisplayNameForCreatureType"))
						{
							dictionary["*CreatureType*"] = For.GetDisplayName(int.MaxValue, null, "CreatureType", AsIfKnown: true, Single: false, NoConfusion: true, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: true, Short: true);
						}
						else
						{
							dictionary["*CreatureType*"] = For.DisplayNameOnlyDirectAndStripped;
						}
					}
					if (text2.Contains("*CreatureTypeCap*") && !dictionary.ContainsKey("*CreatureTypeCap*"))
					{
						if (For.HasTagOrProperty("UseFullDisplayNameForCreatureType"))
						{
							dictionary["*CreatureTypeCap*"] = Grammar.MakeTitleCase(For.GetDisplayName(int.MaxValue, null, "CreatureType", AsIfKnown: true, Single: false, NoConfusion: true, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: true));
						}
						else
						{
							dictionary["*CreatureTypeCap*"] = Grammar.MakeTitleCase(For.DisplayNameOnlyDirectAndStripped);
						}
					}
					foreach (Match item2 in Regex.Matches(text2, "(\\*([^*]+)\\*)"))
					{
						if (dictionary.ContainsKey(item2.Groups[1].Value))
						{
							continue;
						}
						string text3 = For.GetPropertyOrTag("HeroNameTitle" + item2.Groups[2].Value);
						if (text3 != null)
						{
							if (text3.Contains(","))
							{
								text3 = text3.Split(',').GetRandomElement();
							}
							dictionary[item2.Groups[1].Value] = text3;
						}
					}
				}
				if (TemplateVars != null && TemplateVars.Count > 0)
				{
					foreach (KeyValuePair<string, List<NameValue>> templateVar in TemplateVars)
					{
						if (text2.Contains(templateVar.Key))
						{
							string key = "*" + templateVar.Key + "*";
							if (!dictionary.ContainsKey(key))
							{
								dictionary[key] = templateVar.Value.GetRandomNameElement();
							}
						}
					}
				}
				foreach (KeyValuePair<string, List<NameValue>> defaultTemplateVar in NameStyles.DefaultTemplateVars)
				{
					if (text2.Contains(defaultTemplateVar.Key))
					{
						string key2 = "*" + defaultTemplateVar.Key + "*";
						if (!dictionary.ContainsKey(key2))
						{
							dictionary[key2] = defaultTemplateVar.Value.GetRandomNameElement();
						}
					}
				}
				text = HistoricStringExpander.ExpandString(text2, null, null, dictionary);
				if (text.Contains("*"))
				{
					text = Regex.Replace(text, " *\\*[^*]+\\*", "");
				}
				text = text.Trim();
			}
			if (!Grammar.ContainsBadWords(text))
			{
				break;
			}
			if (++num > 1000)
			{
				return "NameGenBadWordsFail" + ++NameGenerationBadWordsFailures;
			}
		}
		return ApplyFormats(text);
	}

	public string ApplyFormats(string Text)
	{
		return ApplyFormats(Format, Text);
	}

	public static string ApplyFormats(string Format, string Text)
	{
		if (Format.Contains(","))
		{
			foreach (string item in Format.CachedCommaExpansion())
			{
				Text = ApplyFormat(item, Text);
			}
			return Text;
		}
		return ApplyFormat(Format, Text);
	}

	public static string ApplyFormat(string Format, string Text)
	{
		switch (Format)
		{
		case "TitleCase":
			Text = Grammar.MakeTitleCase(Text);
			break;
		case "AllCaps":
			Text = ColorUtility.ToUpperExceptFormatting(Text);
			break;
		case "LowerCase":
			Text = ColorUtility.ToLowerExceptFormatting(Text);
			break;
		case "Capitalized":
			Text = ColorUtility.CapitalizeExceptFormatting(Text);
			break;
		case "SpacesToHyphens":
			Text = ColorUtility.ReplaceExceptFormatting(Text, ' ', '-');
			break;
		}
		return Text;
	}

	public NameScope CheckApply(string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null)
	{
		NameScope nameScope = null;
		foreach (NameScope scope in Scopes)
		{
			if (scope.ApplyTo(Genotype, Subtype, Species, Culture, Faction, Gender, Mutations, Tag, Special) && (nameScope == null || scope.Priority > nameScope.Priority))
			{
				nameScope = scope;
			}
		}
		return nameScope;
	}
}
