using System;

namespace XRL.World.Parts;

[Serializable]
public class CatacombsExitTeleporter : ITeleporter
{
	public string targetZones = "JoppaWorld.53.3.2.0.11,JoppaWorld.53.3.2.2.11";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetAdjacentNavigationWeightEvent.ID && ID != GetNavigationWeightEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		E.MinWeight(60);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(2);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object != ParentObject && E.Object.IsCombatObject())
		{
			string randomElement = targetZones.CachedCommaExpansion().GetRandomElement();
			GameObject firstObjectWithPart = ZoneManager.instance.GetZone(randomElement).GetFirstObjectWithPart("StairsUp");
			if (firstObjectWithPart != null)
			{
				Cell connectedSpawnLocation = firstObjectWithPart.CurrentCell.GetConnectedSpawnLocation();
				E.Object.TeleportTo(connectedSpawnLocation, 0);
				if (E.Object.IsPlayer() && E.Object.CurrentCell == connectedSpawnLocation)
				{
					connectedSpawnLocation.ParentZone.SetActive();
				}
				E.Object.TeleportSwirl();
			}
		}
		return base.HandleEvent(E);
	}
}
