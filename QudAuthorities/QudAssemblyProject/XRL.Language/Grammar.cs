using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using HistoryKit;
using XRL.Rules;
using XRL.World;

namespace XRL.Language;

public static class Grammar
{
	public static char[] weirdLowerAs = new char[8] { '\u0083', '\u0084', '\u0085', '\u0086', '\u00a0', '¦', 'à', 'a' };

	public static char[] weirdUpperAs = new char[4] { 'A', '\u008e', '\u008f', '\u0092' };

	public static char[] weirdLowerEs = new char[6] { 'e', '\u0082', '\u0088', '\u0089', '\u008a', 'î' };

	public static char[] weirdUpperEs = new char[3] { 'E', '\u0090', 'ä' };

	public static char[] weirdLowerIs = new char[6] { 'i', '\u008b', '\u008c', '\u008d', '¡', '­' };

	public static char[] weirdUpperIs = new char[3] { 'I', '­', '³' };

	public static char[] weirdLowerOs = new char[8] { 'o', '\u0093', '\u0094', '\u0095', '¢', 'å', 'ë', 'ø' };

	public static char[] weirdUpperOs = new char[5] { 'O', '\u0099', 'è', 'é', 'í' };

	public static char[] weirdLowerUs = new char[5] { 'u', '\u0096', '\u0097', '£', 'æ' };

	public static char[] weirdUpperUs = new char[3] { 'U', '\u009a', 'æ' };

	public static char[] weirdLowerCs = new char[3] { 'c', '\u0087', '\u009b' };

	public static char[] weirdLowerFs = new char[2] { 'f', '\u009f' };

	public static char[] weirdLowerNs = new char[4] { 'n', '¤', 'ã', 'ï' };

	public static char[] weirdLowerTs = new char[2] { 't', 'ç' };

	public static char[] weirdLowerYs = new char[2] { 'y', '\u0098' };

	public static char[] weirdUpperBs = new char[2] { 'B', 'á' };

	public static char[] weirdUpperCs = new char[2] { 'C', '\u0080' };

	public static char[] weirdUpperYs = new char[2] { 'Y', '\u009d' };

	public static char[] weirdUpperLs = new char[2] { 'L', '\u009c' };

	public static char[] weirdUpperRs = new char[2] { 'R', '\u009e' };

	public static char[] weirdUpperNs = new char[3] { 'N', '¥', 'î' };

	private static char[] obfuscators = new char[13]
	{
		'\u0005', '\u000f', '\u0016', '\u001e', '\u001f', '²', 'Û', 'Ü', 'Ý', 'Þ',
		'ß', 'ø', 'þ'
	};

	private static string[] shortPrepositions = new string[12]
	{
		"at", "by", "in", "of", "on", "to", "up", "from", "with", "into",
		"over", "out"
	};

	private static string[] prepositions = new string[11]
	{
		"at", "by", "in", "of", "on", "to", "up", "from", "with", "into",
		"out"
	};

	private static string[] articles = new string[3] { "an", "a", "the" };

	private static string[] demonstrativePronouns = new string[10] { "this", "that", "those", "these", "such", "none", "neither", "who", "whom", "whose" };

	private static string[] conjunctions = new string[6] { "and", "but", "or", "nor", "for", "as" };

