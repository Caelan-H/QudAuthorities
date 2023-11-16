using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;

namespace XRL.UI;

public class TerminalScreen
{
	public string renderedText;

	public string mainText;

	public List<string> Options = new List<string>();

	public List<int> OptionLines = new List<int>();

	public int HackOption = -1;

	public string hackText;

	public List<string> hackOptions;

	public int nLicensesRemaining => terminal.nLicenses - terminal.nLicensesUsed;

	public CyberneticsTerminal terminal => CyberneticsTerminal.instance;

	public virtual void Back()
	{
		terminal.currentScreen = null;
	}

	public virtual void TextComplete()
	{
	}

	public virtual void Activate()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void ClearOptions()
	{
		Options.Clear();
		HackOption = -1;
	}

	public void Update()
	{
		OnUpdate();
		if (terminal.HackActive)
		{
			if (mainText != null && mainText != hackText)
			{
				mainText = TextFilters.Leet(mainText);
				hackText = mainText;
			}
			bool flag = false;
			for (int i = 0; i < Options.Count; i++)
			{
				if (i != HackOption && Options[i] != null && (hackOptions == null || i >= hackOptions.Count || hackOptions[i] != Options[i]))
				{
					Options[i] = TextFilters.Leet(Options[i]);
					flag = true;
				}
			}
			if (flag)
			{
				hackOptions = new List<string>(Options);
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(StringFormat.ClipText("{{|" + mainText + "}}", 67, KeepNewlines: true)).Append("\n\n");
		int num = stringBuilder.ToString().Split('\n').Length;
		StringBuilder stringBuilder2 = new StringBuilder();
		int num2 = 65;
		for (int j = 0; j < Options.Count; j++)
		{
			if (terminal.nSelected == j)
			{
				stringBuilder.Append('>');
			}
			else
			{
				stringBuilder.Append(' ');
			}
			stringBuilder2.Clear();
			if (j == HackOption)
			{
				stringBuilder2.Append("{{R|CTRL-ENTER. ").Append(StringFormat.ClipText(Options[j], 60, KeepNewlines: true)).Append("}}");
				Markup.Transform(stringBuilder2);
			}
			else
			{
				stringBuilder2.Append((char)num2++).Append(". ").Append(StringFormat.ClipText("{{|" + Options[j] + "}}", 60, KeepNewlines: true));
			}
			stringBuilder2.Append('\n');
			stringBuilder.Append(stringBuilder2);
			OptionLines.Add(num);
			num += stringBuilder2.ToString().Count((char ch) => ch == '\n');
		}
		renderedText = stringBuilder.ToString();
	}

	public virtual void BeforeRender(ScreenBuffer buffer)
	{
	}
}
