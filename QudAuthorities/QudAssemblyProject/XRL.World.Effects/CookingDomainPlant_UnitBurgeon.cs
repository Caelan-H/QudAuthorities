using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPlant_UnitBurgeoningLowTier : ProceduralCookingEffectUnitMutation<Burgeoning>
{
	public override void Init(GameObject target)
	{
		base.Init(target);
	}

	public CookingDomainPlant_UnitBurgeoningLowTier()
	{
		AddedTier = "1-2";
		BonusTier = "2-3";
	}
}
[Serializable]
public class CookingDomainPlant_UnitBurgeoningHighTier : ProceduralCookingEffectUnitMutation<Burgeoning>
{
	public override void Init(GameObject target)
	{
		base.Init(target);
	}

	public CookingDomainPlant_UnitBurgeoningHighTier()
	{
		AddedTier = "7-8";
		BonusTier = "5-6";
	}
}
