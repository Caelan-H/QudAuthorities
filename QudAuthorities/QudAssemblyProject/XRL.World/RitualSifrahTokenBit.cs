using System;
using XRL.World.Tinkering;

namespace XRL.World;

[Serializable]
public class RitualSifrahTokenBit : SocialSifrahTokenBit
{
	public RitualSifrahTokenBit()
	{
		Description = "offer bit";
	}

	public RitualSifrahTokenBit(BitType bitType)
		: base(bitType)
	{
		Description = "offer " + bitType.Description;
	}
}
