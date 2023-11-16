using XRL.Rules;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class GritGateWares : BaseMerchantWares
{
	public override void Stock(GameObject GO, string Context = null)
	{
		GO.TakeObject("Lead Slug", Stat.Random(800, 1000), Silent: true, 0);
		GO.TakeObject("Wooden Arrow", Stat.Random(800, 1000), Silent: true, 0);
		GO.TakeObject("Plump Mushroom", Stat.Random(0, 2), Silent: true, 0);
		GO.TakeObject("Pickaxe", Silent: true, 0);
	}
}
