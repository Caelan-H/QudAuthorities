using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[UIView("Keybinds", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "Keybinds", UICanvasHost = 1)]
public class KeybindsScreen : SingletonWindowBase<KeybindsScreen>, ControlManager.IControllerChangedEvent
{
	public FrameworkContext inputTypeContext;

	public UITextSkin inputTypeText;

	public Image background;

	public FrameworkScroller hotkeyBar;

	public FrameworkScroller keybindsScroller;

	public FrameworkScroller categoryScroller;

	public RectTransform safeArea;

	public EmbarkBuilderModuleBackButton backButton;

	public TaskCompletionSource<bool> completionSource;

	public NavigationContext globalContext = new NavigationContext();

	public ScrollContext<NavigationContext> midHorizNav = new ScrollContext<NavigationContext>();

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public List<FrameworkDataElement> menuItems = new List<FrameworkDataElement>();

	public Dictionary<string, bool> categoryExpanded = new Dictionary<string, bool>();

	public float lastWidth;

	public float breakpointBackButtonWidth;

	public bool wasInScroller;

	private bool SelectFirst = true;

	public static readonly MenuOption REMOVE_BIND = new MenuOption
	{
		Id = "REMOVE_BIND",
		KeyDescription = "Delete",
		InputCommand = "U Negative",
		Description = "remove keybind"
	};

	public static readonly MenuOption RESTORE_DEFAULTS = new MenuOption
	{
		Id = "RESTORE_DEFAULTS",
		KeyDescription = "+",
		InputCommand = "V Positive",
		Description = "restore defaults"
	};

	public static readonly MenuOption LAPTOP_DEFAULTS = new MenuOption
	{
		Id = "LAPTOP_DEFAULTS",
		KeyDescription = "-",
		InputCommand = "V Negative",
		Description = "laptop defaults"
	};

	public List<MenuOption> keyMenuOptions = new List<MenuOption>();

	public FrameworkDataElement lastSelectedElement;

	public InputMapper inputMapper;

	private static bool done = false;

	public bool bChangesMade;

	private Joystick currentJoystick;

	public ControllerType currentControllerType;

	public bool breakBackButton => lastWidth <= breakpointBackButtonWidth;

	public static MenuOption BACK_BUTTON => EmbarkBuilderOverlayWindow.BackMenuOption;

	public Player currentPlayer => GameManager.Instance.player;

	public static bool IsPrerelease => Options.PrereleaseInputManager;

	public void getCurrentActiveControl()
	{
		if (IsPrerelease)
		{
			Controller lastActiveController = currentPlayer.controllers.GetLastActiveController();
			if (lastActiveController == null)
			{
				currentControllerType = ControllerType.Keyboard;
				ControlManager.instance.controllerFontType = ControlManager.ControllerFontType.Keyboard;
			}
			else if (lastActiveController.type == ControllerType.Joystick)
			{
				currentControllerType = ControllerType.Joystick;
				currentJoystick = lastActiveController as Joystick;
				ControlManager.instance.controllerFontType = ControlManager.ControllerFontType.XBox;
			}
			else
			{
				currentControllerType = ControllerType.Keyboard;
				ControlManager.instance.controllerFontType = ControlManager.ControllerFontType.Keyboard;
			}
		}
		else
		{
			currentControllerType = ControllerType.Keyboard;
			ControlManager.instance.controllerFontType = ControlManager.ControllerFontType.Keyboard;
		}
	}

	public async Task<bool> KeybindsMenu()
	{
		done = false;
		getCurrentActiveControl();
		QueryKeybinds();
		SelectFirst = true;
		completionSource?.TrySetCanceled();
		completionSource = new TaskCompletionSource<bool>();
		await The.UiContext;
		ControlManager.ResetInput();
		Show();
		bool info = await completionSource.Task;
		DisableNavContext();
		await The.UiContext;
		Hide();
		return info;
	}

	public async void Exit()
	{
		if (bChangesMade)
		{
			switch (await Popup.ShowYesNoCancelAsync("Would you like to save your changes?"))
			{
			case DialogResult.Cancel:
				return;
			case DialogResult.Yes:
				if (IsPrerelease)
				{
					ReInput.userDataStore.Save();
				}
				else
				{
					LegacyKeyMapping.SaveCurrentKeymap();
				}
				bChangesMade = false;
				break;
			case DialogResult.No:
				if (IsPrerelease)
				{
					ReInput.userDataStore.Load();
				}
				else
				{
					LegacyKeyMapping.LoadCurrentKeymap();
				}
				bChangesMade = false;
				break;
			}
		}
		completionSource?.TrySetResult(result: false);
	}

