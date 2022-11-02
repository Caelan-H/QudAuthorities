using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.UI;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace Qud.UI;

[ExecuteAlways]
[UIView("PlayerStatusBar", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "PlayerStatusBar", UICanvasHost = 1)]
public class PlayerStatusBar : SingletonWindowBase<PlayerStatusBar>
{
	private struct BarValue
	{
		public int Min;

		public int Max;

		public int Value;

		public void SetBar(HPBar bar)
		{
			bar.BarStart = Min;
			bar.BarEnd = Max;
			bar.BarValue = Value;
			bar.WantsUpdate = true;
		}
	}

	private enum BoolDataType
	{
		AutoExploreActive,
		ObjectFinderActive,
		MinimapActive,
		WindowsPinned
	}

	private enum StringDataType
	{
		FoodWater,
		Time,
		Temp,
		Weight,
		Zone,
		HPBar,
		PlayerName
	}

	public UIThreeColorProperties PlayerBody3c;

	public UnityEngine.GameObject TopLeftSecondaryStatBlock;

	public UnityEngine.GameObject TimeText;

	public UITextSkin FoodStatusText;

	public UITextSkin TempText;

	public UITextSkin WeightText;

	public UITextSkin ZoneText;

	public UITextSkin PlayerNameText;

	public Image TimeClockImage;

	public List<Sprite> QudTimeImages;

	public Sprite SheolTimeImage;

	public ActiveButton WindowPinButton;

	public ActiveButton ExploreButton;

	public ActiveButton FindButton;

	public ActiveButton MinimapButton;

	public List<UnityEngine.GameObject> Spacers;

	public HPBar HPBar;

	public HPBar XPBar;

	private StringBuilder SB = new StringBuilder();

	private ConsoleChar playerChar;

	private ConsoleChar lastPlayerChar;

	private bool playerCharDirty;

	private Dictionary<string, string> playerStats = new Dictionary<string, string>
	{
		{ "Level", "1" },
		{ "XP", "0" }
	};

	private Dictionary<string, string> lastPlayerStats = new Dictionary<string, string>();

	private bool playerStatsDirty;

	private object barLock = new object();

	private BarValue hpbarValue;

	private bool barValueDirty;

	private object boolLock = new object();

	private Dictionary<BoolDataType, bool> boolData = new Dictionary<BoolDataType, bool>
	{
		{
			BoolDataType.ObjectFinderActive,
			false
		},
		{
			BoolDataType.AutoExploreActive,
			false
		},
		{
			BoolDataType.MinimapActive,
			false
		},
		{
			BoolDataType.WindowsPinned,
			false
		}
	};

	private Dictionary<BoolDataType, bool> lastBoolData = new Dictionary<BoolDataType, bool>();

	private bool boolDataDirty;

	private int timeOfDayImage = -2;

	private int lastTimeOfDayImage = -2;

	private bool timeOfDayImageDirty;

	private object timeLock = new object();

	private Dictionary<StringDataType, string> playerStringData = new Dictionary<StringDataType, string> { 
	{
		StringDataType.FoodWater,
		""
	} };

	private Dictionary<StringDataType, string> lastPlayerStringData = new Dictionary<StringDataType, string>();

	private bool playerStringsDirty;

	private StringBuilder UpdateSB = new StringBuilder();

	private float LastWidth;

	private bool reflow;

	public void Awake()
	{
		playerChar = new ConsoleChar();
		lastPlayerChar = new ConsoleChar();
	}

	private void UpdateStat(string statName, Statistic stat, XRL.World.GameObject Player)
	{
		string text = stat.GetDisplayValue();
		if (stat.Name == "DV")
		{
			text = Stats.GetCombatDV(Player).ToString();
		}
		else if (stat.Name == "MA")
		{
			text = Stats.GetCombatMA(Player).ToString();
		}
		if (lastPlayerStats.TryGetValue(statName, out var value) && value == text)
		{
			return;
		}
		lock (playerStats)
		{
			string text4 = (lastPlayerStats[statName] = (playerStats[statName] = text));
			playerStatsDirty = true;
		}
	}

