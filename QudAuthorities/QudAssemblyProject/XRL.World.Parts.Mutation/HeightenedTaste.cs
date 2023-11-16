using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class HeightenedTaste : BaseMutation
{
	public HeightenedTaste()
	{
		DisplayName = "Heightened Taste";
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
