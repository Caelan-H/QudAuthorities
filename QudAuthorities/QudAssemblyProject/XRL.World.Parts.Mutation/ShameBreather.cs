using System;
using ConsoleLib.Console;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ShameBreather : BreatherBase
{
	public ShameBreather()
	{
		DisplayName = "Shame Breath";
	}

	public override string GetCommandDisplayName()
	{
		return "Breathe Shame Gas";
	}

	public override string GetDescription()
	{
		return "You breathe shame gas.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("Breathes shame gas in a cone.\n" + "Cone length: " + GetConeLength() + " tiles\n", "Cone angle: ", GetConeAngle().ToString(), " degrees\n"), "Cooldown: 15 rounds\n");
	}

	public override string GetGasBlueprint()
	{
		return "ShameGas80";
	}

	public override void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		DrawBreathInCell(C, Buffer, "B", "b", "C");
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyShamed");
		Object.RegisterPartEvent(this, "ApplyShameGas");
		Object.RegisterPartEvent(this, "CanApplyShamed");
		Object.RegisterPartEvent(this, "CanApplyShameGas");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyShamed" || E.ID == "CanApplyShameGas" || E.ID == "ApplyShamed" || E.ID == "ApplyShameGas")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
