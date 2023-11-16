using XRL.Rules;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class Tier1HumanoidMissile
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		GO.TakeObject(GameObjectFactory.Factory.CreateObject("Short Bow", 0, 0, null, null, Context), Silent: false, 0);
		for (int i = 0; i < Stat.Random(20, 40); i++)
		{
			GO.TakeObject(GameObjectFactory.Factory.CreateObject("Wooden Arrow", 0, 0, null, null, Context), Silent: false, 0);
		}
		return true;
	}
}
