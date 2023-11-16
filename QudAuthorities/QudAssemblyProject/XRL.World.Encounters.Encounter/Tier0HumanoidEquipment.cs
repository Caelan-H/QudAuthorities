using XRL.Rules;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class Tier0HumanoidEquipment
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		GO.TakeObjectFromPopulation("Melee Weapons 1", null, Silent: false, 0, 0, 0, Context);
		if (Stat.Random(1, 100) < 75)
		{
			GO.TakeObjectFromPopulation("Armor 1", null, Silent: false, 0, 0, 0, Context);
		}
		if (Stat.Random(1, 100) < 5)
		{
			GO.TakeObjectFromPopulation("Junk 0", null, Silent: false, 0, 0, 0, Context);
		}
		return true;
	}
}
