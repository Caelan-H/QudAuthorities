namespace XRL.UI;

public class GritGateTerminalScreenGoodbye : GritGateTerminalScreen
{
	public GritGateTerminalScreenGoodbye()
	{
		mainText = "Access denied. Continue on your path through the Thick World.";
		Options.Add("...");
	}

	public override void Back()
	{
		base.terminal.currentScreen = null;
	}

	public override void Activate()
	{
		base.terminal.currentScreen = null;
	}
}
