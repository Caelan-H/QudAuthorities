using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Biomes;

public static class FungusColonizedTemplate
{
	public static void Apply(GameObject GO, string InfectionBlueprint, Zone Z)
	{
		if (GO.pRender == null || GO == null || !GO.HasPart("Body"))
		{
			return;
		}
		GO.RequirePart<DisplayNameAdjectives>().RequireAdjective("fungus-ridden");
		FungalSporeInfection.ApplyFungalInfection(GO, InfectionBlueprint);
		FungalSporeInfection.ApplyFungalInfection(GO, InfectionBlueprint);
		for (int num = Stat.Random(1, 4); num > 0; num--)
		{
			FungalSporeInfection.ApplyFungalInfection(GO, InfectionBlueprint);
		}
		if (!Z.GetZoneProperty("relaxedbiomes").EqualsNoCase("true"))
		{
			if (!GO.pBrain.FactionMembership.ContainsKey("Fungi"))
			{
				GO.pBrain.FactionMembership.Add("Fungi", 100);
			}
			else
			{
				GO.pBrain.FactionMembership["Fungi"] = 100;
			}
		}
	}
}
