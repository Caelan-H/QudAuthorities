using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainFungus_FungusReputationUnit : ProceduralCookingEffectUnit
{
	public int Tier;

	private bool bApplied;

	public override string GetDescription()
	{
		return "+300 reputation with fungi";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		if (Object.IsPlayer())
		{
			XRLCore.Core.Game.PlayerReputation.modify("Fungi", 300, null, null, silent: true, transient: true);
			bApplied = true;
		}
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		if (bApplied)
		{
			XRLCore.Core.Game.PlayerReputation.modify("Fungi", -300, null, null, silent: true, transient: true);
		}
	}
}