	private void UpdateBool(BoolDataType dataType, bool value)
	{
		if (lastBoolData.TryGetValue(dataType, out var value2) && value2 == value)
		{
			return;
		}
		lock (boolLock)
		{
			boolData[dataType] = value;
			lastBoolData[dataType] = value;
			boolDataDirty = true;
		}
	}

	private void UpdateString(StringDataType type, string data, bool toRTF = true)
	{
		if (lastPlayerStringData.TryGetValue(type, out var value) && value == data)
		{
			return;
		}
		lock (playerStringData)
		{
			lastPlayerStringData[type] = data;
			if (toRTF)
			{
				playerStringData[type] = RTF.FormatToRTF(data);
			}
			else
			{
				playerStringData[type] = data;
			}
			playerStringsDirty = true;
		}
	}

	public override void Init()
	{
		XRLCore.RegisterAfterRenderCallback(AfterRender);
		XRLCore.RegisterOnBeginPlayerTurnCallback(BeginEndTurn);
		XRLCore.RegisterOnEndPlayerTurnCallback(BeginEndTurn);
		XRLCore.RegisterOnPassedTenPlayerTurnCallback(BeginEndTurn);
	}

	private void AfterRender(XRLCore core, ScreenBuffer buffer)
	{
		XRL.World.GameObject body = core.Game.Player.Body;
		if (body != null)
		{
			Cell currentCell = body.GetCurrentCell();
			if (currentCell != null)
			{
				ConsoleChar consoleChar = buffer[currentCell.X, currentCell.Y];
				if (consoleChar != lastPlayerChar && playerChar != null)
				{
					lock (playerChar)
					{
						playerChar.Copy(consoleChar);
						lastPlayerChar.Copy(consoleChar);
						playerCharDirty = true;
					}
				}
			}
		}
		BeginEndTurn(core);
	}

