using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Biomes;

public static class FungusFriendTemplate
{
	public static void Apply(GameObject GO, string InfectionBlueprint, Zone Z)
	{
		if (GO.pRender != null && GO != null && GO.HasPart("Body"))
		{
			GO.RequirePart<SocialRoles>().RequireRole("friend to fungi");
			if (33.in100())
			{
				FungalSporeInfection.ApplyFungalInfection(GO, InfectionBlueprint);
			}
			if (15.in100())
			{
				FungalSporeInfection.ApplyFungalInfection(GO, InfectionBlueprint);
			}
			if (!Z.GetZoneProperty("relaxedbiomes").EqualsNoCase("true"))
			{
				GO.pBrain.FactionMembership["Fungi"] = 100;
			}
		}
	}
}
