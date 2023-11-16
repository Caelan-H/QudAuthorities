using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;
using XRL.Language;
using XRL.Messages;
using XRL.UI.Framework;
using XRL.World;

namespace XRL.UI;

[UIView("Popup", false, false, false, null, null, false, 0, false)]
public class Popup : IWantsTextConsoleInit
{
	protected class PopupTextBuilder
	{
		public class Section
		{
			public int Selection;

			public int StartOffset;

			public int EndOffset;

			public int Count => 1 + EndOffset - StartOffset;
		}

		public const int IntroSelection = -1;

		public List<string> Lines = new List<string>(24);

		public int MaxClippedWidth;

		public int MaxWidth = 72;

		public int Padding;

		public bool RespectNewlines = true;

		public int Spacing;

		public string SpacingText = "";

		public List<Section> Sections = new List<Section>();

		public void AddSection(string text, int selection)
		{
			Section section = new Section();
			section.Selection = selection;
			section.StartOffset = Lines.Count;
			Lines.AddRange(StringFormat.ClipTextToArray(text, MaxWidth - Padding, out var MaxClippedWidth, RespectNewlines, KeepColorsAcrossNewlines: true, TransformMarkup: false, TransformMarkupIfMultipleLines: true));
			this.MaxClippedWidth = Math.Max(this.MaxClippedWidth, MaxClippedWidth + Padding);
			section.EndOffset = Lines.Count - 1;
			Sections.Add(section);
		}

		public void AddSpacing(int lines, string text = null)
		{
			for (int i = 0; i < lines; i++)
			{
				Lines.Add(Markup.Transform(text ?? SpacingText));
			}
		}

		public Section GetSection(int selection)
		{
			foreach (Section section in Sections)
			{
				if (section.Selection == selection)
				{
					return section;
				}
			}
			return null;
		}

		public string[] GetSelectionLines(int selection)
		{
			Section section = GetSection(selection);
			if (section == null)
			{
				return new string[0];
			}
			int count = section.Count + ((selection >= 0 && selection != Sections[Sections.Count - 1].Selection) ? Spacing : 0);
			return Lines.GetRange(section.StartOffset, count).ToArray();
		}

		public int GetSelectionStart(int selection)
		{
			return GetSection(selection)?.StartOffset ?? Lines.Count;
		}
	}

	public static TextConsole _TextConsole;

	public static ScreenBuffer _ScreenBuffer;

	[Obsolete("We don't use it. You shouldn't either.")]
	public static Thread ThinkingThread = null;

	[Obsolete("We don't use it. You shouldn't either.  See XRL.UI.Loading.SetLoadStatus(string) or LoadTask(string, Action)")]
	public static string ThinkingString = "";

	[Obsolete("We don't use it. You shouldn't either.  See XRL.UI.Loading.SetHideLoad(bool).")]
	public static bool DisableThinking = false;

	[Obsolete("Not used.")]
	public static string LastThinking = "";

	[Obsolete("Not used.")]
	public static bool bPaused = false;

	[NonSerialized]
	private static List<string> RenderDirectLines = new List<string>();

	public static TextBlock _RenderBlock = new TextBlock("", 78, 5000);

	public static bool bSuppressPopups = false;

	[NonSerialized]
	private static List<string> ColorPickerOptionStrings = new List<string>(32);

	[NonSerialized]
	private static List<string> ColorPickerOptions = new List<string>(32);

	[NonSerialized]
	private static List<char> ColorPickerKeymap = new List<char>(32);

	[NonSerialized]
	public static ScreenBuffer ScrapBuffer = ScreenBuffer.create(80, 25);

	[NonSerialized]
	public static ScreenBuffer ScrapBuffer2 = ScreenBuffer.create(80, 25);

	public static bool DisplayedLoadError = false;

	public static string SPACING_DARK_LINE = "{{K|================================================================================}}";

	public static string SPACING_BRIGHT_LINE = "{{Y|================================================================================}}";

	public static string SPACING_GREY_RAINBOW_LINE = "{{K-y-Y-y sequence|================================================================================}}";

	[Obsolete("Will be removed, no longer used.")]
	public static void StartThinkingThread(string String)
	{
	}

	[Obsolete("Will be removed, no longer used.")]
	public static void StartThinkingThread(object oString)
	{
	}

	[Obsolete("Will be removed, no longer used.")]
	public static void PauseThinking()
	{
		Loading.SetHideLoadStatus(hidden: true);
	}

	[Obsolete("Will be removed, no longer used.")]
	public static void ResumeThinking()
	{
		Loading.SetHideLoadStatus(hidden: false);
	}

	[Obsolete("Will be removed, no longer used.  See XRL.UI.Loading.SetLoadingStatus()")]
	public static void StartThinking(string DisplayString)
	{
		Loading.SetLoadingStatus(DisplayString);
	}

	[Obsolete("Will be removed, no longer used.  See XRL.UI.Loading.SetLoadingStatus()")]
	public static void EndThinking()
	{
		Loading.SetLoadingStatus(null);
	}

	public void Init(TextConsole TextConsole_, ScreenBuffer ScreenBuffer_)
	{
		_TextConsole = TextConsole_;
		_ScreenBuffer = ScreenBuffer_;
	}

