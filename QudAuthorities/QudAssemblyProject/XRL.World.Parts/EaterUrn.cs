using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using XRL.Names;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class EaterUrn : IPart
{
	public string Inscription = "";

	public string Prefix = "";

	public string Postfix = "";

	public bool NeedsGeneration = true;

	public string faction = "Eater";

	public string name;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				Look.ShowLooker(0, cell.X, cell.Y);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (NeedsGeneration)
		{
			GenerateUrn();
		}
		E.Prefix.Append(Prefix);
		E.Base.Append(Inscription);
		E.Postfix.Append(Postfix);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void GenerateUrn()
	{
		if (name == null)
		{
			if (faction == "Eater")
			{
				name = NameMaker.Eater();
			}
			else if (faction == "YdFreehold")
			{
				name = NameMaker.YdFreeholder();
			}
			else
			{
				name = MutantNameMaker.MakeMutantName();
			}
		}
		string input = ((faction == "Eater") ? HistoricStringExpander.ExpandString("<spice.tombstones.eaterUrnIntro.!random>") : ((!(faction == "YdFreehold")) ? HistoricStringExpander.ExpandString("<spice.tombstones.genericUrnIntro.!random>") : HistoricStringExpander.ExpandString("<spice.tombstones.ydFreeholderUrnIntro.!random>")));
		string text = "LibraryCorpus.json";
		MarkovBook.EnsureCorpusLoaded(text);
		MarkovChainData data = MarkovBook.CorpusData[text];
		string text2 = "\"" + MarkovChain.GenerateShortSentence(data, null, 12).TrimEnd(' ') + "\"";
		text2 = Markup.Wrap(text2);
		List<string> list = new List<string>(16);
		int maxWidth = 25;
		list.Add("");
		string[] array = StringFormat.ClipText(input, maxWidth).Split('\n');
		foreach (string item in array)
		{
			list.Add(item);
		}
		list.Add("");
		list.Add("");
		array = StringFormat.ClipText(name, maxWidth).Split('\n');
		foreach (string item2 in array)
		{
			list.Add(item2);
		}
		list.Add("");
		list.Add("");
		array = StringFormat.ClipText(text2, maxWidth).Split('\n');
		foreach (string item3 in array)
		{
			list.Add(item3);
		}
		list.Add("");
		for (int j = 0; j < list.Count; j++)
		{
			Inscription += "\nÿÿÿ";
			if (j % 2 == 0)
			{
				Inscription += " ";
			}
			else
			{
				Inscription += " ";
			}
			Inscription += list[j].PadLeft(17 + (list[j].Length / 2 - 1), 'ÿ').PadRight(31, 'ÿ');
			if (j % 2 == 0)
			{
				Inscription += " ";
			}
			else
			{
				Inscription += " ";
			}
		}
		NeedsGeneration = false;
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('/');
		stringBuilder.Append('-', 27);
		stringBuilder.Append('\\');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append("\n");
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append(" &K");
		stringBuilder.Append('_', 31);
		stringBuilder.Append(" &y");
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append("\n");
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append(" &K");
		stringBuilder.Append('_', 31);
		stringBuilder.Append(" &y");
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append("\n");
		Prefix = stringBuilder.ToString();
		StringBuilder stringBuilder2 = new StringBuilder();
		for (int k = 0; k < 2; k++)
		{
			stringBuilder2.Append("\n");
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append(" &K");
			stringBuilder2.Append('_', 21);
			stringBuilder2.Append(" &y");
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
		}
		Inscription += "\n";
		Inscription += stringBuilder2.ToString();
	}
}
