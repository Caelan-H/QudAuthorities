using XRL.UI;

namespace XRL.World;

public abstract class PsionicSifrah : SifrahGame
{
	public static readonly string CATEGORY = "Psionic";

	public static bool AnyEnabled
	{
		get
		{
			if (!Options.SifrahRealityDistortion)
			{
				return Options.SifrahPsychicCombat;
			}
			return true;
		}
	}

	public PsionicSifrah()
	{
		CorrectTokenSound = "warp_ray";
		CorrectTokenSoundDelay = 400;
		IncorrectTokenSound = "Spark2";
		IncorrectTokenSoundDelay = 300;
	}

	public override string GetSifrahCategory()
	{
		return CATEGORY;
	}

	public static void AwardInsight()
	{
		if (AnyEnabled)
		{
			SifrahGame.AwardInsight(CATEGORY, "psionics");
		}
	}
}
