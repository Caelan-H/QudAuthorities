using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedTelekinesis : ModImprovedMutationBase<Telekinesis>
{
	public ModImprovedTelekinesis()
	{
	}

	public ModImprovedTelekinesis(int Tier)
		: base(Tier)
	{
		base.Tier = Tier;
	}
}
