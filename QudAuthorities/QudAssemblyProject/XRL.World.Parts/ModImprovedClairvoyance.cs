using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedClairvoyance : ModImprovedMutationBase<Clairvoyance>
{
	public ModImprovedClairvoyance()
	{
	}

	public ModImprovedClairvoyance(int Tier)
		: base(Tier)
	{
	}
}
