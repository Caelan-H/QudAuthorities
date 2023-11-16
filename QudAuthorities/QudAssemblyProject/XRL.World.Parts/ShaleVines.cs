using System;

namespace XRL.World.Parts;

[Serializable]
public class ShaleVines : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ZoneBuilt");
		base.Register(Object);
	}

	public void GrowVines()
	{
		if (ParentObject.CurrentCell != null && ParentObject.CurrentCell.AnyAdjacentCell((Cell c) => c.HasObjectWithPart("LiquidVolume") && c.HasOpenLiquidVolume() && c.GetOpenLiquidVolume().IsWaterPuddle() && c.HasWadingDepthLiquid()))
		{
			ParentObject.SetStringProperty("PaintedWall", "vineshale1");
			ParentObject.SetStringProperty("PaintedWallExtension", ".png");
			ParentObject.pRender.ColorString = "&r^g";
			ParentObject.pRender.TileColor = "&r";
			ParentObject.pRender.DetailColor = "g";
		}
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		GrowVines();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneBuilt")
		{
			GrowVines();
		}
		return true;
	}
}
