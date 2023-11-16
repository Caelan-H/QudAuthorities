using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenEffectShaken : RitualSifrahTokenEffectShaken
{
	public PsionicSifrahTokenEffectShaken()
	{
	}

	public PsionicSifrahTokenEffectShaken(int Chance)
		: base(Chance)
	{
	}
}
