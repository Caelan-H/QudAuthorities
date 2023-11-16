using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedLightManipulation : ModImprovedMutationBase<LightManipulation>
{
	public ModImprovedLightManipulation()
	{
	}

	public ModImprovedLightManipulation(int Tier)
		: base(Tier)
	{
	}
}
