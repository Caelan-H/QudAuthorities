using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Biomes;

public static class SlimespitterTemplate
{
	public static void Apply(GameObject GO)
	{
		if (GO.HasPart("Combat"))
		{
			GO.Slimewalking = true;
			if (GO.pRender != null)
			{
				GO.pRender.DisplayName = "slime-spitting " + GO.pRender.DisplayName;
				GO.pRender.ColorString = "&G^k";
				GO.pRender.TileColor = "&G^k";
			}
			if (!GO.HasPart("SlimeGlands") && GO.HasPart("Mutations") && GO.HasPart("ActivatedAbilities"))
			{
				GO.GetPart<Mutations>().AddMutation(new LiquidSpitter("slime"), 1);
			}
		}
	}
}
