namespace XRL.UI;

public class GritGateTerminalScreenBasicAccess : GritGateTerminalScreen
{
	public GritGateTerminalScreenBasicAccess()
	{
		mainText = "Basic user access grant. What do you wish from the Thin World?";
		Options.Add("Who are you?");
		Options.Add("What is the Thin World?");
		Options.Add("Deliver me a bit of knowledge from the Thin World.");
		Options.Add("Exit.");
	}

	public override void Back()
	{
		base.terminal.currentScreen = null;
	}

	public override void Activate()
	{
		if (base.terminal.nSelected == 0)
		{
			base.terminal.currentScreen = new GritGateTerminalScreenWhoAreYou();
		}
		if (base.terminal.nSelected == 1)
		{
			base.terminal.currentScreen = new GritGateTerminalScreenThinWorld();
		}
		if (base.terminal.nSelected == 2)
		{
			base.terminal.currentScreen = new GritGateTerminalScreenKnowledge();
		}
		if (base.terminal.nSelected == 3)
		{
			base.terminal.currentScreen = null;
		}
	}
}
