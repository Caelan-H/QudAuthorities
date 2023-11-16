using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Rewired;
using XRL.Core;
using XRL.World;

namespace XRL.UI;

public class FactionsScreen : IScreen
{
	private static int WriteFaction(ScreenBuffer SB, GameObject GO, string sFaction, int bSelected, int x, int y)
	{
		Faction faction = Factions.get(sFaction);
		if (!faction.Visible)
		{
			return 0;
		}
		if (bSelected == 1)
		{
			SB.Goto(x, y);
			SB.Write("> ");
		}
		else
		{
			SB.Goto(x + 2, y);
		}
		TextBlock textBlock = new TextBlock(faction.DisplayName, 30, 5);
		for (int i = 0; i < textBlock.Lines.Count; i++)
		{
			SB.Goto(x, y + i);
			if (bSelected == 1)
			{
				SB.Write("{{W|" + textBlock.Lines[i] + "}}");
			}
			else
			{
				SB.Write(textBlock.Lines[i]);
			}
		}
		if (bSelected == 2)
		{
			SB.Goto(x + 30, y);
			SB.Write("> ");
		}
		else
		{
			SB.Goto(x + 32, y);
		}
		int num = XRLCore.Core.Game.PlayerReputation.get(sFaction);
		char color = Reputation.getColor(num);
		SB.Write("{{" + color + "|" + string.Format("{0,5}", num) + "}}");
		return textBlock.Lines.Count;
	}

