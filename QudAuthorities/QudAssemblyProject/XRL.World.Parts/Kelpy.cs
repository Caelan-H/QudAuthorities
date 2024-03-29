using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Kelpy : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		try
		{
			if (!Options.DisableFloorTextureObjects)
			{
				Zone parentZone = E.Cell.ParentZone;
				for (int i = 0; i < parentZone.Width; i++)
				{
					for (int j = 0; j < parentZone.Height; j++)
					{
						Paint(parentZone.GetCell(i, j));
					}
				}
			}
		}
		catch
		{
		}
		return base.HandleEvent(E);
	}

	public static void Paint(Cell C)
	{
		int num = Stat.Random(1, 8);
		if (num <= 4)
		{
			C.PaintTile = DirtPicker.GetRandomGrassTile();
		}
		else if (num <= 5)
		{
			C.PaintTile = "Tiles/tile-dirt1.png";
			C.PaintDetailColor = "k";
		}
		else
		{
			C.PaintTile = "Tiles/tile-dirt1.png";
			C.PaintDetailColor = "k";
		}
		num = Stat.Random(1, 4);
		if (num <= 2)
		{
			C.PaintColorString = "&w";
		}
		else if (num <= 4)
		{
			C.PaintColorString = "&W";
		}
		int num2 = Stat.Random(1, 5);
		if (num2 == 1)
		{
			C.PaintRenderString = ",";
		}
		if (num2 == 2)
		{
			C.PaintRenderString = ".";
		}
		if (num2 == 3)
		{
			C.PaintRenderString = "ù";
		}
		if (num2 == 4)
		{
			C.PaintRenderString = "ú";
		}
	}
}
