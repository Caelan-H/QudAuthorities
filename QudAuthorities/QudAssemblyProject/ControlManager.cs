using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConsoleLib.Console;
using Rewired;
using TMPro;
using UnityEngine;
using XRL;
using XRL.UI;

[ExecuteAlways]
public class ControlManager : MonoBehaviour
{
	public class ControlInputContext
	{
		public Func<bool> isInInput = isFalse;

		private static bool isFalse()
		{
			return false;
		}
	}

	[Serializable]
	public enum ControllerFontType
	{
		XBox,
		PS5,
		Switch,
		Keyboard,
		Default
	}

	[Serializable]
	public struct ControllerFontSelection
	{
		public ControllerFontType controllerType;

		public TMP_FontAsset fontAsset;
	}

	public interface IControllerChangedEvent
	{
		void ControllerChanged();
	}

	public static class Keybinds
	{
		public static readonly string ACCEPT = "accept";
	}

	public TMP_FontAsset keyboardGlyphFont;

	public List<ControllerFontSelection> controllerFonts = new List<ControllerFontSelection>();

	public ControllerFontType controllerFontType = ControllerFontType.Default;

	private ControllerFontType? lastControllerFontType;

	private ControllerType? lastControllerType;

	public static ControlInputContext currentContext = new ControlInputContext();

	public static ControlManager instance;

	private static List<string> EnabledLayers = new List<string>();

	private static HashSet<string> EnabledLayersCheck = new HashSet<string>();

	public static Dictionary<string, string> _mapRewiredIDToLegacyID = new Dictionary<string, string>
	{
		{ "Accept", "CmdUse" },
		{ "Cancel", "CmdCancel" },
		{ "Move Northwest", "CmdMoveNW" },
		{ "Move North", "CmdMoveN" },
		{ "Navigate Up", "CmdMoveN" },
		{ "Move Northeast", "CmdMoveNE" },
		{ "Move East", "CmdMoveE" },
		{ "Navigate Right", "CmdMoveE" },
		{ "Move Southeast", "CmdMoveSE" },
		{ "Move South", "CmdMoveS" },
		{ "Navigate Down", "CmdMoveS" },
		{ "Move Southwest", "CmdMoveSW" },
		{ "Move West", "CmdMoveW" },
		{ "Navigate Left", "CmdMoveW" },
		{ "Toggle Message Log", "CmdShowSidebar" }
	};

	public static Dictionary<int, UnityEngine.KeyCode> _consoleKeycodeToUnityKeycode = null;

	private static Dictionary<int, string> _ControllerMapCategoryCache = new Dictionary<int, string>();

	public static string frameString = null;

	public static List<UnityEngine.KeyCode> submitKeys = new List<UnityEngine.KeyCode>
	{
		UnityEngine.KeyCode.KeypadEnter,
		UnityEngine.KeyCode.Return,
		UnityEngine.KeyCode.Space
	};

