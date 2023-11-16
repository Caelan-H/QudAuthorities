using System;
using System.Collections;
using System.Collections.Generic;
using ConsoleLib.Console;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

namespace Qud.UI;

[ExecuteAlways]
[UIView("PopupMessage", false, false, false, null, "PopupMessage", false, 0, false, IgnoreForceFullscreen = true, UICanvasHost = 1)]
public class PopupMessage : WindowBase
{
	public UITextSkin Message;

	public Action<QudMenuItem> commandCallback;

	public Action<QudMenuItem> selectCallback;

	public QudTextMenuController controller;

	public ControlledInputField inputBox;

	public UIThreeColorProperties contextImage;

	public UITextSkin contextText;

	public GameObject contextContainer;

	public GameObject TitleContainer;

	public UITextSkin Title;

	public static List<QudMenuItem> AnyKey = new List<QudMenuItem>();

	public static List<QudMenuItem> _CancelButton = new List<QudMenuItem>
	{
		new QudMenuItem
		{
			text = "{{W|[Esc]}} {{y|Cancel}}",
			command = "Cancel",
			hotkey = "Cancel"
		}
	};

	public static List<QudMenuItem> _CopyButton = new List<QudMenuItem>
	{
		new QudMenuItem
		{
			text = "{{W|[C]}} {{y|Copy}}",
			command = "Copy",
			hotkey = "char:c"
		},
		new QudMenuItem
		{
			text = "{{W|[Esc]}} {{y|Cancel}}",
			command = "Cancel",
			hotkey = "Cancel"
		}
	};

	public static List<QudMenuItem> _SingleButton = new List<QudMenuItem>
	{
		new QudMenuItem
		{
			text = "{{W|press space}}",
			command = "Cancel",
			hotkey = "Cancel"
		}
	};

	public static List<QudMenuItem> _YesNoButton = new List<QudMenuItem>
	{
		new QudMenuItem
		{
			text = "{{W|[y]}} {{y|Yes}}",
			command = "Yes",
			hotkey = "Y"
		},
		new QudMenuItem
		{
			text = "{{W|[n]}} {{y|No}}",
			command = "No",
			hotkey = "N,Cancel"
		}
	};

	public static List<QudMenuItem> _YesNoButtonJoystick = new List<QudMenuItem>
	{
		new QudMenuItem
		{
			text = "{{y|Yes}}",
			command = "Yes",
			hotkey = "Y"
		},
		new QudMenuItem
		{
			text = "{{y|No}}",
			command = "No",
			hotkey = "N,Cancel"
		}
	};

	public static List<QudMenuItem> _YesNoCancelButton = new List<QudMenuItem>
	{
		new QudMenuItem
		{
			text = "{{W|[y]}} {{y|Yes}}",
			command = "Yes",
			hotkey = "Y"
		},
		new QudMenuItem
		{
			text = "{{W|[n]}} {{y|No}}",
			command = "No",
			hotkey = "N"
		},
		new QudMenuItem
		{
			text = "{{W|[Esc]}} {{y|Cancel}}",
			command = "Cancel",
			hotkey = "Cancel"
		}
	};

	public static List<QudMenuItem> _AcceptCancelButton = new List<QudMenuItem>
	{
		new QudMenuItem
		{
			text = "{{W|[Enter]}} {{y|Accept}}",
			command = "keep",
			hotkey = "Submit"
		},
		new QudMenuItem
		{
			text = "{{W|[Esc]}} {{y|Cancel}}",
			command = "Cancel",
			hotkey = "Cancel"
		}
	};

	public static List<QudMenuItem> _AcceptCancelColorButton = new List<QudMenuItem>
	{
		new QudMenuItem
		{
			text = "{{W|[Enter]}} {{y|Accept}}",
			command = "keep",
			hotkey = "Submit"
		},
		new QudMenuItem
		{
			text = "{{W|[Esc]}} {{y|Cancel}}",
			command = "Cancel",
			hotkey = "Cancel"
		},
		new QudMenuItem
		{
			text = "{{W|[F1]}} {{y|Color}}",
			command = "Color",
			hotkey = "CmdHelp"
		}
	};

	public static List<QudMenuItem> _AcceptCancelTradeButton = new List<QudMenuItem>
	{
		new QudMenuItem
		{
			text = "{{W|[Tab] [T]}} {{y|Trade}}",
			command = "trade",
			hotkey = "char:t,Tab"
		},
		new QudMenuItem
		{
			text = "{{W|[Enter]}} {{y|Accept}}",
			command = "keep",
			hotkey = "Submit"
		},
		new QudMenuItem
		{
			text = "{{W|[Esc]}} {{y|Cancel}}",
			command = "Cancel",
			hotkey = "Cancel"
		}
	};

	public static List<QudMenuItem> AcceptButton = new List<QudMenuItem>
	{
		new QudMenuItem
		{
			text = "{{W|[Enter]}} {{y|Accept}}",
			command = "keep",
			hotkey = "Submit"
		}
	};

