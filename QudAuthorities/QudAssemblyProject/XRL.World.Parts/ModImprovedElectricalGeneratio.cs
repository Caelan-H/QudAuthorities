using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedElectricalGeneration : ModImprovedMutationBase<ElectricalGeneration>
{
	public ModImprovedElectricalGeneration()
	{
	}

	public ModImprovedElectricalGeneration(int Tier)
		: base(Tier)
	{
	}
}
