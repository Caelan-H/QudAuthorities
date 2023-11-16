using System;
using System.Collections.Generic;
using System.IO;
using Assets.QupKit.Components;
using ConsoleLib.Console;
using Qud.API;
using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

public class SaveManagementView : BaseView
{
	private static readonly string idContentControl = "Controls/Scroll View/Viewport/Content";

	private static readonly string idScrollViewControl = "Controls/Scroll View";

	public static SaveManagementView instance;

	private static List<GameObject> ChoiceButtons = new List<GameObject>();

	public static List<char> Hotkeys = new List<char>();

	public static string Title = "";

	public new static string Text = "";

	public static Action action1 = null;

	public static Action action2 = null;

	public static Action action3 = null;

	public Vector2 originalDialogSize = Vector2.zero;

	public Vector2 originalTextSize = Vector2.zero;

	public List<SaveGameInfo> games;

	public static void SetExplicitNavUp(Selectable from, Selectable to)
	{
		Navigation navigation = from.navigation;
		navigation.selectOnUp = to;
		from.navigation = navigation;
		Navigation navigation2 = to.navigation;
		navigation2.selectOnDown = from;
		to.navigation = navigation2;
	}

	private void Refresh()
	{
		for (int i = 0; i < ChoiceButtons.Count; i++)
		{
			ChoiceButtons[i].Destroy();
		}
		ChoiceButtons.Clear();
		games = SavesAPI.GetSavedGameInfo();
		if (originalDialogSize == Vector2.zero)
		{
			originalDialogSize = GetChildComponent<RectTransform>("Controls").sizeDelta;
		}
		for (int j = 0; j < games.Count; j++)
		{
			GameObject childGameObject = GetChildGameObject(idContentControl);
			GameObject gameObject = PrefabManager.Create("SaveManagementRow");
			gameObject.GetComponent<HoverTextButton>().CommandID = "Play:" + j;
			if (Hotkeys != null && Hotkeys.Count > j)
			{
				gameObject.GetComponent<HoverTextButton>().Hotkey = (UnityEngine.KeyCode)(97 + (Hotkeys[j] - 97));
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
			else if (j == games.Count - 1)
			{
				SetExplicitNavUp(FindChild("PrevButton").GetComponent<Button>(), gameObject.GetComponent<Button>());
			}
			if (j > 0)
			{
				SetExplicitNavUp(gameObject.GetComponent<Button>(), ChoiceButtons[j - 1].GetComponent<Button>());
			}
			gameObject.transform.Find("Controls/Delete").gameObject.GetComponent<HoverTextButton>().CommandID = "Delete:" + j;
			gameObject.transform.Find("Text/Name").gameObject.GetComponent<UnityEngine.UI.Text>().text = Sidebar.FormatToRTF(games[j].Name + ", " + games[j].Description + " [" + games[j].Version + "]");
			gameObject.transform.Find("Text/Code").gameObject.GetComponent<UnityEngine.UI.Text>().text = Sidebar.FormatToRTF(games[j].Info);
			UnityEngine.UI.Text component = gameObject.transform.Find("Text/Code").gameObject.GetComponent<UnityEngine.UI.Text>();
			component.text = component.text + "\n" + games[j].SaveTime;
			UnityEngine.UI.Text component2 = gameObject.transform.Find("Text/Code").gameObject.GetComponent<UnityEngine.UI.Text>();
			component2.text = component2.text + "\n{" + games[j].ID + "}";
			ChoiceButtons.Add(gameObject);
			gameObject.transform.SetParent(childGameObject.transform, worldPositionStays: true);
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			if (j == 0)
			{
				Select(gameObject);
			}
		}
		Canvas.ForceUpdateCanvases();
		GetChildComponent<ScrollRect>(idScrollViewControl).verticalNormalizedPosition = 1f;
	}

	public override void OnCreate()
	{
		instance = this;
	}

	public override void Enter()
	{
		base.Enter();
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
		if (Command.StartsWith("Play:"))
		{
			Keyboard.PushMouseEvent("Play:" + Convert.ToInt32(Command.Split(':')[1]));
		}
		if (Command.StartsWith("Delete:"))
		{
			int i = Convert.ToInt32(Command.Split(':')[1]);
			Popup_MessageBox.Buttons = 2;
			Popup_MessageBox.Default = 2;
			Popup_MessageBox.Button1 = "Yes";
			Popup_MessageBox.Button2 = "No";
			Popup_MessageBox.Text = Sidebar.FormatToRTF("Are you sure you want to delete " + games[i].Description + "?");
			Popup_MessageBox.Title = "Delete Build";
			Popup_MessageBox.action1 = delegate
			{
				Directory.Delete(games[i].Directory, recursive: true);
				LegacyViewManager.Instance.SetActiveView("SaveManagement");
				Refresh();
			};
			Popup_MessageBox.action2 = delegate
			{
				LegacyViewManager.Instance.SetActiveView("SaveManagement");
			};
			LegacyViewManager.Instance.SetActiveView("Popup:MessageBox", bHideOldView: false);
		}
		if (Command == "Back")
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
		}
		base.OnCommand(Command);
	}
}
