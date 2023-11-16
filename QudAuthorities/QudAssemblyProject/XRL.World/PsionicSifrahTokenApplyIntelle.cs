using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenApplyIntellect : SifrahToken
{
	public PsionicSifrahTokenApplyIntellect()
	{
		Description = "apply intellect";
		Tile = "Items/ms_intelligence.bmp";
		RenderString = "§";
		ColorString = "&B";
		DetailColor = 'Y';
	}
}
