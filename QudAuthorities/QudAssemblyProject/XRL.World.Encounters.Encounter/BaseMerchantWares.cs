namespace XRL.World.Encounters.EncounterObjectBuilders;

public abstract class BaseMerchantWares
{
	public abstract void Stock(GameObject GO, string Context);

	public virtual void MerchantConfiguration(GameObject GO)
	{
	}

	public bool BuildObject(GameObject GO, string Context = null)
	{
		if (!GO.HasIntProperty("markednonstock"))
		{
			if (GO.pBrain != null)
			{
				GO.pBrain.PerformEquip();
			}
			GO.SetIntProperty("Merchant", 1);
			foreach (GameObject @object in GO.Inventory.Objects)
			{
				if (!@object.HasPropertyOrTag("norestock") && !@object.HasProperty("_stock"))
				{
					@object.SetIntProperty("norestock", 1);
				}
			}
			GO.SetIntProperty("markednonstock", 1);
		}
		Stock(GO, Context);
		foreach (GameObject object2 in GO.Inventory.Objects)
		{
			if (!object2.HasPropertyOrTag("norestock"))
			{
				object2.SetIntProperty("_stock", 1);
			}
		}
		GO.RequirePart<Restocker>().RequireWaresBuilder(GetType().Name);
		MerchantConfiguration(GO);
		return true;
	}
}