	public ScreenReturn Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("Factions");
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		List<string> factionsByName = new List<string>();
		int topRow = 0;
		int cursorRow = 0;
		int num = 0;
		foreach (Faction item in Factions.loop())
		{
			if (item.Visible)
			{
				factionsByName.Add(item.Name);
			}
		}
		factionsByName.Sort(new FactionNameComparer());
		Keys keys = Keys.None;
		bool flag = false;
		TextBlock TB = null;
		int tbStart = 0;
		int num2 = 18;
		int nDisplayed = 18;
		int num3 = 0;
		int num4 = -1;
		int num5 = -1;
		Action SetUpTextBlock = delegate
		{
			TB = new TextBlock(Faction.getRepPageDescription(factionsByName[cursorRow]), 28, 9999);
			tbStart = 0;
		};
		Action action = delegate
		{
			cursorRow = Math.Max(cursorRow - 1, 0);
			if (cursorRow == topRow - 1)
			{
				topRow = Math.Max(topRow - 1, 0);
			}
			SetUpTextBlock();
		};
		Action action2 = delegate
		{
			cursorRow = Math.Min(cursorRow + 1, factionsByName.Count - 1);
			if (cursorRow >= topRow + nDisplayed)
			{
				topRow = Math.Min(topRow + (1 + (18 - nDisplayed)), factionsByName.Count - 1);
			}
			SetUpTextBlock();
		};
		SetUpTextBlock();
		string s = "< {{W|7}} Equipment | Quests {{W|9}} >";
		if (ControlManager.activeControllerType == ControllerType.Joystick)
		{
			s = "< {{W|" + ControlManager.getCommandInputDescription("Previous Page", mapGlyphs: false) + "}} Equipment | Quests {{W|" + ControlManager.getCommandInputDescription("Next Page", mapGlyphs: false) + "}} >";
		}
		while (!flag)
		{
			Event.ResetPool();
			scrapBuffer.Clear();
			scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			scrapBuffer.Goto(35, 0);
			scrapBuffer.Write("[ {{W|Reputation}} ]");
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
			scrapBuffer.Goto(5, 3);
			scrapBuffer.Write("{{W|Faction}}");
			scrapBuffer.Goto(35, 3);
			scrapBuffer.Write("{{W|Reputation}}");
			for (int i = 0; i + tbStart < TB.Lines.Count && i < num2; i++)
			{
				scrapBuffer.Goto(50, 5 + i);
				scrapBuffer.Write(TB.Lines[i + tbStart]);
			}
			if (TB.Lines.Count > num2)
			{
				int num6 = (int)((float)num2 / (float)TB.Lines.Count * 23f);
				int num7 = (int)((float)tbStart / (float)TB.Lines.Count * 23f);
				scrapBuffer.Fill(79, 1, 79, 23, 177, ColorUtility.MakeColor(ColorUtility.Bright(TextColor.Black), TextColor.Black));
				scrapBuffer.Fill(79, 1 + num7, 79, 1 + num7 + num6, 177, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			}
			if (num == 2)
			{
				scrapBuffer.Goto(48, 5);
				scrapBuffer.Write("{{Y|>}}");
			}
			int num8 = 5;
			nDisplayed = 18;
			for (int j = topRow; j < topRow + nDisplayed && j < factionsByName.Count; j++)
			{
				int num9 = ((j != cursorRow) ? WriteFaction(scrapBuffer, GO, factionsByName[j], 0, 3, num8) : WriteFaction(scrapBuffer, GO, factionsByName[j], 1 + num, 3, num8));
				num8 += num9;
				if (num9 > 0)
				{
					nDisplayed -= num9 - 1;
				}
				num3 = j;
			}
			if (num4 != -1)
			{
				if (topRow + nDisplayed > num4 && cursorRow > 0)
				{
					action();
					continue;
				}
				num4 = -1;
			}
			if (num5 != -1)
			{
				if (cursorRow - nDisplayed < num5 && cursorRow < factionsByName.Count - 1)
				{
					action2();
					continue;
				}
				num5 = -1;
			}
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			keys = ConsoleLib.Console.Keyboard.getvk(MapDirectionToArrows: true);
			switch (keys)
			{
			case Keys.NumPad7:
				GameManager.Instance.PopGameView();
				return ScreenReturn.Previous;
			case Keys.NumPad9:
				GameManager.Instance.PopGameView();
				return ScreenReturn.Next;
			case Keys.Escape:
			case Keys.NumPad5:
				GameManager.Instance.PopGameView();
				return ScreenReturn.Exit;
			case Keys.Right:
			case Keys.NumPad6:
				num++;
				break;
			}
			if (keys == Keys.Left || keys == Keys.NumPad4)
			{
				num--;
			}
			if (num < 0)
			{
				num = 0;
			}
			if (num > 2)
			{
				num = 2;
			}
			if (keys == Keys.Up || keys == Keys.NumPad8)
			{
				if (num < 2)
				{
					action();
				}
				else if (tbStart > 0)
				{
					tbStart--;
				}
			}
			if (keys == Keys.Down || keys == Keys.NumPad2)
			{
				if (num < 2)
				{
					action2();
				}
				else if (tbStart < TB.Lines.Count - num2)
				{
					tbStart++;
				}
			}
			if (keys == Keys.Prior)
			{
				if (num < 2)
				{
					if (cursorRow == topRow)
					{
						num4 = cursorRow;
					}
					else
					{
						cursorRow = topRow;
						SetUpTextBlock();
					}
				}
				else
				{
					tbStart = Math.Max(tbStart - num2, 0);
				}
			}
			if (keys == Keys.Next)
			{
				if (num < 2)
				{
					if (cursorRow == num3)
					{
						num5 = cursorRow;
					}
					else
					{
						cursorRow = num3;
						SetUpTextBlock();
					}
				}
				else
				{
					tbStart = Math.Max(Math.Min(tbStart + num2, TB.Lines.Count - num2), 0);
				}
			}
			if (keys == Keys.Enter || keys == Keys.Space)
			{
				if (num == 0)
				{
					factionsByName.Sort(new FactionNameComparer());
				}
				if (num == 1)
				{
					factionsByName.Sort(new FactionRepComparer());
				}
			}
		}
		GameManager.Instance.PopGameView();
		return ScreenReturn.Exit;
	}
}
