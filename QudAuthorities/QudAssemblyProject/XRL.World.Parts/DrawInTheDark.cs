using System;
using ConsoleLib.Console;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class DrawInTheDark : IPart
{
	public string ForegroundTileColor = "";

	public string BackgroundTileColor = "";

	public string DetailColor = "";

	public string BackgroundColor = "";

	public string ForegroundColor = "";

	public override bool Render(RenderEvent E)
	{
		if (ParentObject.pPhysics.CurrentCell != null && ParentObject.pPhysics.currentCell.IsExplored() && !ParentObject.pPhysics.currentCell.IsVisible())
		{
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (ParentObject.pPhysics.currentCell == null || !ParentObject.pPhysics.currentCell.IsExplored() || ParentObject.pPhysics.currentCell.IsVisible())
		{
			return;
		}
		int x = ParentObject.pPhysics.currentCell.X;
		int y = ParentObject.pPhysics.currentCell.Y;
		if (Options.UseTiles && !string.IsNullOrEmpty(ParentObject.pRender.Tile))
		{
			if (!string.IsNullOrEmpty(BackgroundTileColor))
			{
				buffer.Buffer[x, y].SetBackground(BackgroundTileColor[0]);
			}
			else if (!string.IsNullOrEmpty(BackgroundColor))
			{
				buffer.Buffer[x, y].SetBackground(BackgroundColor[0]);
			}
			if (!string.IsNullOrEmpty(ForegroundTileColor))
			{
				buffer.Buffer[x, y].SetForeground(ForegroundTileColor[0]);
			}
			else if (!string.IsNullOrEmpty(ForegroundColor))
			{
				buffer.Buffer[x, y].SetForeground(ForegroundColor[0]);
			}
			if (!string.IsNullOrEmpty(DetailColor))
			{
				buffer.Buffer[x, y].Detail = ColorUtility.ColorMap[DetailColor[0]];
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(BackgroundColor))
			{
				buffer.Buffer[x, y].SetBackground(BackgroundColor[0]);
			}
			if (!string.IsNullOrEmpty(ForegroundColor))
			{
				buffer.Buffer[x, y].SetForeground(ForegroundColor[0]);
			}
		}
	}
}
