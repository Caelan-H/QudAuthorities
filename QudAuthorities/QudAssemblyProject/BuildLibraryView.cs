using System;
using System.Collections.Generic;
using Assets.QupKit.Components;
using ConsoleLib.Console;
using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;

[UIView("BuildLibrary", false, false, false, "Menu", "BuildLibrary", false, 0, false)]
public class BuildLibraryView : BaseView
{
	private static List<GameObject> ChoiceButtons = new List<GameObject>();

	public static List<char> Hotkeys = new List<char>();

	public static string Title = "";

	public new static string Text = "";

	public static Action action1 = null;

	public static Action action2 = null;

	public static Action action3 = null;

	public Vector2 originalDialogSize = Vector2.zero;

	public Vector2 originalTextSize = Vector2.zero;

	public static void SetExplicitNavUp(Selectable from, Selectable to)
	{
		Navigation navigation = from.navigation;
		navigation.selectOnUp = to;
		from.navigation = navigation;
		Navigation navigation2 = to.navigation;
		navigation2.selectOnDown = from;
		to.navigation = navigation2;
	}

	public void Refresh()
	{
		for (int i = 0; i < ChoiceButtons.Count; i++)
		{
			ChoiceButtons[i].Destroy();
		}
		ChoiceButtons.Clear();
		base.Enter();
		BuildLibrary.Init();
		if (originalDialogSize == Vector2.zero)
		{
			originalDialogSize = GetChildComponent<RectTransform>("Controls").sizeDelta;
		}
		for (int j = 0; j < BuildLibrary.BuildEntries.Count; j++)
		{
			GameObject childGameObject = GetChildGameObject("Controls/Scroll View/Viewport/Content");
			GameObject gameObject = PrefabManager.Create("BuildLibraryDialogButton");
			gameObject.GetComponent<HoverTextButton>().CommandID = "Choice:" + j;
			string text = "";
			if (Hotkeys != null && Hotkeys.Count > j)
			{
				gameObject.GetComponent<HoverTextButton>().Hotkey = (UnityEngine.KeyCode)(97 + (Hotkeys[j] - 97));
				text = "&W" + Hotkeys[j] + "&y) ";
			}
			else
			{
				gameObject.GetComponent<HoverTextButton>().Hotkey = UnityEngine.KeyCode.None;
			}
			_ = gameObject.GetComponent<Button>().navigation;
			if (j == 0)
			{
				SetExplicitNavUp(gameObject.GetComponent<Button>(), FindChild("PrevButton").GetComponent<Button>());
			}
			else if (j == BuildLibrary.BuildEntries.Count - 1)
			{
				SetExplicitNavUp(FindChild("PrevButton").GetComponent<Button>(), gameObject.GetComponent<Button>());
			}
			if (j > 0)
			{
				SetExplicitNavUp(gameObject.GetComponent<Button>(), ChoiceButtons[j - 1].GetComponent<Button>());
			}
			gameObject.transform.Find("Controls/Rename").gameObject.GetComponent<HoverTextButton>().CommandID = "Rename:" + j;
			gameObject.transform.Find("Controls/Delete").gameObject.GetComponent<HoverTextButton>().CommandID = "Delete:" + j;
			gameObject.transform.Find("Controls/Tweet").gameObject.GetComponent<HoverTextButton>().CommandID = "Tweet:" + j;
			gameObject.transform.Find("Controls/Copy").gameObject.GetComponent<HoverTextButton>().CommandID = "Copy:" + j;
			gameObject.transform.Find("Text/Name").gameObject.GetComponent<UnityEngine.UI.Text>().text = Sidebar.FormatToRTF(text + BuildLibrary.BuildEntries[j].Name);
			gameObject.transform.Find("Text/Code").gameObject.GetComponent<UnityEngine.UI.Text>().text = Sidebar.FormatToRTF(text + BuildLibrary.BuildEntries[j].Code.ToUpper());
			ChoiceButtons.Add(gameObject);
			gameObject.transform.parent = childGameObject.transform;
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			if (j == 0)
			{
				Select(gameObject);
			}
		}
		Canvas.ForceUpdateCanvases();
		GetChildComponent<ScrollRect>("Controls/Scroll View").verticalNormalizedPosition = 1f;
	}

	public override void Enter()
	{
		Refresh();
	}

	public override void OnLeave()
	{
		for (int i = 0; i < ChoiceButtons.Count; i++)
		{
			ChoiceButtons[i].Destroy();
		}
		ChoiceButtons.Clear();
	}

	public override void OnCommand(string Command)
	{
		if (Command.StartsWith("Copy:"))
		{
			int index = Convert.ToInt32(Command.Split(':')[1]);
			ClipboardHelper.SetClipboardData(BuildLibrary.BuildEntries[index].Code.ToUpper());
		}
		if (Command.StartsWith("Delete:"))
		{
			int j = Convert.ToInt32(Command.Split(':')[1]);
			Popup_MessageBox.Buttons = 2;
			Popup_MessageBox.Default = 2;
			Popup_MessageBox.Button1 = "Yes";
			Popup_MessageBox.Button2 = "No";
			Popup_MessageBox.Text = Sidebar.FormatToRTF("Are you sure you want to delete " + BuildLibrary.BuildEntries[j].Name + "?");
			Popup_MessageBox.Title = "Delete Build";
			Popup_MessageBox.action1 = delegate
			{
				BuildLibrary.DeleteBuild(BuildLibrary.BuildEntries[j].Code.ToUpper());
				LegacyViewManager.Instance.SetActiveView("BuildLibrary");
				Refresh();
			};
			Popup_MessageBox.action2 = delegate
			{
				LegacyViewManager.Instance.SetActiveView("BuildLibrary");
				Refresh();
			};
			LegacyViewManager.Instance.SetActiveView("Popup:MessageBox", bHideOldView: false);
		}
		if (Command.StartsWith("Rename:"))
		{
			int i = Convert.ToInt32(Command.Split(':')[1]);
			Popup_AskString.Default = BuildLibrary.BuildEntries[i].Name;
			Popup_AskString.Title = "Enter a new name for " + BuildLibrary.BuildEntries[i].Name + ".";
			Popup_AskString.callback = delegate(string str)
			{
				if (str != null)
				{
					BuildLibrary.BuildEntries[i].Name = str;
					BuildLibrary.UpdateBuild(BuildLibrary.BuildEntries[i]);
				}
				LegacyViewManager.Instance.SetActiveView("BuildLibrary");
			};
			LegacyViewManager.Instance.SetActiveView("Popup:AskString", bHideOldView: false);
		}
		if (Command.StartsWith("Tweet:"))
		{
			int index2 = Convert.ToInt32(Command.Split(':')[1]);
			Social.ShareToTwitter("HEY! Try my Caves of Qud character build. I call it, \"" + BuildLibrary.BuildEntries[index2].Name + "\".\n" + BuildLibrary.BuildEntries[index2].Code.ToUpper() + "\n#cavesofqud");
		}
		if (Command == "AddBuild")
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.E));
		}
		if (Command == "Back")
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
		}
		if (Command.StartsWith("Choice:"))
		{
			Keyboard.PushMouseEvent(Command);
		}
		base.OnCommand(Command);
	}
}
