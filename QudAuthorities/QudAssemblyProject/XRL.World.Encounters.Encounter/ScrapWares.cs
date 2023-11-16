using XRL.Rules;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class ScrapWares : BaseMerchantWares
{
	public override void Stock(GameObject GO, string Context = null)
	{
		GO.TakeObjectsFromPopulation("Scrap 1", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Scrap 2", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Scrap 3", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Scrap 4", Stat.Random(5, 7), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObject("DataDisk", Stat.Random(5, 7), Silent: true, 0, Context);
		GO.TakeObject("Waterskin", Stat.Random(3, 5), Silent: true, 0, Context);
	}
}
