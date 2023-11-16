using System;
using System.Collections.Generic;
using Assets.QupKit.Components;
using ConsoleLib.Console;
using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

[UIView("HighScores", false, false, false, "Menu", "HighScores", false, 0, false)]
public class HighScoresView : BaseView
{
	private static List<GameObject> ChoiceButtons = new List<GameObject>();

	public static string Title = "";

	public new static string Text = "";

	public static Action action1 = null;

	public static Action action2 = null;

	public static Action action3 = null;

	public static HighScoresView instance;

	public override void OnCreate()
	{
		instance = this;
		base.OnCreate();
	}

	public void CenterToItem(RectTransform obj)
	{
		ScrollRect childComponent = GetChildComponent<ScrollRect>("Controls/Scroll View");
		float num = GetChildComponent<RectTransform>("Controls/Scroll View").anchorMin.y - obj.anchoredPosition.y;
		num += (float)obj.transform.GetSiblingIndex() / (float)childComponent.content.transform.childCount;
		num /= 1000f;
		num = (childComponent.verticalNormalizedPosition = Mathf.Clamp01(1f - num));
		Debug.Log(num);
	}

	public void SetLeaderboardResults(string title, List<string> results, int playerPosition)
	{
		ClearChoices();
		GameObject gameObject = null;
		GameObject childGameObject = GetChildGameObject("Controls/Scroll View/Viewport/Content");
		for (int i = 0; i < results.Count; i++)
		{
			if (!string.IsNullOrEmpty(results[i]))
			{
				GameObject gameObject2 = PrefabManager.Create("HighScoreEntry");
				gameObject2.GetComponent<HoverTextButton>().CommandID = "Choice:" + i;
				gameObject2.GetComponent<HoverTextButton>().Hotkey = (UnityEngine.KeyCode)(49 + i);
				gameObject2.transform.Find("Subtext").gameObject.GetComponent<UnityEngine.UI.Text>().text = Sidebar.FormatToRTF(results[i]);
				ChoiceButtons.Add(gameObject2);
				gameObject2.transform.SetParent(childGameObject.transform, worldPositionStays: false);
				gameObject2.transform.localScale = new Vector3(1f, 1f, 1f);
				if (i == 0)
				{
					Select(gameObject2);
				}
				if (playerPosition != -1 && i == playerPosition)
				{
					gameObject = gameObject2;
				}
			}
		}
		GetChildComponent<UnityEngine.UI.Text>("Controls/Scroll View/Viewport/Content/Title").text = Sidebar.FormatToRTF(title);
		Canvas.ForceUpdateCanvases();
		GetChildComponent<ScrollRect>("Controls/Scroll View").verticalNormalizedPosition = 1f;
		GetChild("PrevBoard").rootObject.SetActive(title != "Local Scores");
		GetChild("NextBoard").rootObject.SetActive(title != "Local Scores");
		if (gameObject != null)
		{
			CenterToItem(gameObject.GetComponent<RectTransform>());
		}
	}

	public override void Enter()
	{
		base.Enter();
	}

	public void ClearChoices()
	{
		for (int i = 0; i < ChoiceButtons.Count; i++)
		{
			ChoiceButtons[i].Destroy();
		}
		ChoiceButtons.Clear();
	}

	public override void OnLeave()
	{
		ClearChoices();
	}

	public override void OnCommand(string Command)
	{
		if (Command.StartsWith("Choice:"))
		{
			int num = Convert.ToInt32(Command.Split(':')[1]);
			Keyboard.PushKey(new Keyboard.XRLKeyEvent((UnityEngine.KeyCode)(49 + num), (char)(49 + num)));
		}
		if (Command.StartsWith("Trade"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Tab, '\t'));
		}
		if (Command.StartsWith("Page:"))
		{
			Keyboard.PushMouseEvent(Command);
		}
		if (Command.StartsWith("PrevBoard"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad7));
		}
		if (Command.StartsWith("NextBoard"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad9));
		}
		if (Command.StartsWith("Back"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
		}
		base.OnCommand(Command);
	}
}
