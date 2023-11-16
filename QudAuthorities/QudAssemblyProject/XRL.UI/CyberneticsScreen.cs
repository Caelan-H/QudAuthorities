using ConsoleLib.Console;
using XRL.Language;

namespace XRL.UI;

public class CyberneticsScreen : TerminalScreen
{
	public override void BeforeRender(ScreenBuffer buffer)
	{
		int num = 3;
		buffer.Goto(num + 3, 24);
		buffer.Write(" Credits: " + base.terminal.nCredits + " ");
		buffer.Goto(num + 38, 24);
		buffer.Write(" License Tier: " + base.terminal.nLicenses + " ");
		buffer.Write(" Points Used: " + base.terminal.nLicensesUsed + " ");
		bool hackActive = base.terminal.HackActive;
		int hackLevel = base.terminal.HackLevel;
		if (hackActive || hackLevel > 0)
		{
			string text = (hackActive ? TextFilters.Leet("HACK LEVEL") : "Hack Level");
			buffer.Goto(num + 3, 2);
			buffer.Write(" {{G|" + text + ": " + hackLevel + "}} ");
		}
		int securityAlertLevel = base.terminal.SecurityAlertLevel;
		if (hackActive || securityAlertLevel > 0)
		{
			string text2 = ((securityAlertLevel <= 0) ? "y" : ((securityAlertLevel >= hackLevel - 1) ? "R" : ((securityAlertLevel > hackLevel - 3) ? "r" : "W")));
			string text3 = (hackActive ? TextFilters.Leet("SECURITY ALERT LEVEL") : "Security Alert Level");
			buffer.Goto(72 - num - ColorUtility.LengthExceptFormatting(text3), 2);
			buffer.Write(" {{" + text2 + "|" + text3 + ": " + securityAlertLevel + "}} ");
		}
	}
}