	public static void RenderDirect(int XPos, int YPos, ScreenBuffer Buffer, string Message, string BottomLineNoFormat, string BottomLine, int StartingLine)
	{
		Message = Markup.Transform(Message);
		if (Message.Contains("\r\n"))
		{
			Message = Message.Replace("\r\n", "\n");
		}
		if (Message.Contains("\r"))
		{
			Message = Message.Replace("\r", "\n");
		}
		string[] array = Message.Split('\n');
		RenderDirectLines.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			while (array[i].Length > 78)
			{
				string item = array[i].Substring(0, 78);
				string text = (array[i] = array[i].Substring(78, array[i].Length - 78));
				RenderDirectLines.Add(item);
			}
			RenderDirectLines.Add(array[i]);
		}
		int num = BottomLineNoFormat.Length + 2;
		for (int j = 0; j < RenderDirectLines.Count; j++)
		{
			if (RenderDirectLines[j].Length + 2 >= num)
			{
				num = RenderDirectLines[j].Length + 2;
			}
		}
		int num2 = num / 2;
		int num3 = RenderDirectLines.Count / 2;
		if (num3 < 1)
		{
			num3 = 1;
		}
		num3++;
		num2++;
		int Top = 12 - num3;
		int Bottom = 12 + num3 + 1;
		int Left = 40 - num2;
		int Right = 40 + num2;
		TextConsole.Constrain(ref Left, ref Right, ref Top, ref Bottom);
		Buffer.Fill(Left, Top, Right, Bottom, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
		Buffer.ThickSingleBox(Left, Top, Right, Bottom, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		for (int k = StartingLine; k < RenderDirectLines.Count && Top + 2 + k - StartingLine < 24; k++)
		{
			Buffer.Goto(XPos + 2, YPos + 2 + k - StartingLine);
			if (StartingLine > 0 && k == StartingLine)
			{
				Buffer.Write("<up for more...>");
			}
			else if (Top + 2 + k - StartingLine == 23)
			{
				Buffer.Write("<down for more...>");
			}
			else
			{
				Buffer.Write(RenderDirectLines[k]);
			}
		}
		int num4 = num2 * 2;
		num4 -= BottomLineNoFormat.Length;
		num4 /= 2;
		num4++;
		Buffer.Goto(Left + num4, Bottom);
		Buffer.Write(BottomLine);
	}

	public static int Render(List<string> Message, string BottomLineNoFormat, string BottomLine, ScreenBuffer Buffer, int StartingLine)
	{
		int num = 22;
		int num2 = Message.Count;
		if (num2 > 20)
		{
			num2 = 20;
		}
		for (int i = 0; i < Message.Count; i++)
		{
			int num3 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(Message[i]) + 2;
			if (num3 > num)
			{
				num = num3;
			}
		}
		int num4 = num / 2;
		int num5 = Message.Count / 2;
		if (num5 < 1)
		{
			num5 = 1;
		}
		num5++;
		num4++;
		int Top = 12 - num5;
		int Bottom = 12 + num5 + 1;
		int Left = 40 - num4;
		int Right = 40 + num4;
		TextConsole.Constrain(ref Left, ref Right, ref Top, ref Bottom);
		Buffer.Fill(Left, Top, Right, Bottom + 1, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
		Buffer.ThickSingleBox(Left, Top, Right, Bottom + 1, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		for (int j = StartingLine; j < Message.Count && Top + 2 + j - StartingLine < 24; j++)
		{
			Buffer.Goto(Left + 2, Top + 2 + j - StartingLine);
			if (StartingLine > 0 && j == StartingLine)
			{
				Buffer.Write("<up for more...>");
			}
			else if (Top + 2 + j - StartingLine == 23)
			{
				Buffer.Write("<down for more...>");
			}
			else
			{
				Buffer.Write(Message[j]);
			}
		}
		int num6 = num4 * 2;
		num6 -= BottomLineNoFormat.Length;
		num6 /= 2;
		num6++;
		Buffer.Goto(Left + num6, Bottom);
		Buffer.Write(BottomLine);
		return num2;
	}

	public static void Render(string Message, string BottomLineNoFormat, string BottomLine, ScreenBuffer Buffer, int StartingLine)
	{
		Message = Markup.Transform(Message);
		if (Message.Contains("\r\n"))
		{
			Message = Message.Replace("\r\n", "\n");
		}
		if (Message.Contains("\r"))
		{
			Message = Message.Replace("\r", "\n");
		}
		List<string> list = new List<string>(Message.Split('\n'));
		int num = 22;
		for (int i = 0; i < list.Count; i++)
		{
			int num2 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(list[i]) + 2;
			if (num2 > num)
			{
				num = num2;
			}
		}
		int num3 = num / 2;
		int num4 = list.Count / 2;
		if (num4 < 1)
		{
			num4 = 1;
		}
		num4++;
		num3++;
		int Top = 12 - num4;
		int Bottom = 12 + num4 + 1;
		int Left = 40 - num3;
		int Right = 40 + num3;
		TextConsole.Constrain(ref Left, ref Right, ref Top, ref Bottom);
		Buffer.Fill(Left, Top, Right, Bottom, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
		Buffer.ThickSingleBox(Left, Top, Right, Bottom, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		for (int j = StartingLine; j < list.Count && Top + 2 + j - StartingLine < 24; j++)
		{
			Buffer.Goto(Left + 2, Top + 2 + j - StartingLine);
			if (StartingLine > 0 && j == StartingLine)
			{
				Buffer.Write("<up for more...>");
			}
			else if (Top + 2 + j - StartingLine == 23)
			{
				Buffer.Write("<down for more...>");
			}
			else
			{
				Buffer.Write(list[j]);
			}
		}
		int num5 = num3 * 2;
		num5 -= BottomLineNoFormat.Length;
		num5 /= 2;
		num5++;
		Buffer.Goto(Left + num5, Bottom);
		Buffer.Write(BottomLine);
	}

	public static int RenderBlock(string Message, string BottomLineNoFormat, string BottomLine, ScreenBuffer Buffer, int StartingLine, int MinWidth = -1, int MinHeight = -1, string Title = null, bool BottomLineRegions = false, IRenderable Icon = null, bool centerIcon = true, bool rightIcon = false)
	{
		Message = Markup.Transform(Message);
		Title = Markup.Transform(Title);
		if (Message.Contains("\r\n"))
		{
			Message = Message.Replace("\r\n", "\n");
		}
		if (Message.Contains("\r"))
		{
			Message = Message.Replace("\r", "\n");
		}
		List<string> lines = new TextBlock(Message, 78, 5000, ReverseBlocks: false, rightIcon ? (-3) : 0).Lines;
		int num = 22;
		if (Icon != null && centerIcon)
		{
			lines.Insert(0, "");
			lines.Insert(0, "");
		}
		if (Title != null)
		{
			int num2 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(Title) + 2;
			if (num2 > MinWidth)
			{
				MinWidth = num2;
			}
		}
		if (MinWidth > -1)
		{
			num = MinWidth;
		}
		for (int i = 0; i < lines.Count; i++)
		{
			int num3 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(lines[i]) + 2;
			if (i == 0 && Icon != null && !centerIcon)
			{
				num3 += 3;
			}
			if (num3 > num)
			{
				num = num3;
			}
		}
		int num4 = num / 2;
		int num5 = lines.Count / 2;
		num4++;
		if (MinHeight > -1)
		{
			num5 = MinHeight;
		}
		if (num5 < 2)
		{
			num5 = 2;
		}
		int Top = 10 - num5;
		int Bottom = Top + lines.Count + 3;
		int Left = 40 - num4;
		int Right = 40 + num4;
		TextConsole.Constrain(ref Left, ref Right, ref Top, ref Bottom);
		if (Right < MinWidth - 1)
		{
			Right = MinHeight - 1;
		}
		if (Bottom < MinHeight - 1)
		{
			Bottom = MinHeight - 1;
		}
		Buffer.Fill(Left, Top, Right, Bottom, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
		Buffer.ThickSingleBox(Left, Top, Right, Bottom, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		if (Title != null)
		{
			Buffer.Goto(Left + 1, Top);
			Buffer.Write(Title);
		}
		for (int j = StartingLine; j < lines.Count && Top + 2 + j - StartingLine < 24; j++)
		{
			int num6 = Left + 2;
			int y = Top + 2 + j - StartingLine;
			if (j == 0 && Icon != null && !centerIcon && !rightIcon)
			{
				Buffer.Goto(num6 + 2, y);
			}
			else
			{
				Buffer.Goto(num6, y);
			}
			if (StartingLine > 0 && j == StartingLine)
			{
				Buffer.Write("<up for more...>");
			}
			else if (Top + 2 + j - StartingLine == 23)
			{
				Buffer.Write("<down for more...>");
			}
			else
			{
				Buffer.Write(lines[j]);
			}
			if (j == 0 && Icon != null)
			{
				if (centerIcon)
				{
					Buffer.Goto(num6 + (num - 1) / 2, y);
				}
				else if (rightIcon)
				{
					Buffer.Goto(Right - 3, y);
					Buffer.Write("   ");
					Buffer.Goto(Right - 2, y);
				}
				else
				{
					Buffer.Goto(num6, y);
				}
				Buffer.Write(Icon);
			}
		}
		int num7 = num4 * 2;
		num7 -= BottomLineNoFormat.Length;
		num7 /= 2;
		num7++;
		Buffer.Goto(Left + num7, Bottom);
		Buffer.Write(BottomLine);
		if (BottomLineRegions)
		{
			GameManager.Instance.ClearRegions();
			List<int> list = new List<int>();
			list.Add(0);
			for (int k = 0; k < BottomLineNoFormat.Length; k++)
			{
				if (BottomLineNoFormat[k] == ' ')
				{
					list.Add(k);
				}
			}
			list.Add(BottomLineNoFormat.Length);
			for (int l = 0; l < list.Count - 1; l++)
			{
				GameManager.Instance.AddRegion(Left + num7 + list[l], Bottom - 1, Left + num7 + list[l + 1], Bottom + 1, "LeftOption:" + (l + 1), "RightOption:" + (l + 1));
			}
		}
		return lines.Count;
	}

	public static int RenderBlock(StringBuilder Message, string BottomLineNoFormat, string BottomLine, ScreenBuffer Buffer, int StartingLine, int MinWidth, int MinHeight, string Title)
	{
		Markup.Transform(Message);
		Title = Markup.Transform(Title);
		if (Message.Contains("\r\n"))
		{
			Message = Message.Replace("\r\n", "\n");
		}
		if (Message.Contains("\r"))
		{
			Message = Message.Replace("\r", "\n");
		}
		_RenderBlock.Format(Message);
		List<string> lines = _RenderBlock.Lines;
		int num = 22;
		if (Title != null)
		{
			int num2 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(Title) + 2;
			if (num2 > MinWidth)
			{
				MinWidth = num2;
			}
		}
		if (MinWidth > -1)
		{
			num = MinWidth;
		}
		for (int i = 0; i < lines.Count; i++)
		{
			int num3 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(lines[i]) + 2;
			if (num3 > num)
			{
				num = num3;
			}
		}
		int num4 = num / 2;
		int num5 = lines.Count / 2;
		num4++;
		if (MinHeight > -1)
		{
			num5 = MinHeight;
		}
		if (num5 < 2)
		{
			num5 = 2;
		}
		int Top = 10 - num5;
		int Bottom = Top + lines.Count + 3;
		int Left = 40 - num4;
		int Right = 40 + num4;
		TextConsole.Constrain(ref Left, ref Right, ref Top, ref Bottom);
		Buffer.Fill(Left, Top, Right, Bottom, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
		Buffer.ThickSingleBox(Left, Top, Right, Bottom, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		if (Title != null)
		{
			Buffer.Goto(Left + 1, Top);
			Buffer.Write(Title);
		}
		for (int j = StartingLine; j < lines.Count && Top + 2 + j - StartingLine < 24; j++)
		{
			Buffer.Goto(Left + 2, Top + 2 + j - StartingLine);
			if (StartingLine > 0 && j == StartingLine)
			{
				Buffer.Write("<up for more...>");
			}
			else if (Top + 2 + j - StartingLine == 23)
			{
				Buffer.Write("<down for more...>");
			}
			else
			{
				Buffer.Write(lines[j]);
			}
		}
		int num6 = num4 * 2;
		num6 -= BottomLineNoFormat.Length;
		num6 /= 2;
		num6++;
		Buffer.Goto(Left + num6, Bottom);
		Buffer.Write(BottomLine);
		return lines.Count;
	}

	public static void ShowFail(string Message, bool CopyScrap = true, bool Capitalize = true, bool DimBackground = true)
	{
		Show(Message, CopyScrap, Capitalize, DimBackground, LogMessage: false);
	}

	public static async Task ShowKeybindAsync(string Message, bool CopyScrap = true, bool Capitalize = true, bool DimBackground = true, bool LogMessage = true)
	{
		if (bSuppressPopups)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message);
			}
		}
		else
		{
			await NewPopupMessageAsync(Message, PopupMessage.AnyKey);
		}
	}

	public static async Task ShowAsync(string Message, bool CopyScrap = true, bool Capitalize = true, bool DimBackground = true, bool LogMessage = true)
	{
		if (bSuppressPopups)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message);
			}
		}
		else
		{
			await NewPopupMessageAsync(Message, PopupMessage.SingleButton);
		}
	}

	public static void Show(string Message, bool CopyScrap = true, bool Capitalize = true, bool DimBackground = true, bool LogMessage = true)
	{
		if (bSuppressPopups)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message);
			}
		}
		else
		{
			ShowBlock(Message, CopyScrap, Capitalize, DimBackground, LogMessage);
		}
	}

