using System;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenPayACompliment : SifrahToken
{
	public SocialSifrahTokenPayACompliment()
	{
		Description = "pay a compliment";
		Tile = "Items/ms_face_heart.png";
		RenderString = "\u0003";
		ColorString = "&M";
		DetailColor = 'W';
	}
}
