using System;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenCharge : TinkeringSifrahTokenCharge
{
	public SocialSifrahTokenCharge()
	{
		Description = "offer {{C|1}} charge from an energy cell";
	}

	public SocialSifrahTokenCharge(int Amount)
		: base(Amount)
	{
		Description = "offer {{C|" + Amount + "}} charge from an energy cell";
	}
}
