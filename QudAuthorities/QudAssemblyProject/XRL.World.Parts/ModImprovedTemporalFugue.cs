using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedTemporalFugue : ModImprovedMutationBase<TemporalFugue>
{
	public ModImprovedTemporalFugue()
	{
	}

	public ModImprovedTemporalFugue(int Tier)
		: base(Tier)
	{
	}
}
