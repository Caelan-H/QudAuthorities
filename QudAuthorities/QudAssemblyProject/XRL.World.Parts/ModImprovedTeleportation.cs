using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedTeleportation : ModImprovedMutationBase<Teleportation>
{
	public ModImprovedTeleportation()
	{
	}

	public ModImprovedTeleportation(int Tier)
		: base(Tier)
	{
	}
}
