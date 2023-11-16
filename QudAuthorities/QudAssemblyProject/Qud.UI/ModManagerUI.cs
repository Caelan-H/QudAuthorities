using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XRL;
using XRL.UI;

namespace Qud.UI;

[UIView("ModManager", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "ModManager", UICanvasHost = 1)]
public class ModManagerUI : SingletonWindowBase<ModManagerUI>
{
	public class ModManagerSorter : Comparer<ModInfo>
	{
		public override int Compare(ModInfo a, ModInfo b)
		{
			return ConsoleLib.Console.ColorUtility.CompareExceptFormatting(a.DisplayTitle, b.DisplayTitle);
		}
	}

	public ModScrollerOne ms1;

	public UnityEngine.UI.Text SelectedModTitle;

	public GameObject SelectedModAuthorSpacer;

	public UnityEngine.UI.Text SelectedModAuthor;

	public UnityEngine.UI.Text SelectedModDescription;

	public ImageTinyFrame SelectedModImage;

	public UnityEvent nextHideCallback;

	private int _initialHash;

	private Dictionary<bool, string> _shouldEnableTexts = new Dictionary<bool, string>
	{
		{ false, "{{W|[v]}} {{y|Disable all}}" },
		{ true, "{{W|[v]}} {{y|Enable all}}" }
	};

	private Dictionary<bool, string> _shouldEnableTextsJoystick = new Dictionary<bool, string>
	{
		{ false, "{{y|Disable all}}" },
		{ true, "{{y|Enable all}}" }
	};

	private Dictionary<ModState, string> _spaceButtonTexts = new Dictionary<ModState, string>
	{
		{
			ModState.Disabled,
			"{{W|[space]}} {{y|Enable mod}}"
		},
		{
			ModState.NeedsApproval,
			"{{W|[space]}} {{y|Approve mod}}"
		},
		{
			ModState.Enabled,
			"{{W|[space]}} {{y|Disable mod}}"
		},
		{
			ModState.Failed,
			"{{W|[space]}} {{y|Recompile mod}}"
		}
	};