	public static List<UnityEngine.KeyCode> keysIgnoredWhileInAnInputField = new List<UnityEngine.KeyCode>
	{
		UnityEngine.KeyCode.Backspace,
		UnityEngine.KeyCode.Keypad0,
		UnityEngine.KeyCode.Keypad1,
		UnityEngine.KeyCode.Keypad2,
		UnityEngine.KeyCode.Keypad3,
		UnityEngine.KeyCode.Keypad4,
		UnityEngine.KeyCode.Keypad5,
		UnityEngine.KeyCode.Keypad6,
		UnityEngine.KeyCode.Keypad7,
		UnityEngine.KeyCode.Keypad8,
		UnityEngine.KeyCode.Keypad9,
		UnityEngine.KeyCode.Alpha0,
		UnityEngine.KeyCode.Alpha1,
		UnityEngine.KeyCode.Alpha2,
		UnityEngine.KeyCode.Alpha3,
		UnityEngine.KeyCode.Alpha4,
		UnityEngine.KeyCode.Alpha5,
		UnityEngine.KeyCode.Alpha6,
		UnityEngine.KeyCode.Alpha7,
		UnityEngine.KeyCode.Alpha8,
		UnityEngine.KeyCode.Alpha9,
		UnityEngine.KeyCode.A,
		UnityEngine.KeyCode.B,
		UnityEngine.KeyCode.C,
		UnityEngine.KeyCode.D,
		UnityEngine.KeyCode.E,
		UnityEngine.KeyCode.F,
		UnityEngine.KeyCode.G,
		UnityEngine.KeyCode.H,
		UnityEngine.KeyCode.I,
		UnityEngine.KeyCode.J,
		UnityEngine.KeyCode.K,
		UnityEngine.KeyCode.L,
		UnityEngine.KeyCode.M,
		UnityEngine.KeyCode.N,
		UnityEngine.KeyCode.O,
		UnityEngine.KeyCode.P,
		UnityEngine.KeyCode.Q,
		UnityEngine.KeyCode.R,
		UnityEngine.KeyCode.S,
		UnityEngine.KeyCode.T,
		UnityEngine.KeyCode.U,
		UnityEngine.KeyCode.V,
		UnityEngine.KeyCode.W,
		UnityEngine.KeyCode.X,
		UnityEngine.KeyCode.Y,
		UnityEngine.KeyCode.Z,
		UnityEngine.KeyCode.Space,
		UnityEngine.KeyCode.Comma,
		UnityEngine.KeyCode.Colon,
		UnityEngine.KeyCode.Semicolon,
		UnityEngine.KeyCode.LeftArrow,
		UnityEngine.KeyCode.RightArrow,
		UnityEngine.KeyCode.Slash,
		UnityEngine.KeyCode.Quote,
		UnityEngine.KeyCode.DoubleQuote,
		UnityEngine.KeyCode.LeftBracket,
		UnityEngine.KeyCode.RightBracket
	};

	public static Player player = null;

	public static float delaytime = 0.5f;

	public static float repeattime = 0.1f;

	public static Dictionary<UnityEngine.KeyCode, float> delayTimers = new Dictionary<UnityEngine.KeyCode, float>();

	public static Dictionary<UnityEngine.KeyCode, float> repeatTimers = new Dictionary<UnityEngine.KeyCode, float>();

	public static bool mappingMode = false;

	private static Dictionary<string, string> controllerGlyphs = new Dictionary<string, string>
	{
		{ "Cross", "\ue900" },
		{ "A", "\ue900" },
		{ "Button 0", "\ue900" },
		{ "Circle", "\ue901" },
		{ "B", "\ue901" },
		{ "Button 1", "\ue901" },
		{ "Square", "\ue902" },
		{ "X", "\ue902" },
		{ "Button 2", "\ue902" },
		{ "Triangle", "\ue903" },
		{ "Y", "\ue903" },
		{ "Button 3", "\ue903" },
		{ "Left Shoulder", "\ue915" },
		{ "L1", "\ue915" },
		{ "LB", "\ue915" },
		{ "Right Shoulder", "\ue917" },
		{ "R1", "\ue917" },
		{ "RB", "\ue917" },
		{ "Left Trigger", "\ue916" },
		{ "L2", "\ue916" },
		{ "LT", "\ue916" },
		{ "Right Trigger", "\ue918" },
		{ "R2", "\ue918" },
		{ "RT", "\ue918" },
		{ "D-Pad Up", "\ue90b" },
		{ "D-Pad Down", "\ue90d" },
		{ "D-Pad Left", "\ue90e" },
		{ "D-Pad Right", "\ue90c" },
		{ "Start", "\ue91a" },
		{ "Options", "\ue91a" },
		{ "Select", "\ue919" },
		{ "Back", "\ue919" },
		{ "Create", "\ue919" },
		{ "Left Stick X", "\ue908\ue906" },
		{ "Left Stick Y", "\ue905\ue907" },
		{ "Right Stick X", "\ue913\ue911" },
		{ "Right Stick Y", "\ue910\ue912" }
	};

	public static GameManager gameManager => GameManager.Instance;

