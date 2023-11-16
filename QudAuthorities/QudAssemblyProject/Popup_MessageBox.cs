using System;
using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

[UIView("Popup:MessageBox", false, false, false, "Menu", "Popup:MessageBox", false, 0, false)]
public class Popup_MessageBox : BaseView
{
	public static bool DimBackground = true;

	public static int Buttons = 0;

	public static int Default = 0;

	public static bool AllowEscape = true;

	public static string Button1 = "Yes";

	public static string Button2 = "No";

	public static string Button3 = "Cancel";

	public static string Title;

	public new static string Text;

	public static Action action1 = null;

	public static Action action2 = null;

	public static Action action3 = null;

	public Vector2 originalDialogSize = Vector2.zero;

	public Vector2 originalTextSize = Vector2.zero;

	public T MessageBox<T>()
	{
		return GetChildComponent<T>("Controls/MessageScrollArea/Viewport/Message");
	}

	public override void Leave()
	{
		DimBackground = true;
		base.Leave();
	}

	public override void Enter()
	{
		base.Enter();
		if (DimBackground)
		{
			GetChildComponent<RectTransform>("Image").gameObject.SetActive(value: true);
		}
		else
		{
			GetChildComponent<RectTransform>("Image").gameObject.SetActive(value: false);
		}
		if (originalDialogSize == Vector2.zero)
		{
			originalDialogSize = GetChildComponent<RectTransform>("Controls").sizeDelta;
			originalTextSize = GetChildComponent<RectTransform>("Controls/MessageScrollArea").sizeDelta;
		}
		GetChildGameObject("Controls/1Button").SetActive(Buttons == 1 && AllowEscape);
		GetChildGameObject("Controls/2Button").SetActive(Buttons == 2 && AllowEscape);
		GetChildGameObject("Controls/3Button").SetActive(Buttons == 3 && AllowEscape);
		GetChildGameObject("Controls/1ButtonNoEsc").SetActive(Buttons == 1 && !AllowEscape);
		GetChildGameObject("Controls/2ButtonNoEsc").SetActive(Buttons == 2 && !AllowEscape);
		GetChildGameObject("Controls/3ButtonNoEsc").SetActive(Buttons == 3 && !AllowEscape);
		string text = Buttons + (AllowEscape ? "Button" : "ButtonNoEsc");
		if (Buttons >= 1)
		{
			GetChildComponent<UnityEngine.UI.Text>("Controls/" + text + "/Button1/Text").text = Button1;
		}
		if (Buttons >= 2)
		{
			GetChildComponent<UnityEngine.UI.Text>("Controls/" + text + "/Button2/Text").text = Button2;
		}
		if (Buttons >= 3)
		{
			GetChildComponent<UnityEngine.UI.Text>("Controls/" + text + "/Button3/Text").text = Button3;
		}
		GetChildComponent<RectTransform>("Controls/MessageScrollArea").sizeDelta = originalTextSize;
		GetChildComponent<UnityEngine.UI.Text>("Controls/Title").text = Title;
		MessageBox<UnityEngine.UI.Text>().text = Text;
		Canvas.ForceUpdateCanvases();
		Vector2 sizeDelta = default(Vector2);
		sizeDelta.x = MessageBox<UnityEngine.UI.Text>().preferredWidth;
		sizeDelta.y = Math.Max(originalTextSize.y, MessageBox<UnityEngine.UI.Text>().preferredHeight);
		MessageBox<RectTransform>().sizeDelta = sizeDelta;
		sizeDelta = MessageBox<RectTransform>().sizeDelta;
		Vector2 sizeDelta2 = GameObject.Find("Legacy Main Canvas").GetComponent<RectTransform>().sizeDelta;
		Vector2 vector = originalDialogSize + (sizeDelta - originalTextSize);
		sizeDelta2 *= 0.95f;
		if (sizeDelta2.y < vector.y)
		{
			sizeDelta.y -= vector.y - sizeDelta2.y;
			sizeDelta.x += 20f;
		}
		if (sizeDelta2.x < vector.x)
		{
			sizeDelta.x -= vector.x - sizeDelta2.x;
		}
		GetChildComponent<RectTransform>("Controls/MessageScrollArea").sizeDelta = sizeDelta;
		sizeDelta.x = Math.Max(sizeDelta.x, 850f);
		GetChildComponent<RectTransform>("Controls").sizeDelta = originalDialogSize + (sizeDelta - originalTextSize);
		Select(GetChildGameObject("Controls/" + Buttons + "Button/Button" + Default));
	}

	public override void OnCommand(string Command)
	{
		if (Command == "Click1" && action1 != null)
		{
			action1();
		}
		if (Command == "Click2" && action2 != null)
		{
			action2();
		}
		if (Command == "Click3" && action3 != null)
		{
			action3();
		}
		base.OnCommand(Command);
	}
}
