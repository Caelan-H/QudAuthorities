using System;
using System.Linq;

namespace XRL.World.Parts;

[Serializable]
public class DoubleEnclosing : IPart
{
	public string Direction;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "SyncOpened");
		Object.RegisterPartEvent(this, "SyncClosed");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "SyncOpened")
		{
			GameObject gameObject = ParentObject.CurrentCell.GetCellFromDirection(Direction).GetObjectsWithPart("Enclosing").First();
			Enclosing part = gameObject.GetPart<Enclosing>();
			if (part.OpenColor != null)
			{
				gameObject.pRender.ColorString = part.OpenColor;
			}
			if (part.OpenTileColor != null)
			{
				gameObject.pRender.TileColor = part.OpenTileColor;
			}
			if (part.OpenRenderString != null)
			{
				gameObject.pRender.RenderString = part.OpenRenderString;
			}
			if (part.OpenTile != null)
			{
				gameObject.pRender.Tile = part.OpenTile;
			}
			if (part.OpenLayer != int.MinValue)
			{
				gameObject.pRender.RenderLayer = part.OpenLayer;
			}
			return true;
		}
		if (E.ID == "SyncClosed")
		{
			GameObject gameObject2 = ParentObject.CurrentCell.GetCellFromDirection(Direction).GetObjectsWithPart("Enclosing").First();
			Enclosing part2 = gameObject2.GetPart<Enclosing>();
			if (part2.ClosedColor != null)
			{
				gameObject2.pRender.ColorString = part2.ClosedColor;
			}
			if (part2.ClosedTile != null)
			{
				gameObject2.pRender.TileColor = part2.ClosedTile;
			}
			if (part2.ClosedRenderString != null)
			{
				gameObject2.pRender.RenderString = part2.ClosedRenderString;
			}
			if (part2.ClosedTile != null)
			{
				gameObject2.pRender.Tile = part2.ClosedTile;
			}
			if (part2.ClosedLayer != int.MinValue)
			{
				gameObject2.pRender.RenderLayer = part2.ClosedLayer;
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