	private static string[] ordinalsRoman = new string[10] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };

	private static string[] articleStartingWords = new string[20]
	{
		"a", "an", "A", "An", "the", "The", "a", "an", "A", "An",
		"the", "The", "a", "an", "A", "An", "the", "The", "Some", "some"
	};

	private static string[] badEndingWords = new string[21]
	{
		"this", "were", "every", "are", "which", "their", "has", "your", "that", "who",
		"our", "additional", "its", "he", "her", "during", "no", "she's", "he's", "than",
		"they"
	};

	private static string[] badStartingWords = new string[6] { "were", "though", "them", "him,", "her", "is" };

	private static string[] articleExceptions = new string[62]
	{
		"heir", "heiress", "heirloom", "herb", "herbal", "hour", "one", "ubiquitous", "ubiquitously", "ubiquity",
		"unary", "unicorn", "unicorns", "unicycle", "unidirectional", "unidirectionality", "unidirectionally", "unification", "unifications", "unified",
		"unifier", "unifies", "uniform", "uniformed", "uniformity", "uniformly", "uniforms", "unify", "unifying", "union",
		"unionization", "unionize", "unionized", "unionizer", "unionizers", "unionizes", "unionizing", "unions", "uniprocessor", "unique",
		"uniquely", "uniqueness", "unison", "unit", "unitary", "unities", "uniting", "unity", "univalve", "univalved",
		"univalves", "universal", "universal", "universality", "universally", "universe", "universes", "universities", "university", "usable",
		"use", "useless"
	};

	private static string[] badWords = new string[5] { "nigg", "fag", "nigr", "niggr", "kike" };

	private static string[] badWordsExact = new string[3] { "chikan", "clit", "hamas" };

	private static char[] punctuation = new char[5] { '.', '!', ',', ':', ';' };

	private static Dictionary<string, string> singularToPlural = new Dictionary<string, string>();

	private static Dictionary<string, string> pluralToSingular = new Dictionary<string, string>();

	private static Dictionary<string, string> irregularPluralization = new Dictionary<string, string>
	{
		{ "atterkop", "atterkoppen" },
		{ "attorney general", "attorneys general" },
		{ "bergrisi", "bergrisar" },
		{ "child", "children" },
		{ "childe", "childer" },
		{ "commando", "commandos" },
		{ "court martial", "courts martial" },
		{ "die", "dice" },
		{ "djinn", "djinni" },
		{ "dunadan", "dunadain" },
		{ "eldjotun", "eldjotnar" },
		{ "eldthurs", "eldthursar" },
		{ "felljotun", "felljotnar" },
		{ "fife", "fifes" },
		{ "fomor", "fomori" },
		{ "foot", "feet" },
		{ "forefoot", "forefeet" },
		{ "genus", "genera" },
		{ "goose", "geese" },
		{ "hindfoot", "hindfeet" },
		{ "hrimthurs", "hrimthursar" },
		{ "ifrit", "ifriti" },
		{ "jabberwock", "jabberwocky" },
		{ "jerky", "jerkys" },
		{ "jotun", "jotnar" },
		{ "kin", "kin" },
		{ "kindred", "kindred" },
		{ "kinsmen", "kinsmen" },
		{ "kinswomen", "kinswomen" },
		{ "knight templar", "knights templar" },
		{ "leaf", "leaves" },
		{ "loaf", "loaves" },
		{ "longstaff", "longstaves" },
		{ "mosquito", "mosquitos" },
		{ "mouse", "mice" },
		{ "notary public", "notaries public" },
		{ "octopus", "octopodes" },
		{ "opus", "opera" },
		{ "ordo", "ordines" },
		{ "ox", "oxen" },
		{ "pancreas", "pancreata" },
		{ "person", "people" },
		{ "platypus", "platypoda" },
		{ "plus", "plusses" },
		{ "quarterstaff", "quarterstaves" },
		{ "rhinox", "rhinoxen" },
		{ "risi", "risar" },
		{ "secretary general", "secretaries general" },
		{ "shaman", "shamans" },
		{ "staff", "staves" },
		{ "sturmjotun", "sturmjotnar" },
		{ "surgeon general", "surgeons general" },
		{ "talisman", "talismans" },
		{ "thief", "thieves" },
		{ "tooth", "teeth" },
		{ "topaz", "topazes" },
		{ "townsperson", "townspeople" },
		{ "moment in time chosen arbitrarily", "moments in time chosen arbitrarily" }
	};

	private static string[] identicalPluralization = new string[40]
	{
		"barracks", "bison", "buffalo", "caribou", "chitin", "chosen", "corps", "deer", "einheriar", "fish",
		"fruit", "geisha", "gi", "hellspawn", "katana", "kraken", "lamia", "kris", "means", "moose",
		"naga", "ninja", "nunchaku", "oni", "remains", "sai", "scissors", "series", "sheep", "shrimp",
		"shuriken", "spawn", "species", "sputum", "waterworks", "wakizashi", "yeti", "yoroi", "young", "pentaceps"
	};

	private static string[] latinPluralization = new string[101]
	{
		"abacus", "adytum", "alkalus", "alumnus", "alumno", "alumna", "anima", "animo", "animus", "antenna",
		"apex", "appendix", "arboretum", "astrum", "automaton", "axis", "bacterium", "ballista", "cacosteum", "cactus",
		"cestus", "cinctus", "cognomen", "corpus", "datum", "desideratum", "dictum", "dominatrix", "drosophilium", "ellipsis",
		"emerita", "emerito", "emeritus", "epona", "eques", "equus", "erratum", "esophagus", "exoculus", "exodus",
		"fascia", "focus", "forum", "fungus", "haruspex", "hippocampus", "hippopotamus", "hypha", "iambus", "illuminata",
		"illuminato", "illuminatus", "imperator", "imperatrix", "incarnus", "larva", "locus", "lorica", "maga", "mago",
		"magus", "manica", "matrix", "medium", "melia", "momentum", "neurosis", "nexus", "nomen", "nucleus",
		"patagium", "pegasus", "penis", "persona", "phenomenon", "phoenix", "pilum", "plexus", "praenomen", "psychosis",
		"quantum", "radius", "rectum", "sanctum", "scintilla", "scriptorium", "scrotum", "scutum", "septum", "simulacrum",
		"stratum", "substratum", "testis", "tympani", "ultimatum", "uterus", "vagina", "vertex", "vomitorium", "vortex",
		"vulva"
	};

	private static string[] greekPluralization1 = new string[37]
	{
		"archon", "aristos", "astron", "bebelos", "charisma", "chimera", "daimon", "domos", "echthros", "eidolon",
		"ephemeris", "epopis", "hegemon", "horos", "hystrix", "kentaur", "kharisma", "kudos", "laryngis", "larynx",
		"lemma", "logos", "mestor", "minotaur", "mnemon", "mythos", "omphalos", "ouros", "patris", "pharynx",
		"pragma", "rhetor", "rhinoceros", "schema", "stigma", "telos", "topos"
	};

	private static string[] greekPluralization2 = new string[6] { "diokesis", "ganglion", "noumenon", "numenon", "praxis", "therion" };

	private static string[] hebrewPluralization = new string[18]
	{
		"aswad", "chaya", "cherub", "galgal", "golem", "kabbalah", "keruv", "khaya", "nefesh", "nephil",
		"neshamah", "qabalah", "ruach", "ruakh", "sephirah", "seraph", "yechida", "yekhida"
	};

	private static string[] dualPluralization = new string[2] { "emerita", "emeritus" };

	private static Dictionary<string, string> firstPersonToThirdPerson = new Dictionary<string, string>();

	private static Dictionary<string, string> firstPersonToThirdPersonWithSpace = new Dictionary<string, string>();

	private static Dictionary<string, string> thirdPersonToFirstPerson = new Dictionary<string, string>();

	private static Dictionary<string, string> irregularThirdPerson = new Dictionary<string, string>
	{
		{ "'re", "'s" },
		{ "'ve", "'s" },
		{ "are", "is" },
		{ "aren't", "isn't" },
		{ "cannot", "cannot" },
		{ "can't", "can't" },
		{ "caught", "caught" },
		{ "could", "could" },
		{ "couldn't", "couldn't" },
		{ "don't", "doesn't" },
		{ "grew", "grew" },
		{ "had", "had" },
		{ "have", "has" },
		{ "may", "may" },
		{ "might", "might" },
		{ "must", "must" },
		{ "shall", "shall" },
		{ "shouldn't", "shouldn't" },
		{ "should", "should" },
		{ "sought", "sought" },
		{ "were", "was" },
		{ "will", "will" },
		{ "won't", "won't" },
		{ "wouldn't", "wouldn't" },
		{ "would", "would" }
	};

	private static StringBuilder sb1 = new StringBuilder(512);

	private static StringBuilder sb2 = new StringBuilder(32);

	public static string Pluralize(string word)
	{
		if (string.IsNullOrEmpty(word))
		{
			return word;
		}
		if (word[0] == '=')
		{
			return "=pluralize=" + word;
		}
		if (singularToPlural.TryGetValue(word, out var value))
		{
			return value;
		}
		if (irregularPluralization.TryGetValue(word, out value))
		{
			return FoundPlural(word, value);
		}
		Match match = Regex.Match(word, "^{{(.*?)\\|(.*)}}$");
		if (match.Success)
		{
			return FoundPlural(word, "{{" + match.Groups[1].Value + "|" + Pluralize(match.Groups[2].Value) + "}}");
		}
		match = Regex.Match(word, "(.*?)(&.(?:\\^.)?)$");
		if (match.Success)
		{
			return FoundPlural(word, Pluralize(match.Groups[1].Value) + match.Groups[2].Value);
		}
		match = Regex.Match(word, "^(&.(?:\\^.)?)(.*?)$");
		if (match.Success)
		{
			return FoundPlural(word, match.Groups[1].Value + Pluralize(match.Groups[2].Value));
		}
		match = Regex.Match(word, "^([*\\-_~'\"/])(.*)(\\1)$");
		if (match.Success)
		{
			return FoundPlural(word, match.Groups[1].Value + Pluralize(match.Groups[2].Value) + match.Groups[3].Value);
		}
		match = Regex.Match(word, "(.*?)( +)$");
		if (match.Success)
		{
			return FoundPlural(word, Pluralize(match.Groups[1].Value) + match.Groups[2].Value);
		}
		match = Regex.Match(word, "^( +)(.*?)$");
		if (match.Success)
		{
			return FoundPlural(word, match.Groups[1].Value + Pluralize(match.Groups[2].Value));
		}
		match = Regex.Match(word, "^(.*)( \\(.*\\))$");
		if (match.Success)
		{
			return FoundPlural(word, Pluralize(match.Groups[1].Value) + match.Groups[2].Value);
		}
		match = Regex.Match(word, "^(.*)( \\[.*\\])$");
		if (match.Success)
		{
			return FoundPlural(word, Pluralize(match.Groups[1].Value) + match.Groups[2].Value);
		}
		match = Regex.Match(word, "^(.*)( (?:of|in a|in an|in the|into|for|from|o'|to|with) .*)$", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			return FoundPlural(word, Pluralize(match.Groups[1].Value) + match.Groups[2].Value);
		}
		match = Regex.Match(word, "^(.*)( (?:mk\\.?|mark) *(?:[ivx]+))$", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			return FoundPlural(word, Pluralize(match.Groups[1].Value) + match.Groups[2].Value);
		}
		if (!word.Contains(" "))
		{
			match = Regex.Match(word, "^(.*)-(.*)$");
			if (match.Success)
			{
				return FoundPlural(word, match.Groups[1].Value + "-" + Pluralize(match.Groups[2].Value));
			}
		}
		if (identicalPluralization.Contains(word))
		{
			return FoundPlural(word, word);
		}
		if (word.EndsWith("folk"))
		{
			return FoundPlural(word, word);
		}
		if (latinPluralization.Contains(word))
		{
			if (word.EndsWith("us"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "i");
			}
			if (word.EndsWith("a"))
			{
				return FoundPlural(word, word + "e");
			}
			if (word.EndsWith("num"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "i");
			}
			if (word.EndsWith("um") || word.EndsWith("on"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "a");
			}
			if (word.EndsWith("en"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "ina");
			}
			if (word.EndsWith("is"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "es");
			}
			if (word.EndsWith("es"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "ites");
			}
			if (word.EndsWith("ex") || word.EndsWith("ix"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "ices");
			}
			return FoundPlural(word, word);
		}
		if (greekPluralization1.Contains(word))
		{
			if (word.EndsWith("os") || word.EndsWith("on"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 1) + "i");
			}
			if (word.EndsWith("is") || word.EndsWith("ix") || word.EndsWith("as"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "des");
			}
			if (word.EndsWith("ys"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "daes");
			}
			if (word.EndsWith("ma"))
			{
				return FoundPlural(word, word + "ta");
			}
			if (word.EndsWith("a"))
			{
				return FoundPlural(word, word + "e");
			}
			if (word.EndsWith("x"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "ges");
			}
			if (word.EndsWith("or"))
			{
				return FoundPlural(word, word + "es");
			}
			if (word.EndsWith("r"))
			{
				return FoundPlural(word, word + "oi");
			}
			return FoundPlural(word, word + "a");
		}
		if (greekPluralization2.Contains(word))
		{
			if (word.EndsWith("on"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "a");
			}
			if (word.EndsWith("is"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "es");
			}
			return FoundPlural(word, word);
		}
		if (hebrewPluralization.Contains(word))
		{
			if (word.EndsWith("ah"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 2) + "ot");
			}
			if (word.EndsWith("da"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 1) + "ot");
			}
			if (word.EndsWith("esh"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 3) + "ashot");
			}
			if (word.EndsWith("ch") || word.EndsWith("kh"))
			{
				return FoundPlural(word, word + "ot");
			}
			if (word.EndsWith("a"))
			{
				return FoundPlural(word, word.Substring(0, word.Length - 1) + "im");
			}
			return FoundPlural(word, word + "im");
		}
		if (word.Contains(" "))
		{
			string[] array = word.Split(' ');
			StringBuilder stringBuilder = Event.NewStringBuilder();
			if (dualPluralization.Contains(array[array.Length - 1]))
			{
				for (int i = 0; i < array.Length - 2; i++)
				{
					stringBuilder.Append(array[i]);
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(Pluralize(array[array.Length - 2]));
				stringBuilder.Append(" ");
				stringBuilder.Append(Pluralize(array[array.Length - 1]));
			}
			else
			{
				for (int j = 0; j < array.Length - 1; j++)
				{
					stringBuilder.Append(array[j]);
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(Pluralize(array[array.Length - 1]));
			}
			return FoundPlural(word, stringBuilder.ToString().Trim());
		}
		if (ColorUtility.HasUpperExceptFormatting(word))
		{
			if (ColorUtility.IsAllUpperExceptFormatting(word))
			{
				return FoundPlural(word, ColorUtility.ToUpperExceptFormatting(Pluralize(ColorUtility.ToLowerExceptFormatting(word))));
			}
			if (ColorUtility.IsFirstUpperExceptFormatting(word))
			{
				return FoundPlural(word, ColorUtility.CapitalizeExceptFormatting(Pluralize(ColorUtility.ToLowerExceptFormatting(word))));
			}
			return FoundPlural(word, Pluralize(ColorUtility.ToLowerExceptFormatting(word)));
		}
		if (word.EndsWith("elf") || word.EndsWith("olf") || word.EndsWith("arf") || word.EndsWith("alf"))
		{
			return FoundPlural(word, word.Substring(0, word.Length - 1) + "ves");
		}
		if (word.EndsWith("man"))
		{
			return FoundPlural(word, word.Substring(0, word.Length - 2) + "en");
		}
		if (word.EndsWith("ife"))
		{
			return FoundPlural(word, word.Substring(0, word.Length - 2) + "ves");
		}
		if (word.EndsWith("mensch"))
		{
			return FoundPlural(word, word + "en");
		}
		if (word.Length == 1)
		{
			return FoundPlural(word, word + "s");
		}
		char c = word[word.Length - 1];
		char c2 = word[word.Length - 2];
		string text = word.Substring(word.Length - 2, 2);
		value = word;
		if (c == 'z' && (c2 == 'a' || c2 == 'e' || c2 == 'i' || c2 == 'o' || c2 == 'u'))
		{
			value += "z";
		}
		if (c != 's' && c != 'x' && c != 'z')
		{
			switch (text)
			{
			case "sh":
			case "ss":
			case "ch":
				goto IL_0b24;
			}
			if (c != 'o' || c2 == 'o' || c2 == 'b')
			{
				value = ((c != 'y' || c2 == 'a' || c2 == 'e' || c2 == 'i' || c2 == 'o' || c2 == 'u') ? (value + "s") : (value.Substring(0, value.Length - 1) + "ies"));
				goto IL_0b78;
			}
		}
		goto IL_0b24;
		IL_0b24:
		value += "es";
		goto IL_0b78;
		IL_0b78:
		return FoundPlural(word, value);
	}

	private static string FoundPlural(string word, string plural)
	{
		if (!singularToPlural.ContainsKey(word))
		{
			singularToPlural.Add(word, plural);
		}
		if (!pluralToSingular.ContainsKey(plural))
		{
			pluralToSingular.Add(plural, word);
		}
		return plural;
	}

	public static string ThirdPerson(string word, bool PrependSpace = false)
	{
		if (string.IsNullOrEmpty(word))
		{
			return word;
		}
		string value = "";
		if (PrependSpace)
		{
			if (firstPersonToThirdPersonWithSpace.TryGetValue(word, out value))
			{
				return value;
			}
		}
		else if (firstPersonToThirdPerson.TryGetValue(word, out value))
		{
			return value;
		}
		if (irregularThirdPerson.TryGetValue(word, out value))
		{
			return FoundThirdPerson(word, value, PrependSpace);
		}
		Match match = Regex.Match(word, "(.*?)(&.(?:\\^.)?)$");
		if (match.Success)
		{
			return FoundThirdPerson(word, ThirdPerson(match.Groups[1].Value) + match.Groups[2].Value, PrependSpace);
		}
		match = Regex.Match(word, "^(&.(?:\\^.)?)(.*?)$");
		if (match.Success)
		{
			return FoundThirdPerson(word, match.Groups[1].Value + ThirdPerson(match.Groups[2].Value), PrependSpace);
		}
		match = Regex.Match(word, "^([*\\-_~'\"/])(.*)(\\1)$");
		if (match.Success)
		{
			return FoundThirdPerson(word, match.Groups[1].Value + ThirdPerson(match.Groups[2].Value) + match.Groups[3].Value, PrependSpace);
		}
		match = Regex.Match(word, "(.*?)( +)$");
		if (match.Success)
		{
			return FoundThirdPerson(word, ThirdPerson(match.Groups[1].Value) + match.Groups[2].Value, PrependSpace);
		}
		match = Regex.Match(word, "^( +)(.*?)$");
		if (match.Success)
		{
			return FoundThirdPerson(word, match.Groups[1].Value + ThirdPerson(match.Groups[2].Value), PrependSpace);
		}
		match = Regex.Match(word, "^(.+)( (?:and|or) )(.+)$", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			return FoundThirdPerson(word, ThirdPerson(match.Groups[1].Value) + match.Groups[2].Value + ThirdPerson(match.Groups[3].Value), PrependSpace);
		}
		if (word.Contains(" "))
		{
			string[] array = word.Split(' ');
			StringBuilder stringBuilder = Event.NewStringBuilder();
			for (int i = 0; i < array.Length - 1; i++)
			{
				stringBuilder.Append(array[i]);
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(ThirdPerson(array[array.Length - 1]));
			return FoundThirdPerson(word, stringBuilder.ToString(), PrependSpace);
		}
		match = Regex.Match(word, "^(.*)-(.*)$");
		if (match.Success)
		{
			return FoundThirdPerson(word, match.Groups[1].Value + "-" + ThirdPerson(match.Groups[2].Value), PrependSpace);
		}
		if (ColorUtility.HasUpperExceptFormatting(word))
		{
			if (ColorUtility.IsAllUpperExceptFormatting(word))
			{
				return FoundThirdPerson(word, ColorUtility.ToUpperExceptFormatting(ThirdPerson(ColorUtility.ToLowerExceptFormatting(word))), PrependSpace);
			}
			if (ColorUtility.IsFirstUpperExceptFormatting(word))
			{
				return FoundThirdPerson(word, ColorUtility.CapitalizeExceptFormatting(ThirdPerson(ColorUtility.ToLowerExceptFormatting(word))), PrependSpace);
			}
			return FoundThirdPerson(word, ThirdPerson(ColorUtility.ToLowerExceptFormatting(word)), PrependSpace);
		}
		if (word.Length == 1)
		{
			return FoundThirdPerson(word, word + "s", PrependSpace);
		}
		char c = word[word.Length - 1];
		char c2 = word[word.Length - 2];
		string text = word.Substring(word.Length - 2, 2);
		value = word;
		if (c == 'z' && (c2 == 'a' || c2 == 'e' || c2 == 'i' || c2 == 'o' || c2 == 'u'))
		{
			value += "z";
		}
		if (c != 's' && c != 'x' && c != 'z')
		{
			switch (text)
			{
			case "sh":
			case "ss":
			case "ch":
				goto IL_0414;
			}
			if (c != 'o' || c2 == 'o' || c2 == 'b')
			{
				value = ((c != 'y' || c2 == 'a' || c2 == 'e' || c2 == 'i' || c2 == 'o' || c2 == 'u') ? (value + "s") : (value.Substring(0, value.Length - 1) + "ies"));
				goto IL_0468;
			}
		}
		goto IL_0414;
		IL_0468:
		return FoundThirdPerson(word, value, PrependSpace);
		IL_0414:
		value += "es";
		goto IL_0468;
	}

	/// support method for ThirdPerson()
	private static string FoundThirdPerson(string firstPerson, string thirdPerson, bool PrependSpace)
	{
		if (!firstPersonToThirdPerson.ContainsKey(firstPerson))
		{
			firstPersonToThirdPerson.Add(firstPerson, thirdPerson);
		}
		if (!thirdPersonToFirstPerson.ContainsKey(thirdPerson))
		{
			thirdPersonToFirstPerson.Add(thirdPerson, firstPerson);
		}
		if (PrependSpace)
		{
			thirdPerson = " " + thirdPerson;
			if (!firstPersonToThirdPersonWithSpace.ContainsKey(firstPerson))
			{
				firstPersonToThirdPersonWithSpace.Add(firstPerson, thirdPerson);
			}
		}
		return thirdPerson;
	}

	public static string PastTenseOf(string verb)
	{
		switch (verb)
		{
		case "sleep":
			return "slept";
		case "sit":
			return "sat";
		case "drink":
			return "drank";
		case "put":
			return "put";
		case "are":
			return "was";
		case "have":
			return "had";
		case "eat":
			return "ate";
		default:
			if (verb.EndsWith("e"))
			{
				return verb + "d";
			}
			return verb + "ed";
		}
	}

	public static string CardinalNo(int num)
	{
		if (num != 0)
		{
			return Cardinal(num);
		}
		return "no";
	}

	public static string CardinalNo(long num)
	{
		if (num != 0L)
		{
			return Cardinal(num);
		}
		return "no";
	}

	public static string Cardinal(int num)
	{
		if (num == 0)
		{
			return "zero";
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (num < 0)
		{
			stringBuilder.Append("negative");
			num = -num;
		}
		int magnitude = (int)Math.Floor(Math.Log10(num));
		ProcessMagnitudes(ref num, ref magnitude, stringBuilder);
		if (num >= 20)
		{
			int num2 = num % 10;
			int num3 = (num - num2) / 10;
			num = num2;
			switch (num3)
			{
			case 2:
				stringBuilder.Append("twent");
				break;
			case 3:
				stringBuilder.Append("thirt");
				break;
			case 4:
				stringBuilder.Append("fort");
				break;
			case 5:
				stringBuilder.Append("fift");
				break;
			case 6:
				stringBuilder.Append("sixt");
				break;
			case 7:
				stringBuilder.Append("sevent");
				break;
			case 8:
				stringBuilder.Append("eight");
				break;
			case 9:
				stringBuilder.Append("ninet");
				break;
			}
			if (num == 0)
			{
				stringBuilder.Append("y");
				return stringBuilder.ToString();
			}
			stringBuilder.Append("y-");
		}
		switch (num)
		{
		case 1:
			stringBuilder.Append("one");
			break;
		case 2:
			stringBuilder.Append("two");
			break;
		case 3:
			stringBuilder.Append("three");
			break;
		case 4:
			stringBuilder.Append("four");
			break;
		case 5:
			stringBuilder.Append("five");
			break;
		case 6:
			stringBuilder.Append("six");
			break;
		case 7:
			stringBuilder.Append("seven");
			break;
		case 8:
			stringBuilder.Append("eight");
			break;
		case 9:
			stringBuilder.Append("nine");
			break;
		case 10:
			stringBuilder.Append("ten");
			break;
		case 11:
			stringBuilder.Append("eleven");
			break;
		case 12:
			stringBuilder.Append("twelve");
			break;
		case 13:
			stringBuilder.Append("thirteen");
			break;
		case 14:
			stringBuilder.Append("fourteen");
			break;
		case 15:
			stringBuilder.Append("fifteen");
			break;
		case 16:
			stringBuilder.Append("sixteen");
			break;
		case 17:
			stringBuilder.Append("seventeen");
			break;
		case 18:
			stringBuilder.Append("eighteen");
			break;
		case 19:
			stringBuilder.Append("nineteen");
			break;
		}
		return stringBuilder.ToString();
	}

	public static string Cardinal(long num)
	{
		if (num == 0L)
		{
			return "zero";
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (num < 0)
		{
			stringBuilder.Append("negative");
			num = -num;
		}
		int magnitude = (int)Math.Floor(Math.Log10(num));
		ProcessMagnitudes(ref num, ref magnitude, stringBuilder);
		if (num >= 20)
		{
			long num2 = num % 10;
			long num3 = (num - num2) / 10;
			num = num2;
			long num4 = num3 - 2;
			if ((ulong)num4 <= 7uL)
			{
				switch (num4)
				{
				case 0L:
					stringBuilder.Append("twent");
					break;
				case 1L:
					stringBuilder.Append("thirt");
					break;
				case 2L:
					stringBuilder.Append("fort");
					break;
				case 3L:
					stringBuilder.Append("fift");
					break;
				case 4L:
					stringBuilder.Append("sixt");
					break;
				case 5L:
					stringBuilder.Append("sevent");
					break;
				case 6L:
					stringBuilder.Append("eight");
					break;
				case 7L:
					stringBuilder.Append("ninet");
					break;
				}
			}
			if (num == 0L)
			{
				stringBuilder.Append("y");
				return stringBuilder.ToString();
			}
			stringBuilder.Append("y-");
		}
		long num5 = num - 1;
		if ((ulong)num5 <= 18uL)
		{
			switch (num5)
			{
			case 0L:
				stringBuilder.Append("one");
				break;
			case 1L:
				stringBuilder.Append("two");
				break;
			case 2L:
				stringBuilder.Append("three");
				break;
			case 3L:
				stringBuilder.Append("four");
				break;
			case 4L:
				stringBuilder.Append("five");
				break;
			case 5L:
				stringBuilder.Append("six");
				break;
			case 6L:
				stringBuilder.Append("seven");
				break;
			case 7L:
				stringBuilder.Append("eight");
				break;
			case 8L:
				stringBuilder.Append("nine");
				break;
			case 9L:
				stringBuilder.Append("ten");
				break;
			case 10L:
				stringBuilder.Append("eleven");
				break;
			case 11L:
				stringBuilder.Append("twelve");
				break;
			case 12L:
				stringBuilder.Append("thirteen");
				break;
			case 13L:
				stringBuilder.Append("fourteen");
				break;
			case 14L:
				stringBuilder.Append("fifteen");
				break;
			case 15L:
				stringBuilder.Append("sixteen");
				break;
			case 16L:
				stringBuilder.Append("seventeen");
				break;
			case 17L:
				stringBuilder.Append("eighteen");
				break;
			case 18L:
				stringBuilder.Append("nineteen");
				break;
			}
		}
		return stringBuilder.ToString();
	}

	public static string Ordinal(int num)
	{
		if (num == 0)
		{
			return "zeroth";
		}
		sb1.Length = 0;
		if (num < 0)
		{
			sb1.Append("negative");
			num = -num;
		}
		int magnitude = (int)Math.Floor(Math.Log10(num));
		ProcessMagnitudes(ref num, ref magnitude, sb1, "th");
		if (num >= 20)
		{
			int num2 = num % 10;
			int num3 = (num - num2) / 10;
			num = num2;
			switch (num3)
			{
			case 2:
				sb1.Append("twent");
				break;
			case 3:
				sb1.Append("thirt");
				break;
			case 4:
				sb1.Append("fort");
				break;
			case 5:
				sb1.Append("fift");
				break;
			case 6:
				sb1.Append("sixt");
				break;
			case 7:
				sb1.Append("sevent");
				break;
			case 8:
				sb1.Append("eight");
				break;
			case 9:
				sb1.Append("ninet");
				break;
			}
			if (num == 0)
			{
				sb1.Append("ieth");
				return sb1.ToString();
			}
			sb1.Append("y-");
		}
		switch (num)
		{
		case 1:
			sb1.Append("first");
			break;
		case 2:
			sb1.Append("second");
			break;
		case 3:
			sb1.Append("third");
			break;
		case 4:
			sb1.Append("fourth");
			break;
		case 5:
			sb1.Append("fifth");
			break;
		case 6:
			sb1.Append("sixth");
			break;
		case 7:
			sb1.Append("seventh");
			break;
		case 8:
			sb1.Append("eighth");
			break;
		case 9:
			sb1.Append("ninth");
			break;
		case 10:
			sb1.Append("tenth");
			break;
		case 11:
			sb1.Append("eleventh");
			break;
		case 12:
			sb1.Append("twelfth");
			break;
		case 13:
			sb1.Append("thirteenth");
			break;
		case 14:
			sb1.Append("fourteenth");
			break;
		case 15:
			sb1.Append("fifteenth");
			break;
		case 16:
			sb1.Append("sixteenth");
			break;
		case 17:
			sb1.Append("seventeenth");
			break;
		case 18:
			sb1.Append("eighteenth");
			break;
		case 19:
			sb1.Append("nineteenth");
			break;
		}
		return sb1.ToString();
	}

	public static string Ordinal(long num)
	{
		if (num == 0L)
		{
			return "zeroth";
		}
		sb1.Length = 0;
		if (num < 0)
		{
			sb1.Append("negative");
			num = -num;
		}
		int magnitude = (int)Math.Floor(Math.Log10(num));
		ProcessMagnitudes(ref num, ref magnitude, sb1, "th");
		if (num >= 20)
		{
			long num2 = num % 10;
			long num3 = (num - num2) / 10;
			num = num2;
			long num4 = num3 - 2;
			if ((ulong)num4 <= 7uL)
			{
				switch (num4)
				{
				case 0L:
					sb1.Append("twent");
					break;
				case 1L:
					sb1.Append("thirt");
					break;
				case 2L:
					sb1.Append("fort");
					break;
				case 3L:
					sb1.Append("fift");
					break;
				case 4L:
					sb1.Append("sixt");
					break;
				case 5L:
					sb1.Append("sevent");
					break;
				case 6L:
					sb1.Append("eight");
					break;
				case 7L:
					sb1.Append("ninet");
					break;
				}
			}
			if (num == 0L)
			{
				sb1.Append("ieth");
				return sb1.ToString();
			}
			sb1.Append("y-");
		}
		long num5 = num - 1;
		if ((ulong)num5 <= 18uL)
		{
			switch (num5)
			{
			case 0L:
				sb1.Append("first");
				break;
			case 1L:
				sb1.Append("second");
				break;
			case 2L:
				sb1.Append("third");
				break;
			case 3L:
				sb1.Append("fourth");
				break;
			case 4L:
				sb1.Append("fifth");
				break;
			case 5L:
				sb1.Append("sixth");
				break;
			case 6L:
				sb1.Append("seventh");
				break;
			case 7L:
				sb1.Append("eighth");
				break;
			case 8L:
				sb1.Append("ninth");
				break;
			case 9L:
				sb1.Append("tenth");
				break;
			case 10L:
				sb1.Append("eleventh");
				break;
			case 11L:
				sb1.Append("twelfth");
				break;
			case 12L:
				sb1.Append("thirteenth");
				break;
			case 13L:
				sb1.Append("fourteenth");
				break;
			case 14L:
				sb1.Append("fifteenth");
				break;
			case 15L:
				sb1.Append("sixteenth");
				break;
			case 16L:
				sb1.Append("seventeenth");
				break;
			case 17L:
				sb1.Append("eighteenth");
				break;
			case 18L:
				sb1.Append("nineteenth");
				break;
			}
		}
		return sb1.ToString();
	}

	private static void ProcessMagnitude(ref int num, ref int magnitude, StringBuilder result, string place)
	{
		if (magnitude > 3)
		{
			magnitude -= magnitude % 3;
		}
		int num2 = (int)Math.Floor(Math.Exp((double)magnitude * Math.Log(10.0)));
		int num3 = num % num2;
		int num4 = (num - num3) / num2;
		if (num4 > 0)
		{
			result.Append(Cardinal(num4));
			result.Append(" ");
			result.Append(place);
			num = num3;
			if (num > 0)
			{
				result.Append(" ");
			}
		}
		magnitude--;
	}

	/// support method for Cardinal() and Ordinal()
	private static void ProcessMagnitude(ref long num, ref int magnitude, StringBuilder result, string place)
	{
		if (magnitude > 3)
		{
			magnitude -= magnitude % 3;
		}
		int num2 = (int)Math.Floor(Math.Exp((double)magnitude * Math.Log(10.0)));
		long num3 = num % num2;
		long num4 = (num - num3) / num2;
		if (num4 > 0)
		{
			result.Append(Cardinal(num4));
			result.Append(" ");
			result.Append(place);
			num = num3;
			if (num > 0)
			{
				result.Append(" ");
			}
		}
		magnitude--;
	}

	private static bool ProcessMagnitudes(ref int num, ref int magnitude, StringBuilder result, string suffix = null)
	{
		switch (magnitude)
		{
		case 18:
		case 19:
		case 20:
			ProcessMagnitude(ref num, ref magnitude, result, "quintillion");
			if (num == 0)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			goto case 15;
		case 15:
		case 16:
		case 17:
			ProcessMagnitude(ref num, ref magnitude, result, "quadrillion");
			if (num == 0)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			goto case 12;
		case 12:
		case 13:
		case 14:
			ProcessMagnitude(ref num, ref magnitude, result, "trillion");
			if (num == 0)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			goto case 9;
		case 9:
		case 10:
		case 11:
			ProcessMagnitude(ref num, ref magnitude, result, "billion");
			if (num == 0)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			goto case 6;
		case 6:
		case 7:
		case 8:
			ProcessMagnitude(ref num, ref magnitude, result, "million");
			if (num == 0)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			goto case 3;
		case 3:
		case 4:
		case 5:
			ProcessMagnitude(ref num, ref magnitude, result, "thousand");
			if (num == 0)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			goto case 2;
		case 2:
			if (magnitude <= 1)
			{
				break;
			}
			ProcessMagnitude(ref num, ref magnitude, result, "hundred");
			if (num == 0)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			break;
		}
		return false;
	}

	/// support method for Cardinal() and Ordinal()
	private static bool ProcessMagnitudes(ref long num, ref int magnitude, StringBuilder result, string suffix = null)
	{
		switch (magnitude)
		{
		case 18:
		case 19:
		case 20:
			ProcessMagnitude(ref num, ref magnitude, result, "quintillion");
			if (num == 0L)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			goto case 15;
		case 15:
		case 16:
		case 17:
			ProcessMagnitude(ref num, ref magnitude, result, "quadrillion");
			if (num == 0L)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			goto case 12;
		case 12:
		case 13:
		case 14:
			ProcessMagnitude(ref num, ref magnitude, result, "trillion");
			if (num == 0L)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			goto case 9;
		case 9:
		case 10:
		case 11:
			ProcessMagnitude(ref num, ref magnitude, result, "billion");
			if (num == 0L)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			goto case 6;
		case 6:
		case 7:
		case 8:
			ProcessMagnitude(ref num, ref magnitude, result, "million");
			if (num == 0L)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			goto case 3;
		case 3:
		case 4:
		case 5:
			ProcessMagnitude(ref num, ref magnitude, result, "thousand");
			if (num == 0L)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			goto case 2;
		case 2:
			if (magnitude <= 1)
			{
				break;
			}
			ProcessMagnitude(ref num, ref magnitude, result, "hundred");
			if (num == 0L)
			{
				if (suffix != null)
				{
					result.Append(suffix);
				}
				return true;
			}
			break;
		}
		return false;
	}

	public static string Multiplicative(int num)
	{
		return num switch
		{
			0 => "never", 
			1 => "once", 
			2 => "twice", 
			3 => "thrice", 
			_ => Cardinal(num) + " times", 
		};
	}

	public static string Multiplicative(long num)
	{
		if ((ulong)num <= 3uL)
		{
			switch (num)
			{
			case 0L:
				return "never";
			case 1L:
				return "once";
			case 2L:
				return "twice";
			case 3L:
				return "thrice";
			}
		}
		return Cardinal(num) + " times";
	}

	public static string MakeOrList(string[] words)
	{
		if (words.Length == 0)
		{
			return "";
		}
		if (words.Length == 1)
		{
			return words[0];
		}
		if (words.Length == 2)
		{
			return words[0] + " or " + words[1];
		}
		string value = ",";
		for (int i = 0; i < words.Length; i++)
		{
			if (words[i].Contains(','))
			{
				value = ";";
				break;
			}
		}
		sb1.Length = 0;
		for (int j = 0; j < words.Length - 2; j++)
		{
			sb1.Append(words[j]);
			sb1.Append(value);
		}
		sb1.Append(words[words.Length - 1]);
		return sb1.ToString();
	}

	public static string MakeOrList(List<string> words)
	{
		if (words.Count == 0)
		{
			return "";
		}
		if (words.Count == 1)
		{
			return words[0];
		}
		if (words.Count == 2)
		{
			return words[0] + " or " + words[1];
		}
		string value = ", ";
		for (int i = 0; i < words.Count; i++)
		{
			if (words[i].Contains(','))
			{
				value = "; ";
				break;
			}
		}
		sb1.Length = 0;
		for (int j = 0; j < words.Count - 1; j++)
		{
			sb1.Append(words[j]);
			sb1.Append(value);
		}
		sb1.Append("or ");
		sb1.Append(words[words.Count - 1]);
		return sb1.ToString();
	}

	public static string MakeAndList(string[] words)
	{
		if (words.Length == 0)
		{
			return "";
		}
		if (words.Length == 1)
		{
			return words[0];
		}
		if (words.Length == 2)
		{
			return words[0] + " and " + words[1];
		}
		string value = ", ";
		for (int i = 0; i < words.Length; i++)
		{
			if (words[i].Contains(','))
			{
				value = "; ";
				break;
			}
		}
		sb1.Length = 0;
		for (int j = 0; j < words.Length - 1; j++)
		{
			sb1.Append(words[j]);
			sb1.Append(value);
		}
		sb1.Append("and ");
		sb1.Append(words[words.Length - 1]);
		return sb1.ToString();
	}

	public static string MakeAndList(List<string> words)
	{
		if (words.Count == 0)
		{
			return "";
		}
		if (words.Count == 1)
		{
			return words[0];
		}
		if (words.Count == 2)
		{
			return words[0] + " and " + words[1];
		}
		string value = ", ";
		for (int i = 0; i < words.Count; i++)
		{
			if (words[i].Contains(','))
			{
				value = "; ";
				break;
			}
		}
		sb1.Length = 0;
		for (int j = 0; j < words.Count - 1; j++)
		{
			sb1.Append(words[j]);
			sb1.Append(value);
		}
		sb1.Append("and ");
		sb1.Append(words[words.Count - 1]);
		return sb1.ToString();
	}

	public static string MakePossessive(string word)
	{
		int num = 0;
		while (word.EndsWith("}}"))
		{
			num++;
			word = word.Substring(0, word.Length - 2);
		}
		word = ((!word.EndsWith("s")) ? (word + "'s") : (word + "'"));
		for (int i = 0; i < num; i++)
		{
			word += "}}";
		}
		return word;
	}

	public static string MakeCompoundWord(string Word1, string Word2, bool UseHyphen = false)
	{
		sb1.Clear();
		if (object.Equals(Word1[Word1.Count() - 1], Word2[0]) || UseHyphen)
		{
			sb1.Append(Word1);
			sb1.Append("-");
			sb1.Append(Word2);
		}
		else
		{
			sb1.Append(Word1);
			sb1.Append(Word2);
		}
		return sb1.ToString();
	}

	public static string MakeTitleCase(string Phrase)
	{
		string[] array = Phrase.Split(' ');
		bool flag = true;
		sb1.Length = 0;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (flag)
			{
				sb1.Append(InitialCap(text));
				flag = false;
			}
			else if (IsLowerCapWord(text))
			{
				sb1.Append(text);
			}
			else
			{
				sb1.Append(InitialCap(text));
			}
			sb1.Append(" ");
		}
		return sb1.ToString().TrimEnd(' ');
	}

	public static string InitialCap(string Word)
	{
		if (string.IsNullOrEmpty(Word))
		{
			return Word;
		}
		if (Word.Contains("-"))
		{
			string[] array = Word.Split('-');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Length > 1)
				{
					array[i] = char.ToUpper(array[i][0]) + array[i].Substring(1);
				}
				else if (i == 0)
				{
					array[i] = array[i].ToUpper();
				}
			}
			return string.Join("-", array);
		}
		return char.ToUpper(Word[0]) + Word.Substring(1);
	}

	public static bool IsSupportingWord(string Word)
	{
		string value = (Word.Any(char.IsUpper) ? Word.ToLower() : Word);
		if (!prepositions.Contains(value) && !articles.Contains(value) && !conjunctions.Contains(value) && !badEndingWords.Contains(value))
		{
			return badStartingWords.Contains(value);
		}
		return true;
	}

	public static bool IsLowerCapWord(string Word)
	{
		string value = (Word.Any(char.IsUpper) ? Word.ToLower() : Word);
		if (!shortPrepositions.Contains(value) && !articles.Contains(value))
		{
			return conjunctions.Contains(value);
		}
		return true;
	}

	public static bool IsBadTitleStartingWord(string Word)
	{
		string value = (Word.Any(char.IsUpper) ? Word.ToLower() : Word);
		return badStartingWords.Contains(value);
	}

	private static bool IsArticleException(string Word)
	{
		string value = (Word.Any(char.IsUpper) ? Word.ToLower() : Word);
		return articleExceptions.Contains(value);
	}

	public static string RemoveBadTitleEndingWords(string Phrase)
	{
		string[] array = Phrase.Split(' ');
		if (IsSupportingWord(array[array.Length - 1]))
		{
			array[array.Length - 1] = "";
			return RemoveBadTitleEndingWords(string.Join(" ", array).TrimEnd(' '));
		}
		return Phrase;
	}

	public static string RemoveBadTitleStartingWords(string Phrase)
	{
		string[] array = Phrase.Split(' ');
		if (IsBadTitleStartingWord(array[0]))
		{
			array[0] = "";
			return RemoveBadTitleStartingWords(string.Join(" ", array).TrimStart(' '));
		}
		return Phrase;
	}

	public static string RandomShePronoun()
	{
		if (!50.in100())
		{
			return "she";
		}
		return "he";
	}

	public static string ObjectPronoun(string subjectPronoun)
	{
		if (subjectPronoun == "he")
		{
			return "him";
		}
		return "her";
	}

	public static string PossessivePronoun(string subjectPronoun)
	{
		if (subjectPronoun == "he")
		{
			return "his";
		}
		return "her";
	}

	public static string ReflexivePronoun(string subjectPronoun)
	{
		if (subjectPronoun == "he")
		{
			return "himself";
		}
		return "herself";
	}

	public static string InitCap(string word)
	{
		if (string.IsNullOrEmpty(word))
		{
			return "";
		}
		if (word.Length == 1)
		{
			return char.ToUpper(word[0]).ToString();
		}
		char[] array = word.ToCharArray();
		array[0] = char.ToUpper(array[0]);
		return new string(array);
	}

	public static string InitCapWithFormatting(string word)
	{
		if (string.IsNullOrEmpty(word))
		{
			return "";
		}
		if (word.Length == 1)
		{
			return char.ToUpper(word[0]).ToString();
		}
		char[] array = word.ToCharArray();
		int num = 0;
		while (num < word.Length)
		{
			if (word[num] == '&' || word[num] == '^')
			{
				num++;
				num++;
				continue;
			}
			array[num] = char.ToUpper(array[num]);
			break;
		}
		return new string(array);
	}

	public static string CapAfterNewlines(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return "";
		}
		sb1.Length = 0;
		string[] array = text.Split('\n');
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			sb1.Append(InitCap(array[i]));
			if (i < num - 1)
			{
				sb1.Append('\n');
			}
		}
		return sb1.ToString();
	}

	public static string InitLower(string word)
	{
		if (string.IsNullOrEmpty(word))
		{
			return "";
		}
		if (word.Length == 1)
		{
			if (!char.IsUpper(word[0]))
			{
				return word;
			}
			return char.ToLower(word[0]).ToString();
		}
		if (word[0] == '&' && word.Length >= 3)
		{
			return word.Substring(0, 2) + char.ToLower(word[2]) + word.Substring(3);
		}
		return char.ToLower(word[0]) + word.Substring(1);
	}

	public static string InitLowerIfArticle(string word)
	{
		if (word.Length < 7)
		{
			return word;
		}
		if (word.Substring(0, 5).Contains("A ") || word.Substring(0, 6).Contains("The ") || word.Substring(0, 5).Contains("An ") || word.Substring(0, 7).Contains("Some "))
		{
			if (word[0] == '&')
			{
				return word.Substring(0, 2) + char.ToLower(word[2]) + word.Substring(3);
			}
			return char.ToLower(word[0]) + word.Substring(1);
		}
		return word;
	}

	public static string LowerArticles(string phrase)
	{
		return phrase.Replace("A ", "a ").Replace("The ", "the ").Replace("Some ", "some ");
	}

	public static string MakeTitleCaseWithArticle(string phrase)
	{
		if (phrase.StartsWith("the ") || phrase.StartsWith("The ") || phrase.StartsWith("some ") || phrase.StartsWith("Some "))
		{
			string text = MakeTitleCase(phrase);
			return char.ToLower(text[0]) + text.Substring(1);
		}
		return MakeTitleCase(phrase);
	}

	private static bool IsNonWord(char ch)
	{
		if (char.IsLetterOrDigit(ch))
		{
			return false;
		}
		if (ch == '-')
		{
			return false;
		}
		return true;
	}

	public static bool IndefiniteArticleShouldBeAn(string Word)
	{
		if (string.IsNullOrEmpty(Word))
		{
			return false;
		}
		string text = Word;
		if (ColorUtility.HasFormatting(text))
		{
			text = ColorUtility.StripFormatting(text);
		}
		if (text.Any(char.IsUpper))
		{
			text = text.ToLower();
		}
		int num = text.IndexOf(' ');
		while (true)
		{
			switch (num)
			{
			case 0:
				goto IL_0041;
			default:
				text = text.Substring(0, num);
				break;
			case -1:
				break;
			}
			break;
			IL_0041:
			text = text.Substring(1);
			num = text.IndexOf(' ');
		}
		if (text.Any(IsNonWord))
		{
			sb2.Clear();
			int i = 0;
			for (int length = text.Length; i < length; i++)
			{
				if (!IsNonWord(text[i]))
				{
					sb2.Append(text[i]);
				}
			}
			text = sb2.ToString();
		}
		while (text.Length > 0 && text[0] == '-')
		{
			text = text.Substring(1);
		}
		if (text == "")
		{
			return false;
		}
		char c = text[0];
		if ((c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u') ^ IsArticleException(text))
		{
			return !text.StartsWith("one-");
		}
		return false;
	}

	public static bool IndefiniteArticleShouldBeAn(int Number)
	{
		return IndefiniteArticleShouldBeAn(Cardinal(Number));
	}

	public static string A(string word, bool capitalize = false)
	{
		if (word[0] == '=')
		{
			return "=article=" + word;
		}
		return ((!IndefiniteArticleShouldBeAn(word)) ? (capitalize ? "A " : "a ") : (capitalize ? "An " : "an ")) + word;
	}

	public static void A(string word, StringBuilder result, bool capitalize = false)
	{
		if (word[0] == '=')
		{
			result.Append("=article=").Append(word);
		}
		else
		{
			result.Append((!IndefiniteArticleShouldBeAn(word)) ? (capitalize ? "A " : "a ") : (capitalize ? "An " : "an ")).Append(word);
		}
	}

	public static string A(int number, bool capitalize = false)
	{
		return ((!IndefiniteArticleShouldBeAn(number)) ? (capitalize ? "A " : "a ") : (capitalize ? "An " : "an ")) + number;
	}

	public static void A(int number, StringBuilder result, bool capitalize = false)
	{
		result.Append((!IndefiniteArticleShouldBeAn(number)) ? (capitalize ? "A " : "a ") : (capitalize ? "An " : "an ")).Append(number);
	}

	public static string ConvertAtoAn(string sentence)
	{
		string[] array = sentence.Split(' ');
		sb1.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			sb1.Append(array[i]);
			if (i < array.Length - 1)
			{
				if ((array[i].Equals("a") || array[i].Equals("A")) && !string.IsNullOrEmpty(array[i + 1]) && IndefiniteArticleShouldBeAn(array[i + 1]))
				{
					sb1.Append("n");
				}
				sb1.Append(" ");
			}
		}
		return sb1.ToString();
	}

	public static string AOrAnBeforeNumber(int number)
	{
		if (number == 11)
		{
			return "an";
		}
		while (number >= 10)
		{
			number /= 10;
		}
		if (number == 8)
		{
			return "an";
		}
		return "a";
	}

	public static string GetWordRoot(string word)
	{
		string randomMeaningfulWord = GetRandomMeaningfulWord(word);
		string text = "";
		string[] array = Regex.Split(randomMeaningfulWord, "(?=[aeiouy])");
		for (int i = 0; i <= array.Length - 1; i++)
		{
			text = ((i != array.Length - 1) ? (text + array[i]) : (text + array[i].TrimEnd('a', 'e', 'i', 'o', 'u', 'y')));
		}
		return text;
	}

	public static string Adjectify(string word)
	{
		string randomElement = new string[5] { "ian", "ic", "-like", "ary", "ique" }.GetRandomElement();
		word = TrimTrailingS(word);
		word = RemovePunctuation(word);
		if (randomElement[0] != '-')
		{
			word = GetWordRoot(word);
		}
		return word + randomElement;
	}

	public static string TrimLeadingThe(string phrase)
	{
		if (phrase.StartsWith("the ") && phrase.Length >= 5)
		{
			return phrase.Substring(4);
		}
		return phrase;
	}

	public static string TrimTrailingS(string word)
	{
		if (word[word.Length - 1] == 's')
		{
			return word.Substring(0, word.Length - 1);
		}
		return word;
	}

	public static string TrimTrailingPunctuation(string phrase)
	{
		if (phrase.Length == 0)
		{
			return phrase;
		}
		if (punctuation.Contains(phrase[phrase.Length - 1]))
		{
			return TrimTrailingPunctuation(phrase.Substring(0, phrase.Length - 1));
		}
		return phrase;
	}

	public static string RemovePunctuation(string word)
	{
		char[] array = punctuation;
		for (int i = 0; i < array.Length; i++)
		{
			char value = array[i];
			if (word.IndexOf(value) != -1)
			{
				word = word.Replace(value.ToString() ?? "", "");
			}
		}
		return word;
	}

	public static string GetRandomMeaningfulWord(string phrase)
	{
		string[] array = phrase.Split(' ');
		string text = array[Stat.Random(0, array.Length - 1)];
		while (prepositions.Contains(text) || articles.Contains(text) || conjunctions.Contains(text) || ordinalsRoman.Contains(text) || demonstrativePronouns.Contains(text))
		{
			text = array[Stat.Random(0, array.Length - 1)];
		}
		return text;
	}

	public static string GetWeightedStartingArticle()
	{
		return articleStartingWords.GetRandomElement();
	}

	public static string Stutterize(string sentence, string word)
	{
		string[] array = sentence.Split(' ');
		if (array.Length > 3)
		{
			int num = Stat.Random(0, array.Length - 3);
			int num2 = 0;
			for (int i = 0; i < array.Length - 1; i++)
			{
				if (i == num)
				{
					num2 = 4;
				}
				if (num2 == 1)
				{
					array[i] = array[i] + "... " + word;
					num2--;
				}
				else if (num2 > 0)
				{
					array[i] += "...";
					num2--;
				}
			}
			return string.Join(" ", array);
		}
		return sentence;
	}

	public static string GetProsaicZoneName(Zone Z)
	{
		if (Z.HasProperName)
		{
			return Z.DisplayName;
		}
		if (Z.NameContext != null)
		{
			return "the outskirts of " + Z.NameContext;
		}
		if (Z.NamedByPlayer)
		{
			return Z.DisplayName;
		}
		return Z.GetTerrainObject().a + Z.GetTerrainObject().DisplayNameOnlyDirectAndStripped;
	}

	public static string GetRomanNumeral(int val)
	{
		if (val == 0)
		{
			return "N";
		}
		sb1.Clear();
		if (val < 0)
		{
			sb1.Append("-");
			val = -val;
		}
		if (val >= 1000)
		{
			int i = 0;
			for (int num = val / 1000; i < num; i++)
			{
				sb1.Append('M');
			}
			val %= 1000;
		}
		if (val >= 100)
		{
			int num2 = val / 100;
			switch (num2)
			{
			case 9:
				sb1.Append("CM");
				break;
			case 6:
			case 7:
			case 8:
			{
				sb1.Append('D');
				int k = 0;
				for (int num3 = num2 - 5; k < num3; k++)
				{
					sb1.Append('C');
				}
				break;
			}
			case 5:
				sb1.Append('D');
				break;
			case 4:
				sb1.Append("CD");
				break;
			default:
			{
				for (int j = 0; j < num2; j++)
				{
					sb1.Append('C');
				}
				break;
			}
			}
			val %= 100;
		}
		if (val >= 10)
		{
			int num4 = val / 10;
			switch (num4)
			{
			case 9:
				sb1.Append("XC");
				break;
			case 6:
			case 7:
			case 8:
			{
				sb1.Append('L');
				int m = 0;
				for (int num5 = num4 - 5; m < num5; m++)
				{
					sb1.Append('X');
				}
				break;
			}
			case 5:
				sb1.Append('L');
				break;
			case 4:
				sb1.Append("XL");
				break;
			default:
			{
				for (int l = 0; l < num4; l++)
				{
					sb1.Append('X');
				}
				break;
			}
			}
			val %= 10;
		}
		switch (val)
		{
		case 9:
			sb1.Append("IX");
			break;
		case 6:
		case 7:
		case 8:
		{
			sb1.Append('V');
			int num6 = 0;
			for (int num7 = val - 5; num6 < num7; num6++)
			{
				sb1.Append('I');
			}
			break;
		}
		case 5:
			sb1.Append('V');
			break;
		case 4:
			sb1.Append("IV");
			break;
		default:
		{
			for (int n = 0; n < val; n++)
			{
				sb1.Append('I');
			}
			break;
		}
		}
		return sb1.ToString();
	}

	public static string GetRomanNumeral(long val)
	{
		if (val == 0L)
		{
			return "N";
		}
		sb1.Clear();
		if (val < 0)
		{
			sb1.Append("-");
			val = -val;
		}
		if (val >= 1000)
		{
			long num = 0L;
			for (long num2 = val / 1000; num < num2; num++)
			{
				sb1.Append('M');
			}
			val %= 1000;
		}
		long num3;
		if (val >= 100)
		{
			num3 = val / 100;
			long num4 = num3 - 4;
			if ((ulong)num4 > 5uL)
			{
				goto IL_0106;
			}
			switch (num4)
			{
			case 5L:
				break;
			case 2L:
			case 3L:
			case 4L:
				goto IL_00b4;
			case 1L:
				goto IL_00e5;
			case 0L:
				goto IL_00f4;
			default:
				goto IL_0106;
			}
			sb1.Append("CM");
			goto IL_0124;
		}
		goto IL_012b;
		IL_023c:
		sb1.Append('V');
		long num5 = 0L;
		for (long num6 = val - 5; num5 < num6; num5++)
		{
			sb1.Append('I');
		}
		goto IL_02b1;
		IL_00e5:
		sb1.Append('D');
		goto IL_0124;
		IL_0124:
		val %= 100;
		goto IL_012b;
		IL_00f4:
		sb1.Append("CD");
		goto IL_0124;
		IL_0280:
		sb1.Append("IV");
		goto IL_02b1;
		IL_01b3:
		sb1.Append('L');
		goto IL_01f4;
		IL_01d4:
		long num8;
		for (long num7 = 0L; num7 < num8; num7++)
		{
			sb1.Append('X');
		}
		goto IL_01f4;
		IL_0292:
		for (long num9 = 0L; num9 < val; num9++)
		{
			sb1.Append('I');
		}
		goto IL_02b1;
		IL_012b:
		if (val >= 10)
		{
			num8 = val / 10;
			long num10 = num8 - 4;
			if ((ulong)num10 > 5uL)
			{
				goto IL_01d4;
			}
			switch (num10)
			{
			case 5L:
				break;
			case 2L:
			case 3L:
			case 4L:
				goto IL_017d;
			case 1L:
				goto IL_01b3;
			case 0L:
				goto IL_01c2;
			default:
				goto IL_01d4;
			}
			sb1.Append("XC");
			goto IL_01f4;
		}
		goto IL_01fb;
		IL_01fb:
		long num11 = val - 4;
		if ((ulong)num11 > 5uL)
		{
			goto IL_0292;
		}
		switch (num11)
		{
		case 5L:
			break;
		case 2L:
		case 3L:
		case 4L:
			goto IL_023c;
		case 1L:
			goto IL_0271;
		case 0L:
			goto IL_0280;
		default:
			goto IL_0292;
		}
		sb1.Append("IX");
		goto IL_02b1;
		IL_0271:
		sb1.Append('V');
		goto IL_02b1;
		IL_0106:
		for (int i = 0; i < num3; i++)
		{
			sb1.Append('C');
		}
		goto IL_0124;
		IL_01f4:
		val %= 10;
		goto IL_01fb;
		IL_02b1:
		return sb1.ToString();
		IL_01c2:
		sb1.Append("XL");
		goto IL_01f4;
		IL_00b4:
		sb1.Append('D');
		long num12 = 0L;
		for (long num13 = num3 - 5; num12 < num13; num12++)
		{
			sb1.Append('C');
		}
		goto IL_0124;
		IL_017d:
		sb1.Append('L');
		long num14 = 0L;
		for (long num15 = num8 - 5; num14 < num15; num14++)
		{
			sb1.Append('X');
		}
		goto IL_01f4;
	}

	public static string Weirdify(string word, int Chance = 100)
	{
		char randomElement = weirdLowerAs.GetRandomElement();
		char randomElement2 = weirdUpperAs.GetRandomElement();
		char randomElement3 = weirdLowerEs.GetRandomElement();
		char randomElement4 = weirdLowerEs.GetRandomElement();
		char randomElement5 = weirdLowerIs.GetRandomElement();
		char randomElement6 = weirdUpperIs.GetRandomElement();
		char randomElement7 = weirdLowerOs.GetRandomElement();
		char randomElement8 = weirdUpperOs.GetRandomElement();
		char randomElement9 = weirdLowerUs.GetRandomElement();
		char randomElement10 = weirdUpperUs.GetRandomElement();
		char randomElement11 = weirdLowerCs.GetRandomElement();
		char randomElement12 = weirdLowerFs.GetRandomElement();
		char randomElement13 = weirdLowerNs.GetRandomElement();
		char randomElement14 = weirdLowerTs.GetRandomElement();
		char randomElement15 = weirdLowerYs.GetRandomElement();
		char randomElement16 = weirdUpperBs.GetRandomElement();
		char randomElement17 = weirdUpperCs.GetRandomElement();
		char randomElement18 = weirdUpperYs.GetRandomElement();
		char randomElement19 = weirdUpperLs.GetRandomElement();
		char randomElement20 = weirdUpperRs.GetRandomElement();
		char randomElement21 = weirdUpperNs.GetRandomElement();
		if (If.Chance(Chance))
		{
			word = word.Replace('a', randomElement);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('A', randomElement2);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('e', randomElement3);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('E', randomElement4);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('i', randomElement5);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('I', randomElement6);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('o', randomElement7);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('O', randomElement8);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('u', randomElement9);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('U', randomElement10);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('c', randomElement11);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('f', randomElement12);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('n', randomElement13);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('t', randomElement14);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('y', randomElement15);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('B', randomElement16);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('C', randomElement17);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('Y', randomElement18);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('L', randomElement19);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('R', randomElement20);
		}
		if (If.Chance(Chance))
		{
			word = word.Replace('N', randomElement21);
		}
		return word;
	}

	public static string Obfuscate(string phrase, int noiseValue = 15, int sameObfuscatorChance = 80, int onlyObfuscastorsChance = 5)
	{
		StringBuilder stringBuilder = new StringBuilder();
		char randomElement = obfuscators.GetRandomElement();
		bool flag = (If.Chance(onlyObfuscastorsChance) ? true : false);
		foreach (char c in phrase)
		{
			if (c == ' ')
			{
				stringBuilder.Append(c);
			}
			else if (If.Chance(noiseValue))
			{
				if (If.Chance(sameObfuscatorChance))
				{
					stringBuilder.Append(randomElement);
				}
				else
				{
					stringBuilder.Append(obfuscators.GetRandomElement());
				}
			}
			else if (!flag)
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static int LevenshteinDistance(string s, string t, bool caseInsensitive = true)
	{
		int length = s.Length;
		int length2 = t.Length;
		if (caseInsensitive)
		{
			for (int i = 0; i < length; i++)
			{
				if (char.IsUpper(s[i]))
				{
					s = s.ToLower();
					break;
				}
			}
			for (int j = 0; j < length2; j++)
			{
				if (char.IsUpper(t[j]))
				{
					t = t.ToLower();
					break;
				}
			}
		}
		int[,] array = new int[length + 1, length2 + 1];
		if (length == 0)
		{
			return length2;
		}
		if (length2 == 0)
		{
			return length;
		}
		int num = 0;
		while (num <= length)
		{
			array[num, 0] = num++;
		}
		int num2 = 0;
		while (num2 <= length2)
		{
			array[0, num2] = num2++;
		}
		for (int k = 1; k <= length; k++)
		{
			for (int l = 1; l <= length2; l++)
			{
				int num3 = ((t[l - 1] != s[k - 1]) ? 1 : 0);
				array[k, l] = Math.Min(Math.Min(array[k - 1, l] + 1, array[k, l - 1] + 1), array[k - 1, l - 1] + num3);
			}
		}
		return array[length, length2];
	}

	public static bool ContainsBadWords(string phrase)
	{
		string[] array = badWords;
		foreach (string find in array)
		{
			if (phrase.Contains(find, CompareOptions.IgnoreCase))
			{
				return true;
			}
		}
		array = badWordsExact;
		foreach (string cmp in array)
		{
			if (phrase.EqualsNoCase(cmp))
			{
				return true;
			}
		}
		return false;
	}
}