	public Action onHide;

	public bool wasPushed;

	public static List<QudMenuItem> CancelButton
	{
		get
		{
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				return new List<QudMenuItem>
				{
					new QudMenuItem
					{
						text = "{{W|" + ControlManager.getCommandInputDescription("Cancel") + "}} Cancel",
						command = "Cancel",
						hotkey = "Cancel"
					}
				};
			}
			return _CancelButton;
		}
	}

	public static List<QudMenuItem> CopyButton
	{
		get
		{
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				return new List<QudMenuItem>
				{
					new QudMenuItem
					{
						text = "{{y|Copy}}",
						command = "Copy",
						hotkey = "char:c"
					},
					new QudMenuItem
					{
						text = "{{W|" + ControlManager.getCommandInputDescription("Cancel") + "}} {{y|Cancel}}",
						command = "Cancel",
						hotkey = "Cancel"
					}
				};
			}
			return _CopyButton;
		}
	}

	public static List<QudMenuItem> SingleButton
	{
		get
		{
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				return new List<QudMenuItem>
				{
					new QudMenuItem
					{
						text = "{{W|press " + ControlManager.getCommandInputDescription("Accept") + "}}",
						command = "Cancel",
						hotkey = "Cancel"
					}
				};
			}
			return _SingleButton;
		}
	}

	public static List<QudMenuItem> YesNoButton
	{
		get
		{
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				return _YesNoButtonJoystick;
			}
			return _YesNoButton;
		}
	}

	public static List<QudMenuItem> YesNoCancelButton
	{
		get
		{
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				return new List<QudMenuItem>
				{
					new QudMenuItem
					{
						text = "{{y|Yes}}",
						command = "Yes",
						hotkey = "Y"
					},
					new QudMenuItem
					{
						text = "{{y|No}}",
						command = "No",
						hotkey = "N"
					},
					new QudMenuItem
					{
						text = "{{W|" + ControlManager.getCommandInputDescription("Cancel") + " Cancel}}",
						command = "Cancel",
						hotkey = "Cancel"
					}
				};
			}
			return _YesNoCancelButton;
		}
	}

	public static List<QudMenuItem> AcceptCancelButton
	{
		get
		{
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				return new List<QudMenuItem>
				{
					new QudMenuItem
					{
						text = "{{W|" + ControlManager.getCommandInputDescription("Accept") + "}} {{y|Accept}}",
						command = "keep",
						hotkey = "Submit"
					},
					new QudMenuItem
					{
						text = "{{W|" + ControlManager.getCommandInputDescription("Cancel") + "}} {{y|Cancel}}",
						command = "Cancel",
						hotkey = "Cancel"
					}
				};
			}
			return _AcceptCancelButton;
		}
	}

	public static List<QudMenuItem> AcceptCancelColorButton
	{
		get
		{
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				return new List<QudMenuItem>
				{
					new QudMenuItem
					{
						text = "{{W|" + ControlManager.getCommandInputDescription("Accept") + "}} {{y|Accept}}",
						command = "keep",
						hotkey = "Submit"
					},
					new QudMenuItem
					{
						text = "{{W|" + ControlManager.getCommandInputDescription("Cancel") + "}} {{y|Cancel}}",
						command = "Cancel",
						hotkey = "Cancel"
					},
					new QudMenuItem
					{
						text = "{{W|" + ControlManager.getCommandInputDescription("Help") + "}} {{y|Color}}",
						command = "Color",
						hotkey = "CmdHelp"
					}
				};
			}
			return _AcceptCancelColorButton;
		}
	}

	public static List<QudMenuItem> AcceptCancelTradeButton
	{
		get
		{
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				return new List<QudMenuItem>
				{
					new QudMenuItem
					{
						text = "{{y|Trade}}",
						command = "trade",
						hotkey = "char:t,Tab"
					},
					new QudMenuItem
					{
						text = "{{W|" + ControlManager.getCommandInputDescription("Accept") + "}} {{y|Accept}}",
						command = "keep",
						hotkey = "Submit"
					},
					new QudMenuItem
					{
						text = "{{W|" + ControlManager.getCommandInputDescription("Cancel") + "}} {{y|Cancel}}",
						command = "Cancel",
						hotkey = "Cancel"
					}
				};
			}
			return _AcceptCancelTradeButton;
		}
	}

	public override void Init()
	{
		controller.isCurrentWindow = base.isCurrentWindow;
		base.Init();
	}

	public void ShowPopup(string message, List<QudMenuItem> buttons, Action<QudMenuItem> commandCallback = null, List<QudMenuItem> items = null, Action<QudMenuItem> selectedItemCallback = null, string title = null, bool includeInput = false, string inputDefault = null, int defaultSelected = 0, Action onHide = null, IRenderable contextRender = null, string contextTitle = null, bool pushView = true)
	{
		this.onHide = onHide;
		base.canvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: true);
		Message.SetText("{{y|" + message + "}}");
		Message.GetComponent<LayoutElement>().minWidth = 0f;
		this.commandCallback = commandCallback;
		selectCallback = selectedItemCallback;
		controller.menuData = items ?? new List<QudMenuItem>();
		controller.bottomContextOptions = buttons;
		if (string.IsNullOrEmpty(title))
		{
			TitleContainer.SetActive(value: false);
		}
		else
		{
			TitleContainer.SetActive(value: true);
			Title.SetText("{{W|" + title + "}}");
		}
		if (includeInput)
		{
			inputBox.gameObject.SetActive(value: true);
			if (!controller.inputFields.Contains(inputBox))
			{
				controller.inputFields.Add(inputBox);
			}
			inputBox.text = inputDefault ?? "";
			CapabilityManager.SuggestOnscreenKeyboard();
		}
		else
		{
			inputBox.gameObject.SetActive(value: false);
			if (controller.inputFields.Contains(inputBox))
			{
				controller.inputFields.Remove(inputBox);
			}
		}
		if (contextTitle != null || contextRender != null)
		{
			contextContainer.SetActive(value: true);
			if (contextRender != null)
			{
				contextImage.gameObject.SetActive(value: true);
				contextImage.FromRenderable(contextRender);
				if (contextImage.Background == ConsoleLib.Console.ColorUtility.ColorMap['k'])
				{
					contextImage.Background = Color.clear;
				}
			}
			else
			{
				contextImage.gameObject.SetActive(value: false);
			}
			if (!string.IsNullOrEmpty(contextTitle))
			{
				contextText.gameObject.SetActive(value: true);
				contextText.SetText(contextTitle);
			}
			else
			{
				contextText.gameObject.SetActive(value: false);
			}
		}
		else
		{
			contextContainer.SetActive(value: false);
		}
		controller.UpdateElements();
		LayoutRebuilder.ForceRebuildLayoutImmediate(controller.GetComponent<RectTransform>());
		if (pushView)
		{
			UIManager.pushWindow("PopupMessage");
		}
		wasPushed = pushView;
		controller.selectedOption = defaultSelected;
		StartCoroutine(SizeContentToWidth(defaultSelected));
	}

	public IEnumerator SizeContentToWidth(int defaultSelected)
	{
		controller.selectedOption = -1;
		controller.GetComponent<ContentSizeWithMax>().MaximumPreferredSize = new Vector2(840f, base.rectTransform.rect.height - 100f);
		LayoutRebuilder.ForceRebuildLayoutImmediate(controller.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(controller.GetComponent<RectTransform>());
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(controller.GetComponent<RectTransform>());
		yield return null;
		float num = controller.GetComponent<RectTransform>().rect.width - 80f;
		if (Message.preferredWidth < num)
		{
			Message.GetComponent<LayoutElement>().minWidth = num;
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(controller.GetComponent<RectTransform>());
		yield return null;
		base.canvasGroup.alpha = 1f;
		yield return new WaitForEndOfFrame();
		controller.Reselect(defaultSelected);
	}

	public void OnActivateCommand(QudMenuItem command)
	{
		if (commandCallback != null)
		{
			commandCallback(command);
			commandCallback = null;
			Hide();
		}
	}

	public void OnSelect(QudMenuItem command)
	{
		if (selectCallback != null)
		{
			selectCallback(command);
			selectCallback = null;
			Hide();
		}
	}

	public void OnGUI()
	{
		try
		{
			if (controller.bottomContextOptions.Count == 0 && !Options.PrereleaseInputManager)
			{
				GameManager.pushKeyEvents();
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("keybind", x);
		}
	}

	public void Update()
	{
		if (controller.bottomContextOptions.Count == 0 && controller.menuData.Count == 0)
		{
			if (Options.PrereleaseInputManager)
			{
				if (Input.anyKeyDown)
				{
					commandCallback(default(QudMenuItem));
					selectCallback = null;
					Hide();
				}
			}
			else if (ConsoleLib.Console.Keyboard.HasKey())
			{
				ConsoleLib.Console.Keyboard.getch();
				commandCallback(default(QudMenuItem));
				selectCallback = null;
				Hide();
			}
		}
		foreach (QudMenuItem menuDatum in controller.menuData)
		{
			if (menuDatum.hotkey == null || !menuDatum.hotkey.StartsWith("char:"))
			{
				continue;
			}
			char c = ((menuDatum.hotkey.Length > 5) ? menuDatum.hotkey[5] : '\0');
			if (c != 0 && c != ' ' && (!inputBox.IsSelected() || c != '\b') && ControlManager.isCharDown(c))
			{
				if (commandCallback != null)
				{
					commandCallback(menuDatum);
					commandCallback = null;
					Hide();
				}
				break;
			}
		}
	}

	public override void Hide()
	{
		if (wasPushed)
		{
			wasPushed = false;
			UIManager.popWindow();
		}
		if (onHide != null)
		{
			onHide();
			onHide = null;
		}
		base.Hide();
	}
}
