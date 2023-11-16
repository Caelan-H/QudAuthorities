using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Genkit;
using Qud.API;
using UnityEngine;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
[HasGameBasedStaticCache]
public class MarkovBook : IPart
{
	public int BookSeed;

	public string BookCorpus;

	public string Title;

	[NonSerialized]
	public List<BookPage> Pages;

	[NonSerialized]
	[GameBasedStaticCache]
	public static Dictionary<string, MarkovChainData> CorpusData = new Dictionary<string, MarkovChainData>();

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != HasBeenReadEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(HasBeenReadEvent E)
	{
		if (E.Actor == The.Player && GetHasBeenRead())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Read", "read", "Read", null, 'r', FireOnActor: false, (!GetHasBeenRead()) ? 100 : 0);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Read" && E.Actor.IsPlayer())
		{
			if (Pages == null)
			{
				GenerateFormattedPage();
			}
			BookUI.ShowBook(this);
			if (!GetHasBeenRead())
			{
				SetHasBeenRead(flag: true);
				JournalAPI.AddAccomplishment("You read " + Title + ".", "In the month of " + Calendar.getMonth() + " of " + Calendar.getYear() + ", =name= penned the influential book, " + ParentObject.a + ParentObject.pRender.DisplayName + ".", "general", JournalAccomplishment.MuralCategory.CreatesSomething, JournalAccomplishment.MuralWeight.VeryLow, null, -1L);
			}
		}
		return base.HandleEvent(E);
	}

	public string GetBookKey()
	{
		return "AlreadyRead_" + BookSeed;
	}

	public bool GetHasBeenRead()
	{
		return The.Game.GetStringGameState(GetBookKey()) == "Yes";
	}

	public void SetHasBeenRead(bool flag)
	{
		if (flag)
		{
			The.Game.SetStringGameState(GetBookKey(), "Yes");
		}
		else
		{
			The.Game.SetStringGameState(GetBookKey(), "");
		}
	}

	public static void EnsureCorpusLoaded(string Corpus)
	{
		try
		{
			if (!CorpusData.ContainsKey(Corpus))
			{
				CorpusData.Add(Corpus, MarkovChainData.LoadFromFile(DataManager.FilePath(Corpus)));
			}
			if (The.Game != null && The.Game.HasIntGameState("RuinofHouseIsner_xCoordinate") && !The.Game.HasIntGameState("AddedMarkovSecrets"))
			{
				Stat.ReseedFrom("HouseIsnerLore");
				for (int i = 0; i < 23; i++)
				{
					string secret = LoreGenerator.RuinOfHouseIsnerLore(The.Game.GetIntGameState("RuinofHouseIsner_xCoordinate"), The.Game.GetIntGameState("RuinofHouseIsner_yCoordinate"));
					CorpusData[Corpus] = MarkovChain.AppendSecret(CorpusData[Corpus], secret);
				}
				The.Game.SetIntGameState("AddedMarkovSecrets", 1);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("EnsureCorpusLoaded:" + Corpus, x);
		}
	}

	public void SetContents(int Seed, string Corpus)
	{
		BookSeed = Seed;
		BookCorpus = Corpus;
		EnsureCorpusLoaded(Corpus);
		Stat.ReseedFrom(BookSeed + Hash.String("Title"));
		Title = MarkovChain.GenerateTitle(CorpusData[Corpus]);
		if (20.in100())
		{
			if (50.in100())
			{
				string[] list = new string[10] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
				Title = Title + ", Vol. " + list.GetRandomElement();
			}
			else if (30.in100())
			{
				Title += ": Unabridged";
			}
			else
			{
				int num = ((Stat.Random(1, 10) == 1) ? Stat.Random(1, 100) : Stat.Random(1, 5));
				switch ((num != 11 && num != 12) ? (num % 10) : 0)
				{
				case 1:
					Title = Title + ", " + num + "st Edition";
					break;
				case 2:
					Title = Title + ", " + num + "nd Edition";
					break;
				case 3:
					Title = Title + ", " + num + "rd Edition";
					break;
				default:
					Title = Title + ", " + num + "th Edition";
					break;
				}
			}
		}
		ParentObject.pRender.DisplayName = Title;
	}

	public void GeneratePages()
	{
		Pages = new List<BookPage>();
		EnsureCorpusLoaded(BookCorpus);
		Stat.ReseedFrom(BookSeed + Hash.String("Pages"));
		StringBuilder stringBuilder = new StringBuilder();
		if (10.in100())
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			string data = stringBuilder.ToString();
			BookPage bookPage = new BookPage(Title, data);
			bookPage.TopMargin = 2;
			bookPage.LeftMargin = 2;
			bookPage.RightMargin = 2;
			Pages.Add(bookPage);
		}
		string data2 = "\n";
		BookPage bookPage2 = new BookPage(Title, data2);
		bookPage2.TopMargin = 2;
		bookPage2.LeftMargin = 2;
		bookPage2.RightMargin = 2;
		Pages.Add(bookPage2);
		int num = Stat.Random(5, 7);
		for (int i = 0; i < num; i++)
		{
			string text;
			do
			{
				text = MarkovChain.GenerateParagraph(CorpusData[BookCorpus]).Replace("\n", " ").Replace("  ", " ")
					.Trim();
			}
			while (text.Contains("="));
			BookPage bookPage3 = new BookPage(Title, text);
			bookPage3.TopMargin = 2;
			bookPage3.LeftMargin = 2;
			bookPage3.RightMargin = 2;
			Pages.Add(bookPage3);
		}
	}

	public void GenerateFormattedPage()
	{
		Pages = new List<BookPage>();
		EnsureCorpusLoaded(BookCorpus);
		Stat.ReseedFrom(BookSeed + Hash.String("Pages"));
		string text = "";
		if (10.in100())
		{
			text = ((!50.in100()) ? (text + "{{C|Author's note:}} " + MarkovChain.GenerateSentence(CorpusData[BookCorpus]) + "\n\n") : (text + "{{C|Editor's note:}} " + MarkovChain.GenerateSentence(CorpusData[BookCorpus]) + "\n\n"));
		}
		for (int i = 0; i < Stat.Random(8, 30); i++)
		{
			text += MarkovChain.GenerateParagraph(CorpusData[BookCorpus]);
		}
		if (Stat.Random(0, 100) < 5)
		{
			for (int j = 0; j < Stat.Random(150, 450); j++)
			{
				text += MarkovChain.GenerateParagraph(CorpusData[BookCorpus]);
			}
		}
		string format = "Auto";
		string margins = "1,2,2,2";
		Pages.AddRange(BookUI.AutoformatPages(Title, text, format, margins));
		int count = Regex.Matches(text, "parasangs").Count;
		string[] array = text.Split(' ');
		Debug.LogWarning(count + " / " + array.Length);
	}
}
