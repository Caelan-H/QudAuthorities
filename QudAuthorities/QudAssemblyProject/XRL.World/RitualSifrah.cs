using XRL.UI;

namespace XRL.World;

public abstract class RitualSifrah : SifrahGame
{
	public static readonly string CATEGORY = "Ritual";

	public static bool AnyEnabled => Options.SifrahItemNaming;

	public RitualSifrah()
	{
		CorrectTokenSound = "completion";
		CorrectTokenSoundDelay = 500;
		IncorrectTokenSound = "freeze";
		IncorrectTokenSoundDelay = 200;
	}

	public override string GetSifrahCategory()
	{
		return CATEGORY;
	}

	public static void AwardInsight()
	{
		if (AnyEnabled)
		{
			SifrahGame.AwardInsight(CATEGORY, "ritual");
		}
	}
}
