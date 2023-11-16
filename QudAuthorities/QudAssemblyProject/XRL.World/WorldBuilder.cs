using System;
using XRL.Core;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
public abstract class WorldBuilder
{
	public ZoneManager ZM;

	public XRLGame game => XRLCore.Core.Game;

	public abstract bool BuildWorld(string worldName);

	public void MarkCell(string World, int x, int y, string str)
	{
		Render obj = ZM.GetZone("JoppaWorld").GetCell(x, y).GetObjectInCell(0)
			.GetPart("Render") as Render;
		obj.RenderString = str;
		obj.Tile = null;
	}
}
