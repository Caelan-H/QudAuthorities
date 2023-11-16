using System;

namespace XRL.World.Parts;

[Serializable]
public class StiltTile : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (ParentObject.CurrentCell == null)
			{
				return true;
			}
			Render pRender = ParentObject.pRender;
			if ((ParentObject.CurrentCell.X + ParentObject.CurrentCell.Y) % 2 == 0)
			{
				pRender.ColorString = "&K";
				pRender.RenderString = "ù";
				pRender.Tile = "terrain/sw_cathedral1.bmp";
				ParentObject.pPhysics.CurrentCell.PaintTile = "terrain/sw_cathedral1.bmp";
				ParentObject.pPhysics.CurrentCell.PaintTileColor = "&y";
			}
			else
			{
				pRender.ColorString = "&K";
				pRender.RenderString = "ú";
				pRender.Tile = "terrain/sw_cathedral2.bmp";
				ParentObject.pPhysics.CurrentCell.PaintTile = "terrain/sw_cathedral2.bmp";
				ParentObject.pPhysics.CurrentCell.PaintTileColor = "&y";
			}
			ParentObject.RemovePart(this);
		}
		return base.FireEvent(E);
	}
}
