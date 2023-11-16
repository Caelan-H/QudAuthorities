using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedConfusion : ModImprovedMutationBase<Confusion>
{
	public ModImprovedConfusion()
	{
	}

	public ModImprovedConfusion(int Tier)
		: base(Tier)
	{
	}
}
