using XRL.Rules;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class Tier2Junk
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		for (int i = 0; i < 3; i++)
		{
			if (Stat.Random(1, 100) < 25)
			{
				GO.TakeObjectFromPopulation("Junk 2", null, Silent: false, 0, 0, 0, Context);
			}
		}
		if (Stat.Random(1, 100) < 5)
		{
			GO.TakeObjectFromPopulation("Junk 3", null, Silent: false, 0, 0, 0, Context);
		}
		return true;
	}
}
