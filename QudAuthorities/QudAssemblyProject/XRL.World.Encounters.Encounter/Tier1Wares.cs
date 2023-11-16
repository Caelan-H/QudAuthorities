using XRL.Rules;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class Tier1Wares : BaseMerchantWares
{
	public override void Stock(GameObject GO, string Context = null)
	{
		GO.TakeObjectsFromPopulation("Food 1", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 1", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 2", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 3", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 1", Stat.Random(50, 75), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 2", Stat.Random(50, 75), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObject("Copper Nugget", Stat.Random(0, 2), Silent: true, 0, Context);
		GO.TakeObject("Canteen", 4, Silent: true, 0, Context);
		GO.TakeObject("EmptyWaterskin", 4, Silent: true, 0, Context);
		GO.TakeObject("Torch", 4, Silent: true, 0, Context);
		GO.TakeObject("Wooden Buckler", Silent: true, 0, 0, 0, Context);
		GO.TakeObject("Musket", Silent: true, 0, 0, 0, Context);
		GO.TakeObject("Dagger", Silent: true, 0, 0, 0, Context);
		GO.TakeObject("Mace2", Silent: true, 0, 0, 0, Context);
		GO.TakeObject("Hand Axe", Silent: true, 0, 0, 0, Context);
		GO.TakeObject("Long Sword", Silent: true, 0, 0, 0, Context);
		GO.TakeObject("Leather Armor", Silent: true, 0, 0, 0, Context);
		if (25.in100())
		{
			GO.TakeObject("Spectacles", Silent: true, 0, 0, 0, Context);
		}
	}
}
