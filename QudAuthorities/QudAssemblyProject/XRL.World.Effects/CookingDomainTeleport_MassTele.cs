using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainTeleport_MassTeleportOther_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they teleport all creatures surrounding @them.";
	}

	public override void Apply(GameObject go)
	{
		if (go.OnWorldMap())
		{
			return;
		}
		foreach (Cell localAdjacentCell in go.pPhysics.CurrentCell.GetLocalAdjacentCells())
		{
			localAdjacentCell.GetFirstObjectWithPart("Combat")?.RandomTeleport(Swirl: true);
		}
	}
}
