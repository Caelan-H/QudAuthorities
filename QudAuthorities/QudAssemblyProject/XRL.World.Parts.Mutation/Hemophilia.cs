using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Hemophilia : BaseMutation
{
	public Hemophilia()
	{
		DisplayName = "Hemophilia ({{r|D}})";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyBleeding");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "Your blood does not clot easily.\n\nIt takes much longer than usual for you to stop bleeding.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyBleeding")
		{
			(E.GetParameter("Effect") as Bleeding).SaveTarget += 60;
		}
		return base.FireEvent(E);
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