	private void BeginEndTurn(XRLCore core)
	{
		XRL.World.GameObject body = core.Game.Player.Body;
		if (body == null)
		{
			return;
		}
		UpdateBool(BoolDataType.AutoExploreActive, AutoAct.IsAnyExploration());
		Cell currentCell = body.GetCurrentCell();
		if (currentCell != null && currentCell.ParentZone != null)
		{
			UpdateString(StringDataType.Zone, currentCell.ParentZone.DisplayName);
		}
		foreach (KeyValuePair<string, Statistic> statistic in body.Statistics)
		{
			UpdateStat(statistic.Key, statistic.Value, body);
		}
		UpdateString(StringDataType.PlayerName, body.DisplayNameOnlyDirect);
		if (body.GetPart("Stomach") is Stomach stomach)
		{
			UpdateString(StringDataType.FoodWater, stomach.FoodStatus() + " " + stomach.WaterStatus());
		}
		Inventory obj = body.GetPart("Inventory") as Inventory;
		Body body2 = body.GetPart("Body") as Body;
		if (obj != null && body2 != null)
		{
			int carriedWeight = body.GetCarriedWeight();
			int maxCarriedWeight = body.GetMaxCarriedWeight();
			int freeDrams = body.GetFreeDrams();
			UpdateString(StringDataType.Weight, carriedWeight + "/" + maxCarriedWeight + "# {{blue|" + freeDrams + "$}}");
		}
		UpdateString(StringDataType.Temp, "T:" + body.pPhysics.Temperature + "ø", toRTF: false);
		UpdateString(StringDataType.Time, Calendar.getTime() + " " + Calendar.getDay() + " of " + Calendar.getMonth());
		int num = -1;
		if ((body?.pPhysics?.CurrentCell?.ParentZone?._ZoneID?.StartsWith("Joppa")).GetValueOrDefault())
		{
			int num2 = Calendar.CurrentDaySegment / 10;
			num2 += 875;
			num2 %= 1200;
			int num3 = 675;
			if (num2 < num3)
			{
				num = num2 * 7 / num3;
			}
			else
			{
				num2 -= num3;
				num3 = 525;
				num = 7 + num2 * 3 / num3;
			}
		}
		if (num != lastTimeOfDayImage)
		{
			lock (timeLock)
			{
				lastTimeOfDayImage = (timeOfDayImage = num);
				timeOfDayImageDirty = true;
			}
		}
		if (body.GetIntProperty("Analgesia") > 0)
		{
			UpdateString(StringDataType.HPBar, "HP: " + Strings.WoundLevel(body));
			if (hpbarValue.Value != 1 || hpbarValue.Max != 1)
			{
				lock (barLock)
				{
					hpbarValue.Value = 1;
					hpbarValue.Max = 1;
					barValueDirty = true;
					return;
				}
			}
			return;
		}
		UpdateString(StringDataType.HPBar, "{{Y|HP: {{" + Strings.HealthStatusColor(body) + "|" + body.hitpoints + "}} / " + body.baseHitpoints + "}}");
		if (hpbarValue.Value != body.hitpoints || hpbarValue.Max != body.baseHitpoints)
		{
			lock (barLock)
			{
				hpbarValue.Value = body.hitpoints;
				hpbarValue.Max = body.baseHitpoints;
				barValueDirty = true;
			}
		}
	}

	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public void Update()
	{
		if (playerCharDirty)
		{
			lock (playerChar)
			{
				PlayerBody3c.FromConsoleChar(playerChar);
				if (PlayerBody3c.Background == ConsoleLib.Console.ColorUtility.ColorMap['k'])
				{
					PlayerBody3c.Background = Color.clear;
				}
				playerCharDirty = false;
			}
		}
		if (playerStatsDirty)
		{
			lock (playerStats)
			{
				TopLeftSecondaryStatBlock.GetComponent<StatusBarStatBlock>().UpdateStats(playerStats);
				XPBar.text.text = string.Format("LVL: {0} Exp: {1} / {2}", playerStats["Level"], playerStats["XP"], Leveler.GetXPForLevel(Convert.ToInt32(playerStats["Level"]) + 1));
				XPBar.BarStart = Leveler.GetXPForLevel(Convert.ToInt32(playerStats["Level"]));
				XPBar.BarEnd = Leveler.GetXPForLevel(Convert.ToInt32(playerStats["Level"]) + 1);
				XPBar.BarValue = Convert.ToInt32(playerStats["XP"]);
				XPBar.UpdateBar();
				playerStatsDirty = false;
			}
		}
		if (timeOfDayImageDirty)
		{
			lock (timeLock)
			{
				if (timeOfDayImage >= 0)
				{
					TimeClockImage.sprite = QudTimeImages[timeOfDayImage];
				}
				else
				{
					TimeClockImage.sprite = SheolTimeImage;
				}
			}
		}
		if (Application.isPlaying)
		{
			UpdateBool(BoolDataType.ObjectFinderActive, UIManager.getWindow("NearbyItems").Visible);
			UpdateBool(BoolDataType.MinimapActive, UIManager.getWindow("Minimap").Visible);
			UpdateBool(BoolDataType.WindowsPinned, UIManager.WindowFramePin == 0);
		}
		if (boolDataDirty)
		{
			lock (boolLock)
			{
				ExploreButton.IsActive = boolData[BoolDataType.AutoExploreActive];
				FindButton.IsActive = boolData[BoolDataType.ObjectFinderActive];
				MinimapButton.IsActive = boolData[BoolDataType.MinimapActive];
				WindowPinButton.IsActive = boolData[BoolDataType.WindowsPinned];
				boolDataDirty = false;
			}
		}
		if (playerStringsDirty)
		{
			lock (playerStringData)
			{
				if (FoodStatusText.text != playerStringData[StringDataType.FoodWater])
				{
					FoodStatusText.SetText(playerStringData[StringDataType.FoodWater]);
				}
				UITextSkin component = TimeText.GetComponent<UITextSkin>();
				if (playerStringData.ContainsKey(StringDataType.Time))
				{
					if (component.text != playerStringData[StringDataType.Time])
					{
						component.SetText(playerStringData[StringDataType.Time]);
					}
					TimeText.GetComponent<LayoutElement>().preferredWidth = Math.Min(350f, component.preferredWidth + 5f);
				}
				if (playerStringData.ContainsKey(StringDataType.Temp) && TempText.text != playerStringData[StringDataType.Temp])
				{
					TempText.SetText(playerStringData[StringDataType.Temp]);
				}
				if (playerStringData.ContainsKey(StringDataType.Weight) && WeightText.text != playerStringData[StringDataType.Weight])
				{
					WeightText.SetText(playerStringData[StringDataType.Weight]);
				}
				if (playerStringData.ContainsKey(StringDataType.Zone) && ZoneText.text != playerStringData[StringDataType.Zone])
				{
					ZoneText.SetText(playerStringData[StringDataType.Zone]);
				}
				if (playerStringData.ContainsKey(StringDataType.HPBar) && HPBar.text.text != playerStringData[StringDataType.HPBar])
				{
					HPBar.text.SetText(playerStringData[StringDataType.HPBar]);
				}
				if (playerStringData.ContainsKey(StringDataType.PlayerName) && PlayerNameText.text != playerStringData[StringDataType.PlayerName])
				{
					PlayerNameText.SetText(playerStringData[StringDataType.PlayerName]);
				}
				playerStringsDirty = false;
				LastWidth = 0f;
			}
		}
		if (barValueDirty)
		{
			hpbarValue.SetBar(HPBar);
		}
		HandleSizeUpdate();
	}

