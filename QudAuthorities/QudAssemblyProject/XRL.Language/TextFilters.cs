using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HistoryKit;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Encounters;

namespace XRL.Language;

public static class TextFilters
{
	private static readonly string[] CORVID_WORDS = new string[5] { "{{emote|*CAAW*}}", "{{emote|*CAAAAW*}}", "{{emote|*CAAAAW*}}", "{{emote|*CAAW*}}", "{{emote|*CAAAAAAAAAAW*}}" };

	private static readonly string[] WATERBIRD_WORDS = new string[6] { "{{emote|*HONK*}}", "{{emote|*HONK*}}", "{{emote|*HONK*}}", "{{emote|*HOONK*}}", "{{emote|*HOOOONK*}}", "{{emote|*HOOOOOOOOOONK*}}" };

	private static readonly string[] FISH_WORDS = new string[6] { "{{emote|*blub*}}", "{{emote|*blub blub*}}", "{{emote|*blub blub*}}", "{{emote|*blub*}}", "{{emote|*blub*}}", "{{emote|*blub*}}" };

	private static readonly string[] FROG_WORDS = new string[13]
	{
		"{{emote|*crk*}}", "{{emote|*crrk*}}", "{{emote|*crrk*}}", "{{emote|*crrrrrk*}}", "{{emote|*crrrrrrrrrrrk*}}", "{{emote|*CRRK*}}", "{{emote|*reep*}}", "{{emote|*reep*}}", "{{emote|*reep*}}", "{{emote|*reeeeep*}}",
		"{{emote|*reeeeeeeeeeep*}}", "{{emote|*bep*}}", "{{emote|*rff*}}"
	};

	private static readonly char[] CRYPTIC_MACHINE_CHARS = new char[40]
	{
		'³', '\u00b4', 'µ', '¶', '·', '\u00b8', '¹', 'º', '»', '¼',
		'½', '¾', '¿', 'À', 'Á', 'Â', 'Ã', 'Ä', 'Å', 'Æ',
		'Ç', 'È', 'É', 'Ê', 'Ë', 'Ì', 'Í', 'Î', 'Ï', 'Ð',
		'Ñ', 'Ò', 'Ó', 'Ô', 'Õ', 'Ö', '×', 'Ø', 'Ù', 'Ú'
	};

	private static readonly int CRYPTIC_WORD_LENGTH_LOWER_BOUND = 3;

	private static readonly int CRYPTIC_WORD_LENGTH_UPPER_BOUND = 10;

	private static readonly int CRYPTIC_SENTENCE_LENGTH_LOWER_BOUND = 3;

	private static readonly int CRYPTIC_SENTENCE_LENGTH_UPPER_BOUND = 40;

	public static string Filter(string phrase, string filter, string extras = null, bool FormattingProtect = true)
	{
		return filter switch
		{
			"Angry" => Angry(phrase), 
			"Corvid" => Corvid(phrase), 
			"WaterBird" => WaterBird(phrase), 
			"Fish" => Fish(phrase), 
			"Frog" => Frog(phrase), 
			"Leet" => Leet(phrase, FormattingProtect), 
			"Lallated" => Lallated(phrase, extras), 
			"Weird" => Weird(phrase, extras), 
			"Cryptic Machine" => CrypticMachine(phrase), 
			_ => phrase, 
		};
	}