	public static Keys ShowBlockWithCopy(string Message, string Prompt, string Title, string CopyInfo, bool LogMessage = true)
	{
		string message = "World seed copied to your clipboard.";
		if (UIManager.UseNewPopups)
		{
			bool copied = false;
			WaitNewPopupMessage(Message, PopupMessage.CopyButton, delegate(QudMenuItem item)
			{
				if (item.command == "Copy")
				{
					ClipboardHelper.SetClipboardData(CopyInfo);
					copied = true;
				}
			}, null, Title);
			Keyboard.ClearInput();
			if (copied)
			{
				Show(message, CopyScrap: false);
			}
			return Keys.Space;
		}
		GameManager.Instance.PushGameView("Popup:Text");
		Message = Markup.Transform(Options.OverlayUI ? ("{{white|" + Message + "}}") : Message);
		Title = Markup.Transform(Title);
		if (LogMessage)
		{
			MessageQueue.AddPlayerMessage(Message);
		}
		int num = 0;
		Keys keys = Keys.Pause;
		while (keys == Keys.Pause || keys == Keys.NumPad2 || keys == Keys.NumPad8 || keys == Keys.Next || keys == Keys.Prior || keys == Keys.Next)
		{
			int num2 = RenderBlock(Message, ConsoleLib.Console.ColorUtility.StripFormatting(Prompt), Prompt, ScrapBuffer, num, -1, -1, Title);
			_TextConsole.DrawBuffer(ScrapBuffer);
			keys = Keyboard.getvk(Options.MapDirectionsToKeypad, pumpActions: true);
			if (keys == Keys.NumPad2 && num2 - num > 20)
			{
				num++;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.Next || keys == Keys.Next)
			{
				num += 23;
			}
			if (keys == Keys.Prior)
			{
				num -= 23;
			}
			if (num < 0)
			{
				num = 0;
			}
			if (keys == Keys.C)
			{
				ClipboardHelper.SetClipboardData(CopyInfo);
				Show(message, CopyScrap: false);
			}
		}
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2);
		Loading.SetHideLoadStatus(hidden: false);
		return keys;
	}

	public static void ShowProgress(Action<Progress> a)
	{
		new Progress().start(a);
	}

	public static async Task<QudMenuItem> NewPopupMessageAsync(string message, List<QudMenuItem> buttons = null, List<QudMenuItem> options = null, string title = null, string inputDefault = null, int defaultSelected = 0, string contextTitle = null, IRenderable contextRender = null, bool EscapeNonMarkupFormatting = true)
	{
		TaskCompletionSource<QudMenuItem> t = new TaskCompletionSource<QudMenuItem>();
		return await NavigationController.instance.SuspendContextWhile(async delegate
		{
			await The.UiContext;
			ControlManager.ResetInput();
			PopupMessage messageWindow = UIManager.copyWindow("PopupMessage") as PopupMessage;
			inputDefault = Sidebar.FromCP437(inputDefault);
			try
			{
				messageWindow.ShowPopup(message, buttons ?? PopupMessage.SingleButton, delegate(QudMenuItem item)
				{
					t.TrySetResult(item);
				}, options ?? new List<QudMenuItem>(), delegate(QudMenuItem item)
				{
					t.TrySetResult(item);
				}, title, inputDefault != null, (!EscapeNonMarkupFormatting) ? inputDefault : inputDefault?.Replace("&&", "&").Replace("^^", "^"), defaultSelected, delegate
				{
					t.TrySetCanceled();
				}, contextRender, contextTitle, pushView: false);
				messageWindow.Show();
				QudMenuItem result = await t.Task;
				if (inputDefault != null)
				{
					result.text = Sidebar.ToCP437(messageWindow.inputBox.text);
					if (EscapeNonMarkupFormatting)
					{
						result.text = result.text.Replace("&", "&&").Replace("^", "^^");
					}
				}
				ControlManager.ResetInput();
				return result;
			}
			finally
			{
				UnityEngine.Object.Destroy(messageWindow.gameObject);
			}
		});
	}

	public static async void WaitNewPopupMessage(string message, List<QudMenuItem> buttons = null, Action<QudMenuItem> callback = null, List<QudMenuItem> options = null, string title = null, string inputDefault = null, int defaultSelected = 0, string contextTitle = null, IRenderable contextRender = null, bool EscapeNonMarkupFormatting = true)
	{
		if (Thread.CurrentThread == GameManager.Instance.uiQueue.threadContext)
		{
			callback(await NewPopupMessageAsync(message, buttons, options, title, inputDefault, defaultSelected, contextTitle, contextRender, EscapeNonMarkupFormatting));
			return;
		}
		TaskCompletionSource<QudMenuItem> complete = new TaskCompletionSource<QudMenuItem>();
		PopupMessage messageWindow;
		GameManager.Instance.uiQueue.awaitTask(delegate
		{
			try
			{
				messageWindow = UIManager.getWindow("PopupMessage") as PopupMessage;
				inputDefault = Sidebar.FromCP437(inputDefault);
				messageWindow.ShowPopup(message, buttons ?? PopupMessage.SingleButton, delegate(QudMenuItem i)
				{
					try
					{
						if (inputDefault != null)
						{
							i.text = Sidebar.ToCP437(messageWindow.inputBox.text);
						}
						if (EscapeNonMarkupFormatting)
						{
							i.text = i.text.Replace("&", "&&").Replace("^", "^^");
						}
						complete.SetResult(i);
					}
					catch (Exception exception3)
					{
						complete.TrySetException(exception3);
					}
				}, options ?? new List<QudMenuItem>(), delegate(QudMenuItem i)
				{
					try
					{
						if (inputDefault != null)
						{
							i.text = messageWindow.inputBox.text;
						}
						complete.SetResult(i);
					}
					catch (Exception exception2)
					{
						complete.TrySetException(exception2);
					}
				}, title, inputDefault != null, (!EscapeNonMarkupFormatting) ? inputDefault : inputDefault?.Replace("&&", "&").Replace("^^", "^"), defaultSelected, delegate
				{
					complete.TrySetCanceled();
					messageWindow.gameObject.SetActive(value: false);
					Loading.SetHideLoadStatus(hidden: false);
				}, contextTitle: contextTitle, contextRender: contextRender);
				GameManager.Instance.PushGameView("PopupMessage");
			}
			catch (Exception exception)
			{
				complete.TrySetException(exception);
			}
		});
		complete.Task.Wait();
		if (complete.Task.IsCompleted)
		{
			callback?.Invoke(complete.Task.Result);
		}
		else if (complete.Task.IsFaulted)
		{
			throw complete.Task.Exception ?? new Exception("Await new popup exception");
		}
		ControlManager.ResetInput();
		GameManager.Instance.PopGameView(bHard: true);
	}

	public static Keys ShowBlock(string Message, string Prompt, bool Capitalize = true, bool MuteBackground = true, IRenderable Icon = null, bool centerIcon = true, bool rightIcon = false, bool LogMessage = true)
	{
		if (bSuppressPopups)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message);
			}
			return Keys.Space;
		}
		Message = Markup.Transform(Message);
		if (Capitalize)
		{
			Message = ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Message);
		}
		Prompt = Markup.Transform(Prompt);
		if (LogMessage)
		{
			MessageQueue.AddPlayerMessage(Message);
		}
		if (UIManager.UseNewPopups || ScrapBuffer == null)
		{
			WaitNewPopupMessage(Message, null, null, null, Prompt);
			return Keys.Space;
		}
		int num = 0;
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		new TextBlock(Message, 80, 5000);
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Keys keys = Keys.Pause;
		while (keys == Keys.Pause || keys == Keys.NumPad2 || keys == Keys.NumPad8 || keys == Keys.Next || keys == Keys.Prior || keys == Keys.Next)
		{
			int num2 = RenderBlock(Message, ConsoleLib.Console.ColorUtility.StripFormatting(Prompt), Prompt, ScrapBuffer, num, -1, -1, null, BottomLineRegions: false, Icon, centerIcon, rightIcon);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (Icon != null)
			{
				ScreenBuffer.ClearImposterSuppression();
			}
			if (keys == Keys.NumPad2 && num2 - num > 20)
			{
				num++;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.Next || keys == Keys.Next)
			{
				num += 23;
			}
			if (keys == Keys.Prior)
			{
				num -= 23;
			}
			if (num < 0)
			{
				num = 0;
			}
		}
		Keyboard.ClearInput();
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		return keys;
	}

	public static Keys ShowBlockSpace(string Message, string Prompt, bool Capitalize = true, bool MuteBackground = true, IRenderable Icon = null, bool centerIcon = true, bool rightIcon = false, bool LogMessage = true)
	{
		if (bSuppressPopups)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message);
			}
			return Keys.Space;
		}
		Loading.SetHideLoadStatus(hidden: true);
		Message = Markup.Transform(Message);
		if (Capitalize)
		{
			Message = ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Message);
		}
		Prompt = Markup.Transform(Prompt);
		if (LogMessage)
		{
			MessageQueue.AddPlayerMessage(Message);
		}
		int num = 0;
		new TextBlock(Message, 80, 5000);
		if (UIManager.UseNewPopups)
		{
			WaitNewPopupMessage(Message, null, null, null, "");
			return Keys.Space;
		}
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Keys keys = Keys.Pause;
		while (keys != Keys.Escape && keys != Keys.Space && keys != Keys.Enter && keys != Keys.Enter)
		{
			int num2 = RenderBlock(Message, ConsoleLib.Console.ColorUtility.StripFormatting(Prompt), Prompt, ScrapBuffer, num, -1, -1, null, BottomLineRegions: false, Icon, centerIcon, rightIcon);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (keys == Keys.NumPad2 && num2 - num > 20)
			{
				num++;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.Next || keys == Keys.Next)
			{
				num += 23;
			}
			if (keys == Keys.Prior)
			{
				num -= 23;
			}
			if (num < 0)
			{
				num = 0;
			}
		}
		Keyboard.ClearInput();
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		return keys;
	}

	public static void ShowSpace(string Message, bool LogMessage = true)
	{
		if (bSuppressPopups)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message);
			}
			return;
		}
		Message = Markup.Transform(Message);
		MessageQueue.AddPlayerMessage(Message);
		if (UIManager.UseNewPopups)
		{
			WaitNewPopupMessage(Message);
			return;
		}
		new TextBlock(Message, 80, 5000);
		int startingLine = 0;
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Keyboard.ClearInput();
		List<QudMenuItem> singleButton = PopupMessage.SingleButton;
		string bottomLineNoFormat = ConsoleLib.Console.ColorUtility.StripFormatting(singleButton[0].text);
		string text = singleButton[0].text;
		for (int num = 95; num != 32; num = Keyboard.getch())
		{
			RenderBlock(Message, bottomLineNoFormat, text, ScrapBuffer, startingLine);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
		}
		Keyboard.ClearInput();
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
	}

	public static Keys ShowBlock(string Message, bool CopyScrap = true, bool Capitalize = true, bool DimBackground = true, bool LogMessage = true)
	{
		if (bSuppressPopups)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message, null, Capitalize);
			}
			return Keys.Space;
		}
		Message = Markup.Transform(Message);
		if (Capitalize)
		{
			Message = ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Message);
		}
		if (LogMessage)
		{
			MessageQueue.AddPlayerMessage(Message);
		}
		if (UIManager.UseNewPopups)
		{
			WaitNewPopupMessage(Message);
			return Keys.Space;
		}
		new TextBlock(Message, 80, 5000);
		int num = 0;
		if (CopyScrap)
		{
			ScrapBuffer.Copy(TextConsole.CurrentBuffer);
			ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		}
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Keyboard.ClearInput();
		Keys keys = Keys.Pause;
		while (keys != Keys.Space && keys != Keys.Enter && keys != Keys.Escape)
		{
			int num2 = RenderBlock(Message, "[press space]", "[press {{W|space}}]", ScrapBuffer, num);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			try
			{
				keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("hm", x);
			}
			if (keys == Keys.NumPad2 && num2 - num > 20)
			{
				num++;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.Next || keys == Keys.Next)
			{
				num += 23;
			}
			if (keys == Keys.Prior)
			{
				num -= 23;
			}
			if (num < 0)
			{
				num = 0;
			}
		}
		Keyboard.ClearInput();
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		return keys;
	}

	public static int? AskNumber(string Message, int Start = 0, int Min = 0, int Max = int.MaxValue, string RestrictChars = "0123456789")
	{
		Message = Markup.Transform(Message);
		string text = Start.ToString();
		int startingLine = 0;
		text = Start.ToString();
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		GameManager.Instance.PushGameView("Popup:AskNumber");
		bool flag = false;
		int num = 95;
		while (true)
		{
			ScrapBuffer.Copy(ScrapBuffer2);
			string text2 = "";
			text2 = (flag ? (text + "_") : ("&y^K" + text + "^k"));
			RenderBlock(Message + "\n" + text2, "[\u0018\u0019\u001a\u001b adjust,Enter or space to confirm]", "[{{W|\u0018\u0019\u001a\u001b}} adjust, {{W|Enter}} or space to confirm]", ScrapBuffer, startingLine);
			_TextConsole.DrawBuffer(ScrapBuffer);
			num = Keyboard.getch();
			if (Keyboard.vkCode == Keys.Enter || Keyboard.vkCode == Keys.Space)
			{
				break;
			}
			if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("SubmitString:"))
			{
				text = Keyboard.CurrentMouseEvent.Event.Substring(Keyboard.CurrentMouseEvent.Event.IndexOf(':') + 1);
				Keyboard.ClearInput();
				GameManager.Instance.PopGameView(bHard: true);
				_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
				Loading.SetHideLoadStatus(hidden: false);
				return Convert.ToInt32(text);
			}
			if (Keyboard.vkCode != Keys.MouseEvent)
			{
				if (Keyboard.vkCode == (Keys)131158)
				{
					string clipboardData = ClipboardHelper.GetClipboardData();
					text += clipboardData;
				}
				else
				{
					char c = (char)num;
					if ((char.IsDigit(c) || char.IsLetter(c) || char.IsPunctuation(c) || char.IsSeparator(c) || char.IsSymbol(c) || num == 32) && (RestrictChars == null || RestrictChars.IndexOf(c) != -1))
					{
						if (!flag)
						{
							text = "";
							flag = true;
						}
						text += c;
					}
				}
			}
			if (Keyboard.vkCode == Keys.Back && text.Length > 0)
			{
				text = text.Substring(0, text.Length - 1);
			}
			if (Keyboard.vkCode == Keys.Escape || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick"))
			{
				Keyboard.ClearInput();
				GameManager.Instance.PopGameView(bHard: true);
				_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
				Loading.SetHideLoadStatus(hidden: false);
				return null;
			}
			if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick")
			{
				break;
			}
			if (Keyboard.vkCode == Keys.NumPad8)
			{
				int result = 0;
				int.TryParse(text, out result);
				result += 10;
				text = Math.Min(Max, result).ToString();
			}
			if (Keyboard.vkCode == Keys.NumPad2)
			{
				int result2 = 0;
				int.TryParse(text, out result2);
				result2 -= 10;
				text = Math.Max(Min, result2).ToString();
			}
			if (Keyboard.vkCode == Keys.NumPad6)
			{
				int result3 = 0;
				int.TryParse(text, out result3);
				result3++;
				text = Math.Min(Max, result3).ToString();
			}
			if (Keyboard.vkCode == Keys.NumPad4)
			{
				int result4 = 0;
				int.TryParse(text, out result4);
				result4--;
				text = Math.Max(Min, result4).ToString();
			}
			int result5 = 0;
			int.TryParse(text, out result5);
			result5 = Math.Min(Max, result5);
			text = Math.Max(Min, result5).ToString();
		}
		Keyboard.ClearInput();
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		if (int.TryParse(text, out var result6))
		{
			return result6;
		}
		return null;
	}

	public static async Task<string> AskStringAsync(string Message, string Default = "", int MaxLength = 80, int MinLength = 0, string RestrictChars = null, bool ReturnNullForEscape = false, bool EscapeNonMarkupFormatting = true, bool? AllowColorize = null)
	{
		List<QudMenuItem> buttons = ((AllowColorize ?? Options.GetOption("OptionEnableColorTextInput").EqualsNoCase("Yes")) ? PopupMessage.AcceptCancelColorButton : PopupMessage.AcceptCancelButton);
		string result = Default;
		QudMenuItem qudMenuItem;
		while (true)
		{
			qudMenuItem = await NewPopupMessageAsync(StringFormat.ClipText(Message, 60, KeepNewlines: true), buttons, null, null, result);
			if (!(qudMenuItem.command == "Color"))
			{
				break;
			}
			result = ConsoleLib.Console.ColorUtility.StripFormatting(qudMenuItem.text);
			string text = await ShowColorPickerAsync("Choose color", 0, null, 60, RespectOptionNewlines: false, AllowEscape: false, 0, "", includeNone: true, includePatterns: true, allowBackground: false, result);
			if (!string.IsNullOrEmpty(text))
			{
				result = "{{" + text + "|" + result + "}}";
			}
		}
		if (qudMenuItem.command == "Cancel")
		{
			return ReturnNullForEscape ? null : "";
		}
		return qudMenuItem.text;
	}

	public static string AskString(string Message, string Default = "", int MaxLength = 80, int MinLength = 0, string RestrictChars = null, bool ReturnNullForEscape = false, bool EscapeNonMarkupFormatting = true, bool AllowColorize = false)
	{
		Message = Markup.Transform(Message);
		string text = "";
		int startingLine = 0;
		text = Default ?? "";
		if (UIManager.UseNewPopups)
		{
			return AskStringAsync(Message, Default, MaxLength, MinLength, RestrictChars, ReturnNullForEscape, EscapeNonMarkupFormatting).Result;
		}
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		GameManager.Instance.PushGameView("Popup:AskString");
		int num = 95;
		while (num != 13)
		{
			ScrapBuffer.Copy(ScrapBuffer2);
			RenderBlock(Message + "\n" + text + "_", "[Enter to confirm]", "[{{W|Enter}} to confirm]", ScrapBuffer, startingLine);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			num = Keyboard.getch();
			if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("SubmitString:"))
			{
				text = Keyboard.CurrentMouseEvent.Event.Substring(Keyboard.CurrentMouseEvent.Event.IndexOf(':') + 1);
				if (text.Length >= MinLength)
				{
					Keyboard.ClearInput();
					GameManager.Instance.PopGameView(bHard: true);
					_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
					Loading.SetHideLoadStatus(hidden: false);
					return text;
				}
			}
			if (Keyboard.vkCode != Keys.MouseEvent)
			{
				if (Keyboard.vkCode == (Keys)131158)
				{
					string clipboardData = ClipboardHelper.GetClipboardData();
					text += clipboardData;
				}
				else
				{
					char c = (char)num;
					if ((char.IsDigit(c) || char.IsLetter(c) || char.IsPunctuation(c) || char.IsSeparator(c) || char.IsSymbol(c) || num == 32) && text.Length < MaxLength && (RestrictChars == null || RestrictChars.IndexOf(c) != -1))
					{
						text += c;
						if (EscapeNonMarkupFormatting && (c == '&' || c == '^'))
						{
							text += c;
						}
					}
				}
			}
			if (Keyboard.vkCode == Keys.Back)
			{
				if (EscapeNonMarkupFormatting && text.Length > 1 && (text.EndsWith("&&") || text.EndsWith("^^")))
				{
					text = text.Substring(0, text.Length - 2);
				}
				else if (text.Length > 0)
				{
					text = text.Substring(0, text.Length - 1);
				}
			}
			if ((Keyboard.vkCode == Keys.Escape || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick")) && MinLength <= 0)
			{
				Keyboard.ClearInput();
				GameManager.Instance.PopGameView(bHard: true);
				_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
				Loading.SetHideLoadStatus(hidden: false);
				if (!ReturnNullForEscape)
				{
					return "";
				}
				return null;
			}
			if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick" && text.Length >= MinLength)
			{
				break;
			}
		}
		Keyboard.ClearInput();
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		return text;
	}

	public static XRL.World.GameObject PickGameObject(string Title, List<XRL.World.GameObject> list, bool AllowEscape = false, bool ShowContext = false, int defaultSelected = 0)
	{
		string[] array = new string[list.Count];
		char[] array2 = new char[list.Count];
		IRenderable[] array3 = new IRenderable[list.Count];
		char c = 'a';
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			string text = list[i].DisplayName;
			if (ShowContext)
			{
				string listDisplayContext = list[i].GetListDisplayContext(The.Player);
				if (!string.IsNullOrEmpty(listDisplayContext))
				{
					text = text + " [" + listDisplayContext + "]";
				}
			}
			array[i] = text;
			array2[i] = ((c <= 'z') ? c++ : ' ');
			array3[i] = contextRender(list[i]);
		}
		IRenderable[] icons = array3;
		int num = ShowOptionList(Title, array, array2, 0, null, 60, RespectOptionNewlines: false, AllowEscape, defaultSelected, "", null, null, icons);
		if (num < 0)
		{
			return null;
		}
		return list[num];
	}

	private static RenderEvent contextRender(XRL.World.GameObject context)
	{
		return context?.RenderForUI();
	}

	public static int ShowConversation(string title = null, XRL.World.GameObject context = null, string Intro = null, List<string> Options = null, bool AllowTrade = false, bool bClosable = true)
	{
		List<QudMenuItem> list = new List<QudMenuItem>();
		if (bClosable)
		{
			list.Add(PopupMessage.CancelButton[0]);
		}
		if (AllowTrade)
		{
			list.Insert(0, PopupMessage.AcceptCancelTradeButton[0]);
		}
		int SelectedOption = 0;
		RenderEvent renderEvent = contextRender(context);
		List<QudMenuItem> list2 = new List<QudMenuItem>(Options.Count);
		for (int i = 0; i < Options.Count; i++)
		{
			list2.Add(new QudMenuItem
			{
				text = ((i < 9 && CapabilityManager.AllowKeyboardHotkeys) ? ("{{w|[" + (i + 1) + "]}} ") : "") + Options[i] + "\n\n",
				command = "option:" + i,
				hotkey = ((i < 9) ? ("Alpha" + (i + 1)) : null)
			});
		}
		WaitNewPopupMessage(Intro + "\n\n", list, delegate(QudMenuItem item)
		{
			if (item.command == "Cancel")
			{
				SelectedOption = -1;
			}
			if (item.command == "trade")
			{
				SelectedOption = -2;
			}
			string command = item.command;
			if (command != null && command.StartsWith("option:"))
			{
				SelectedOption = Convert.ToInt32(item.command.Substring("option:".Length));
			}
		}, list2, "", null, 0, title ?? context?.DisplayName, renderEvent);
		return SelectedOption;
	}

	[Obsolete("Use ShowOptionListAsync (note: Option not Options) - sorry! Will remove after Q1 2023")]
	public static Task<int> AsyncShowOptionsList(string Title = "", string[] Options = null, char[] Hotkeys = null, int Spacing = 0, string Intro = null, int MaxWidth = 60, bool RespectOptionNewlines = false, bool AllowEscape = false, int defaultSelected = 0, string SpacingText = "", XRL.World.GameObject context = null, IRenderable[] Icons = null, IRenderable IntroIcon = null, bool centerIntro = false, bool centerIntroIcon = true, int iconPosition = -1)
	{
		return ShowOptionListAsync(Title, Options, Hotkeys, Spacing, Intro, MaxWidth, RespectOptionNewlines, AllowEscape, defaultSelected, SpacingText, context, Icons, IntroIcon, centerIntro, centerIntroIcon, iconPosition);
	}

	public static async Task<int> ShowOptionListAsync(string Title = "", string[] Options = null, char[] Hotkeys = null, int Spacing = 0, string Intro = null, int MaxWidth = 60, bool RespectOptionNewlines = false, bool AllowEscape = false, int defaultSelected = 0, string SpacingText = "", XRL.World.GameObject context = null, IRenderable[] Icons = null, IRenderable IntroIcon = null, bool centerIntro = false, bool centerIntroIcon = true, int iconPosition = -1)
	{
		NavigationController framework = NavigationController.instance;
		NavigationContext oldContext = framework.activeContext;
		framework.activeContext = null;
		TaskCompletionSource<int> t = new TaskCompletionSource<int>();
		ShowOptionList(Title, Options, Hotkeys, Spacing, Intro, MaxWidth, RespectOptionNewlines, AllowEscape, defaultSelected, SpacingText, delegate(int choice)
		{
			t.TrySetResult(choice);
		}, context, Icons, IntroIcon, null, centerIntro, centerIntroIcon, iconPosition, forceNewPopup: true);
		try
		{
			return await t.Task;
		}
		finally
		{
			framework.activeContext = oldContext;
		}
	}

	public static int ShowOptionList(string Title = "", string[] Options = null, char[] Hotkeys = null, int Spacing = 0, string Intro = null, int MaxWidth = 60, bool RespectOptionNewlines = false, bool AllowEscape = false, int defaultSelected = 0, string SpacingText = "", Action<int> onResult = null, XRL.World.GameObject context = null, IRenderable[] Icons = null, IRenderable IntroIcon = null, QudMenuItem[] Buttons = null, bool centerIntro = false, bool centerIntroIcon = true, int iconPosition = -1, bool forceNewPopup = false)
	{
		if (context != null && Intro == null && !UIManager.UseNewPopups)
		{
			Intro = context.DisplayName;
		}
		Title = Markup.Transform(Title);
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		if (MaxWidth > 74)
		{
			MaxWidth = 74;
		}
		int i = 0;
		int SelectedOption = defaultSelected;
		int num = 20;
		int num2 = 0;
		if (Hotkeys != null)
		{
			num2 += 3;
		}
		if (Icons != null && iconPosition == -1)
		{
			num2 += 2;
		}
		PopupTextBuilder popupTextBuilder = new PopupTextBuilder
		{
			SpacingText = SpacingText,
			Spacing = Spacing
		};
		popupTextBuilder.RespectNewlines = RespectOptionNewlines;
		popupTextBuilder.MaxWidth = MaxWidth - 4;
		if (Intro != null)
		{
			popupTextBuilder.AddSection(Intro, -1);
			popupTextBuilder.AddSpacing(1, "");
		}
		popupTextBuilder.Padding = 2 + num2;
		for (int j = 0; j < Options.Length; j++)
		{
			popupTextBuilder.AddSection(Options[j], j);
			if (j != Options.Length - 1)
			{
				popupTextBuilder.AddSpacing(Spacing);
			}
		}
		int num3 = Math.Min(MaxWidth, Math.Max(ConsoleLib.Console.ColorUtility.LengthExceptFormatting(Title) + 2, popupTextBuilder.MaxClippedWidth + 4));
		if (popupTextBuilder.Lines.Count < num)
		{
			num = popupTextBuilder.Lines.Count;
		}
		else
		{
			i = SelectedOption;
		}
		if (IntroIcon != null)
		{
			num += 2;
		}
		if (UIManager.UseNewPopups || forceNewPopup)
		{
			List<QudMenuItem> list = new List<QudMenuItem>(Options.Length);
			for (int k = 0; k < Options.Length; k++)
			{
				if (!CapabilityManager.AllowKeyboardHotkeys)
				{
					Hotkeys = null;
				}
				string text = ((Hotkeys != null) ? ((Hotkeys[k] != 0 && Hotkeys[k] != ' ') ? ("{{W|[" + Hotkeys[k] + "]}} ") : "    ") : "") + "{{y|" + Options[k] + "}}";
				list.Add(new QudMenuItem
				{
					text = text,
					icon = ((Icons != null) ? Icons[k] : null),
					command = "option:" + k,
					hotkey = ((Hotkeys != null) ? ("char:" + Hotkeys[k]) : "")
				});
			}
			new TextBlock(Intro, 80, 5000);
			IRenderable renderable;
			if (context == null)
			{
				renderable = IntroIcon;
			}
			else
			{
				IRenderable renderable2 = contextRender(context);
				renderable = renderable2;
			}
			IRenderable renderable3 = renderable;
			List<QudMenuItem> list2 = new List<QudMenuItem>();
			if (!Buttons.IsNullOrEmpty())
			{
				list2.AddRange(Buttons);
			}
			if (AllowEscape)
			{
				list2.AddRange(PopupMessage.CancelButton);
			}
			WaitNewPopupMessage(Intro, list2, delegate(QudMenuItem item)
			{
				if (item.command == "Cancel")
				{
					SelectedOption = -1;
				}
				else
				{
					string command = item.command;
					if (command != null && command.StartsWith("option:"))
					{
						SelectedOption = Convert.ToInt32(item.command.Substring("option:".Length));
					}
				}
				if (onResult != null)
				{
					onResult(SelectedOption);
				}
			}, list, Title, null, defaultSelected, context?.DisplayName, renderable3);
			return SelectedOption;
		}
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		GameManager.Instance.PushGameView("Popup:Choice");
		Keys[] array = null;
		if (!Buttons.IsNullOrEmpty())
		{
			array = new Keys[Buttons.Length];
			for (int l = 0; l < Buttons.Length; l++)
			{
				UnityEngine.KeyCode key = Keyboard.ParseUnityEngineKeyCode(Buttons[l].hotkey);
				Keyboard.Keymap.TryGetValue(key, out var value);
				array[l] = value;
			}
		}
		Keys keys = Keys.Space;
		bool flag = true;
		int num4 = 0;
		int num5 = 0;
		while (flag || (Keyboard.RawCode != Keys.Space && Keyboard.RawCode != Keys.Enter && Keyboard.RawCode != Keys.Enter))
		{
			ScrapBuffer.Copy(ScrapBuffer2);
			int num6 = (80 - num3 - 4) / 2;
			int num7 = (25 - num - 2) / 2;
			int num8 = num6 + num3 + 4;
			int num9 = num7 + num + 3;
			ScrapBuffer.Fill(num6, num7, num8, num9, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
			ScrapBuffer.ThickSingleBox(num6, num7, num8, num9, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			flag = false;
			int num10 = 0;
			if (IntroIcon != null)
			{
				if (centerIntroIcon)
				{
					ScrapBuffer.Goto(num6 + 2 + (num3 - 1) / 2, num7 + num10 + 2);
				}
				else
				{
					ScrapBuffer.Goto(num6 + 2, num7 + num10 + 2);
				}
				ScrapBuffer.Write(IntroIcon);
				num10 += 2;
			}
			string[] selectionLines = popupTextBuilder.GetSelectionLines(-1);
			for (int m = 0; m < selectionLines.Length; m++)
			{
				if (centerIntro)
				{
					ScrapBuffer.Goto(num6 + 2 + (num3 - ConsoleLib.Console.ColorUtility.LengthExceptFormatting(selectionLines[m])) / 2, num7 + num10 + 2 + m);
				}
				else
				{
					ScrapBuffer.Goto(num6 + 2, num7 + num10 + 2 + m);
				}
				ScrapBuffer.Write(selectionLines[m]);
			}
			if (selectionLines.Length != 0)
			{
				num10 += selectionLines.Length + 1;
			}
			int selectionStart = popupTextBuilder.GetSelectionStart(0);
			int num11 = i;
			while (num10 <= num && num11 < Options.Length)
			{
				bool flag2 = SelectedOption == num11;
				int selectionStart2 = popupTextBuilder.GetSelectionStart(num11);
				string[] selectionLines2 = popupTextBuilder.GetSelectionLines(num11);
				int n;
				for (n = 0; n < selectionLines2.Length; n++)
				{
					if (num10 > num)
					{
						break;
					}
					bool flag3 = n == 0;
					if (flag2 && flag3)
					{
						ScrapBuffer.Goto(num6 + 2, num7 + 2 + num10);
						ScrapBuffer.Write("{{Y|>}} ");
					}
					if (Hotkeys != null)
					{
						char c = Hotkeys[num11];
						if (flag3 && c != ' ')
						{
							ScrapBuffer.Goto(num6 + 4, num7 + 2 + num10);
							ScrapBuffer.Write((flag2 ? "{{W|" : "{{w|") + c + "}}) ");
						}
					}
					ScrapBuffer.Goto(num6 + num2 + 4, num7 + 2 + num10);
					ScrapBuffer.Write(StringFormat.SubstringNotCountingFormat(selectionLines2[n], 0, num3 - num2 - 2));
					if (n == 0 && Icons != null && num11 < Icons.Length && Icons[num11] != null)
					{
						if (iconPosition == -1)
						{
							ScrapBuffer.Goto(num6 + num2 + 2, num7 + 2 + num10);
						}
						else
						{
							ScrapBuffer.Goto(num6 + num2 + 2 + iconPosition, num7 + 2 + num10);
						}
						ScrapBuffer.Write(Icons[num11]);
					}
					num4 = selectionStart2 + n;
					num10++;
				}
				if (n == selectionLines2.Length)
				{
					num5 = num11;
				}
				num11++;
			}
			if (popupTextBuilder.Lines.Count > num)
			{
				if (num4 + 1 < popupTextBuilder.Lines.Count)
				{
					ScrapBuffer.Goto(num6 + 2, num9);
					ScrapBuffer.Write("{{W|<More...>}}");
				}
				if (i > 0)
				{
					ScrapBuffer.Goto(num6 + 2, num7);
					ScrapBuffer.Write("{{W|<More...>}}");
				}
				int num12 = num9 - num7 - 2;
				if (num12 > 0)
				{
					ScreenBuffer scrapBuffer = ScrapBuffer;
					int top = num7 + 1;
					int selectionStart3 = popupTextBuilder.GetSelectionStart(i);
					int handleEnd = num4;
					ScrollbarHelper.Paint(scrapBuffer, top, num8, num12, ScrollbarHelper.Orientation.Vertical, selectionStart, popupTextBuilder.Lines.Count - 1, selectionStart3, handleEnd);
				}
			}
			if (!Buttons.IsNullOrEmpty())
			{
				ScrapBuffer.Goto(num6 + 14, num9);
				for (int handleEnd = 0; handleEnd < Buttons.Length; handleEnd++)
				{
					QudMenuItem qudMenuItem = Buttons[handleEnd];
					ScrapBuffer.Write(qudMenuItem.text);
					ScrapBuffer.X++;
				}
			}
			if (!string.IsNullOrEmpty(Title))
			{
				ScrapBuffer.Goto(num6 + 2, num7);
				ScrapBuffer.Write(StringFormat.ClipLine(" " + Title, num3, AddEllipsis: false));
			}
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			keys = Keyboard.getvk(XRL.UI.Options.MapDirectionsToKeypad);
			if (Icons != null || IntroIcon != null)
			{
				ScreenBuffer.ClearImposterSuppression();
			}
			if (Hotkeys != null)
			{
				for (int num13 = 0; num13 < Hotkeys.Length; num13++)
				{
					if (Keyboard.Char == Hotkeys[num13] && Hotkeys[num13] != ' ')
					{
						SelectedOption = num13;
						Keyboard.RawCode = Keys.Space;
						break;
					}
				}
			}
			if (array != null)
			{
				for (int num14 = 0; num14 < array.Length; num14++)
				{
					if (keys == array[num14] && Buttons[num14].command != null && Buttons[num14].command.StartsWith("option:"))
					{
						return Convert.ToInt32(Buttons[num14].command.Substring("option:".Length));
					}
				}
			}
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("Choice:"))
			{
				SelectedOption = Convert.ToInt32(Keyboard.CurrentMouseEvent.Event.Split(':')[1]);
				break;
			}
			if (AllowEscape && (keys == Keys.Escape || Keyboard.vkCode == Keys.Escape))
			{
				_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
				GameManager.Instance.PopGameView(bHard: true);
				return -1;
			}
			if (keys == Keys.NumPad2)
			{
				SelectedOption++;
			}
			if (keys == Keys.NumPad8)
			{
				SelectedOption--;
			}
			if (keys == Keys.Next || keys == Keys.Next)
			{
				i = (SelectedOption = num5 + 1);
			}
			if (keys == Keys.Prior || keys == Keys.Prior)
			{
				int selectionStart4 = popupTextBuilder.GetSelectionStart(i);
				for (int num15 = num - selectionStart; SelectedOption > 0 && popupTextBuilder.GetSelectionStart(SelectedOption) + num15 > selectionStart4; SelectedOption--)
				{
				}
				i = SelectedOption;
			}
			SelectedOption = Math.Max(0, Math.Min(Options.Length - 1, SelectedOption));
			if (i > SelectedOption)
			{
				i = SelectedOption;
			}
			if (SelectedOption > num5)
			{
				int num16 = num - selectionStart;
				for (int selectionStart5 = popupTextBuilder.GetSelectionStart(SelectedOption + 1); i < Options.Length && popupTextBuilder.GetSelectionStart(i) + num16 < selectionStart5; i++)
				{
				}
			}
		}
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		GameManager.Instance.PopGameView(bHard: true);
		Loading.SetHideLoadStatus(hidden: false);
		if (Keyboard.vkCode == Keys.Escape)
		{
			return -1;
		}
		return SelectedOption;
	}

	public static List<int> PickSeveral(string Title = "", string[] Options = null, char[] Hotkeys = null, int Amount = -1, int Spacing = 0, string Intro = null, int MaxWidth = 60, bool RespectOptionNewlines = false, bool AllowEscape = false, int DefaultSelected = 0, string SpacingText = "", Action<int> OnResult = null, XRL.World.GameObject Context = null, IRenderable[] Icons = null, IRenderable IntroIcon = null, bool CenterIntro = false, bool CenterIntroIcon = true, int IconPosition = -1, bool ForceNewPopup = false)
	{
		List<int> list = new List<int>();
		string[] array = new string[Options.Length];
		QudMenuItem[] array2 = new QudMenuItem[2]
		{
			new QudMenuItem
			{
				text = "{{W|[Backspace]}} {{y|Accept}}",
				command = "option:-2",
				hotkey = "Backspace"
			},
			new QudMenuItem
			{
				command = "option:-3",
				hotkey = "Tab"
			}
		};
		while (true)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (list.Contains(i) ? "{{W|[þ]}} " : "{{y|[ ]}} ");
				array[i] += Options[i];
			}
			array2[1].text = ((list.Count == array.Length) ? "{{W|[Tab]}} {{y|Deselect All}}" : "{{W|[Tab]}} {{y|Select All}}");
			int num = ShowOptionList(Title, array, Hotkeys, Spacing, Intro, MaxWidth, RespectOptionNewlines, AllowEscape, DefaultSelected, SpacingText, OnResult, Context, Icons, IntroIcon, array2, CenterIntro, CenterIntro, IconPosition, ForceNewPopup);
			switch (num)
			{
			case -1:
				return null;
			case -2:
				if (Amount >= 0 && list.Count > Amount)
				{
					Show("You cannot select more than " + Grammar.Cardinal(Amount) + " options!");
					continue;
				}
				return list;
			case -3:
				if (list.Count != array.Length)
				{
					list.Clear();
					list.AddRange(Enumerable.Range(0, array.Length));
				}
				else
				{
					list.Clear();
				}
				continue;
			}
			int num2 = list.IndexOf(num);
			if (num2 >= 0)
			{
				list.RemoveAt(num2);
			}
			else
			{
				list.Add(num);
			}
			DefaultSelected = num;
		}
	}

	public static string ShowColorPicker(string Title, int Spacing = 0, string Intro = null, int MaxWidth = 60, bool RespectOptionNewlines = false, bool AllowEscape = false, int defaultSelected = 0, string SpacingText = "", bool includeNone = true, bool includePatterns = false, bool allowBackground = false, string previewContent = null)
	{
		return ShowColorPickerAsync(Title, Spacing, Intro, MaxWidth, RespectOptionNewlines, AllowEscape, defaultSelected, SpacingText, includeNone, includePatterns, allowBackground, previewContent).Result;
	}

	public static async Task<string> ShowColorPickerAsync(string Title, int Spacing = 0, string Intro = null, int MaxWidth = 60, bool RespectOptionNewlines = false, bool AllowEscape = false, int defaultSelected = 0, string SpacingText = "", bool includeNone = true, bool includePatterns = false, bool allowBackground = false, string previewContent = null)
	{
		ColorPickerOptionStrings.Clear();
		ColorPickerOptions.Clear();
		ColorPickerKeymap.Clear();
		char c = 'a';
		if (includeNone)
		{
			if (string.IsNullOrWhiteSpace(previewContent))
			{
				ColorPickerOptionStrings.Add("none");
			}
			else
			{
				ColorPickerOptionStrings.Add(previewContent + " (no coloring)");
			}
			ColorPickerOptions.Add("");
			ColorPickerKeymap.Add(c++);
		}
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
		StringBuilder stringBuilder2 = XRL.World.Event.NewStringBuilder();
		foreach (SolidColor pickerColor in MarkupShaders.PickerColors)
		{
			if (allowBackground || !pickerColor.Background.HasValue || pickerColor.Background == 'k')
			{
				stringBuilder.Clear();
				stringBuilder2.Clear();
				if (allowBackground)
				{
					pickerColor.AssembleCode(stringBuilder2);
					stringBuilder.Append(stringBuilder2);
				}
				else
				{
					stringBuilder2.Append(pickerColor.Foreground);
					pickerColor.AssembleCode(stringBuilder);
				}
				if (string.IsNullOrWhiteSpace(previewContent))
				{
					stringBuilder.Append(pickerColor.GetDisplayName());
				}
				else
				{
					stringBuilder.Append(previewContent).Append("^k&y (").Append(pickerColor.GetDisplayName())
						.Append(")");
				}
				ColorPickerOptionStrings.Add(stringBuilder.ToString());
				ColorPickerOptions.Add(stringBuilder2.ToString());
				ColorPickerKeymap.Add(c++);
			}
		}
		if (includePatterns)
		{
			List<IMarkupShader> list = new List<IMarkupShader>(MarkupShaders.PickerShaders);
			list.Sort((IMarkupShader a, IMarkupShader b) => string.Compare(a.GetDisplayName(), b.GetDisplayName(), StringComparison.CurrentCultureIgnoreCase));
			foreach (IMarkupShader item in list)
			{
				if (string.IsNullOrWhiteSpace(previewContent))
				{
					ColorPickerOptionStrings.Add(stringBuilder.Clear().Append("{{").Append(item.GetName())
						.Append('|')
						.Append(item.GetDisplayName())
						.Append("}}")
						.ToString());
				}
				else
				{
					ColorPickerOptionStrings.Add(stringBuilder.Clear().Append("{{").Append(item.GetName())
						.Append('|')
						.Append(previewContent)
						.Append("}} (")
						.Append(item.GetDisplayName())
						.Append(")")
						.ToString());
				}
				ColorPickerOptions.Add(item.GetName());
				ColorPickerKeymap.Add(' ');
			}
		}
		int num = await ShowOptionListAsync(Title, ColorPickerOptionStrings.ToArray(), ColorPickerKeymap.ToArray(), Spacing, Intro, MaxWidth, RespectOptionNewlines, AllowEscape, defaultSelected, SpacingText);
		if (num < 0)
		{
			return null;
		}
		return ColorPickerOptions[num];
	}

	public static DialogResult ShowYesNo(string Message, bool AllowEscape = true, DialogResult defaultResult = DialogResult.Yes)
	{
		Message = Markup.Transform(Options.OverlayUI ? ("{{white|" + Message + "}}") : Message);
		Keyboard.ClearMouseEvents();
		int num = 0;
		DialogResult result = defaultResult;
		if (UIManager.UseNewPopups)
		{
			WaitNewPopupMessage(Message, PopupMessage.YesNoButton, delegate(QudMenuItem i)
			{
				if (i.command == "No")
				{
					result = DialogResult.No;
				}
				if (i.command == "Yes")
				{
					result = DialogResult.Yes;
				}
				if (i.command == "Cancel")
				{
					result = DialogResult.No;
				}
			});
			return result;
		}
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		new TextBlock(Message, 80, 5000);
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Keyboard.ClearInput();
		Keys keys = Keys.Space;
		bool flag = true;
		while (flag || ((Keyboard.vkCode != Keys.MouseEvent || !Keyboard.CurrentMouseEvent.Event.StartsWith("LeftOption:")) && Keyboard.RawCode != Keys.Y && Keyboard.RawCode != Keys.N && !(Keyboard.vkCode == Keys.Escape && AllowEscape) && Keyboard.vkCode != Keys.Space && Keyboard.vkCode != Keys.Enter))
		{
			flag = false;
			int num2 = RenderBlock(Message, "[Yes] [No]", "[{{W|Y}}es] [{{W|N}}o]", ScrapBuffer, num, -1, -1, null, BottomLineRegions: true);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (keys == Keys.NumPad2 && num2 - num > 20)
			{
				num++;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.Next)
			{
				num += 23;
			}
			if (keys == Keys.Prior || keys == Keys.Next)
			{
				num -= 23;
			}
			if (num < 0)
			{
				num = 0;
			}
		}
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		if (Keyboard.RawCode == Keys.Y || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftOption:1"))
		{
			result = DialogResult.Yes;
		}
		if (Keyboard.RawCode == Keys.N || (keys == Keys.Escape && AllowEscape) || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftOption:2"))
		{
			result = DialogResult.No;
		}
		Keyboard.ClearInput();
		GameManager.Instance.PopGameView(bHard: true);
		return result;
	}

	public static async Task<DialogResult> ShowYesNoAsync(string Message)
	{
		DialogResult result = DialogResult.Cancel;
		QudMenuItem obj = await NewPopupMessageAsync(Message, PopupMessage.YesNoButton);
		if (obj.command == "No")
		{
			result = DialogResult.No;
		}
		if (obj.command == "Yes")
		{
			result = DialogResult.Yes;
		}
		return result;
	}

	public static async Task<DialogResult> ShowYesNoCancelAsync(string Message)
	{
		DialogResult result = DialogResult.Cancel;
		QudMenuItem obj = await NewPopupMessageAsync(Message, PopupMessage.YesNoCancelButton);
		if (obj.command == "No")
		{
			result = DialogResult.No;
		}
		if (obj.command == "Yes")
		{
			result = DialogResult.Yes;
		}
		if (obj.command == "Cancel")
		{
			result = DialogResult.Cancel;
		}
		return result;
	}

	public static DialogResult ShowYesNoCancel(string Message)
	{
		Message = Markup.Transform(Options.OverlayUI ? ("{{white|" + Message + "}}") : Message);
		int num = 0;
		DialogResult result = DialogResult.Cancel;
		if (UIManager.UseNewPopups)
		{
			WaitNewPopupMessage(Message, PopupMessage.YesNoCancelButton, delegate(QudMenuItem i)
			{
				if (i.command == "No")
				{
					result = DialogResult.No;
				}
				if (i.command == "Yes")
				{
					result = DialogResult.Yes;
				}
				if (i.command == "Cancel")
				{
					result = DialogResult.Cancel;
				}
			});
			return result;
		}
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		new TextBlock(Message, 80, 5000);
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Keyboard.ClearInput();
		Keys keys = Keys.Space;
		while (Keyboard.RawCode != Keys.Y && Keyboard.RawCode != Keys.Enter && Keyboard.RawCode != Keys.N && (Keyboard.vkCode != Keys.MouseEvent || !Keyboard.CurrentMouseEvent.Event.StartsWith("LeftOption:")))
		{
			GameManager.Instance.ClearRegions();
			int num2 = RenderBlock(Message, "[Yes] [No] [ESC-Cancel]", "[{{W|Y}}es] [{{W|N}}o] [{{W|ESC}}-Cancel]", ScrapBuffer, num, -1, -1, null, BottomLineRegions: true);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (keys == Keys.NumPad2 && num2 - num > 20)
			{
				num++;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.Next)
			{
				num += 23;
			}
			if (keys == Keys.Prior || keys == Keys.Next)
			{
				num -= 23;
			}
			if (keys == Keys.Escape || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick"))
			{
				break;
			}
			if (num < 0)
			{
				num = 0;
			}
		}
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		if (Keyboard.RawCode == Keys.Y || Keyboard.RawCode == Keys.Enter || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftOption:1"))
		{
			result = DialogResult.Yes;
		}
		if (Keyboard.RawCode == Keys.N || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftOption:2"))
		{
			result = DialogResult.No;
		}
		GameManager.Instance.PopGameView(bHard: true);
		return result;
	}

	public static int ShowChoice(string Message)
	{
		Message = Markup.Transform(Options.OverlayUI ? ("{{white|" + Message + "}}") : Message);
		string currentGameView = GameManager.Instance.CurrentGameView;
		Popup_Text.Text = Sidebar.FormatToRTF(Message);
		GameManager.Instance.PushGameView("Popup:Text");
		Loading.SetHideLoadStatus(hidden: true);
		int startingLine = 0;
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		Render(Message, "", "", ScrapBuffer, startingLine);
		_TextConsole.DrawBuffer(ScrapBuffer);
		int result = Keyboard.getchraw();
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2);
		GameManager.Instance.CurrentGameView = currentGameView;
		Loading.SetHideLoadStatus(hidden: true);
		return result;
	}

	public static void DisplayLoadError(string Loadable, int Errors = 1)
	{
		bool flag = Errors == 1;
		string text = string.Format("There {0} {1} {2} while loading this {3}.", flag ? "was" : "were", Errors, flag ? "error" : "errors", Loadable);
		if (DisplayedLoadError)
		{
			MessageQueue.AddPlayerMessage(text);
			return;
		}
		DisplayedLoadError = true;
		Show(text + "\n\nYou can check your Player.log or examine the corrupted objects for errors. Once finished you can wish for \"clearcorrupt\" to remove the offending objects and continue playing.");
	}
}
