namespace XRL.World.ZoneBuilders;

public static class ChildrenOfTheTombQuestHandler
{
	public const int NACHAM = 1;

	public const int VAAM = 2;

	public const int DAGASHA = 4;

	public const int KAH = 8;

	public static void ChooseNacham()
	{
		The.Game.SetIntGameState("ChoseNacham", 1);
		AchievementManager.IncrementAchievement("ACH_GAVEALL_REPULSIVE_DEVICE", "STAT_GAVE_REPULSIVE_DEVICE_NACHAM");
	}

	public static void ChooseVaam()
	{
		The.Game.SetIntGameState("ChoseVaam", 1);
		AchievementManager.IncrementAchievement("ACH_GAVEALL_REPULSIVE_DEVICE", "STAT_GAVE_REPULSIVE_DEVICE_VAAM");
	}

	public static void ChooseDagasha()
	{
		The.Game.SetIntGameState("ChoseDagasha", 1);
		AchievementManager.IncrementAchievement("ACH_GAVEALL_REPULSIVE_DEVICE", "STAT_GAVE_REPULSIVE_DEVICE_DAGASHA");
	}

	public static void ChooseKah()
	{
		The.Game.SetIntGameState("ChoseKah", 1);
		AchievementManager.IncrementAchievement("ACH_GAVEALL_REPULSIVE_DEVICE", "STAT_GAVE_REPULSIVE_DEVICE_KAH");
	}

	public static void FinishFrayingFavorites()
	{
		The.Game.SetIntGameState("FinishedFrayingFavorites", 1);
	}
}
