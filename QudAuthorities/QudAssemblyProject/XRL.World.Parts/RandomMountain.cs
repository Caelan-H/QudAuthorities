using System;
using Genkit;
using HistoryKit;

namespace XRL.World.Parts;

[Serializable]
public class RandomMountain : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		SetupMountains();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneLoaded")
		{
			SetupMountains();
		}
		return base.FireEvent(E);
	}

	private void SetupMountains()
	{
		Render pRender = ParentObject.pRender;
		if (!string.Equals(ParentObject.Blueprint, "TerrainMountainsSpindleShadow"))
		{
			pRender.Tile = (If.CoinFlip() ? "Terrain/sw_mountains.bmp" : "Terrain/sw_mountains_2.bmp");
		}
		if (ParentObject.pPhysics.CurrentCell.X > 0 && ParentObject.pPhysics.CurrentCell.X < 79 && ParentObject.pPhysics.CurrentCell.Y > 0 && ParentObject.pPhysics.CurrentCell.Y < 24)
		{
			bool flag = ParentObject.pPhysics.CurrentCell.X % 2 == 0;
			bool num = GetSeededRange(ParentObject.pPhysics.CurrentCell.X / 2 + "," + ParentObject.pPhysics.CurrentCell.Y, 1, 4) == 1;
			int seededRange = GetSeededRange(ParentObject.pPhysics.CurrentCell.Y + "," + ParentObject.pPhysics.CurrentCell.X / 2, 1, 3);
			if (num && !string.Equals(ParentObject.Blueprint, "TerrainMountainsSpindleShadow"))
			{
				Cell cellFromDirection = ParentObject.pPhysics.CurrentCell.GetCellFromDirection("E");
				Cell cellFromDirection2 = ParentObject.pPhysics.CurrentCell.GetCellFromDirection("W");
				if (cellFromDirection2 != null && cellFromDirection != null && ((flag && cellFromDirection.HasObjectWithBlueprint("TerrainMountains")) || (!flag && cellFromDirection2.HasObjectWithBlueprint("TerrainMountains"))))
				{
					if (flag)
					{
						pRender.Tile = "Terrain/tile_mountainleft" + seededRange + ".bmp";
					}
					else
					{
						pRender.Tile = "Terrain/tile_mountainright" + seededRange + ".bmp";
					}
				}
			}
		}
		ParentObject.RemovePart(this);
	}

	public static int GetSeededRange(string Seed, int Low, int High)
	{
		return new Random(Hash.String(Seed)).Next(Low, High);
	}
}
