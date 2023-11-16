using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPlant_UnitPlantReputationLowTier : ProceduralCookingEffectUnit
{
	public int Tier;

	private bool bApplied;

	public override string GetDescription()
	{
		return "+100 reputation with flowers, roots, succulents, trees, vines, and the Consortium of Phyta";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		if (Object.IsPlayer())
		{
			The.Game.PlayerReputation.modify("Flowers", 100, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Roots", 100, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Succulents", 100, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Trees", 100, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Vines", 100, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Consortium", 100, null, null, silent: true, transient: true);
			bApplied = true;
		}
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		if (bApplied)
		{
			The.Game.PlayerReputation.modify("Flowers", -100, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Roots", -100, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Succulents", -100, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Trees", -100, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Vines", -100, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Consortium", -100, null, null, silent: true, transient: true);
		}
	}
}
[Serializable]
public class CookingDomainPlant_UnitPlantReputationHighTier : ProceduralCookingEffectUnit
{
	public int Tier;

	private bool bApplied;

	public override string GetDescription()
	{
		return "+200 reputation with flowers, roots, succulents, trees, vines, and the Consortium of Phyta";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		if (Object.IsPlayer())
		{
			The.Game.PlayerReputation.modify("Flowers", 200, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Roots", 200, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Succulents", 200, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Trees", 200, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Vines", 200, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Consortium", 200, null, null, silent: true, transient: true);
			bApplied = true;
		}
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		if (bApplied)
		{
			The.Game.PlayerReputation.modify("Flowers", -200, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Roots", -200, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Succulents", -200, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Trees", -200, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Vines", -200, null, null, silent: true, transient: true);
			The.Game.PlayerReputation.modify("Consortium", -200, null, null, silent: true, transient: true);
		}
	}
}
