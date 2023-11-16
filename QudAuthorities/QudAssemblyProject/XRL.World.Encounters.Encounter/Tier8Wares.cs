using XRL.Rules;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class Tier8Wares : BaseMerchantWares
{
	public override void Stock(GameObject GO, string Context = null)
	{
		GO.TakeObjectsFromPopulation("Food 8", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 6", Stat.Random(7, 9), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 7", Stat.Random(7, 9), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 8", Stat.Random(7, 9), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 1", Stat.Random(20, 75), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 2", Stat.Random(250, 375), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 3", Stat.Random(3, 5), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 4", Stat.Random(3, 5), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 5", Stat.Random(3, 5), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 6", Stat.Random(3, 5), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 7", Stat.Random(3, 5), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 8", Stat.Random(3, 5), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObject("Canteen", 4, Silent: true, 0, Context);
		GO.TakeObject("EmptyWaterskin", 4, Silent: true, 0, Context);
		GO.TakeObject("Torch", 4, Silent: true, 0, Context);
	}
}
