using XRL.World.Parts;

namespace XRL.World.Effects;

public class CookingDomainMedicinal_OnDrinkHoney : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature drink@s honey,";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "DrinkingFrom");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "DrinkingFrom");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DrinkingFrom")
		{
			LiquidVolume liquidVolume = (E.GetParameter("Container") as GameObject).LiquidVolume;
			if (liquidVolume.GetPrimaryLiquid() != null && liquidVolume.IsPureLiquid("honey"))
			{
				int i = 0;
				for (int num = DrinkMagnifier.Magnify(base.Object, 1); i < num; i++)
				{
					Trigger();
				}
			}
		}
		return base.FireEvent(E);
	}
}
