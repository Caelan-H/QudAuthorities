using System.Text;
using ConsoleLib.Console;
using XRL.World;

namespace XRL.UI;

[UIView("NameEditor", false, true, false, null, null, false, 0, false)]
public class NameEditorUI : IWantsTextConsoleInit
{
	public static TextConsole _TextConsole;

	public static ScreenBuffer _ScreenBuffer;

	public void Init(TextConsole TextConsole_, ScreenBuffer ScreenBuffer_)
	{
		_TextConsole = TextConsole_;
		_ScreenBuffer = ScreenBuffer_;
	}

	public static void Show()
	{
		TextConsole.LoadScrapBuffers();
		bool flag = false;
		int num = 0;
		ConsoleChar[] array = new ConsoleChar[60];
		for (int i = 0; i < 60; i++)
		{
			array[i] = new ConsoleChar(' ', TextColor.Grey);
		}
		while (!flag)
		{
			Event.ResetPool(resetMinEventPools: false);
			_ScreenBuffer.Clear();
			_ScreenBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			_ScreenBuffer.Goto(2, 0);
			_ScreenBuffer.Write("[ Ascii Editor ]");
			_ScreenBuffer.Goto(2, 24);
			_ScreenBuffer.Write("[ F1 - XML Format, F2 - Normal Format ]");
			for (int j = 0; j < 60; j++)
			{
				_ScreenBuffer.Goto(j + 2, 2);
				_ScreenBuffer.Buffer[j + 2, 2].Char = array[j].Char;
				_ScreenBuffer.Buffer[j + 2, 2].Attributes = array[j].Attributes;
			}
			_ScreenBuffer.Goto(2 + num, 3);
			_ScreenBuffer.Write("\u0018");
			for (int k = 0; k < 16; k++)
			{
				_ScreenBuffer.Buffer[2, 5 + k].Char = 'Û';
				_ScreenBuffer.Buffer[2, 5 + k].Attributes = (ushort)k;
			}
			_ScreenBuffer.Buffer[1, ColorUtility.GetForeground(array[num].Attributes) + 5].Char = '\u001a';
			_ScreenBuffer.Buffer[1, ColorUtility.GetForeground(array[num].Attributes) + 5].Attributes = ColorUtility.MakeColor(TextColor.Grey, TextColor.Black);
			_TextConsole.DrawBuffer(_ScreenBuffer);
			switch (Keyboard.getvk(MapDirectionToArrows: true))
			{
			case Keys.Escape:
				return;
			case Keys.F1:
			case Keys.F2:
			{
				StringBuilder stringBuilder = new StringBuilder();
				ushort num2 = 0;
				ushort num3 = 0;
				string value = "&";
				if (Keyboard.vkCode == Keys.F1)
				{
					value = "&amp;";
				}
				for (int l = 0; l <= num; l++)
				{
					ushort foreground3 = ColorUtility.GetForeground(array[l].Attributes);
					if (foreground3 != num2)
					{
						num2 = foreground3;
						foreach (char key in ColorUtility.CharToColorMap.Keys)
						{
							if (ColorUtility.CharToColorMap[key] == foreground3)
							{
								stringBuilder.Append(value);
								stringBuilder.Append(key);
								break;
							}
						}
					}
					ushort background3 = ColorUtility.GetBackground(array[l].Attributes);
					if (background3 != num3)
					{
						num3 = background3;
						foreach (char key2 in ColorUtility.CharToColorMap.Keys)
						{
							if (ColorUtility.CharToColorMap[key2] == background3)
							{
								stringBuilder.Append("^");
								stringBuilder.Append(key2);
								break;
							}
						}
					}
					stringBuilder.Append(array[l].Char);
				}
				break;
			}
			default:
				if (Keyboard.vkCode == Keys.Back)
				{
					array[num].Char = ' ';
					if (num > 0)
					{
						num--;
					}
				}
				else if (Keyboard.vkCode == Keys.Up || Keyboard.vkCode == Keys.NumPad8)
				{
					ushort foreground = ColorUtility.GetForeground(array[num].Attributes);
					ushort background = ColorUtility.GetBackground(array[num].Attributes);
					foreground = (ushort)((foreground < 1) ? 15 : ((ushort)(foreground - 1)));
					array[num].Attributes = ColorUtility.MakeColor(foreground, background);
				}
				else if (Keyboard.vkCode == Keys.Down || Keyboard.vkCode == Keys.NumPad2)
				{
					ushort foreground2 = ColorUtility.GetForeground(array[num].Attributes);
					ushort background2 = ColorUtility.GetBackground(array[num].Attributes);
					foreground2 = (ushort)(foreground2 + 1);
					if (foreground2 > 15)
					{
						foreground2 = 0;
					}
					array[num].Attributes = ColorUtility.MakeColor(foreground2, background2);
				}
				else if (Keyboard.vkCode == Keys.Left || Keyboard.vkCode == Keys.NumPad4)
				{
					if (num > 0)
					{
						num--;
					}
				}
				else if (Keyboard.vkCode == Keys.Right || Keyboard.vkCode == Keys.NumPad6)
				{
					num++;
				}
				else if (Keyboard.vkCode != Keys.MouseEvent)
				{
					array[num].Char = (char)Keyboard.Char;
					num++;
				}
				break;
			}
			if (num > 58)
			{
				num = 58;
			}
		}
		_TextConsole.DrawBuffer(TextConsole.ScrapBuffer2);
	}
}
