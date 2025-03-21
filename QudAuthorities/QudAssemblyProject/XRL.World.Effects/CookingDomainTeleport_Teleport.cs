using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainTeleport_Teleport_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they teleport.";
	}

	public override void Apply(GameObject go)
	{
		if (go.IsPlayer())
		{
			Teleportation.Cast(null, "5-6", null, null, go, automatic: true);
		}
	}
}
