using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHeat_UnitResist : ProceduralCookingEffectUnit
{
	public int Bonus;

	public override string GetDescription()
	{
		return "+" + Bonus + " Heat Resist";
	}

	public override string GetTemplatedDescription()
	{
		return "+10-15 Heat Resist";
	}

	public override void Init(GameObject target)
	{
		Bonus = Stat.Random(10, 15);
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.Statistics["HeatResistance"].Bonus += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["HeatResistance"].Bonus -= Bonus;
	}
}
