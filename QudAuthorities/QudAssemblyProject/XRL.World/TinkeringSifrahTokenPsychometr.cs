using System;

namespace XRL.World;

[Serializable]
public class TinkeringSifrahTokenPsychometry : SifrahToken
{
	public TinkeringSifrahTokenPsychometry()
	{
		Description = "psychometric inspection";
		Tile = "Items/sw_esper.bmp";
		RenderString = "\u00a8";
		ColorString = "&m";
		DetailColor = 'M';
	}

	public TinkeringSifrahTokenPsychometry(string AltDesc)
		: this()
	{
		Description = AltDesc;
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!UsabilityCheckedThisTurn && !ContextObject.pPhysics.UsePsychometry())
		{
			DisabledThisTurn = true;
			return false;
		}
		return true;
	}
}