	public static Dictionary<int, UnityEngine.KeyCode> consoleKeycodeToUnityKeycode
	{
		get
		{
			if (_consoleKeycodeToUnityKeycode == null)
			{
				ConsoleLib.Console.Keyboard.InitKeymap();
				_consoleKeycodeToUnityKeycode = new Dictionary<int, UnityEngine.KeyCode>();
				foreach (KeyValuePair<UnityEngine.KeyCode, Keys> item in ConsoleLib.Console.Keyboard.Keymap)
				{
					if (!_consoleKeycodeToUnityKeycode.ContainsKey((int)item.Value))
					{
						_consoleKeycodeToUnityKeycode.Add((int)item.Value, item.Key);
					}
				}
			}
			return _consoleKeycodeToUnityKeycode;
		}
	}

	public static bool PrereleaseInput => GameManager.Instance?.PrereleaseInput ?? false;

	public static ControllerType activeControllerType
	{
		get
		{
			if (SteamManager.SteamDeck)
			{
				return ControllerType.Joystick;
			}
			if (!Options.PrereleaseInputManager)
			{
				return ControllerType.Keyboard;
			}
			if ((((int?)GameManager.Instance?.player?.controllers?.GetLastActiveController()?.type) ?? (SteamManager.SteamDeck ? 2 : 0)) == 2)
			{
				return ControllerType.Joystick;
			}
			return ControllerType.Keyboard;
		}
	}

	public void Update()
	{
		bool flag = false;
		if (Application.isPlaying && activeControllerType != lastControllerType)
		{
			if (activeControllerType == ControllerType.Keyboard)
			{
				controllerFontType = ControllerFontType.Keyboard;
			}
			else
			{
				controllerFontType = ControllerFontType.XBox;
			}
			lastControllerType = activeControllerType;
			flag = true;
		}
		if (controllerFontType != lastControllerFontType)
		{
			MetricsManager.LogEditorWarning("Changing Controller Font");
			lastControllerFontType = controllerFontType;
			TMP_Settings.fallbackFontAssets.RemoveAll((TMP_FontAsset f) => controllerFonts.Any((ControllerFontSelection cf) => cf.fontAsset == f) || f == keyboardGlyphFont);
			TMP_FontAsset fontAsset = controllerFonts.Find((ControllerFontSelection cf) => cf.controllerType == controllerFontType).fontAsset;
			if ((object)fontAsset != null)
			{
				TMP_Settings.fallbackFontAssets.Add(fontAsset);
			}
			TMP_Settings.fallbackFontAssets.Add(keyboardGlyphFont);
			TMP_Text[] array = UnityEngine.Object.FindObjectsOfType<TMP_Text>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ForceMeshUpdate(ignoreActiveState: false, forceTextReparsing: true);
			}
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		foreach (IControllerChangedEvent item in UnityEngine.Object.FindObjectsOfType(typeof(MonoBehaviour)).OfType<IControllerChangedEvent>())
		{
			if (!(item is MonoBehaviour monoBehaviour) || monoBehaviour.isActiveAndEnabled)
			{
				item.ControllerChanged();
			}
		}
	}

	private void Awake()
	{
		instance = this;
	}

	public void Init()
	{
		player = ReInput.players.GetPlayer(0);
		if (SteamManager.SteamDeck)
		{
			controllerFontType = ControllerFontType.XBox;
			Update();
		}
	}

	public static void DisableAllLayers()
	{
		EnabledLayers.Clear();
		EnabledLayersCheck.Clear();
		if (PrereleaseInput)
		{
			player.controllers.maps.SetAllMapsEnabled(state: false);
		}
	}

	public static void EnableLayer(string layer)
	{
		if (!EnabledLayersCheck.Contains(layer))
		{
			EnabledLayersCheck.Add(layer);
			EnabledLayers.Add(layer);
		}
		if (PrereleaseInput)
		{
			player.controllers.maps.SetMapsEnabled(state: true, layer);
		}
	}

	public static void DisableLayer(string layer)
	{
		if (EnabledLayersCheck.Contains(layer))
		{
			EnabledLayersCheck.Remove(layer);
			EnabledLayers.Remove(layer);
		}
		if (PrereleaseInput)
		{
			player.controllers.maps.SetMapsEnabled(state: false, layer);
		}
	}

