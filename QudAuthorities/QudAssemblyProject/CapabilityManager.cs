using Rewired;
using UnityEngine;

public class CapabilityManager : MonoBehaviour
{
	public static CapabilityManager instance;

	public static GameManager gameManager => GameManager.Instance;

	public static bool AllowKeyboardHotkeys => ControlManager.activeControllerType != ControllerType.Joystick;

	public static void SuggestOnscreenKeyboard()
	{
		if (SteamManager.SteamDeck)
		{
			SteamManager.ShowOnscreenKeyboardIfNecessary();
		}
	}

	private void Awake()
	{
	}

	public string GetDefaultOptionOverrideForCapabilities(string Option, string Default)
	{
		if (Option == "OptionsPrereleaseInputManager" && SteamManager.SteamDeck)
		{
			return "Yes";
		}
		if (Option == "OptionPrereleaseStageScale" && SteamManager.SteamDeck)
		{
			return "1.25";
		}
		return Default;
	}

	public void Init()
	{
		instance = this;
		_ = SteamManager.SteamDeck;
	}
}
