using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class TeleportGateBuilder
{
	public string Blueprint;

	public GlobalLocation InboundTarget;

	public GlobalLocation ReturnTarget;

	public string TwinID;

	public bool BuildZone(Zone Z)
	{
		if (!string.IsNullOrEmpty(Blueprint))
		{
			Cell cell = null;
			if (InboundTarget == null || InboundTarget.ZoneID != Z.ZoneID || InboundTarget.CellX == -1 || InboundTarget.CellY == -1)
			{
				cell = Z.GetEmptyCells().GetRandomElement();
				GameObject gameObject = GameObject.findById(TwinID);
				if (gameObject != null && gameObject.GetPart("TeleportGate") is TeleportGate teleportGate)
				{
					if (teleportGate.Target == null)
					{
						teleportGate.Target = new GlobalLocation(cell);
					}
					else
					{
						teleportGate.Target.CellX = cell.X;
						teleportGate.Target.CellY = cell.Y;
					}
				}
			}
			else
			{
				cell = Z.GetCell(InboundTarget.CellX, InboundTarget.CellY);
			}
			Cell cell2 = cell?.GetEmptyConnectedAdjacentCells(1).GetRandomElement() ?? cell?.GetEmptyConnectedAdjacentCells(2).GetRandomElement();
			if (cell2 == null)
			{
				cell2 = Z.GetEmptyCells().GetRandomElement();
			}
			if (cell2 != null)
			{
				GameObject gameObject2 = GameObject.create(Blueprint);
				if (gameObject2 != null)
				{
					if (ReturnTarget != null && gameObject2.GetPart("TeleportGate") is TeleportGate teleportGate2)
					{
						teleportGate2.Target = ReturnTarget;
					}
					cell2.AddObject(gameObject2);
				}
			}
		}
		return true;
	}
}
