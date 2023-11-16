using System;

namespace XRL.World.Parts;

[Serializable]
[Obsolete("save compat")]
public class HologramWallMaterial : IPart
{
	[NonSerialized]
	public string Tile;

	[FieldSaveVersion(262)]
	public string ColorStrings = "&C,&b,&c,&B";

	[FieldSaveVersion(262)]
	public string DetailColors = "c,C,b,b";

	public string RenderString = "@";

	public int FlickerFrame;

	public int FrameOffset;

	public override bool Render(RenderEvent E)
	{
		HologramMaterial hologramMaterial = ParentObject.RequirePart<HologramMaterial>();
		hologramMaterial.ColorStrings = ColorStrings;
		hologramMaterial.DetailColors = DetailColors;
		ParentObject.RemovePart(this);
		return true;
	}
}
