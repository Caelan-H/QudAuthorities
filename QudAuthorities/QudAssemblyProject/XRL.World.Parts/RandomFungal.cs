using System;
using Genkit;

namespace XRL.World.Parts;

[Serializable]
public class RandomFungal : IPart
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
		SetupFungal();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneLoaded")
		{
			SetupFungal();
		}
		return base.FireEvent(E);
	}

	private void SetupFungal()
	{
		Render pRender = ParentObject.pRender;
		if (ParentObject.pPhysics.CurrentCell.X > 0 && ParentObject.pPhysics.CurrentCell.X < 79 && ParentObject.pPhysics.CurrentCell.Y > 0 && ParentObject.pPhysics.CurrentCell.Y < 24)
		{
			bool flag = ParentObject.pPhysics.CurrentCell.X % 2 == 0;
			bool num = GetSeededRange(ParentObject.pPhysics.CurrentCell.X / 2 + "," + ParentObject.pPhysics.CurrentCell.Y, 1, 4) == 1;
			string text = ((GetSeededRange(ParentObject.pPhysics.CurrentCell.Y + "," + ParentObject.pPhysics.CurrentCell.X / 2, 1, 3) == 1) ? "a" : "b");
			if (num)
			{
				Cell cellFromDirection = ParentObject.pPhysics.CurrentCell.GetCellFromDirection("E");
				Cell cellFromDirection2 = ParentObject.pPhysics.CurrentCell.GetCellFromDirection("W");
				if (cellFromDirection2 != null && cellFromDirection != null && ((flag && cellFromDirection.HasObjectWithPropertyOrTagEqualToValue("Terrain", "Fungal")) || (!flag && cellFromDirection2.HasObjectWithPropertyOrTagEqualToValue("Terrain", "Fungal"))))
				{
					if (flag)
					{
						pRender.Tile = "Terrain/sw_worldmap_rainbowwood_left_" + text + ".bmp";
					}
					else
					{
						pRender.Tile = "Terrain/sw_worldmap_rainbowwood_right_" + text + ".bmp";
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
