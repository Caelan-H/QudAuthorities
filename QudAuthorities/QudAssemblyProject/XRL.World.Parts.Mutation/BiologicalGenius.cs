using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class BiologicalGenius : BaseMutation
{
	public BiologicalGenius()
	{
		DisplayName = "Biological Genius";
		Type = "Mental";
	}

	public override void Register(GameObject Object)
	{
		base.Register(Object);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		ParentObject.ModIntProperty(Scanning.GetScanPropertyName(Scanning.Scan.Bio), NewLevel - base.Level, RemoveIfZero: true);
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
