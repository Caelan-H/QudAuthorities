using System;

namespace Qud.API;

public static class MuralCategoryHelpers
{
	public static JournalAccomplishment.MuralCategory parseCategory(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return JournalAccomplishment.MuralCategory.DoesSomethingRad;
		}
		try
		{
			return (JournalAccomplishment.MuralCategory)Enum.Parse(typeof(JournalAccomplishment.MuralCategory), value);
		}
		catch
		{
			MetricsManager.LogError("Unknown hagiographic category: " + value);
			return JournalAccomplishment.MuralCategory.DoesSomethingRad;
		}
	}

	public static JournalAccomplishment.MuralWeight parseWeight(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return JournalAccomplishment.MuralWeight.Medium;
		}
		try
		{
			return (JournalAccomplishment.MuralWeight)Enum.Parse(typeof(JournalAccomplishment.MuralWeight), value);
		}
		catch
		{
			MetricsManager.LogError("Unknown hagiographic category: " + value);
			return JournalAccomplishment.MuralWeight.Medium;
		}
	}
}
