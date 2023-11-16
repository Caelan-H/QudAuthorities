using XRL.World.Effects;

namespace XRL.World.Biomes;

public static class RustedTemplate
{
	public static void Apply(GameObject GO)
	{
		if (GO.pRender != null)
		{
			if (GO != null && GO.HasPart("Body") && GO.GetBlueprint().DescendsFrom("BaseRobot"))
			{
				GO.ApplyEffect(new Rusted(1));
				GO.pRender.ColorString = "&r^k";
				GO.pRender.TileColor = "&r^k";
			}
			if (GO.GetBlueprint().DescendsFrom("BaseItem"))
			{
				GO.ApplyEffect(new Rusted(1));
			}
		}
	}
}
