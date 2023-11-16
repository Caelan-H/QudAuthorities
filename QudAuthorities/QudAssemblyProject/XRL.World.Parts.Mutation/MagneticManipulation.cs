using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class MagneticManipulation : BaseMutation
{
	public MagneticManipulation()
	{
		DisplayName = "Magnetic Manipulation";
		Type = "Mental";
	}

	public override string GetDescription()
	{
		return "The mutant emits powerful magnetic fields.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("" + "All electrical damage done to the mutant is reduced by " + (30 + Level * 2) + "%", "All electrical damage the mutant deals is increased by ", (30 + Level * 2).ToString(), "%");
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
