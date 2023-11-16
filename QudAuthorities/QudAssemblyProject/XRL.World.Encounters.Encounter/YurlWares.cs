using XRL.Rules;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class YurlWares : BaseMerchantWares
{
	public override void Stock(GameObject GO, string Context = null)
	{
		GO.TakeObjectsFromPopulation("Food 2", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 2", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 3", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 4", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 1", Stat.Random(20, 75), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 2", Stat.Random(250, 375), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Ammo 3", Stat.Random(50, 75), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObject("Ripe Cucumber", Stat.Random(3, 5), Silent: true, 0, Context);
		GO.TakeObject("Copper Nugget", Stat.Random(0, 2), Silent: true, 0, Context);
		GO.TakeObject("Canteen", 4, Silent: true, 0, Context);
		GO.TakeObject("EmptyWaterskin", 4, Silent: true, 0, Context);
		GO.TakeObject("Torch", 4, Silent: true, 0, Context);
		GO.TakeObject("Spectacles", Silent: true, 0, 0, 0, Context);
		GO.TakeObject("Kyakukya Recoiler", Silent: true, 0, 0, 0, Context);
	}
}
