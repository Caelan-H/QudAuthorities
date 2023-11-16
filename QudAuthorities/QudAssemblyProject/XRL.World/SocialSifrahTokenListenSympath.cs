using System;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenListenSympathetically : SifrahToken
{
	public SocialSifrahTokenListenSympathetically()
	{
		Description = "listen sympathetically";
		Tile = "Items/sw_heightenedhearing.bmp";
		RenderString = "@";
		ColorString = "&y";
		DetailColor = 'w';
	}
}
