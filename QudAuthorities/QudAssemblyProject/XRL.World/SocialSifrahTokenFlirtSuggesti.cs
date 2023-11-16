using System;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenFlirtSuggestively : SifrahToken
{
	public SocialSifrahTokenFlirtSuggestively()
	{
		Description = "flirt suggestively";
		Tile = "Items/sw_banana.bmp";
		RenderString = "õ";
		ColorString = "&W";
		DetailColor = 'Y';
	}
}