	public void Update()
	{
		bool flag = NavigationController.instance.activeContext?.IsInside(keybindsScroller.GetNavigationContext()) ?? false;
		float width = base.rectTransform.rect.width;
		if (flag != wasInScroller || lastWidth != width)
		{
			wasInScroller = flag;
			lastWidth = width;
			backButton.gameObject.SetActive(!breakBackButton);
			safeArea.offsetMin = new Vector2(breakBackButton ? 10 : 150, safeArea.offsetMin.y);
			safeArea.offsetMax = new Vector2(breakBackButton ? (-10) : (-150), safeArea.offsetMax.y);
			UpdateMenuBars();
		}
		if (Input.GetKeyDown(UnityEngine.KeyCode.F2))
		{
			HandleMenuOption(LAPTOP_DEFAULTS);
		}
	}

	public void ControllerChanged()
	{
		UpdateMenuBars();
	}

	public void SetupContext()
	{
		globalContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		globalContext.buttonHandlers.Set(InputButtonTypes.CancelButton, XRL.UI.Framework.Event.Helpers.Handle(Exit));
		globalContext.commandHandlers = new Dictionary<string, Action>
		{
			{
				BACK_BUTTON.InputCommand,
				XRL.UI.Framework.Event.Helpers.Handle(Exit)
			},
			{
				REMOVE_BIND.InputCommand,
				XRL.UI.Framework.Event.Helpers.Handle(delegate
				{
					HandleMenuOption(REMOVE_BIND);
				})
			},
			{
				RESTORE_DEFAULTS.InputCommand,
				XRL.UI.Framework.Event.Helpers.Handle(delegate
				{
					HandleMenuOption(RESTORE_DEFAULTS);
				})
			},
			{
				LAPTOP_DEFAULTS.InputCommand,
				XRL.UI.Framework.Event.Helpers.Handle(delegate
				{
					HandleMenuOption(LAPTOP_DEFAULTS);
				})
			}
		};
		midHorizNav.SetAxis(InputAxisTypes.NavigationXAxis);
		midHorizNav.contexts.Clear();
		midHorizNav.contexts.Add(backButton.navigationContext);
		midHorizNav.contexts.Add(categoryScroller.GetNavigationContext());
		midHorizNav.contexts.Add(vertNav);
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Clear();
		vertNav.contexts.Add(inputTypeContext.RequireContext<NavigationContext>());
		inputTypeContext.context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
		{
			InputButtonTypes.AcceptButton,
			XRL.UI.Framework.Event.Helpers.Handle(SelectInputType)
		} };
		vertNav.contexts.Add(keybindsScroller.GetNavigationContext());
		vertNav.contexts.Add(hotkeyBar.GetNavigationContext());
		midHorizNav.Setup();
		midHorizNav.parentContext = globalContext;
	}

	public void UpdateMenuBars()
	{
		keyMenuOptions.Clear();
		keyMenuOptions.Add(new MenuOption
		{
			InputCommand = "NavigationXYAxis",
			Description = "navigate"
		});
		keyMenuOptions.Add(new MenuOption
		{
			InputCommand = "Accept",
			Description = "select"
		});
		keyMenuOptions.Add(REMOVE_BIND);
		if (breakBackButton)
		{
			keyMenuOptions.Add(BACK_BUTTON);
		}
		keyMenuOptions.Add(RESTORE_DEFAULTS);
		keyMenuOptions.Add(LAPTOP_DEFAULTS);
		hotkeyBar.BeforeShow(null, keyMenuOptions);
		hotkeyBar.GetNavigationContext().disabled = false;
		hotkeyBar.onSelected.RemoveAllListeners();
		hotkeyBar.onSelected.AddListener(HandleMenuOption);
		foreach (NavigationContext item in hotkeyBar.scrollContext.contexts.GetRange(0, 3))
		{
			item.disabled = true;
		}
	}

	public void restoreRewiredDefaults()
	{
		IList<Player> players = ReInput.players.Players;
		for (int i = 0; i < players.Count; i++)
		{
			Player player = players[i];
			for (int j = 0; j <= 9; j++)
			{
				if (currentControllerType == ControllerType.Keyboard)
				{
					player.controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
					player.controllers.maps.LoadDefaultMaps(ControllerType.Mouse);
				}
				else
				{
					player.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
				}
			}
		}
	}

	public async void HandleMenuOption(FrameworkDataElement data)
	{
		if (data == RESTORE_DEFAULTS)
		{
			if (!IsPrerelease)
			{
				if (await Popup.ShowYesNoAsync("Are you sure you want to override your keymap with the default?") == DialogResult.Yes)
				{
					bChangesMade = true;
					LegacyKeyMapping.CurrentMap = LegacyKeyMapping.LoadKeymap(DataManager.FilePath("DefaultKeymap.json"));
					LegacyKeyMapping.CurrentMap.ApplyDefaults();
				}
			}
			else if (await Popup.ShowYesNoAsync("Are you sure you want to override your keymap with the default?") == DialogResult.Yes)
			{
				bChangesMade = true;
				restoreRewiredDefaults();
			}
			QueryKeybinds();
			Show();
		}
		else if (data == LAPTOP_DEFAULTS)
		{
			if (IsPrerelease)
			{
				await Popup.ShowAsync("Automatic laptop defaults aren't currently implemented for the prerelease input manager.");
			}
			else if (await Popup.ShowYesNoAsync("Are you sure you want to override your keymap with the laptop default?") == DialogResult.Yes)
			{
				bChangesMade = true;
				LegacyKeyMapping.CurrentMap = LegacyKeyMapping.LoadLegacyKeymap(DataManager.FilePath("DefaultLaptopKeymap.xml"));
				LegacyKeyMapping.CurrentMap.ApplyDefaults();
			}
			QueryKeybinds();
			Show();
		}
		else
		{
			if (data != REMOVE_BIND)
			{
				return;
			}
			int num = keybindsScroller.scrollContext.contexts.FindIndex((NavigationContext s) => s.IsActive());
			if (num == -1)
			{
				return;
			}
			FrameworkDataElement frameworkDataElement = keybindsScroller.scrollContext.data[num];
			if (!(frameworkDataElement is KeybindDataRow row))
			{
				return;
			}
			ScrollContext<KeybindType, NavigationContext> rowScroller = keybindsScroller.selectionClones[num].GetComponent<KeybindRow>().subscroller;
			if (!IsPrerelease)
			{
				Dictionary<int, string> primaryMapKeyToCommand = LegacyKeyMapping.CurrentMap.getPrimaryKeyToCommand(row.KeyId);
				Dictionary<string, int> primaryMapCommandToKey = LegacyKeyMapping.CurrentMap.getPrimaryCommandToKey(row.KeyId);
				Dictionary<int, string> secondaryMapKeyToCommand = LegacyKeyMapping.CurrentMap.getSecondaryKeyToCommand(row.KeyId);
				Dictionary<string, int> secondaryMapCommandToKey = LegacyKeyMapping.CurrentMap.getSecondaryCommandToKey(row.KeyId);
				if (await Popup.ShowYesNoAsync("Are you sure you want to clear the binding for {{C|" + row.KeyDescription + "}}?") == DialogResult.Yes)
				{
					bChangesMade = true;
					if (rowScroller.selectedPosition == 0)
					{
						if (primaryMapCommandToKey.ContainsKey(row.KeyId))
						{
							if (primaryMapKeyToCommand.ContainsKey(primaryMapCommandToKey[row.KeyId]))
							{
								primaryMapKeyToCommand.Remove(primaryMapCommandToKey[row.KeyId]);
							}
							primaryMapCommandToKey.Remove(row.KeyId);
						}
					}
					else if (rowScroller.selectedPosition == 1 && secondaryMapCommandToKey.ContainsKey(row.KeyId))
					{
						if (secondaryMapKeyToCommand.ContainsKey(secondaryMapCommandToKey[row.KeyId]))
						{
							secondaryMapKeyToCommand.Remove(secondaryMapCommandToKey[row.KeyId]);
						}
						secondaryMapCommandToKey.Remove(row.KeyId);
					}
				}
			}
			else if (rowScroller.selectedPosition == 0)
			{
				await Popup.ShowAsync("TODO: Delete prerelease primary keybind " + row.KeyId);
			}
			else if (rowScroller.selectedPosition == 1)
			{
				await Popup.ShowAsync("TODO: Delete prerelease alternate keybind " + row.KeyId);
			}
			QueryKeybinds();
			Show();
		}
	}

	public IEnumerable<FrameworkDataElement> GetMenuItems()
	{
		foreach (FrameworkDataElement menuItem in menuItems)
		{
			if (menuItem is KeybindDataRow keybindDataRow)
			{
				if (categoryExpanded[keybindDataRow.CategoryId])
				{
					yield return menuItem;
				}
			}
			else if (menuItem is KeybindCategoryRow keybindCategoryRow)
			{
				keybindCategoryRow.Collapsed = !categoryExpanded[keybindCategoryRow.CategoryId];
				yield return menuItem;
			}
		}
	}

	public IEnumerable<FrameworkDataElement> GetCategoryItems()
	{
		foreach (FrameworkDataElement menuItem in menuItems)
		{
			if (menuItem is KeybindCategoryRow keybindCategoryRow)
			{
				keybindCategoryRow.Collapsed = !categoryExpanded[keybindCategoryRow.CategoryId];
				yield return menuItem;
			}
		}
	}

	public override void Show()
	{
		base.Show();
		backButton?.gameObject.SetActive(value: true);
		if (backButton.navigationContext == null)
		{
			backButton.Awake();
		}
		backButton.navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		backButton.navigationContext.buttonHandlers.Set(InputButtonTypes.AcceptButton, XRL.UI.Framework.Event.Helpers.Handle(Exit));
		keybindsScroller.scrollContext.wraps = false;
		keybindsScroller.BeforeShow(null, GetMenuItems());
		categoryScroller.scrollContext.wraps = true;
		categoryScroller.BeforeShow(null, GetCategoryItems());
		foreach (KeybindRow item in keybindsScroller.selectionClones.Select((FrameworkUnityScrollChild s) => s.GetComponent<KeybindRow>()))
		{
			item.GetNavigationContext();
			if (item != null)
			{
				item.onRebind.RemoveAllListeners();
				item.onRebind.AddListener(HandleRebind);
			}
		}
		if (SelectFirst)
		{
			SelectFirst = false;
			keybindsScroller.scrollContext.selectedPosition = 1;
		}
		keybindsScroller.onSelected.RemoveAllListeners();
		keybindsScroller.onSelected.AddListener(HandleSelect);
		keybindsScroller.onHighlight.RemoveAllListeners();
		keybindsScroller.onHighlight.AddListener(HandleHighlight);
		categoryScroller.onSelected.RemoveAllListeners();
		categoryScroller.onSelected.AddListener(HandleSelectLeft);
		categoryScroller.onHighlight.RemoveAllListeners();
		categoryScroller.onHighlight.AddListener(HandleHighlightLeft);
		UpdateMenuBars();
		SetupContext();
		EnableNavContext();
	}

	public void HandleSelect(FrameworkDataElement element)
	{
		if (element is KeybindCategoryRow keybindCategoryRow)
		{
			categoryExpanded[keybindCategoryRow.CategoryId] = !categoryExpanded[keybindCategoryRow.CategoryId];
			Show();
		}
	}

	public void HandleHighlight(FrameworkDataElement element)
	{
		lastSelectedElement = element;
		string catId = null;
		if (element is KeybindCategoryRow keybindCategoryRow)
		{
			catId = keybindCategoryRow.CategoryId;
		}
		else if (element is KeybindDataRow keybindDataRow)
		{
			catId = keybindDataRow.CategoryId;
		}
		if (catId != null)
		{
			int num2 = (categoryScroller.selectedPosition = (categoryScroller.scrollContext.selectedPosition = GetCategoryItems().ToList().FindIndex((FrameworkDataElement s) => (s as KeybindCategoryRow)?.CategoryId == catId)));
		}
	}

	public void HandleSelectLeft(FrameworkDataElement element)
	{
		KeybindCategoryRow cat = element as KeybindCategoryRow;
		if (cat != null)
		{
			categoryExpanded[cat.CategoryId] = true;
			Show();
			int index = GetMenuItems().ToList().FindIndex((FrameworkDataElement s) => (s as KeybindCategoryRow)?.CategoryId == cat.CategoryId);
			keybindsScroller.scrollContext.contexts[index].Activate();
			ScrollViewCalcs.ScrollToTopOfRect(keybindsScroller.selectionClones[index].GetComponent<RectTransform>());
		}
	}

	public void HandleHighlightLeft(FrameworkDataElement element)
	{
		KeybindCategoryRow cat = element as KeybindCategoryRow;
		if (cat == null)
		{
			return;
		}
		FrameworkDataElement frameworkDataElement = GetMenuItems().Skip(keybindsScroller.selectedPosition).FirstOrDefault();
		if (((frameworkDataElement is KeybindCategoryRow keybindCategoryRow) ? keybindCategoryRow.CategoryId : ((frameworkDataElement is KeybindDataRow keybindDataRow) ? keybindDataRow.CategoryId : null)) != cat.CategoryId)
		{
			int num = GetMenuItems().ToList().FindIndex((FrameworkDataElement s) => (s as KeybindCategoryRow)?.CategoryId == cat.CategoryId);
			ScrollViewCalcs.ScrollToTopOfRect(keybindsScroller.selectionClones[num].GetComponent<RectTransform>());
			int num3 = (keybindsScroller.selectedPosition = (keybindsScroller.scrollContext.selectedPosition = num));
		}
	}

	public override void Hide()
	{
		base.Hide();
		DisableNavContext();
		Input.ResetInputAxes();
		base.gameObject.SetActive(value: false);
		GameManager.Instance.skipAnInput = true;
	}

	public void EnableNavContext()
	{
		globalContext.disabled = false;
		keybindsScroller.GetNavigationContext().Activate();
	}

	public void DisableNavContext(bool deactivate = true)
	{
		if (deactivate)
		{
			NavigationContext activeContext = NavigationController.instance.activeContext;
			if (activeContext != null && activeContext.IsInside(globalContext))
			{
				NavigationController.instance.activeContext = null;
			}
		}
		globalContext.disabled = true;
	}

	public async void HandleRebind(KeybindDataRow binding, KeybindType bindType, KeybindRow unityRow)
	{
		KeybindBox box = ((bindType != 0) ? unityRow.box2 : unityRow.box1);
		box.editMode = true;
		box.Update();
		if (!IsPrerelease)
		{
			await HandleLegacyRebind(binding, bindType, unityRow);
		}
		else
		{
			await HandleRewiredRebind(binding, bindType);
		}
		Debug.Log("Requerying");
		box.editMode = false;
		ControlManager.ResetInput();
		QueryKeybinds();
		Show();
	}

	private async Task HandleRewiredRebind(KeybindDataRow binding, KeybindType bind)
	{
		done = false;
		ControlManager.mappingMode = true;
		ActionElementMap selectedBinding = ((bind == KeybindType.Primary) ? binding.rewiredBind1 : binding.rewiredBind2);
		try
		{
			new ManualResetEvent(initialState: false);
			if (inputMapper == null)
			{
				inputMapper = new InputMapper();
				inputMapper.options = new InputMapper.Options
				{
					timeout = 5f
				};
				inputMapper.ConflictFoundEvent += async delegate(InputMapper.ConflictFoundEventData o)
				{
					Debug.Log("Conflict");
					ControlManager.mappingMode = false;
					if (o.isProtected)
					{
						await Popup.ShowAsync(selectedBinding.actionDescriptiveName + "  is already in use and is protected from reassignment.");
						o.responseCallback(InputMapper.ConflictResponse.Cancel);
					}
					else
					{
						string message = "There was a conflict do you want to replace the existing binding?";
						if (o.conflicts.Count > 0)
						{
							ElementAssignmentConflictInfo elementAssignmentConflictInfo = o.conflicts.First();
							message = ControlManager.ConvertBindingTextToGlyphs(elementAssignmentConflictInfo.elementDisplayName) + " is already in use as " + elementAssignmentConflictInfo.action.descriptiveName + " do you want to replace this binding?";
						}
						if (await Popup.ShowYesNoAsync(message) == DialogResult.Yes)
						{
							o.responseCallback(InputMapper.ConflictResponse.Replace);
							bChangesMade = true;
						}
						else
						{
							o.responseCallback(InputMapper.ConflictResponse.Cancel);
						}
					}
				};
				inputMapper.CanceledEvent += delegate
				{
					Debug.Log("Canceled");
					complete();
				};
				inputMapper.TimedOutEvent += delegate
				{
					Debug.Log("Timed Out");
					complete();
				};
				inputMapper.ErrorEvent += delegate
				{
					Debug.Log("Errored");
					complete();
				};
				inputMapper.InputMappedEvent += delegate
				{
					Debug.Log("Mapped");
					bChangesMade = true;
					complete();
				};
				inputMapper.StartedEvent += delegate
				{
					Debug.Log("Started");
				};
				inputMapper.StoppedEvent += delegate
				{
					Debug.Log("Stopped");
					complete();
				};
			}
			await Task.Yield();
			await Task.Yield();
			await Task.Yield();
			if (inputMapper.Start(new InputMapper.Context
			{
				actionId = binding.rewiredInputAction.id,
				controllerMap = binding.rewiredControllerMap,
				actionRange = binding.rewiredAxisRange,
				actionElementMapToReplace = ((bind == KeybindType.Primary) ? binding.rewiredBind1 : binding.rewiredBind2)
			}))
			{
				Debug.Log("success");
				while (!done)
				{
					await Task.Delay(10);
				}
				Debug.Log("done was true");
			}
			else
			{
				Debug.Log("fail");
			}
		}
		catch (Exception)
		{
			ControlManager.mappingMode = false;
			done = true;
		}
		static void complete()
		{
			done = true;
			ControlManager.mappingMode = false;
			Debug.Log("Completed");
		}
	}

	private async Task HandleLegacyRebind(KeybindDataRow binding, KeybindType bindType, KeybindRow unityRow)
	{
		await Popup.ShowKeybindAsync("Press a key to bind to " + binding.KeyDescription);
		int Meta = ConsoleLib.Console.Keyboard.MetaKey;
		if (ConsoleLib.Console.Keyboard.vkCode == Keys.Escape)
		{
			return;
		}
		string sMeta = "{{W|" + ConsoleLib.Console.Keyboard.MetaToString(Meta) + "}}";
		Dictionary<int, string> primaryMapKeyToCommand = LegacyKeyMapping.CurrentMap.getPrimaryKeyToCommand(binding.KeyId);
		Dictionary<string, int> primaryMapCommandToKey = LegacyKeyMapping.CurrentMap.getPrimaryCommandToKey(binding.KeyId);
		if (primaryMapKeyToCommand.ContainsKey(Meta) && LegacyKeyMapping.CommandsByID.ContainsKey(primaryMapKeyToCommand[Meta]))
		{
			if (await Popup.ShowYesNoAsync(sMeta + " is already bound to {{C|" + LegacyKeyMapping.CommandsByID[primaryMapKeyToCommand[Meta]].DisplayText + "}}. Do you want to bind it to {{C|" + binding.KeyDescription + "}} instead?") == DialogResult.No)
			{
				return;
			}
			primaryMapCommandToKey.Remove(primaryMapKeyToCommand[Meta]);
			primaryMapKeyToCommand.Remove(Meta);
		}
		Dictionary<int, string> secondaryMapKeyToCommand = LegacyKeyMapping.CurrentMap.getSecondaryKeyToCommand(binding.KeyId);
		Dictionary<string, int> secondaryMapCommandToKey = LegacyKeyMapping.CurrentMap.getSecondaryCommandToKey(binding.KeyId);
		if (secondaryMapKeyToCommand.ContainsKey(Meta) && LegacyKeyMapping.CommandsByID.ContainsKey(secondaryMapKeyToCommand[Meta]))
		{
			if (await Popup.ShowYesNoAsync(sMeta + " is already bound to {{C|" + LegacyKeyMapping.CommandsByID[secondaryMapKeyToCommand[Meta]].DisplayText + "}}. Do you want to bind it to {{C|" + binding.KeyDescription + "}} instead?") == DialogResult.No)
			{
				return;
			}
			secondaryMapCommandToKey.Remove(secondaryMapKeyToCommand[Meta]);
			secondaryMapKeyToCommand.Remove(Meta);
		}
		bChangesMade = true;
		switch (bindType)
		{
		case KeybindType.Primary:
			if (primaryMapCommandToKey.ContainsKey(binding.KeyId))
			{
				primaryMapKeyToCommand.Remove(primaryMapCommandToKey[binding.KeyId]);
				primaryMapCommandToKey.Remove(binding.KeyId);
			}
			primaryMapCommandToKey.Add(binding.KeyId, Meta);
			primaryMapKeyToCommand.Add(Meta, binding.KeyId);
			break;
		case KeybindType.Alternate:
			if (secondaryMapCommandToKey.ContainsKey(binding.KeyId))
			{
				secondaryMapKeyToCommand.Remove(secondaryMapCommandToKey[binding.KeyId]);
				secondaryMapCommandToKey.Remove(binding.KeyId);
			}
			secondaryMapCommandToKey.Add(binding.KeyId, Meta);
			secondaryMapKeyToCommand.Add(Meta, binding.KeyId);
			break;
		}
	}

	public async void SelectInputType()
	{
		List<Joystick> controllerList = currentPlayer.controllers.Joysticks.ToList();
		List<string> list = currentPlayer.controllers.Joysticks.Select((Joystick j) => j.name).ToList();
		list.Insert(0, "Keyboard && Mouse");
		if (IsPrerelease)
		{
			int num = await Popup.ShowOptionListAsync("Select Controller", list.ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
			if (num < 0)
			{
				return;
			}
			if (num == 0)
			{
				currentControllerType = ControllerType.Keyboard;
				currentJoystick = null;
				ControlManager.instance.controllerFontType = ControlManager.ControllerFontType.Keyboard;
			}
			else
			{
				currentControllerType = ControllerType.Joystick;
				currentJoystick = controllerList[num - 1];
				ControlManager.instance.controllerFontType = ControlManager.ControllerFontType.XBox;
			}
		}
		QueryKeybinds();
		Show();
	}

	private ControllerMap GetControllerMap(ControllerType type, string currentMapCategoryId)
	{
		if (currentPlayer == null)
		{
			return null;
		}
		int controllerId = 0;
		switch (type)
		{
		case ControllerType.Joystick:
			if (currentPlayer.controllers.joystickCount > 0)
			{
				controllerId = currentJoystick.id;
				break;
			}
			return null;
		default:
			throw new NotImplementedException();
		case ControllerType.Keyboard:
		case ControllerType.Mouse:
			break;
		}
		return currentPlayer.controllers.maps.GetFirstMapInCategory(type, controllerId, currentMapCategoryId);
	}

	private ControllerMap RequireControllerMap(ControllerType controllerType, int controllerId, string mapCategory)
	{
		ControllerMap controllerMap = GetControllerMap(controllerType, mapCategory);
		if (controllerMap == null)
		{
			currentPlayer.controllers.maps.AddEmptyMap(controllerType, controllerId, mapCategory, "Default");
			controllerMap = currentPlayer.controllers.maps.GetMap(controllerType, controllerId, mapCategory, "Default");
		}
		return controllerMap;
	}

	public void QueryKeybinds()
	{
		menuItems.Clear();
		if (!IsPrerelease)
		{
			inputTypeText.text = "{{C|Configuring Controller:}} {{c|Legacy Keyboard}}";
		}
		else if (currentControllerType == ControllerType.Keyboard || currentJoystick == null)
		{
			inputTypeText.text = "{{C|Configuring Controller:}} {{c|Keyboard && Mouse}}";
		}
		else
		{
			inputTypeText.text = "{{C|Configuring Controller:}} {{c|" + currentJoystick.name + "}}";
		}
		if (IsPrerelease)
		{
			ReInput.players.GetPlayer(0).controllers.maps.GetAllMaps().ToList();
			{
				foreach (InputCategory item in ReInput.mapping.ActionCategories.Where((InputCategory a) => a.userAssignable))
				{
					ControllerMap controllerMap = RequireControllerMap(currentControllerType, currentJoystick?.id ?? 0, item.name);
					menuItems.Add(new KeybindCategoryRow
					{
						CategoryId = item.id.ToString(),
						CategoryDescription = item.descriptiveName,
						rewiredInputCategory = item
					});
					if (!categoryExpanded.ContainsKey(item.id.ToString()))
					{
						categoryExpanded[item.id.ToString()] = true;
					}
					foreach (InputAction item2 in from a in ReInput.mapping.ActionsInCategory(item.id)
						where a.userAssignable
						select a)
					{
						ActionElementMap rewiredBind = null;
						string input = "";
						ActionElementMap rewiredBind2 = null;
						string input2 = "";
						_ = item2.type;
						_ = 1;
						_ = item2.type;
						List<ActionElementMap> list = controllerMap.GetElementMapsWithAction(item2.id).ToList();
						if (list.Count > 0)
						{
							rewiredBind = list[0];
							input = list[0].elementIdentifierName;
						}
						if (list.Count > 1)
						{
							rewiredBind2 = list[1];
							input2 = list[1].elementIdentifierName;
						}
						if (list.Count > 2 && item2.type == InputActionType.Button)
						{
							Debug.LogError("     There are three mappings for button " + item2.descriptiveName);
							foreach (ActionElementMap item3 in list)
							{
								Debug.LogError("     " + item3.actionDescriptiveName + " " + item3.elementIdentifierName);
							}
						}
						if (list.Count > 5 && item2.type == InputActionType.Axis)
						{
							Debug.LogError("     There are three mappings for axis " + item2.descriptiveName);
							foreach (ActionElementMap item4 in list)
							{
								Debug.LogError("     " + item4.actionDescriptiveName + " " + item4.elementIdentifierName);
							}
						}
						if (item2.type == InputActionType.Button)
						{
							menuItems.Add(new KeybindDataRow
							{
								CategoryId = item.id.ToString(),
								KeyId = item2.id.ToString(),
								KeyDescription = item2.descriptiveName,
								Bind1 = ControlManager.ConvertBindingTextToGlyphs(input, currentControllerType),
								rewiredBind1 = rewiredBind,
								Bind2 = ControlManager.ConvertBindingTextToGlyphs(input2, currentControllerType),
								rewiredBind2 = rewiredBind2,
								rewiredInputAction = item2,
								rewiredInputCategory = item,
								rewiredControllerMap = controllerMap,
								rewiredAxisRange = AxisRange.Full
							});
						}
						else if (item2.type == InputActionType.Axis)
						{
							List<ActionElementMap> list2 = list.Where((ActionElementMap e) => e.axisType == AxisType.Normal).ToList();
							List<ActionElementMap> list3 = list.Where((ActionElementMap e) => e.axisContribution == Pole.Positive && e.axisType == AxisType.Split).ToList();
							List<ActionElementMap> list4 = list.Where((ActionElementMap e) => e.axisContribution == Pole.Negative && e.axisType == AxisType.Split).ToList();
							if (list2.Count > 2)
							{
								Debug.LogError("     There are >two fullBind mappings for axis " + item2.descriptiveName);
							}
							if (list3.Count > 2)
							{
								Debug.LogError("     There are >two posBind mappings for axis " + item2.descriptiveName);
							}
							if (list4.Count > 2)
							{
								Debug.LogError("     There are >two negBind mappings for axis " + item2.descriptiveName);
							}
							if (currentControllerType == ControllerType.Joystick)
							{
								menuItems.Add(new KeybindDataRow
								{
									CategoryId = item.id.ToString(),
									KeyId = item2.id.ToString(),
									KeyDescription = item2.descriptiveName + " stick",
									rewiredInputAction = item2,
									Bind1 = ControlManager.ConvertBindingTextToGlyphs((list2.Count > 0) ? list2[0].elementIdentifierName : "", currentControllerType),
									rewiredBind1 = ((list2.Count > 0) ? list2[0] : null),
									Bind2 = ControlManager.ConvertBindingTextToGlyphs((list2.Count > 1) ? list2[1].elementIdentifierName : "", currentControllerType),
									rewiredBind2 = ((list2.Count > 1) ? list2[1] : null),
									rewiredInputCategory = item,
									rewiredControllerMap = controllerMap,
									rewiredAxisRange = AxisRange.Full
								});
							}
							menuItems.Add(new KeybindDataRow
							{
								CategoryId = item.id.ToString(),
								KeyId = item2.id.ToString(),
								KeyDescription = item2.descriptiveName + "+",
								Bind1 = ControlManager.ConvertBindingTextToGlyphs((list3.Count > 0) ? list3[0].elementIdentifierName : "", currentControllerType),
								rewiredBind1 = ((list3.Count > 0) ? list3[0] : null),
								Bind2 = ControlManager.ConvertBindingTextToGlyphs((list3.Count > 1) ? list3[1].elementIdentifierName : "", currentControllerType),
								rewiredBind2 = ((list3.Count > 1) ? list3[1] : null),
								rewiredInputAction = item2,
								rewiredInputCategory = item,
								rewiredControllerMap = controllerMap,
								rewiredAxisRange = AxisRange.Positive,
								rewairedAxisContribution = Pole.Positive
							});
							menuItems.Add(new KeybindDataRow
							{
								CategoryId = item.id.ToString(),
								KeyId = item2.id.ToString(),
								KeyDescription = item2.descriptiveName + "-",
								Bind1 = ControlManager.ConvertBindingTextToGlyphs((list4.Count > 0) ? list4[0].elementIdentifierName : "", currentControllerType),
								rewiredBind1 = ((list4.Count > 0) ? list4[0] : null),
								Bind2 = ControlManager.ConvertBindingTextToGlyphs((list4.Count > 1) ? list4[1].elementIdentifierName : "", currentControllerType),
								rewiredBind2 = ((list4.Count > 1) ? list4[1] : null),
								rewiredInputAction = item2,
								rewiredInputCategory = item,
								rewiredControllerMap = controllerMap,
								rewiredAxisRange = AxisRange.Negative,
								rewairedAxisContribution = Pole.Negative
							});
						}
					}
				}
				return;
			}
		}
		foreach (string item5 in LegacyKeyMapping.CategoriesInOrder)
		{
			menuItems.Add(new KeybindCategoryRow
			{
				CategoryId = item5,
				CategoryDescription = item5
			});
			if (!categoryExpanded.ContainsKey(item5))
			{
				categoryExpanded[item5] = true;
			}
			foreach (GameCommand item6 in LegacyKeyMapping.CommandsByCategory[item5])
			{
				int value = -1;
				int value2 = -1;
				LegacyKeyMapping.CurrentMap.getPrimaryCommandToKey(item6.ID)?.TryGetValue(item6.ID, out value);
				LegacyKeyMapping.CurrentMap.getSecondaryCommandToKey(item6.ID)?.TryGetValue(item6.ID, out value2);
				menuItems.Add(new KeybindDataRow
				{
					CategoryId = item6.Category,
					KeyId = item6.ID,
					KeyDescription = item6.DisplayText,
					Bind1 = ControlManager.ConvertModifierGlyphs(ConsoleLib.Console.Keyboard.MetaToString(value)),
					Bind2 = ControlManager.ConvertModifierGlyphs(ConsoleLib.Console.Keyboard.MetaToString(value2))
				});
			}
		}
	}
}
