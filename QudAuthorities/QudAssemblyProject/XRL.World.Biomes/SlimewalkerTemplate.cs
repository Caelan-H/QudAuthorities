namespace XRL.World.Biomes;

public static class SlimewalkerTemplate
{
	public static void Apply(GameObject GO)
	{
		GO.Slimewalking = true;
		if (GO.pRender != null && GO != null && GO.HasPart("Body"))
		{
			if (GO.Body.GetPartCount("Foot") > 0 || GO.Body.GetPartCount("Feet") > 0)
			{
				GO.pRender.DisplayName = "web-toed " + GO.pRender.DisplayName;
			}
			else
			{
				GO.pRender.DisplayName = "slimy-finned " + GO.pRender.DisplayName;
			}
			GO.pRender.ColorString = "&g^k";
			GO.pRender.TileColor = "&g^k";
		}
	}
}
