using System;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenOfferMaintenanceServices : SifrahToken
{
	public SocialSifrahTokenOfferMaintenanceServices()
	{
		Description = "offer maintenance services";
		Tile = "Items/sw_toolbox.bmp";
		RenderString = "÷";
		ColorString = "&c";
		DetailColor = 'C';
	}
}