	private Dictionary<bool, string> shouldEnableTexts
	{
		get
		{
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				return _shouldEnableTextsJoystick;
			}
			return _shouldEnableTexts;
		}
	}

	private Dictionary<ModState, string> spaceButtonTexts
	{
		get
		{
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				return new Dictionary<ModState, string>
				{
					{
						ModState.Disabled,
						"{{W|[" + ControlManager.getCommandInputDescription("Accept") + "]}} {{y|Enable mod}}"
					},
					{
						ModState.NeedsApproval,
						"{{W|[" + ControlManager.getCommandInputDescription("Accept") + "]}} {{y|Approve mod}}"
					},
					{
						ModState.Enabled,
						"{{W|[" + ControlManager.getCommandInputDescription("Accept") + "]}} {{y|Disable mod}}"
					},
					{
						ModState.Failed,
						"{{W|[" + ControlManager.getCommandInputDescription("Accept") + "]}} {{y|Recompile mod}}"
					}
				};
			}
			return _spaceButtonTexts;
		}
	}

	public override void Hide()
	{
		MainMenuView.instance?.DisplayAlert();
		nextHideCallback.Invoke();
		nextHideCallback.RemoveAllListeners();
		base.Hide();
		base.gameObject.SetActive(value: false);
	}

	public override void Show()
	{
		base.gameObject.SetActive(value: true);
		base.Show();
		SetupMods();
	}

	public void SetBackButtonText(string text)
	{
		QudMenuItem value = ms1.bottomContextOptions[0];
		if (ControlManager.activeControllerType == ControllerType.Joystick)
		{
			value.text = "{{W|[" + ControlManager.getCommandInputDescription("Cancel") + "]}} {{y|" + text + "}}";
		}
		else
		{
			value.text = "{{W|[Esc]}} {{y|" + text + "}}";
		}
		ms1.bottomContextOptions[0] = value;
	}

	public void SetupMods()
	{
		if (ms1 != null)
		{
			List<ModInfo> list = new List<ModInfo>(ModManager.Mods);
			list.Sort(new ModManagerSorter());
			ms1.mods = list;
			ms1.isCurrentWindow = base.isCurrentWindow;
			_initialHash = GetModsHash();
		}
	}

	public int GetModsHash()
	{
		return ModManager.Mods.ConvertAll((ModInfo Info) => (Info.IsApproved && Info.IsEnabled) ? Info.ID.GetHashCode() : 0).Aggregate(0, (int a1, int a2) => a1 ^ a2);
	}

	public override void Init()
	{
		base.Init();
	}

	public void OnSelect(ModInfo modInfo)
	{
		SelectedModTitle.text = RTF.FormatToRTF("{{W|" + modInfo.DisplayTitle + "}}");
		if (string.IsNullOrEmpty(modInfo.Manifest.Author))
		{
			SelectedModAuthor.gameObject.SetActive(value: false);
			SelectedModAuthorSpacer.gameObject.SetActive(value: false);
		}
		else
		{
			SelectedModAuthor.gameObject.SetActive(value: true);
			SelectedModAuthorSpacer.gameObject.SetActive(value: true);
			SelectedModAuthor.text = RTF.FormatToRTF("{{C|by " + modInfo.Manifest.Author + "}}");
		}
		SelectedModDescription.text = RTF.FormatToRTF("{{c|" + modInfo.Manifest.Description + "}}");
		SelectedModImage.image.sprite = modInfo.GetSprite();
	}

	public void Awake()
	{
	}

	public void Update()
	{
		bool key = ms1.mods.FindCount((ModInfo i) => i.IsEnabled) < ms1.mods.Count / 2;
		int index = ms1.bottomContextOptions.FindIndex((QudMenuItem i) => i.command == "ToggleAll");
		if (ms1.bottomContextOptions[index].text != shouldEnableTexts[key])
		{
			QudMenuItem value = ms1.bottomContextOptions[index];
			value.text = shouldEnableTexts[key];
			ms1.bottomContextOptions[index] = value;
		}
		int index2 = ms1.bottomContextOptions.FindIndex((QudMenuItem i) => i.command == "");
		string text = "{{W|[space]}} {{y|press button}}";
		if (ControlManager.activeControllerType == ControllerType.Joystick)
		{
			text = "{{W|[" + ControlManager.getCommandInputDescription("Accept") + "]}} {{y|press button}}";
		}
		if (ms1.selectedOption < ms1.mods.Count)
		{
			text = spaceButtonTexts[ms1.mods[ms1.selectedOption].State];
		}
		if (ms1.bottomContextOptions[index2].text != text)
		{
			QudMenuItem value2 = ms1.bottomContextOptions[index2];
			value2.text = text;
			ms1.bottomContextOptions[index2] = value2;
		}
		if (ms1.menuBottomContext.GetComponent<RectTransform>().rect.width < ms1.menuBottomContext.GetComponent<LayoutGroup>().preferredWidth)
		{
			ms1.menuBottomContext.buttons[index2].gameObject.SetActive(value: false);
		}
	}

	private IEnumerator ReloadModConfig(Action callback)
	{
		Loading.SetLoadingStatus("Reloading mod configuration...");
		yield return 0;
		yield return 0;
		yield return 0;
		try
		{
			ModManager.WriteModSettings();
			ModManager.Refresh();
			ModManager.BuildScriptMods();
			The.Core.HotloadConfiguration();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		finally
		{
			callback();
		}
		yield return 0;
		Loading.SetLoadingStatus(null);
		yield return 0;
	}

	public void OnCancel()
	{
		if (GetModsHash() != _initialHash)
		{
			StartCoroutine(ReloadModConfig(delegate
			{
				Hide();
				UIManager.popWindow();
			}));
		}
		else
		{
			UIManager.popWindow();
		}
	}

	public void OnActivateContext(QudMenuItem data)
	{
		QudMenuItem qudMenuItem = data;
		Debug.Log("Activate Context: " + qudMenuItem.ToString());
		if (data.command == "ToggleAll")
		{
			int num = ms1.mods.FindCount((ModInfo i) => i.IsEnabled);
			bool shouldEnable = num < ms1.mods.Count / 2;
			ms1.mods.ForEach(delegate(ModInfo i)
			{
				i.IsEnabled = shouldEnable;
			});
		}
		else if (data.command == "Undo")
		{
			ModManager.ReadModSettings(Reload: true);
			SetupMods();
		}
		else if (data.command == "Reload")
		{
			StartCoroutine(ReloadModConfig(SetupMods));
		}
	}
}
