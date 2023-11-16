using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class FlipTileByAdjacentWall : IPart
{
	public string WallCheckDirection = "W";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ZoneBuiltEvent.ID)
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public void checkTile()
	{
		if (ParentObject.pRender.Tile == null || ParentObject.pRender.Tile.Contains("_flipped"))
		{
			return;
		}
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return;
		}
		Cell cellFromDirection = cell.GetCellFromDirection(WallCheckDirection);
		Cell cellFromDirection2 = cell.GetCellFromDirection(Directions.GetOppositeDirection(WallCheckDirection));
		if (cellFromDirection != null && cellFromDirection2 != null && cellFromDirection.HasWall() && !cellFromDirection2.HasWall())
		{
			string text = "";
			if (ParentObject.pRender.Tile.Contains("."))
			{
				text = ParentObject.pRender.Tile.Substring(ParentObject.pRender.Tile.LastIndexOf("."));
				ParentObject.pRender.Tile = ParentObject.pRender.Tile.Substring(0, ParentObject.pRender.Tile.LastIndexOf("."));
			}
			ParentObject.pRender.Tile += "_flipped";
			ParentObject.pRender.Tile += text;
		}
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		checkTile();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		checkTile();
		return base.HandleEvent(E);
	}
}
