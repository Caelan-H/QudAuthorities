using System;
using XRL.World.Tinkering;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenBit : TinkeringSifrahTokenBit
{
	public SocialSifrahTokenBit()
	{
		Description = "gift bit";
	}

	public SocialSifrahTokenBit(BitType bitType)
		: base(bitType)
	{
		Description = "gift " + bitType.Description;
	}
}
