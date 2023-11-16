using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Gills : BaseMutation
{
	[Obsolete("save compat")]
	public Guid Placeholder;

	public Gills()
	{
		DisplayName = "Gills";
	}

	public override void Register(GameObject Object)
	{
		base.Register(Object);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}