	public static string mapRewiredIDToLegacyID(string commandId)
	{
		if (_mapRewiredIDToLegacyID.TryGetValue(commandId, out var value))
		{
			return value;
		}
		return commandId;
	}

	public static List<UnityEngine.KeyCode> GetHotkeySpread(IEnumerable<string> layersToInclude = null)
	{
		if (layersToInclude == null)
		{
			layersToInclude = EnabledLayers;
		}
		List<UnityEngine.KeyCode> list = new List<UnityEngine.KeyCode>();
		for (int i = 97; i <= 122; i++)
		{
			if (!isKeyMapped((UnityEngine.KeyCode)i, layersToInclude))
			{
				list.Add((UnityEngine.KeyCode)i);
			}
		}
		for (int j = 48; j <= 57; j++)
		{
			if (!isKeyMapped((UnityEngine.KeyCode)j, layersToInclude))
			{
				list.Add((UnityEngine.KeyCode)j);
			}
		}
		return list;
	}

	public static UnityEngine.KeyCode mapCommandToPrimaryLegacyKeycode(string id)
	{
		string key = mapRewiredIDToLegacyID(id);
		foreach (KeyValuePair<string, Dictionary<string, int>> item in LegacyKeyMapping.CurrentMap.PrimaryMapCommandToKeyLayer)
		{
			if (item.Value.TryGetValue(key, out var value) && consoleKeycodeToUnityKeycode.TryGetValue(value, out var value2))
			{
				return value2;
			}
		}
		return UnityEngine.KeyCode.None;
	}

	public static UnityEngine.KeyCode mapCommandToSecondaryLegacyKeycode(string id)
	{
		string key = mapRewiredIDToLegacyID(id);
		foreach (KeyValuePair<string, Dictionary<string, int>> item in LegacyKeyMapping.CurrentMap.SecondaryMapCommandToKeyLayer)
		{
			if (item.Value.TryGetValue(key, out var value) && consoleKeycodeToUnityKeycode.TryGetValue(value, out var value2))
			{
				return value2;
			}
		}
		return UnityEngine.KeyCode.None;
	}

	public static string GetControllerMapCategory(ControllerMap map)
	{
		try
		{
			string value = null;
			if (_ControllerMapCategoryCache.TryGetValue(map.id, out value))
			{
				return value;
			}
			value = ReInput.mapping.GetActionCategory(map.categoryId).name;
			_ControllerMapCategoryCache[map.categoryId] = value;
			return value;
		}
		catch (Exception x)
		{
			MetricsManager.LogException("GetControllerMapCategory", x);
			return "Default";
		}
	}

