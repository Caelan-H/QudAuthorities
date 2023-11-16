using System;
using System.Collections.Generic;
using Assets.QupKit.Components;
using ConsoleLib.Console;
using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

[UIView("Options", false, true, false, "Menu", "Options", false, 0, false)]
public class OptionsView : BaseView
{
	private static List<GameObject> ChoiceButtons = new List<GameObject>();

	public static string Title = "";

	public new static string Text = "";

	public static Action action1 = null;

	public static Action action2 = null;

	public static Action action3 = null;

	public static OptionsView instance;

	public bool categoryInit;

	private string firstCategory;

	private GameObject firstCategoryControl;

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

	public void CheckChanged(string id, bool newValue)
	{
		Options.SetOption(id, newValue ? "Yes" : "No");
		Options.UpdateFlags();
	}

	public void ComboChanged(string id, string newValue)
	{
		Options.SetOption(id, newValue);
		Options.UpdateFlags();
	}

	public void SliderChanged(string id, string newValue)
	{
		Options.SetOption(id, newValue);
		Options.UpdateFlags();
	}

	public void SetCategory(string category)
	{
		ClearChoices();
		GameObject gameObject = null;
		GameObject childGameObject = GetChildGameObject("Controls/Scroll View/Viewport/Content");
		GameObject gameObject2 = PrefabManager.Create("OptionsHeader");
		gameObject2.GetComponent<UnityEngine.UI.Text>().text = category;
		gameObject2.transform.SetParent(childGameObject.transform);
		gameObject2.transform.localScale = new Vector3(1f, 1f, 1f);
		ChoiceButtons.Add(gameObject2);
		List<Selectable> list = new List<Selectable>();
		for (int i = 0; i < Options.OptionsByCategory[category].Count; i++)
		{
			GameOption option = Options.OptionsByCategory[category][i];
			GameObject gameObject3 = null;
			if (option.Type == "Checkbox")
			{
				gameObject3 = PrefabManager.Create("OptionsCheckbox");
				if (Options.GetOption(option.ID) == "Yes")
				{
					gameObject3.GetComponent<Toggle>().isOn = true;
				}
				else
				{
					gameObject3.GetComponent<Toggle>().isOn = false;
				}
				string oid2 = option.ID;
				gameObject3.GetComponent<Toggle>().onValueChanged.AddListener(delegate(bool newval)
				{
					CheckChanged(oid2, newval);
				});
			}
			else if (option.Type == "Button")
			{
				gameObject3 = PrefabManager.Create("OptionsButton");
				gameObject3.transform.Find("Title").GetComponent<UnityEngine.UI.Text>().text = option.DisplayText;
				if (option.ID == "OptionsKeybindings")
				{
					gameObject3.GetComponent<Button>().onClick.AddListener(delegate
					{
						Keyboard.PushMouseEvent("Keybindings");
					});
				}
			}
			else if (option.Type == "Slider")
			{
				gameObject3 = PrefabManager.Create("OptionsSlider");
				int num = Convert.ToInt32(Options.GetOption(option.ID));
				gameObject3.transform.Find("Slider").GetComponent<Slider>().value = (float)(num - option.Min) / (float)(option.Max - option.Min);
				string oid = option.ID;
				gameObject3.transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate(float newval)
				{
					int num2 = (int)Mathf.Clamp(newval * (float)(option.Max - option.Min) + (float)option.Min, option.Min, option.Max);
					SliderChanged(oid, num2.ToString());
				});
			}
			else if (option.Type == "Combo" || option.Type == "BigCombo")
			{
				gameObject3 = PrefabManager.Create("OptionsCombo");
				gameObject3.transform.Find("Dropdown").GetComponent<Dropdown>().options.Clear();
				foreach (string value in option.Values)
				{
					gameObject3.transform.Find("Dropdown").GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(value));
				}
				gameObject3.transform.Find("Dropdown").GetComponent<Dropdown>().captionText.text = Options.GetOption(option.ID);
				gameObject3.transform.Find("Dropdown").GetComponent<Dropdown>().value = option.Values.IndexOf(Options.GetOption(option.ID));
				gameObject3.transform.Find("Dropdown").GetComponent<Dropdown>().onValueChanged.AddListener(delegate(int newval)
				{
					string iD = option.ID;
					ComboChanged(iD, option.Values[newval]);
				});
			}
			if (gameObject3 == null)
			{
				Debug.LogError("Didn't create a control for :" + option.ID);
			}
			if (gameObject3 != null && gameObject3.transform.Find("Label") != null)
			{
				gameObject3.transform.Find("Label").GetComponent<UnityEngine.UI.Text>().text = Sidebar.FormatToRTF(option.DisplayText);
			}
			ChoiceButtons.Add(gameObject3);
			gameObject3.transform.SetParent(childGameObject.transform);
			gameObject3.transform.localScale = new Vector3(1f, 1f, 1f);
			if (gameObject3.GetComponent<Selectable>() != null)
			{
				list.Add(gameObject3.GetComponent<Selectable>());
			}
			else if ((bool)gameObject3.GetComponentInChildren<Selectable>())
			{
				list.Add(gameObject3.GetComponentInChildren<Selectable>());
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Navigation navigation = default(Navigation);
			navigation.mode = Navigation.Mode.Explicit;
			if (j == 0)
			{
				navigation.selectOnUp = list[list.Count - 1];
			}
			else
			{
				navigation.selectOnUp = list[j - 1];
			}
			if (j == list.Count - 1)
			{
				navigation.selectOnDown = list[0];
			}
			else
			{
				navigation.selectOnDown = list[j + 1];
			}
			if (list[j] as Slider != null)
			{
				navigation.selectOnLeft = null;
				navigation.selectOnRight = null;
			}
			else if (firstCategoryControl != null)
			{
				navigation.selectOnLeft = firstCategoryControl.GetComponent<Selectable>();
				navigation.selectOnRight = firstCategoryControl.GetComponent<Selectable>();
			}
			list[j].navigation = navigation;
		}
		Canvas.ForceUpdateCanvases();
		GetChildComponent<ScrollRect>("Controls/Scroll View").verticalNormalizedPosition = 1f;
		if (gameObject != null)
		{
			CenterToItem(gameObject.GetComponent<RectTransform>());
		}
	}

	public override void Enter()
	{
		base.Enter();
		if (!categoryInit)
		{
			categoryInit = true;
			foreach (string key in Options.OptionsByCategory.Keys)
			{
				GameObject gameObject = PrefabManager.Create("OptionsLeftTab");
				gameObject.GetComponent<HoverTextButton>().CommandID = "Category:" + key;
				gameObject.transform.Find("Title").gameObject.GetComponent<UnityEngine.UI.Text>().text = Sidebar.FormatToRTF(key);
				gameObject.transform.SetParent(GetChild("Categories").rootObject.transform);
				gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				if (firstCategory == null)
				{
					firstCategory = key;
					firstCategoryControl = gameObject;
				}
			}
		}
		Canvas.ForceUpdateCanvases();
		SetCategory(firstCategory);
		if (firstCategoryControl != null)
		{
			Select(firstCategoryControl);
		}
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
		if (Command.StartsWith("Category:"))
		{
			SetCategory(Command.Split(':')[1]);
		}
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
