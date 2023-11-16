using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using UnityEngine;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

[HasGameBasedStaticCache]
[UIView("BookUI", false, false, false, null, null, false, 0, false)]
public class BookUI : IWantsTextConsoleInit
{
	public static int ScrollPosition;

	public static int IndexPosition;

	public static TextConsole Console;

	public static ScreenBuffer Buffer;

	public static Dictionary<string, string> BookCorpus = new Dictionary<string, string>();

	public static Dictionary<string, List<BookPage>> Books = new Dictionary<string, List<BookPage>>();

	public static List<string> DynamicBooks = new List<string>();

	public static readonly Dictionary<string, Action<XmlDataHelper>> _Nodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "books", HandleNodes },
		{ "book", HandleBookNode }
	};

	private static bool IncludeCorpusData = false;

	private static StringBuilder CorpusBuilder = new StringBuilder();

	public static void Reset()
	{
		for (int i = 0; i < DynamicBooks.Count; i++)
		{
			if (Books.ContainsKey(DynamicBooks[i]))
			{
				Books.Remove(DynamicBooks[i]);
			}
		}
		DynamicBooks = new List<string>();
	}

	public void Init(TextConsole _Console, ScreenBuffer _Buffer)
	{
		Console = _Console;
		Buffer = _Buffer;
		InitBooks();
	}

	public static void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(_Nodes);
	}

	public static void InitBooks(bool bIncludeCorpusData = false)
	{
		IncludeCorpusData = bIncludeCorpusData;
		Books.Clear();
		CorpusBuilder.Clear();
		List<(string, ModInfo)> Paths = new List<(string, ModInfo)>();
		Paths.Add((DataManager.FilePath("Books.xml"), null));
		ModManager.ForEachFile("Books.xml", delegate(string path, ModInfo modInfo)
		{
			Paths.Add((path, modInfo));
		});
		foreach (var (fileName, modInfo2) in Paths)
		{
			using XmlDataHelper xmlDataHelper = DataManager.GetXMLStream(fileName, modInfo2);
			xmlDataHelper.HandleNodes(_Nodes);
			xmlDataHelper.Close();
		}
	}

	public static void HandleBookNode(XmlDataHelper Reader)
	{
		string BookID = Reader.GetAttribute("ID");
		string BookTitle = Reader.GetAttribute("Title");
		string Format = Reader.GetAttribute("Format");
		string Margins = Reader.GetAttribute("Margins");
		CorpusBuilder.Length = 0;
		if (Books.ContainsKey(BookID))
		{
			Books[BookID].Clear();
		}
		else
		{
			Books.Add(BookID, new List<BookPage>());
		}
		Reader.HandleNodes(new Dictionary<string, Action<XmlDataHelper>> { 
		{
			"page",
			delegate
			{
				string text = Markup.Transform(Reader.ReadString(), refreshAtNewline: true);
				if (IncludeCorpusData)
				{
					CorpusBuilder.Append(text).Append('\n');
				}
				text = text.Replace("[[", "").Replace("]]", "");
				if (!string.IsNullOrEmpty(Format))
				{
					Books[BookID].AddRange(AutoformatPages(BookTitle, text, Format, Margins));
				}
				else
				{
					Books[BookID].Add(new BookPage(BookTitle, text));
				}
				Reader.DoneWithElement();
			}
		} });
		if (IncludeCorpusData)
		{
			Match match = Regex.Match(CorpusBuilder.ToString(), "\\[\\[.*?\\]\\]");
			while (match != null && !string.IsNullOrEmpty(match.Value))
			{
				CorpusBuilder.Replace("[[" + match.Groups[0].Value + "]]", "");
				Debug.Log("Removing book header from corpus: " + match.Groups[0]?.ToString() + "...");
				match = match.NextMatch();
			}
			BookCorpus[BookID] = GameText.VariableReplace(CorpusBuilder);
		}
	}

	public static int NextWordLength(string Text, int Pos, StringBuilder Line, int LineWidth)
	{
		int num = 0;
		while (Pos < Text.Length)
		{
			if (Text[Pos] == ' ')
			{
				return num;
			}
			if (Text[Pos] == '\n')
			{
				return num;
			}
			if (Text[Pos] == '&' || Text[Pos] == '^')
			{
				Line.Append(Text[Pos]);
				Pos++;
				Line.Append(Text[Pos]);
			}
			else
			{
				num++;
				Line.Append(Text[Pos]);
			}
			Pos++;
		}
		return num;
	}

	public static bool NextLine(string Text, ref int Pos, StringBuilder Line, int LineWidth)
	{
		int num = 0;
		while (Pos < Text.Length)
		{
			if (Text[Pos] == '&' || Text[Pos] == '^')
			{
				Line.Append(Text[Pos]);
				Pos++;
				Line.Append(Text[Pos]);
			}
			else
			{
				num++;
				Line.Append(Text[Pos]);
			}
			Pos++;
			if (Pos < Text.Length)
			{
				if (Text[Pos] == '\n')
				{
					Line.Append('\n');
					Pos++;
					break;
				}
				if (num + NextWordLength(Text, Pos, Line, LineWidth) >= LineWidth)
				{
					Line.Append('\n');
					break;
				}
			}
		}
		if (Pos >= Text.Length)
		{
			return false;
		}
		return true;
	}

	public static List<BookPage> AutoformatPages(string Title, string Text, string Format, string Margins)
	{
		List<BookPage> list = new List<BookPage>();
		int num = 2;
		int num2 = 2;
		int num3 = 2;
		int num4 = 2;
		if (!string.IsNullOrEmpty(Margins))
		{
			string[] array = Margins.Split(',');
			if (array.GetUpperBound(0) >= 0)
			{
				num3 = int.Parse(array[0]);
			}
			if (array.GetUpperBound(0) >= 1)
			{
				num2 = int.Parse(array[1]);
			}
			if (array.GetUpperBound(0) >= 2)
			{
				num4 = int.Parse(array[2]);
			}
			if (array.GetUpperBound(0) >= 3)
			{
				num = int.Parse(array[3]);
			}
		}
		int maxWidth = 80 - num - num2;
		int num5 = 24 - num3 - num4;
		StringBuilder stringBuilder = new StringBuilder(1024);
		int MaxClippedWidth = 0;
		List<string> list2 = StringFormat.ClipTextToArray(GameText.VariableReplace(Text), maxWidth, out MaxClippedWidth, KeepNewlines: true);
		int num6 = 0;
		for (int i = 0; i < list2.Count; i++)
		{
			stringBuilder.Append(list2[i]);
			num6++;
			if (num6 >= num5 || i == list2.Count - 1)
			{
				num6 = 0;
				BookPage bookPage = new BookPage(Title, stringBuilder.ToString());
				bookPage.Format = Format;
				bookPage.LeftMargin = num;
				bookPage.RightMargin = num2;
				bookPage.TopMargin = num3;
				bookPage.BottomMargin = num4;
				list.Add(bookPage);
				stringBuilder.Length = 0;
			}
			else
			{
				stringBuilder.Append("\n");
			}
		}
		return list;
	}

	public static void RenderPage(string BookID, int nPage)
	{
		BookPage bookPage;
		if (BookID[0] == '@')
		{
			if (!Books.ContainsKey(BookID))
			{
				string text = BookID.Substring(1);
				Type type = ModManager.ResolveType("XRL.World.Parts." + text);
				object obj = Activator.CreateInstance(type);
				List<IBookContents.BookPageInfo> obj2 = (List<IBookContents.BookPageInfo>)type.GetMethod("GetContents").Invoke(obj, new object[0]);
				List<BookPage> list = new List<BookPage>();
				foreach (IBookContents.BookPageInfo item in obj2)
				{
					if (item.Format == "Auto")
					{
						list.AddRange(AutoformatPages(item.Title, GameText.VariableReplace(item.Text), item.Format, item.Margins));
					}
					else
					{
						list.Add(new BookPage(item.Title, GameText.VariableReplace(item.Text)));
					}
				}
				Books.Add(BookID, list);
				DynamicBooks.Add(BookID);
			}
			bookPage = Books[BookID][nPage];
		}
		else
		{
			bookPage = Books[BookID][nPage];
		}
		int num = 1 + bookPage.TopMargin;
		int num2 = 0;
		for (int i = num2; i < bookPage.Lines.Count; i++)
		{
			if (num >= 24)
			{
				break;
			}
			Buffer.Goto(bookPage.LeftMargin, num);
			Buffer.Write(GameText.VariableReplace(bookPage.Lines[i]));
			num++;
		}
		Buffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		Buffer.Goto(2, 0);
		Buffer.Write("[ {{Y|" + bookPage.Title + "}} ]");
		Buffer.Goto(2, 24);
		Buffer.Write("[ Page {{C|" + (nPage + 1) + "}} of {{C|" + Books[BookID].Count + "}} ]");
		Buffer.Goto(35, 24);
		Buffer.Write(" [{{W|4}}-Previous Page {{W|6}}-Next Page {{W|Escape}}-Exit]");
		if (bookPage.Lines.Count > 23)
		{
			for (int j = 1; j < 24; j++)
			{
				Buffer.Goto(79, j);
				Buffer.Write(177, ConsoleLib.Console.ColorUtility.Bright((ushort)0), 0);
			}
			_ = (int)Math.Ceiling((double)bookPage.Lines.Count / 23.0);
			int num3 = (int)((double)(int)Math.Ceiling((double)bookPage.Lines.Count + 23.0) / 23.0);
			_ = 0;
			if (num3 <= 0)
			{
				num3 = 1;
			}
			int num4 = 23 / num3;
			if (num4 <= 0)
			{
				num4 = 1;
			}
			int num5 = (int)((double)(23 - num4) * ((double)num2 / (double)(bookPage.Lines.Count - 23)));
			num5++;
			for (int k = num5; k < num5 + num4; k++)
			{
				Buffer.Goto(79, k);
				Buffer.Write(219, ConsoleLib.Console.ColorUtility.Bright(7), 0);
			}
		}
	}

	public static void RenderPage(MarkovBook Book, int nPage)
	{
		BookPage bookPage = Book.Pages[nPage];
		int num = 1 + bookPage.TopMargin;
		int num2 = 0;
		for (int i = num2; i < bookPage.Lines.Count; i++)
		{
			if (num >= 24)
			{
				break;
			}
			Buffer.Goto(bookPage.LeftMargin, num);
			Buffer.Write(GameText.VariableReplace(bookPage.Lines[i]));
			num++;
		}
		Buffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		Buffer.Goto(2, 0);
		Buffer.Write("[ {{Y|" + bookPage.Title + "}} ]");
		Buffer.Goto(2, 24);
		Buffer.Write("[ Page {{C|" + (nPage + 1) + "}} of {{C|" + Book.Pages.Count + "}} ]");
		Buffer.Goto(35, 24);
		Buffer.Write(" [{{W|4}}-Previous Page {{W|6}}-Next Page {{W|Escape}}-Exit]");
		if (bookPage.Lines.Count > 23)
		{
			for (int j = 1; j < 24; j++)
			{
				Buffer.Goto(79, j);
				Buffer.Write(177, ConsoleLib.Console.ColorUtility.Bright((ushort)0), 0);
			}
			_ = (int)Math.Ceiling((double)bookPage.Lines.Count / 23.0);
			int num3 = (int)((double)(int)Math.Ceiling((double)bookPage.Lines.Count + 23.0) / 23.0);
			_ = 0;
			if (num3 <= 0)
			{
				num3 = 1;
			}
			int num4 = 23 / num3;
			if (num4 <= 0)
			{
				num4 = 1;
			}
			int num5 = (int)((double)(23 - num4) * ((double)num2 / (double)(bookPage.Lines.Count - 23)));
			num5++;
			for (int k = num5; k < num5 + num4; k++)
			{
				Buffer.Goto(79, k);
				Buffer.Write(219, ConsoleLib.Console.ColorUtility.Bright(7), 0);
			}
		}
	}

	public static void ShowBook(string BookID, Action<int> onShowPage = null)
	{
		GameManager.Instance.PushGameView("Book");
		int num = 0;
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer2(bLoadFromCurrent: true);
		Keys num2;
		do
		{
			XRL.World.Event.ResetPool(resetMinEventPools: false);
			Buffer.Clear();
			Buffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			RenderPage(BookID, num);
			Console.DrawBuffer(Buffer);
			onShowPage?.Invoke(num);
			num2 = Keyboard.getvk(MapDirectionToArrows: true);
			if (num2 == Keys.NumPad6 && num < Books[BookID].Count - 1)
			{
				SoundManager.PlaySound("SFX_Books_Page Turn_1", 0.2f);
				num++;
			}
			if (num2 == Keys.NumPad4 && num > 0)
			{
				SoundManager.PlaySound("SFX_Books_Page Turn_1", 0.2f);
				num--;
			}
		}
		while (num2 != Keys.Escape);
		GameManager.Instance.PopGameView(bHard: true);
		scrapBuffer.Draw();
	}

	public static void ShowBook(MarkovBook Book, Action<int> onShowPage = null, Action<int> afterShowPage = null)
	{
		GameManager.Instance.PushGameView("Book");
		int num = 0;
		Keys num2;
		do
		{
			XRL.World.Event.ResetPool(resetMinEventPools: false);
			Buffer.Clear();
			Buffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			RenderPage(Book, num);
			Console.DrawBuffer(Buffer);
			onShowPage?.Invoke(num);
			num2 = Keyboard.getvk(MapDirectionToArrows: true);
			if (num2 == Keys.NumPad6 && num < Book.Pages.Count - 1)
			{
				SoundManager.PlaySound("SFX_Books_Page Turn_1", 0.2f);
				afterShowPage?.Invoke(num);
				num++;
			}
			if (num2 == Keys.NumPad4 && num > 0)
			{
				SoundManager.PlaySound("SFX_Books_Page Turn_1", 0.2f);
				afterShowPage?.Invoke(num);
				num--;
			}
		}
		while (num2 != Keys.Escape);
		afterShowPage?.Invoke(num);
		GameManager.Instance.PopGameView(bHard: true);
	}

	public static void ShowBook(string PageText, string BookTitle, Action<int> onShowPage = null, Action<int> afterShowPage = null)
	{
		PageText = Markup.Transform(PageText);
		BookTitle = Markup.Transform(BookTitle);
		MarkovBook markovBook = new MarkovBook();
		markovBook.Title = BookTitle;
		markovBook.Pages = new List<BookPage>();
		string format = "Auto";
		string margins = "1,2,2,2";
		markovBook.Pages.AddRange(AutoformatPages(BookTitle, PageText, format, margins));
		ShowBook(markovBook, onShowPage, afterShowPage);
	}

	public static void ShowBook(List<string> PageText, string BookTitle, Action<int> onShowPage = null, Action<int> afterShowPage = null)
	{
		BookTitle = Markup.Transform(BookTitle);
		MarkovBook markovBook = new MarkovBook();
		markovBook.Title = BookTitle;
		markovBook.Pages = new List<BookPage>();
		string format = "Auto";
		string margins = "1,2,2,2";
		foreach (string item in PageText)
		{
			markovBook.Pages.AddRange(AutoformatPages(BookTitle, Markup.Transform(item), format, margins));
		}
		ShowBook(markovBook, onShowPage, afterShowPage);
	}
}
