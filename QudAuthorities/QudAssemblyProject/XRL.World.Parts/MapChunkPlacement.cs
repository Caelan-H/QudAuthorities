using System;
using Genkit;
using XRL.EditorFormats.Map;

namespace XRL.World.Parts;

[Serializable]
public class MapChunkPlacement : IPart
{
	public string Map;

	public int Width;

	public int Height;

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
			MapFile mapFile = MapFile.LoadWithMods(Map);
			Zone parentZone = ParentObject.GetCurrentCell().ParentZone;
			Point2D pos2D = ParentObject.GetCurrentCell().Pos2D;
			int num = 2;
			if (HasTag("ChunkPadding"))
			{
				num = Convert.ToInt32("ChunkPadding");
			}
			if (pos2D.x + Width + num >= parentZone.Width)
			{
				pos2D.x -= pos2D.x + Width + num - parentZone.Width;
			}
			if (pos2D.y + Height + num >= parentZone.Height)
			{
				pos2D.y -= pos2D.y + Height + num - parentZone.Height;
			}
			if (HasTag("ChunkHint") && GetTag("ChunkHint", "") == "Center")
			{
				pos2D.x = parentZone.Width / 2 - Width / 2;
				pos2D.y = parentZone.Height / 2 - Height / 2;
			}
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					Cell cell = parentZone.GetCell(pos2D.x + i, pos2D.y + j);
					if (cell != null)
					{
						mapFile.Cells[i, j].ApplyTo(cell, CheckEmpty: true, delegate(Cell X)
						{
							X.ClearWalls();
						});
					}
				}
			}
		}
		return true;
	}
}
