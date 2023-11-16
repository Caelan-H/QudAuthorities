namespace XRL.World.Capabilities;

public static class DifficultyEvaluation
{
	public const int IMPOSSIBLE = 15;

	public const int VERY_TOUGH = 10;

	public const int TOUGH = 5;

	public const int AVERAGE = -5;

	public const int EASY = -10;

	public const int TRIVIAL = int.MinValue;

	public static int? GetDifficultyRating(GameObject GO, GameObject who = null, bool IgnoreHideCon = false)
	{
		if (GO == null)
		{
			return -20;
		}
		if (who == null)
		{
			who = The.Player;
			if (who == null)
			{
				return null;
			}
		}
		if (!GO.HasPart("Combat"))
		{
			return null;
		}
		if (!IgnoreHideCon && GO.HasPropertyOrTag("HideCon"))
		{
			return null;
		}
		int num = GO.Stat("Level");
		if (!GO.IsPlayer())
		{
			switch (GO.GetPropertyOrTag("Role"))
			{
			case "Minion":
				num -= 5;
				break;
			case "Skirmisher":
				num -= 3;
				break;
			case "Hero":
				num += 5;
				break;
			}
		}
		int num2 = who.Stat("Level");
		if (!who.IsPlayer())
		{
			switch (who.GetPropertyOrTag("Role"))
			{
			case "Minion":
				num2 -= 5;
				break;
			case "Skirmisher":
				num2 -= 3;
				break;
			case "Hero":
				num2 += 5;
				break;
			}
		}
		return num - num2;
	}

	public static string GetDifficultyDescription(GameObject GO, GameObject who = null, int? Rating = null)
	{
		if (!Rating.HasValue)
		{
			Rating = GetDifficultyRating(GO, who);
			if (!Rating.HasValue)
			{
				return null;
			}
		}
		if (Rating >= 15)
		{
			return "{{R|Impossible}}";
		}
		if (Rating >= 10)
		{
			return "{{r|Very Tough}}";
		}
		if (Rating >= 5)
		{
			return "{{W|Tough}}";
		}
		if (Rating >= -5)
		{
			return "{{w|Average}}";
		}
		if (Rating >= -10)
		{
			return "{{g|Easy}}";
		}
		return "{{G|Trivial}}";
	}

	public static int GetDifficultyFromDescription(string Description)
	{
		switch (Description)
		{
		case "{{R|Impossible}}":
		case "&RImpossible":
		case "Impossible":
			return 15;
		case "{{r|Very Tough}}":
		case "&rVery Tough":
		case "Very Tough":
			return 10;
		case "{{W|Tough}}":
		case "&WTough":
		case "Tough":
			return 5;
		case "{{w|Average}}":
		case "&wAverage":
		case "Average":
			return -5;
		case "{{g|Easy}}":
		case "&gEasy":
		case "Easy":
			return -10;
		default:
			return int.MinValue;
		}
	}
}
