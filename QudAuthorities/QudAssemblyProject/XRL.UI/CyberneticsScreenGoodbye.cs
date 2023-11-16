using ConsoleLib.Console;
using XRL.Language;

namespace XRL.UI;

public class CyberneticsScreenGoodbye : CyberneticsScreen
{
	public CyberneticsScreenGoodbye()
	{
		mainText = "You are no aristocrat. Goodbye.";
		Options.Add("...");
		if (XRL.UI.Options.SifrahHacking)
		{
			HackOption = Options.Count;
			Options.Add(ColorUtility.EscapeFormatting(TextFilters.Leet("attempt hack")));
		}
	}

	public override void Activate()
	{
		base.terminal.currentScreen = null;
		if (base.terminal.nSelected == HackOption && base.terminal.AttemptHack())
		{
			base.terminal.currentScreen = new CyberneticsScreenMainMenu();
		}
	}
}
