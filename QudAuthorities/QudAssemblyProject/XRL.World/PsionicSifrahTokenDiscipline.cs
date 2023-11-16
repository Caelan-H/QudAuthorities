using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenDiscipline : SifrahToken
{
	public PsionicSifrahTokenDiscipline()
	{
		Description = "draw on reserves of self-discipline";
		Tile = "Items/sw_gem.bmp";
		RenderString = "è";
		ColorString = "&K";
		DetailColor = 'Y';
	}
}
