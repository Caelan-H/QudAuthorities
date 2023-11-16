namespace XRL.UI;

public class GritGateTerminalScreenSapient : GritGateTerminalScreen
{
	public GritGateTerminalScreenSapient()
	{
		mainText = "Our respective ways of reckoning that inquiry are irreconcilable. I have no answer.";
		Options.Add("What is the Thin World?");
		Options.Add("Deliver me a bit of knowledge from the Thin World.");
		Options.Add("Exit.");
	}

	public override void Back()
	{
		base.terminal.currentScreen = new GritGateTerminalScreenBasicAccess();
	}

	public override void Activate()
	{
		if (base.terminal.nSelected == 0)
		{
			base.terminal.currentScreen = new GritGateTerminalScreenThinWorld();
		}
		if (base.terminal.nSelected == 1)
		{
			base.terminal.currentScreen = new GritGateTerminalScreenKnowledge();
		}
		if (base.terminal.nSelected == 2)
		{
			base.terminal.currentScreen = null;
		}
	}
}
