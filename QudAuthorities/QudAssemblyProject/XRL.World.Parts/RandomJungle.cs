using System;
using Genkit;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class RandomJungle : IPart
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
		SetupJungle();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneLoaded")
		{
			SetupJungle();
		}
		return base.FireEvent(E);
	}

	private void SetupJungle()
	{
		Render pRender = ParentObject.pRender;
		if (Stat.Random(1, 10) == 1)
		{
			pRender.ColorString = "&G";
		}
		else
		{
			pRender.ColorString = "&g";
		}
		int num = Stat.Random(1, 10);
		if (num == 1)
		{
			pRender.RenderString = "\u0005";
		}
		else if (num < 5)
		{
			pRender.RenderString = "\u009d";
		}
		else if (num < 8)
		{
			pRender.RenderString = "ç";
		}
		else if (num < 11)
		{
			pRender.RenderString = "æ";
		}
		string text = "";
		int num2 = Stat.Random(1, 3);
		if (num2 == 1)
		{
			text = "a";
		}
		if (num2 == 2)
		{
			text = "b";
		}
		if (num2 == 3)
		{
			text = "c";
		}
		pRender.Tile = "terrain/tile_jungle" + Stat.Random(1, 3) + text + ".png";
		pRender.DetailColor = "G";
		if (ParentObject.pPhysics.CurrentCell.X > 0 && ParentObject.pPhysics.CurrentCell.X < 79 && ParentObject.pPhysics.CurrentCell.Y > 0 && ParentObject.pPhysics.CurrentCell.Y < 24)
		{
			bool flag = ParentObject.pPhysics.CurrentCell.X % 2 == 0;
			bool num3 = GetSeededRange(ParentObject.pPhysics.CurrentCell.X / 2 + "," + ParentObject.pPhysics.CurrentCell.Y, 1, 4) == 1;
			int seededRange = GetSeededRange(ParentObject.pPhysics.CurrentCell.Y + "," + ParentObject.pPhysics.CurrentCell.X / 2, 1, 3);
			if (num3)
			{
				Cell cellFromDirection = ParentObject.pPhysics.CurrentCell.GetCellFromDirection("E");
				Cell cellFromDirection2 = ParentObject.pPhysics.CurrentCell.GetCellFromDirection("W");
				if (cellFromDirection2 != null && cellFromDirection != null && ((flag && cellFromDirection.HasObjectWithBlueprint("TerrainJungle")) || (!flag && cellFromDirection2.HasObjectWithBlueprint("TerrainJungle"))))
				{
					if (flag)
					{
						pRender.Tile = "terrain/tile_jungleleft" + seededRange + text + ".png";
					}
					else
					{
						pRender.Tile = "terrain/tile_jungleright" + seededRange + text + ".png";
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
