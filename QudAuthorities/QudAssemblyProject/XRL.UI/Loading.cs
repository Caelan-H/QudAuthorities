using System;
using System.Diagnostics;
using Qud.UI;

namespace XRL.UI;

public static class Loading
{
	private static CleanStack<string> LoadStatuses = new CleanStack<string>();

	public static void LoadTask(string description, Action work, bool showToUser = true)
	{
		Stopwatch stopwatch = new Stopwatch();
		MetricsManager.LogInfo($"{description}");
		if (showToUser)
		{
			LoadStatuses.Push(description);
			SetLoadingStatus(description, waitForUiUpdate: true);
		}
		stopwatch.Start();
		Exception ex = null;
		try
		{
			work();
		}
		catch (Exception ex2)
		{
			ex = ex2;
		}
		stopwatch.Stop();
		if (showToUser)
		{
			LoadStatuses.Pop();
			SetLoadingStatus(LoadStatuses.Peek());
		}
		if (ex != null)
		{
			MetricsManager.LogError($"Done {description} in {stopwatch.ElapsedMilliseconds}ms", ex);
			throw ex;
		}
		MetricsManager.LogInfo($"Done {description} in {stopwatch.ElapsedMilliseconds}ms");
	}

	public static void SetLoadingStatus(string description, bool waitForUiUpdate = false)
	{
		SingletonWindowBase<LoadingStatusWindow>.instance?.SetLoadingStatus(description, waitForUiUpdate);
	}

	public static void SetHideLoadStatus(bool hidden)
	{
		if (!(SingletonWindowBase<LoadingStatusWindow>.instance == null))
		{
			SingletonWindowBase<LoadingStatusWindow>.instance.StayHidden = hidden;
		}
	}
}
