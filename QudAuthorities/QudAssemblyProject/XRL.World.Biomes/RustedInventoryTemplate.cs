using XRL.World.Effects;

namespace XRL.World.Biomes;

public static class RustedInventoryTemplate
{
	public static void Apply(GameObject GO)
	{
		if (GO.pRender == null)
		{
			return;
		}
		if (GO != null && GO.HasPart("Body"))
		{
			foreach (GameObject equippedObject in GO.Body.GetEquippedObjects())
			{
				if (50.in100())
				{
					equippedObject.ApplyEffect(new Rusted(1));
				}
			}
		}
		if (GO == null || !GO.HasPart("Inventory"))
		{
			return;
		}
		foreach (GameObject @object in GO.Inventory.GetObjects())
		{
			if (50.in100())
			{
				@object.ApplyEffect(new Rusted(1));
			}
		}
	}
}
