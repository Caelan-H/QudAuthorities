using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.Effects;

public class OnDrinkWaterProceduralCookingTrigger : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature drink@s freshwater, there's a 25% chance";
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
		if (E.ID == "DrinkingFrom" && (E.GetParameter("Container") as GameObject).LiquidVolume.IsFreshWater() && Stat.Random(1, 100) <= 25)
		{
			int i = 0;
			for (int num = DrinkMagnifier.Magnify(base.Object, 1); i < num; i++)
			{
				Trigger();
			}
		}
		return base.FireEvent(E);
	}
}
