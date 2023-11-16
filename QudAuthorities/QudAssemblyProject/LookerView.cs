using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using QupKit;
using UnityEngine;
using XRL.UI;
using XRL.World;

[UIView("Looker", false, false, false, "Looker,Menu", "Looker", false, 0, false)]
public class LookerView : BaseView
{
	private static List<UnityEngine.GameObject> ChoiceButtons = new List<UnityEngine.GameObject>();

	public static string Title = "";

	public new static string Text = "";

	public static Action action1 = null;

	public static Action action2 = null;

	public static Action action3 = null;

	public Vector2 originalDialogSize = Vector2.zero;

	public Vector2 originalTextSize = Vector2.zero;

	public static LookerView instance;

	public static XRL.World.GameObject lookedObject;

	public override void OnCreate()
	{
		instance = this;
		base.OnCreate();
	}

	public void UpdateLookedObject(XRL.World.GameObject GO)
	{
		lookedObject = GO;
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
		UpdateLookedObject(null);
	}

	public override void OnCommand(string Command)
	{
		if (Command.StartsWith("Choice:"))
		{
			int num = Convert.ToInt32(Command.Split(':')[1]);
			Keyboard.PushKey(new Keyboard.XRLKeyEvent((UnityEngine.KeyCode)(49 + num), (char)(49 + num)));
		}
		if (Command.StartsWith("Interact"))
		{
			Keyboard.PushMouseEvent("Interact");
		}
		if (Command.StartsWith("Back"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
		}
		base.OnCommand(Command);
	}
}
