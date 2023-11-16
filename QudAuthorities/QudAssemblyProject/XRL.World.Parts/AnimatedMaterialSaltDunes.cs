using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialSaltDunes : IPart
{
	public int nFrameOffset;

	public AnimatedMaterialSaltDunes()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 60);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		if (ParentObject.pPhysics == null || ParentObject.pRender == null || ParentObject.pPhysics.CurrentCell == null)
		{
			return true;
		}
		if ((XRLCore.CurrentFrame + nFrameOffset) % 60 % 30 == 0 && (ParentObject.pRender.RenderString == "÷" || ParentObject.pRender.RenderString == "~") && (ParentObject.pRender.Tile == "Terrain/sw_desert.bmp" || ParentObject.pRender.Tile == "Terrain/sw_plains.bmp") && Stat.RandomCosmetic(1, 20) == 1)
		{
			Cell cell = ParentObject.pPhysics.CurrentCell;
			Zone parentZone = cell.ParentZone;
			string direction = "W";
			if (Stat.RandomCosmetic(1, 2) == 1)
			{
				direction = "SW";
			}
			Cell cellFromDirection = cell.GetCellFromDirection(direction);
			if (cell.X != 0 && cell.Y != parentZone.Height - 1 && cellFromDirection != null && cellFromDirection.Objects.Count > 0 && cellFromDirection.Objects[0].GetPart("AnimatedMaterialSaltDunes") != null)
			{
				Render render = cellFromDirection.Objects[0].GetPart("Render") as Render;
				ParentObject.pRender.RenderString = render.RenderString;
				ParentObject.pRender.Tile = render.Tile;
			}
			else if (Stat.RandomCosmetic(1, 2) == 1)
			{
				ParentObject.pRender.RenderString = "÷";
			}
			else
			{
				ParentObject.pRender.RenderString = "~";
			}
			if ((!(ParentObject.pRender.RenderString == "÷") && !(ParentObject.pRender.RenderString == "~")) || (!(ParentObject.pRender.Tile == "Terrain/sw_desert.bmp") && !(ParentObject.pRender.Tile == "Terrain/sw_plains.bmp")))
			{
				ParentObject.pRender.RenderString = "÷";
				ParentObject.pRender.Tile = "Terrain/sw_desert.bmp";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}
}
