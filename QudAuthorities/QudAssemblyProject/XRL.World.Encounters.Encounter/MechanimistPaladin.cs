using XRL.World.Parts;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class MechanimistPaladin
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		GameObject gameObject = null;
		gameObject = ((!75.in100()) ? GameObject.create("Cudgel4", 0, 0, Context) : GameObject.create("Cudgel3", 0, 0, Context));
		if (50.in100() && !gameObject.HasPart("ModFreezing"))
		{
			gameObject.AddPart(new ModFreezing(4));
		}
		GO.TakeObject(gameObject, Silent: false, 0);
		if (50.in100())
		{
			GO.TakeObject("Leather Boots", Silent: false, 0);
		}
		else
		{
			GO.TakeObject("Steel Boots", Silent: false, 0);
		}
		return true;
	}
}
