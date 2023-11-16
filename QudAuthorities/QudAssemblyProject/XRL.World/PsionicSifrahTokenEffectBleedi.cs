using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenEffectBleeding : RitualSifrahTokenEffectBleeding
{
	public PsionicSifrahTokenEffectBleeding()
	{
	}

	public PsionicSifrahTokenEffectBleeding(int Chance)
		: base(Chance)
	{
	}
}
