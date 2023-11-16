using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class WallWalker : BaseMutation
{
	public WallWalker()
	{
		DisplayName = "Wall Climber";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You can move across walls, and only across walls.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (GO.pBrain != null)
		{
			GO.pBrain.WallWalker = true;
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		if (GO.pBrain != null)
		{
			GO.pBrain.WallWalker = false;
		}
		return base.Unmutate(GO);
	}
}
