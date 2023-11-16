using XRL.Liquids;
using XRL.World.Parts;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class AlchemistEquipment
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		foreach (BaseLiquid allLiquid in LiquidVolume.getAllLiquids())
		{
			GameObject gameObject = GameObjectFactory.Factory.CreateObject("Phial", 0, 0, null, null, Context);
			LiquidVolume liquidVolume = gameObject.LiquidVolume;
			liquidVolume.ComponentLiquids.Clear();
			liquidVolume.ComponentLiquids.Add(allLiquid.ID, 1000);
			liquidVolume.Volume = 1;
			liquidVolume.Update();
			GO.TakeObject(gameObject, Silent: false, 0);
		}
		return true;
	}
}
