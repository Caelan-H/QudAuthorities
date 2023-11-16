using System;
using ConsoleLib.Console;
using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

[UIView("Popup:AskString", false, false, false, "StringInput", "Popup:AskString", false, 0, false)]
public class Popup_AskString : BaseView
{
	public static string Title;

	public static string Default;

	public static Action<string> callback;

	public Vector2 originalDialogSize = Vector2.zero;

	public Vector2 originalTextSize = Vector2.zero;

	public override void Enter()
	{
		base.Enter();
		if (originalDialogSize == Vector2.zero)
		{
			originalDialogSize = GetChildComponent<RectTransform>("Controls").sizeDelta;
			originalTextSize = GetChildComponent<RectTransform>("Controls/Title").sizeDelta;
		}
		GetChildComponent<InputField>("Controls/Input").text = Default;
		GetChildComponent<InputField>("Controls/Input").caretPosition = Default.Length;
		base.EventSystemManager.SetSelectedGameObject(GetChildGameObject("Controls/Input"));
		GetChildComponent<InputField>("Controls/Input").ActivateInputField();
		GetChildComponent<UnityEngine.UI.Text>("Controls/Title").text = Title;
		Canvas.ForceUpdateCanvases();
		GetChildComponent<RectTransform>("Controls/Title").sizeDelta = new Vector2(GetChildComponent<UnityEngine.UI.Text>("Controls/Title").preferredWidth, GetChildComponent<UnityEngine.UI.Text>("Controls/Title").preferredHeight);
		Vector2 vector = GetChildComponent<RectTransform>("Controls/Title").sizeDelta;
		if (vector.x < 850f)
		{
			vector = new Vector2(850f, vector.y);
		}
		if (vector.y < originalTextSize.y)
		{
			vector = new Vector2(vector.x, originalTextSize.y);
		}
		GetChildComponent<RectTransform>("Controls").sizeDelta = originalDialogSize + (vector - originalTextSize);
		CapabilityManager.SuggestOnscreenKeyboard();
	}

	public override void OnCommand(string Command)
	{
		if (Command == "Ok")
		{
			if (callback != null)
			{
				callback(GetChildComponent<InputField>("Controls/Input").text);
				callback = null;
			}
			else
			{
				Keyboard.PushMouseEvent("SubmitString:" + GetChildComponent<InputField>("Controls/Input").text);
			}
		}
		if (Command == "Cancel" || Command == "Back")
		{
			if (callback != null)
			{
				callback(null);
				callback = null;
			}
			else
			{
				Keyboard.PushKey(UnityEngine.KeyCode.Escape);
			}
		}
		base.OnCommand(Command);
	}
}
