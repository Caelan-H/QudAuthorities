using XRL.World.Parts;

namespace XRL.World.Biomes;

public static class QudzuSymbioteTemplate
{
	public static void Apply(GameObject GO, Zone Z)
	{
		if (GO.pRender != null && GO != null && GO.HasPart("Body") && !GO.GetBlueprint().DescendsFrom("BaseRobot"))
		{
			GO.AddPart(new QudzuMelee());
			GO.RequirePart<SocialRoles>().RequireRole("{{r|qudzu}} symbiote");
			GO.pRender.ColorString = "&r";
			GO.pRender.TileColor = "&r";
			if (GO.pBrain != null && !Z.GetZoneProperty("relaxedbiomes").EqualsNoCase("true"))
			{
				GO.pBrain.FactionMembership["Vines"] = 100;
			}
		}
	}
}
