using System;

namespace XRL.World;

[Serializable]
public class RitualSifrahTokenSingHymn : SifrahToken
{
	public RitualSifrahTokenSingHymn()
	{
		Description = "sing a hymn";
		Tile = "Items/sw_cherubic.bmp";
		RenderString = "\u000e";
		ColorString = "&Y";
		DetailColor = 'W';
	}
}
