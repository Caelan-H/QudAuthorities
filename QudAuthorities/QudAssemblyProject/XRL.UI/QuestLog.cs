using System.Collections.Generic;
using ConsoleLib.Console;
using Rewired;
using XRL.Core;
using XRL.World;

namespace XRL.UI;

public class QuestLog : IScreen
{
	public ScreenReturn Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("Quests");
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
		bool flag = false;
		int num = 0;
		List<string> list = new List<string>();
		list.Add("");
		foreach (Quest value in XRLCore.Core.Game.Quests.Values)
		{
			if (XRLCore.Core.Game.FinishedQuests.ContainsKey(value.ID))
			{
				continue;
			}
			list.Add(value.DisplayName);
			list.Add("");
			foreach (QuestStep value2 in value.StepsByID.Values)
			{
				int MaxClippedWidth = 74;
				string input = ((!value2.Finished) ? (" {{white|ù " + value2.Name + "}}") : (" {{green|û}} {{white|" + value2.Name + "}}"));
				List<string> list2 = StringFormat.ClipTextToArray(input, 71, out MaxClippedWidth);
				for (int i = 0; i < list2.Count; i++)
				{
					if (i == 0)
					{
						list.Add(list2[i]);
					}
					if (i > 0)
					{
						list.Add("   " + list2[i]);
					}
				}
				if (!value2.Finished)
				{
					string[] array = value2.Text.Replace("\r", "").Split('\n');
					for (int j = 0; j < array.Length; j++)
					{
						foreach (string item in StringFormat.ClipTextToArray(array[j], 71, out MaxClippedWidth, KeepNewlines: true))
						{
							list.Add("   " + item);
						}
					}
				}
				list.Add("");
			}
			if (!string.IsNullOrEmpty(value.BonusAtLevel))
			{
				foreach (string item2 in value.BonusAtLevel.CachedCommaExpansion())
				{
					list.Add("  Bonus reward for completing this quest by level &C" + item2 + "&y.");
				}
			}
			list.Add("");
		}
		string s = "< {{W|7}} Factions | Journal {{W|9}} >";
		if (ControlManager.activeControllerType == ControllerType.Joystick)
		{
			s = "< {{W|" + ControlManager.getCommandInputDescription("Previous Page", mapGlyphs: false) + "}} Factions | Journal {{W|" + ControlManager.getCommandInputDescription("Next Page", mapGlyphs: false) + "}} >";
		}
		while (!flag)
		{
			Event.ResetPool();
			scrapBuffer.Clear();
			scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			scrapBuffer.Goto(37, 0);
			scrapBuffer.Write("[ &WQuests&y ]");
			scrapBuffer.Goto(79 - ColorUtility.StripFormatting(s).Length, 24);
			scrapBuffer.Write(s);
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				scrapBuffer.Goto(60, 0);
				scrapBuffer.Write(" {{W|" + ControlManager.getCommandInputDescription("Back", mapGlyphs: false) + "}} to exit ");
			}
			else
			{
				scrapBuffer.Goto(60, 0);
				scrapBuffer.Write(" {{W|ESC}} or {{W|5}} to exit ");
			}
			int num2 = 1;
			for (int k = 0; k < 23; k++)
			{
				scrapBuffer.Goto(4, num2 + k);
				if (num + k < list.Count)
				{
					scrapBuffer.Write(list[num + k]);
				}
			}
			if (list.Count > 23)
			{
				int num3 = (int)(23f / (float)list.Count * 23f);
				int num4 = (int)((float)num / (float)list.Count * 23f);
				scrapBuffer.Fill(79, 1, 79, 23, 177, ColorUtility.MakeColor(ColorUtility.Bright(TextColor.Black), TextColor.Black));
				scrapBuffer.Fill(79, 1 + num4, 79, 1 + num4 + num3, 177, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			}
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			Keys keys = ConsoleLib.Console.Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			else
			{
				switch (keys)
				{
				case Keys.NumPad2:
					num++;
					break;
				case Keys.Prior:
					num -= 23;
					break;
				case Keys.Next:
					num += 23;
					break;
				}
			}
			if (num > list.Count - 23)
			{
				num = list.Count - 23;
			}
			if (num < 0)
			{
				num = 0;
			}
			if (keys != Keys.Escape)
			{
				switch (keys)
				{
				case Keys.NumPad5:
					break;
				case Keys.Escape:
				case Keys.NumPad7:
					GameManager.Instance.PopGameView();
					return ScreenReturn.Previous;
				default:
					if (keys != Keys.Escape && keys != Keys.NumPad9)
					{
						continue;
					}
					GameManager.Instance.PopGameView();
					return ScreenReturn.Next;
				}
			}
			GameManager.Instance.PopGameView();
			return ScreenReturn.Exit;
		}
		GameManager.Instance.PopGameView();
		return ScreenReturn.Exit;
	}
}
