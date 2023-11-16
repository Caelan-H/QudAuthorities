namespace XRL.UI;

public class GritGateTerminalScreenWhoAreYou : GritGateTerminalScreen
{
	public GritGateTerminalScreenWhoAreYou()
	{
		mainText = "I am Ereshkigal, your liaison with the Thin World. Ask of me.";
		Options.Add("Are you sapient?");
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
			base.terminal.currentScreen = new GritGateTerminalScreenSapient();
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
