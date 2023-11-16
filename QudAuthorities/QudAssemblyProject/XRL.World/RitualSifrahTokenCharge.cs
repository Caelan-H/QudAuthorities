using System;

namespace XRL.World;

[Serializable]
public class RitualSifrahTokenCharge : SocialSifrahTokenCharge
{
	public RitualSifrahTokenCharge()
	{
	}

	public RitualSifrahTokenCharge(int Amount)
		: base(Amount)
	{
	}
}
