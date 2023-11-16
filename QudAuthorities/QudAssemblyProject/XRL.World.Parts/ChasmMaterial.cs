using System;
using ConsoleLib.Console;

namespace XRL.World.Parts;

[Serializable]
public class ChasmMaterial : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" || E.ID == "EndTurn")
		{
			Cell cellFromDirection = ParentObject.pPhysics.CurrentCell.GetCellFromDirection("D", BuiltOnly: false);
			if (cellFromDirection == null)
			{
				return true;
			}
			if (cellFromDirection.ParentZone.ZoneID == ParentObject.pPhysics.CurrentCell.ParentZone.ZoneID)
			{
				ParentObject.pRender.RenderString = " ";
				ParentObject.pRender.DetailColor = "k";
				ParentObject.pRender.TileColor = "&K";
				ParentObject.pRender.ColorString = "&K";
				ParentObject.pRender.Tile = null;
				return true;
			}
			ConsoleChar consoleChar = new ConsoleChar();
			string s = cellFromDirection.Render(consoleChar, Visible: true, LightLevel.Light, Explored: true, bAlt: false);
			ParentObject.pRender.RenderString = ColorUtility.StripFormatting(s);
			ParentObject.pRender.DetailColor = "k";
			ParentObject.pRender.TileColor = "&K";
			ParentObject.pRender.ColorString = "&K";
			ParentObject.pRender.Tile = consoleChar.Tile;
		}
		return true;
	}
}
