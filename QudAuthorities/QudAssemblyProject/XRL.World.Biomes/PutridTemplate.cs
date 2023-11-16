using XRL.World.Parts;

namespace XRL.World.Biomes;

public static class PutridTemplate
{
	public static void Apply(GameObject GO)
	{
		GO.SetIntProperty("Putrid", 1);
		if (GO.pRender != null && GO != null && GO.HasPart("Body"))
		{
			GO.pRender.DisplayName = "putrid " + GO.pRender.DisplayName;
			GO.pRender.ColorString = "&g^W";
			GO.AddPart(new VomitOnHit());
		}
		if (GO.HasStat("Hitpoints"))
		{
			GO.Statistics["Hitpoints"].BaseValue *= 2;
		}
		if (GO.HasStat("AV"))
		{
			GO.Statistics["AV"].BaseValue += 3;
		}
		GO.AddPart(new SpawnOnDeath("Bloatfly"));
	}
}