	public static bool GetFirstElementAssignmentConflict(Player player, ElementAssignmentConflictCheck conflictCheck, out ElementAssignmentConflictInfo conflict, IEnumerable<string> layersToInclude, bool skipDisabledMaps = true, bool forceCheckAllCategories = true)
	{
		try
		{
			foreach (ElementAssignmentConflictInfo c in player.controllers.conflictChecking.ElementAssignmentConflicts(conflictCheck, skipDisabledMaps, forceCheckAllCategories))
			{
				if (layersToInclude == null || layersToInclude.Any((string l) => l == GetControllerMapCategory(c.controllerMap)))
				{
					conflict = c;
					return true;
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("GetFirstElementAssignmentConflict", x);
		}
		conflict = default(ElementAssignmentConflictInfo);
		return false;
	}

	public static bool isKeyMapped(UnityEngine.KeyCode key)
	{
		return isKeyMapped(key, EnabledLayers);
	}

	public static bool isKeyMapped(UnityEngine.KeyCode key, IEnumerable<string> layersToInclude)
	{
		try
		{
			ElementAssignmentConflictInfo conflict;
			return isKeyMapped(key, layersToInclude, out conflict);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("isKeyMapped", x);
			return false;
		}
	}

	public static bool isKeyMapped(UnityEngine.KeyCode key, IEnumerable<string> layersToInclude, out ElementAssignmentConflictInfo conflict)
	{
		try
		{
			if (PrereleaseInput)
			{
				ElementAssignmentConflictCheck conflictCheck = new ElementAssignment(key, ModifierKeyFlags.None, 0, Pole.Positive).ToElementAssignmentConflictCheck();
				conflictCheck.playerId = player.id;
				conflictCheck.controllerType = ControllerType.Keyboard;
				conflictCheck.controllerMapId = 0;
				conflictCheck.controllerMapCategoryId = 0;
				if (GetFirstElementAssignmentConflict(player, conflictCheck, out conflict, layersToInclude, layersToInclude == null))
				{
					return true;
				}
				if (GetFirstElementAssignmentConflict(ReInput.players.SystemPlayer, conflictCheck, out conflict, layersToInclude, layersToInclude == null))
				{
					return true;
				}
				return false;
			}
			conflict = default(ElementAssignmentConflictInfo);
			return LegacyKeyMapping.IsKeyMapped(key, layersToInclude);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("isKeyMapped", x);
			conflict = default(ElementAssignmentConflictInfo);
		}
		return false;
	}

	public static bool isKeyDown(UnityEngine.KeyCode key)
	{
		if (mappingMode)
		{
			return false;
		}
		if (currentContext.isInInput() && keysIgnoredWhileInAnInputField.Any(Input.GetKey))
		{
			return false;
		}
		return Input.GetKeyDown(key);
	}

	public static bool isCharDown(char c)
	{
		if (mappingMode)
		{
			return false;
		}
		if (currentContext.isInInput() && keysIgnoredWhileInAnInputField.Any(Input.GetKey))
		{
			return false;
		}
		ConsoleLib.Console.Keyboard.InitKeymap();
		if (Input.inputString != null && Input.inputString.Contains(c))
		{
			return true;
		}
		bool needShift = false;
		if (char.IsLetter(c) && char.IsUpper(c))
		{
			needShift = true;
		}
		if (ConsoleLib.Console.Keyboard.reverselccharmap.TryGetValue(c, out var value))
		{
			value.Any((UnityEngine.KeyCode code) => Input.GetKeyDown(code) && (needShift || (!Input.GetKey(UnityEngine.KeyCode.LeftShift) && !Input.GetKey(UnityEngine.KeyCode.RightShift))));
		}
		if (ConsoleLib.Console.Keyboard.reverseuccharmap.TryGetValue(c, out value))
		{
			value.Any((UnityEngine.KeyCode code) => Input.GetKeyDown(code) && (!needShift || Input.GetKey(UnityEngine.KeyCode.LeftShift) || Input.GetKey(UnityEngine.KeyCode.RightShift)));
		}
		return false;
	}

	public static void restoreRewiredDefaults()
	{
		IList<Player> players = ReInput.players.Players;
		for (int i = 0; i < players.Count; i++)
		{
			Player player = players[i];
			for (int j = 0; j <= 9; j++)
			{
				player.controllers.maps.LoadMap(ControllerType.Keyboard, 0, j, 0);
				player.controllers.maps.LoadMap(ControllerType.Mouse, 0, j, 0);
				player.controllers.maps.LoadMap(ControllerType.Joystick, 0, j, 0);
				player.controllers.maps.LoadMap(ControllerType.Custom, 0, j, 0);
			}
		}
	}

	public static string getCommandInputDescription(string id, bool mapGlyphs = true)
	{
		if (PrereleaseInput)
		{
			if (id == "CmdReload")
			{
				id = "Reload";
			}
			if (id == "NavigationXYAxis")
			{
				if (activeControllerType != ControllerType.Joystick)
				{
					return "\ue80a";
				}
				return "\ue90a";
			}
			string text = player?.controllers?.maps?.GetFirstElementMapWithAction(activeControllerType, id, skipDisabledMaps: true)?.elementIdentifierName;
			if (text == null)
			{
				text = player?.controllers?.maps?.GetFirstElementMapWithAction(activeControllerType, id, skipDisabledMaps: false)?.elementIdentifierName;
			}
			if (CapabilityManager.AllowKeyboardHotkeys && text == null)
			{
				text = player?.controllers?.maps?.GetFirstElementMapWithAction(id, skipDisabledMaps: true)?.elementIdentifierName;
			}
			if (text == null && CapabilityManager.AllowKeyboardHotkeys)
			{
				if (id == "Cancel")
				{
					return "Esc";
				}
				if (id == "Accept")
				{
					return "Space";
				}
			}
			if (text == id)
			{
				return "";
			}
			if (mapGlyphs)
			{
				text = ConvertBindingTextToGlyphs(text, activeControllerType);
			}
			else if (activeControllerType == ControllerType.Joystick)
			{
				text = "(" + text + ")";
			}
			return text;
		}
		if (id == "NavigationXYAxis")
		{
			return "\ue90a";
		}
		if (id == "NavigationPageYAxis+")
		{
			return "End";
		}
		string text2 = ConsoleLib.Console.Keyboard.MetaToString(LegacyKeyMapping.GetKeyFromCommand(id));
		if (text2 == "None")
		{
			if (id == "Cancel")
			{
				return "Esc";
			}
			if (id == "Accept")
			{
				return "Space";
			}
			return null;
		}
		return ConvertModifierGlyphs(text2);
	}

	public static void ResetInput()
	{
		if (GameManager.IsOnUIContext())
		{
			Input.ResetInputAxes();
		}
		ConsoleLib.Console.Keyboard.ClearInput();
	}

	public static bool GetLegacyKeyDown(UnityEngine.KeyCode button, bool repeat = false)
	{
		if (mappingMode)
		{
			return false;
		}
		if (GameManager.IsOnUIContext() && !Application.isPlaying)
		{
			return false;
		}
		if (button == UnityEngine.KeyCode.None)
		{
			return false;
		}
		if (Input.GetKeyDown(button))
		{
			delayTimers.Set(button, delaytime);
			repeatTimers.Remove(button);
			return true;
		}
		if (!repeat)
		{
			return false;
		}
		if (Input.GetKey(button))
		{
			if (!delayTimers.ContainsKey(button))
			{
				delayTimers.Add(button, delaytime);
			}
			else if (delayTimers[button] <= 0f)
			{
				if (!repeatTimers.ContainsKey(button))
				{
					repeatTimers.Add(button, repeattime);
				}
				else
				{
					repeatTimers[button] -= Time.deltaTime;
					if (repeatTimers[button] <= 0f)
					{
						repeatTimers[button] = repeattime;
						return true;
					}
				}
			}
			else
			{
				delayTimers[button] -= Time.deltaTime;
			}
		}
		else
		{
			delayTimers.Remove(button);
			repeatTimers.Remove(button);
		}
		return false;
	}

	public static bool isCommandDown(string id, bool repeat = true)
	{
		if (mappingMode)
		{
			return false;
		}
		if (SynchronizationContext.Current == The.UiContext && !Application.isPlaying)
		{
			return false;
		}
		if (currentContext.isInInput() && (keysIgnoredWhileInAnInputField.Any(Input.GetKeyDown) || keysIgnoredWhileInAnInputField.Any(Input.GetKeyUp)))
		{
			return false;
		}
		if (PrereleaseInput)
		{
			if (id == "Toggle Message Log")
			{
				if (gameManager.player.GetButtonDown("Toggle Message Log") || (gameManager.player.GetButton("Alternate") && gameManager.player.GetButtonDown("AltToggleMessageLog")))
				{
					return true;
				}
				return false;
			}
			if (id == "Submit" && submitKeys.Any(Input.GetKeyUp))
			{
				return true;
			}
			switch (id)
			{
			case "Navigate Up":
				if (gameManager.player.GetButtonDownRepeating("Navigate Vertical") && gameManager.player.GetAxis("Navigate Vertical") > 0f)
				{
					return true;
				}
				return isCommandDown("Move North");
			case "Navigate Down":
				if (gameManager.player.GetNegativeButtonDownRepeating("Navigate Vertical") && gameManager.player.GetAxis("Navigate Vertical") < 0f)
				{
					return true;
				}
				return isCommandDown("Move South");
			case "Navigate Left":
				if (gameManager.player.GetNegativeButtonDownRepeating("Navigate Horizontal") && gameManager.player.GetAxis("Navigate Horizontal") < 0f)
				{
					return true;
				}
				return isCommandDown("Move West");
			case "Navigate Right":
				if (gameManager.player.GetButtonDownRepeating("Navigate Horizontal") && gameManager.player.GetAxis("Navigate Horizontal") > 0f)
				{
					return true;
				}
				return isCommandDown("Move East");
			default:
				return gameManager.player.GetButtonDown(id);
			}
		}
		if (id == "Accept" && GetLegacyKeyDown(UnityEngine.KeyCode.Return))
		{
			return true;
		}
		if (id == "Accept" && GetLegacyKeyDown(UnityEngine.KeyCode.KeypadEnter))
		{
			return true;
		}
		if (id == "Accept" && GetLegacyKeyDown(UnityEngine.KeyCode.Space))
		{
			return true;
		}
		if (id == "Cancel" && GetLegacyKeyDown(UnityEngine.KeyCode.Escape))
		{
			return true;
		}
		if (id == "Submit" && submitKeys.Any(Input.GetKeyUp))
		{
			return true;
		}
		if ((id == "Move North" || id == "Navigate Up") && GetLegacyKeyDown(UnityEngine.KeyCode.UpArrow, repeat))
		{
			return true;
		}
		if ((id == "Move South" || id == "Navigate Down") && GetLegacyKeyDown(UnityEngine.KeyCode.DownArrow, repeat))
		{
			return true;
		}
		if ((id == "Move East" || id == "Navigate Right") && GetLegacyKeyDown(UnityEngine.KeyCode.RightArrow, repeat))
		{
			return true;
		}
		if ((id == "Move West" || id == "Navigate Left") && GetLegacyKeyDown(UnityEngine.KeyCode.LeftArrow, repeat))
		{
			return true;
		}
		if (id == "Page Up" && Input.GetKeyDown(UnityEngine.KeyCode.PageUp))
		{
			return true;
		}
		if (id == "Page Down" && Input.GetKeyDown(UnityEngine.KeyCode.PageDown))
		{
			return true;
		}
		if (id == "Page Left" && Input.GetKeyDown(UnityEngine.KeyCode.Home))
		{
			return true;
		}
		if (id == "Page Right" && Input.GetKeyDown(UnityEngine.KeyCode.End))
		{
			return true;
		}
		if (id == "V Positive" && Input.GetKeyDown(UnityEngine.KeyCode.KeypadPlus))
		{
			return true;
		}
		if (id == "V Negative" && Input.GetKeyDown(UnityEngine.KeyCode.KeypadMinus))
		{
			return true;
		}
		if (id == "U Positive" && Input.GetKeyDown(UnityEngine.KeyCode.Insert))
		{
			return true;
		}
		if (id == "U Negative" && Input.GetKeyDown(UnityEngine.KeyCode.Delete))
		{
			return true;
		}
		UnityEngine.KeyCode keyCode = mapCommandToPrimaryLegacyKeycode(id);
		UnityEngine.KeyCode keyCode2 = mapCommandToSecondaryLegacyKeycode(id);
		if (currentContext.isInInput())
		{
			if (keyCode == UnityEngine.KeyCode.Backspace)
			{
				keyCode = UnityEngine.KeyCode.None;
			}
			if (keyCode2 == UnityEngine.KeyCode.Backspace)
			{
				keyCode2 = UnityEngine.KeyCode.None;
			}
		}
		if (!GetLegacyKeyDown(keyCode, repeat))
		{
			return GetLegacyKeyDown(keyCode2, repeat);
		}
		return true;
	}

	public static string ConvertBindingTextToGlyphs(string input, ControllerType type)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		if (type == ControllerType.Keyboard)
		{
			return ConvertModifierGlyphs(input);
		}
		if (controllerGlyphs.TryGetValue(input, out var value))
		{
			return value;
		}
		return input;
	}

	public static string ConvertBindingTextToGlyphs(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return ConvertBindingTextToGlyphs(input, player.controllers.GetLastActiveController().type);
	}

	public static string ConvertModifierGlyphs(string input)
	{
		return input?.Replace("Ctrl", "\ue816").Replace("Alt", "\ue818").Replace("Shift", "\ue802");
	}
}
