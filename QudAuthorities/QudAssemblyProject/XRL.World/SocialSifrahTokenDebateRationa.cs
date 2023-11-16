using System;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenDebateRationally : SifrahToken
{
	public SocialSifrahTokenDebateRationally()
	{
		Description = "debate rationally";
		Tile = "Items/sw_book1.bmp";
		RenderString = "\u0014";
		ColorString = "&w";
		DetailColor = 'W';
	}
}
