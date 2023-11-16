using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.World;

namespace XRL.Names;

public class NameMaker
{
	public static string MakeName(GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, Dictionary<string, string> TitleContext = null, bool FailureOkay = false, bool SpecialFaildown = false)
	{
		return NameStyles.Generate(For, Genotype, Subtype, Species, Culture, Faction, Gender, Mutations, Tag, Special, TitleContext, FailureOkay, SpecialFaildown);
	}

	public static void MakeName(ref string Into, GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, Dictionary<string, string> TitleContext = null, bool SpecialFaildown = false)
	{
		string text = NameStyles.Generate(For, Genotype, Subtype, Species, Culture, Faction, Gender, Mutations, Tag, Special, TitleContext, FailureOkay: true, SpecialFaildown);
		if (!string.IsNullOrEmpty(text))
		{
			Into = text;
		}
	}

	public static string Eater()
	{
		if (If.Chance(50))
		{
			return MakeName(null, null, null, null, "Eater");
		}
		return MakeName(EncountersAPI.GetASampleCreature());
	}

	public static string YdFreeholder()
	{
		return MakeName(EncountersAPI.GetASampleCreature());
	}
}
