using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Biomes;

public static class FirethumbedTemplate
{
	public static void Apply(GameObject GO)
	{
		GO.Slimewalking = true;
		if (GO.pRender != null && GO != null && GO.HasPart("Body") && GO.Body.GetPartCount("Hand") > 0)
		{
			GO.pRender.DisplayName = "firethumbed " + GO.pRender.DisplayName;
			GO.pRender.ColorString = "&R^k";
			GO.pRender.TileColor = "&R^k";
			if (!GO.HasPart("FlamingHands") && GO.HasPart("Mutations") && GO.HasPart("ActivatedAbilities"))
			{
				GO.GetPart<Mutations>().AddMutation(new FlamingHands(), 6);
			}
		}
	}
}
