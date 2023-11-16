using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class UrchinBelcher : Belcher
{
	public UrchinBelcher()
	{
		DisplayName = "Urchin Belching";
		EventKey = "CommandBelchUrchins";
		Description = "You belch forth various urchins.";
		BelchTable = "UrchinsToBelch";
		CommandName = "Belch Urchins";
		CommandDescription = "You belch forth various urchins.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat("You belch urchins in a nearby area.\n" + "Range: " + GetRange(Level) + "\n", "Radius: ", GetRadius().ToString(), "\n"), "Number of urchins: 1d2+", (Level / 4).ToString(), " \n"), "Cooldown: ", GetCooldown(Level).ToString(), " rounds\n");
	}
}
