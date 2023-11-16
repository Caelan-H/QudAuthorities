using System;

namespace XRL.World;

[Serializable]
public class TinkeringSifrahTokenVisualInspection : SifrahToken
{
	public TinkeringSifrahTokenVisualInspection()
	{
		Description = "visual inspection";
		Tile = "Items/sw_nightvision.bmp";
		RenderString = "\u001d";
		ColorString = "&y";
		DetailColor = 'B';
	}
}