	public void HandleSizeUpdate()
	{
		if (LastWidth < base.rectTransform.rect.width)
		{
			Spacers[0].SetActive(value: true);
			PlayerNameText.gameObject.SetActive(value: true);
			Spacers[1].SetActive(value: true);
			TimeText.gameObject.SetActive(value: true);
			Spacers[2].SetActive(value: true);
			TopLeftSecondaryStatBlock.gameObject.SetActive(value: true);
			Spacers[3].SetActive(value: true);
			ZoneText.gameObject.SetActive(value: true);
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
		}
		LastWidth = base.rectTransform.rect.width;
		if (Spacers[0].activeInHierarchy && Spacers[0].GetComponent<RectTransform>().sizeDelta.x < 15f)
		{
			Spacers[0].SetActive(value: false);
			PlayerNameText.gameObject.SetActive(value: false);
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
		}
		if (Spacers[2].activeInHierarchy && Spacers[2].GetComponent<RectTransform>().sizeDelta.x < 15f)
		{
			Spacers[2].SetActive(value: false);
			TimeText.gameObject.SetActive(value: false);
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
		}
		if (Spacers[3].activeInHierarchy && Spacers[1].activeInHierarchy && Spacers[1].GetComponent<RectTransform>().sizeDelta.x < 15f)
		{
			Spacers[3].SetActive(value: false);
			ZoneText.gameObject.SetActive(value: false);
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
		}
		if (Spacers[1].activeInHierarchy && Spacers[1].GetComponent<RectTransform>().sizeDelta.x < 15f)
		{
			Spacers[1].SetActive(value: false);
			TopLeftSecondaryStatBlock.gameObject.SetActive(value: false);
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
		}
	}

	public void ClickButton(string button)
	{
		switch (button)
		{
		case "Explore":
			Keyboard.PushMouseEvent("Command:CmdAutoExplore");
			break;
		case "Up":
			Keyboard.PushMouseEvent("Command:CmdMoveU");
			break;
		case "Down":
			Keyboard.PushMouseEvent("Command:CmdMoveD");
			break;
		case "Char":
			Keyboard.PushMouseEvent("Command:CmdCharacter");
			break;
		case "Rest":
			Keyboard.PushMouseEvent("Command:CmdWaitMenu");
			break;
		case "Look":
			Keyboard.PushMouseEvent("Command:CmdLook");
			break;
		case "Finder":
			UIManager.getWindow<NearbyItemsWindow>("NearbyItems").TogglePreferredState();
			break;
		case "Minimap":
			UIManager.getWindow<MinimapWindow>("Minimap").TogglePreferredState();
			break;
		case "WindowLock":
			if (UIManager.WindowFramePin == 1)
			{
				UIManager.WindowFramePin = 0;
			}
			else
			{
				UIManager.WindowFramePin = 1;
			}
			break;
		}
	}
}
