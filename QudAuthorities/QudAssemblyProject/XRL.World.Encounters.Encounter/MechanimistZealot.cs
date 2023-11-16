namespace XRL.World.Encounters.EncounterObjectBuilders;

public class MechanimistZealot
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		if (75.in100())
		{
			GO.TakeObjectFromPopulation("Melee Weapons 3", null, Silent: false, 0, 0, 0, Context);
		}
		else
		{
			GO.TakeObjectFromPopulation("Melee Weapons 4", null, Silent: false, 0, 0, 0, Context);
		}
		return true;
	}
}
