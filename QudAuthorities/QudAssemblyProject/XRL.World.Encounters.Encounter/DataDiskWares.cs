using XRL.Rules;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class DataDiskWares : BaseMerchantWares
{
	public override void Stock(GameObject GO, string Context = null)
	{
		GO.TakeObject("DataDisk", Stat.Random(5, 7), Silent: true, 0, Context);
	}
}
