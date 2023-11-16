using System;
using Genkit;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class RandomDeepJungle : IPart
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
		SetupDeepJungle();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneLoaded")
		{
			SetupDeepJungle();
		}
		return base.FireEvent(E);
	}

	private void SetupDeepJungle()
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
		if (num < 3)
		{
			pRender.RenderString = "á";
		}
		else if (num < 6)
		{
			pRender.RenderString = "\u009d";
		}
		else if (num < 9)
		{
			pRender.RenderString = "ç";
		}
		else if (num < 11)
		{
			pRender.RenderString = "æ";
		}
		int num2 = Stat.Random(1, 4);
		_ = 1;
		_ = 2;
		_ = 3;
		_ = 4;
		pRender.Tile = "Terrain/sw_deepjungle_" + num2 + ".bmp";
		pRender.DetailColor = (50.in100() ? "G" : "g");
		if (ParentObject.pPhysics.CurrentCell.X > 0 && ParentObject.pPhysics.CurrentCell.X < 79 && ParentObject.pPhysics.CurrentCell.Y > 0 && ParentObject.pPhysics.CurrentCell.Y < 24)
		{
			bool flag = ParentObject.pPhysics.CurrentCell.X % 2 == 0;
			bool num3 = GetSeededRange(ParentObject.pPhysics.CurrentCell.X / 2 + "," + ParentObject.pPhysics.CurrentCell.Y, 1, 4) == 1;
			string text = ((GetSeededRange(ParentObject.pPhysics.CurrentCell.Y + "," + ParentObject.pPhysics.CurrentCell.X / 2, 1, 3) == 1) ? "a" : "b");
			if (num3)
			{
				Cell cellFromDirection = ParentObject.pPhysics.CurrentCell.GetCellFromDirection("E");
				Cell cellFromDirection2 = ParentObject.pPhysics.CurrentCell.GetCellFromDirection("W");
				if (cellFromDirection2 != null && cellFromDirection != null && ((flag && cellFromDirection.HasObjectWithBlueprint("TerrainDeepJungle")) || (!flag && cellFromDirection2.HasObjectWithBlueprint("TerrainDeepJungle"))))
				{
					if (flag)
					{
						pRender.Tile = "Terrain/sw_deepjungle_" + text + "_left.bmp";
					}
					else
					{
						pRender.Tile = "Terrain/sw_deepjungle_" + text + "_right.bmp";
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