	public static string Corvid(string text)
	{
		string[] array = text.Split(' ');
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (Stat.Random(0, 5) == 0)
			{
				list.Add(text2 + " " + CORVID_WORDS.GetRandomElement() + " ");
			}
			else
			{
				list.Add(text2);
			}
		}
		return string.Join(" ", list.ToArray());
	}

	public static string WaterBird(string text)
	{
		string[] array = text.Split(' ');
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (Stat.Random(0, 5) == 0)
			{
				list.Add(text2 + " " + WATERBIRD_WORDS.GetRandomElement() + " ");
			}
			else
			{
				list.Add(text2);
			}
		}
		return string.Join(" ", list.ToArray());
	}

	public static string Fish(string text)
	{
		string[] array = text.Split(' ');
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (Stat.Random(0, 5) == 0)
			{
				list.Add(text2 + " " + FISH_WORDS.GetRandomElement() + " ");
			}
			else
			{
				list.Add(text2);
			}
		}
		return string.Join(" ", list.ToArray());
	}

	public static string Frog(string text)
	{
		string[] array = text.Split(' ');
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (Stat.Random(0, 10) == 0)
			{
				list.Add(text2 + " " + FROG_WORDS.GetRandomElement() + " ");
			}
			else
			{
				list.Add(text2);
			}
		}
		return string.Join(" ", list.ToArray());
	}

	public static string Angry(string phrase)
	{
		string[] array = phrase.Split(new string[1] { ". " }, StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			if (50.in100())
			{
				array[i] = Grammar.Stutterize(array[i], HistoricStringExpander.ExpandString("<spice.textFilters.angry.!random>"));
			}
		}
		return string.Join(". ", array);
	}

	public static string Lallated(string Text, string Noise)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("*Text*", Text);
		dictionary.Add("*Noise*", Noise.Split(',').GetRandomElement());
		Dictionary<string, string> vars = dictionary;
		return HistoricStringExpander.ExpandString("<spice.textFilters.lallated.!random>", null, null, vars);
	}

	public static string Leet(string Text, bool FormattingProtect = true)
	{
		if (Text == null)
		{
			return null;
		}
		Text = Regex.Replace(Text, "anned\\b", FormattingProtect ? "&&" : "&", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "and\\b", FormattingProtect ? "&&" : "&", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "ude\\b", "00|)", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "too\\b", "2", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bto\\b", "2", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bone\\b", "1", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bwon\\b", "1", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\btwo\\b", "1", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bthree\\b", "3", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bfou?r\\b", "4", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bfive\\b", "5", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bsix\\b", "6", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bseven\\b", "7", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\beight\\b", "8", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "ate\\b", "8", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bare\\b", "R", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "nine\\b", "9", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\byou\\b", "U", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "at\\b", "@", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "S\\b", "Z");
		Text = Regex.Replace(Text, "s\\b", "z");
		Text = Regex.Replace(Text, "a(?=\\w)", "4", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "(?<=\\w)a", "4", RegexOptions.IgnoreCase);
		Text = Text.Replace("EW", "00").Replace("ew", "00").Replace("B", "8")
			.Replace("b", "8")
			.Replace("C", "(")
			.Replace("c", "(")
			.Replace("D", "|)")
			.Replace("d", "|)")
			.Replace("E", "3")
			.Replace("e", "3")
			.Replace("H", "#")
			.Replace("h", "#")
			.Replace("I", "1")
			.Replace("i", "1")
			.Replace("O", "0")
			.Replace("o", "0")
			.Replace("S", "5")
			.Replace("s", "5")
			.Replace("T", "7")
			.Replace("t", "7")
			.Replace("V", "\\/")
			.Replace("v", "\\/")
			.Replace("W", "\\/\\/")
			.Replace("w", "\\/\\/");
		return Text;
	}

	public static string Weird(string Text, string PatternSpec = null)
	{
		int index = 0;
		if (!string.IsNullOrEmpty(PatternSpec))
		{
			if (XRLCore.Core.Game.GetObjectGameState("PsychicManager") is PsychicManager psychicManager)
			{
				foreach (PsychicFaction psychicFaction in psychicManager.PsychicFactions)
				{
					if (psychicFaction.factionName == PatternSpec)
					{
						return psychicFaction.Weirdify(Text);
					}
				}
				foreach (ExtraDimension extraDimension in psychicManager.ExtraDimensions)
				{
					if (extraDimension.Name == PatternSpec)
					{
						return extraDimension.Weirdify(Text);
					}
				}
			}
			try
			{
				index = Convert.ToInt32(PatternSpec);
			}
			catch
			{
			}
		}
		if (Text.Contains("a"))
		{
			Text = Text.Replace("a", Grammar.weirdLowerAs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("A"))
		{
			Text = Text.Replace("A", Grammar.weirdUpperAs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("e"))
		{
			Text = Text.Replace("e", Grammar.weirdLowerEs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("E"))
		{
			Text = Text.Replace("E", Grammar.weirdUpperEs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("i"))
		{
			Text = Text.Replace("i", Grammar.weirdLowerIs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("I"))
		{
			Text = Text.Replace("I", Grammar.weirdUpperIs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("o"))
		{
			Text = Text.Replace("o", Grammar.weirdLowerOs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("O"))
		{
			Text = Text.Replace("O", Grammar.weirdUpperOs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("u"))
		{
			Text = Text.Replace("u", Grammar.weirdLowerUs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("U"))
		{
			Text = Text.Replace("U", Grammar.weirdUpperUs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("c"))
		{
			Text = Text.Replace("c", Grammar.weirdLowerCs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("f"))
		{
			Text = Text.Replace("f", Grammar.weirdLowerFs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("n"))
		{
			Text = Text.Replace("n", Grammar.weirdLowerNs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("t"))
		{
			Text = Text.Replace("t", Grammar.weirdLowerTs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("y"))
		{
			Text = Text.Replace("y", Grammar.weirdLowerYs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("B"))
		{
			Text = Text.Replace("B", Grammar.weirdUpperBs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("C"))
		{
			Text = Text.Replace("C", Grammar.weirdUpperCs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("Y"))
		{
			Text = Text.Replace("Y", Grammar.weirdUpperYs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("L"))
		{
			Text = Text.Replace("L", Grammar.weirdUpperLs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("R"))
		{
			Text = Text.Replace("R", Grammar.weirdUpperRs.GetCyclicElement(index).ToString());
		}
		if (Text.Contains("N"))
		{
			Text = Text.Replace("N", Grammar.weirdUpperNs.GetCyclicElement(index).ToString());
		}
		return Text;
	}

	public static string GenerateCrypticWord()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		for (int i = 0; i < Stat.Random(CRYPTIC_WORD_LENGTH_LOWER_BOUND, CRYPTIC_WORD_LENGTH_UPPER_BOUND); i++)
		{
			stringBuilder.Append(CRYPTIC_MACHINE_CHARS.GetRandomElement());
		}
		return stringBuilder.ToString();
	}

	public static string CrypticMachine(string Text)
	{
		if (Text.Contains("*READOUT*"))
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("{{c|");
			for (int i = 0; i < Stat.Random(CRYPTIC_SENTENCE_LENGTH_LOWER_BOUND, CRYPTIC_SENTENCE_LENGTH_UPPER_BOUND); i++)
			{
				stringBuilder.Append(GenerateCrypticWord());
				stringBuilder.Append(' ');
			}
			return stringBuilder.ToString().TrimEnd(' ') + "}}";
		}
		string[] array = Text.Split(' ');
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (Stat.Random(0, 10) == 0)
			{
				list.Add(text + " {{c|" + GenerateCrypticWord() + "}} ");
			}
			else
			{
				list.Add(text);
			}
		}
		return string.Join(" ", list.ToArray());
	}
}
