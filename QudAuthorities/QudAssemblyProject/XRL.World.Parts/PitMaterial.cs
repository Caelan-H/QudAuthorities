using System;
using ConsoleLib.Console;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class PitMaterial : IPart
{
	private bool EverPainted;

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
		Object.RegisterPartEvent(this, "FirmPitEdges");
		Object.RegisterPartEvent(this, "ZoneBuilt");
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	private bool isPitAdjacent(Cell ce)
	{
		return ce?.HasObject("Pit") ?? true;
	}

	private void PaintPit()
	{
		ParentObject.RemovePart("StairHighlight");
		if (ParentObject.pPhysics.CurrentCell.AllInDirections(Directions.DirectionList, 1, isPitAdjacent))
		{
			Cell cellFromDirection = ParentObject.pPhysics.CurrentCell.GetCellFromDirection("D", BuiltOnly: false);
			if (cellFromDirection == null)
			{
				return;
			}
			ConsoleChar consoleChar = new ConsoleChar();
			string s = cellFromDirection.Render(consoleChar, Visible: true, LightLevel.Light, Explored: true, bAlt: false);
			ParentObject.pRender.RenderString = ColorUtility.StripFormatting(s);
			ParentObject.pRender.DetailColor = "k";
			ParentObject.pRender.TileColor = "&K";
			ParentObject.pRender.ColorString = "&K";
			ParentObject.pRender.Tile = consoleChar.Tile;
			ParentObject.RemoveStringProperty("PaintedWall");
			ParentObject.RemoveStringProperty("PaintWith");
			ParentObject.SetStringProperty("PaintWith", "PitVoid");
			ParentObject.GetPart<StairsDown>().PullDown = true;
			ParentObject.pRender.DisplayName = "open air";
			ParentObject.RemoveStringProperty("OverrideIArticle");
			ParentObject.pRender.RenderLayer = 7;
		}
		else
		{
			ParentObject.SetStringProperty("PaintedWall", "tile_pit_");
			ParentObject.SetStringProperty("PaintWith", "!PitVoid");
			ParentObject.GetPart<StairsDown>().PullDown = false;
			ParentObject.pRender.DisplayName = "craggy ledge";
			ParentObject.SetStringProperty("OverrideIArticle", "a");
			ParentObject.pRender.RenderString = "ú";
			(ParentObject.GetPart("Description") as Description)._Short = "Ground material splinters and opens onto a void.";
			ParentObject.pRender.RenderLayer = 0;
		}
		EverPainted = true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		FireEvent(Event.New("FirmPitEdges"));
		PaintPit();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "FirmPitEdges")
		{
			if (ParentObject.pPhysics.CurrentCell.AllInDirections(Directions.DirectionList, 1, isPitAdjacent))
			{
				if (ParentObject.pPhysics.CurrentCell.GetCellFromDirection("D", BuiltOnly: false) == null)
				{
					return true;
				}
				ParentObject.GetPart<StairsDown>().PullDown = true;
				ParentObject.pRender.DisplayName = "open air";
				ParentObject.RemoveStringProperty("OverrideIArticle");
				ParentObject.pRender.RenderLayer = 7;
			}
			else
			{
				ParentObject.GetPart<StairsDown>().PullDown = false;
				ParentObject.pRender.DisplayName = "craggy ledge";
				ParentObject.SetStringProperty("OverrideIArticle", "a");
				ParentObject.pRender.RenderString = "ú";
				(ParentObject.GetPart("Description") as Description)._Short = "Ground material splinters and opens onto a void.";
				ParentObject.pRender.RenderLayer = 0;
			}
		}
		if (E.ID == "ZoneBuilt" || E.ID == "EndTurn")
		{
			PaintPit();
		}
		return true;
	}
}
