using XRL.Rules;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class GlowpadOasisWares : BaseMerchantWares
{
	public override void Stock(GameObject GO, string Context = null)
	{
		GO.TakeObjectsFromPopulation("Junk 1", Stat.Random(4, 8), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 2", Stat.Random(4, 8), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 3", Stat.Random(4, 8), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 4", Stat.Random(4, 8), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 5", Stat.Random(2, 4), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObjectsFromPopulation("Junk 6", Stat.Random(1, 2), null, Silent: true, 0, 0, 0, Context);
		GO.TakeObject("Grit Gate Recoiler", Silent: true, 0, 0, 0, Context);
		GO.TakeObject("Joppa Recoiler", Silent: true, 0, 0, 0, Context);
	}

	public override void MerchantConfiguration(GameObject GO)
	{
		GO.RequirePart<Restocker>().RestockFrequency = 16800L;
	}
}
