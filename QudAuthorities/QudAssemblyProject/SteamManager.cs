using System;
using System.Text;
using Steamworks;
using UnityEngine;
using XRL.UI;

public static class SteamManager
{
	public static uint AppID = 333640u;

	public static bool Initialized = false;

	public static bool StoreStats = true;

	public static bool SteamDeck = false;

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	public static void ResetAchievements()
	{
		Debug.Log("Resetting achievements");
		if (Initialized)
		{
			SteamUserStats.ResetAllStats(bAchievementsToo: true);
			StoreStats = true;
		}
	}

	public static void ShowOnscreenKeyboardIfNecessary()
	{
		try
		{
			if (Initialized && Options.GetOption("OptionFloatingKeyboard") == "Yes")
			{
				SteamUtils.ShowFloatingGamepadTextInput(EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine, 0, 0, 80, 1);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Showing onscreen keyboard", x);
		}
	}

	public static bool GetAchievement(string ID)
	{
		if (Initialized && SteamUserStats.GetAchievement(ID, out var pbAchieved))
		{
			return pbAchieved;
		}
		return false;
	}

	public static void SetAchievement(string ID)
	{
		if (Initialized)
		{
			StoreStats |= SteamUserStats.SetAchievement(ID);
		}
	}

	public static bool UpdateAchievement(string ID, ref bool Achieved)
	{
		if (Initialized)
		{
			if (!SteamUserStats.GetAchievement(ID, out var pbAchieved))
			{
				return false;
			}
			if (Achieved && !pbAchieved)
			{
				SteamUserStats.SetAchievement(ID);
				StoreStats = true;
				return false;
			}
			if (!Achieved && pbAchieved)
			{
				Achieved = true;
				return true;
			}
		}
		return false;
	}

	public static int GetStat(string ID)
	{
		if (Initialized && SteamUserStats.GetStat(ID, out int pData))
		{
			return pData;
		}
		return 0;
	}

	public static void SetStat(string ID, int data)
	{
		if (Initialized)
		{
			StoreStats |= SteamUserStats.SetStat(ID, data);
		}
	}

	public static bool UpdateStat(string ID, ref int Data)
	{
		if (Initialized)
		{
			if (!SteamUserStats.GetStat(ID, out int pData))
			{
				return false;
			}
			if (Data > pData)
			{
				SteamUserStats.SetStat(ID, Data);
				StoreStats = true;
				return false;
			}
			if (Data < pData)
			{
				Data = pData;
				return true;
			}
		}
		return false;
	}

	public static void IndicateAchievementProgress(string ID, uint current, uint max)
	{
		if (Initialized)
		{
			SteamUserStats.IndicateAchievementProgress(ID, current, max);
		}
	}

	public static void Awake()
	{
		Initialized = false;
		for (int i = 0; i < Environment.GetCommandLineArgs().Length; i++)
		{
			if (Environment.GetCommandLineArgs()[i].ToUpper() == "STEAM:NO")
			{
				Debug.Log("Steam disabled via STEAM:NO command line argument.");
				return;
			}
		}
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
		}
		Initialized = SteamAPI.Init();
		if (!Initialized)
		{
			Debug.Log("Steam could not be initalized.");
			return;
		}
		SteamDeck = SteamUtils.IsSteamRunningOnSteamDeck();
		Debug.Log("Requesting stats...");
		SteamUserStats.RequestCurrentStats();
	}

	public static void Shutdown()
	{
		if (Initialized)
		{
			Debug.Log("Shutting down Steam...");
			SteamAPI.Shutdown();
		}
	}

	public static void Update()
	{
		if (Initialized)
		{
			LeaderboardManager.Update();
			SteamAPI.RunCallbacks();
			if (StoreStats)
			{
				SteamUserStats.StoreStats();
				StoreStats = false;
			}
		}
	}
}
