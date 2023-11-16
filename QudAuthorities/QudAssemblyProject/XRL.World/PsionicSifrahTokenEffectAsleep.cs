using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenEffectAsleep : RitualSifrahTokenEffectAsleep
{
	public PsionicSifrahTokenEffectAsleep()
	{
	}

	public PsionicSifrahTokenEffectAsleep(int Chance)
		: base(Chance)
	{
	}
}
