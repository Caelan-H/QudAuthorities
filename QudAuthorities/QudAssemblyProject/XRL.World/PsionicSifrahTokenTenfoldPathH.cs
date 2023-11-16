using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenTenfoldPathHok : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathHok()
	{
		Description = "draw on the creativity of Hok";
		Tile = "Items/ms_hok.bmp";
		RenderString = ")";
		ColorString = "&y";
		DetailColor = 'Y';
	}
}
[Serializable]
public class PsionicSifrahTokenTenfoldPathHod : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathHod()
	{
		Description = "draw on the majesty of Hod";
		Tile = "Items/ms_hod.bmp";
		RenderString = "(";
		ColorString = "&O";
		DetailColor = 'Y';
	}
}
