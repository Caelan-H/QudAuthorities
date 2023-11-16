using System;
using ConsoleLib.Console;

namespace XRL.World.Parts;

[Serializable]
public class PaintTest : IPart
{
	public override bool Render(RenderEvent E)
	{
		if (ParentObject.pPhysics.CurrentCell != null)
		{
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		int num = (int)(IComponent<GameObject>.frameTimerMS % 1000 / 200);
		foreach (Cell localAdjacentCell in ParentObject.pPhysics.CurrentCell.GetLocalAdjacentCells(num))
		{
			if (localAdjacentCell.location.Distance(ParentObject.pPhysics.CurrentCell.location) == num)
			{
				buffer.Buffer[localAdjacentCell.X, localAdjacentCell.Y].SetBackground('G');
				buffer.Buffer[localAdjacentCell.X, localAdjacentCell.Y].Detail = ColorUtility.ColorMap['G'];
			}
		}
	}
}
