namespace XRL.UI;

public class GritGateTerminalScreenThinWorld : GritGateTerminalScreen
{
	public GritGateTerminalScreenThinWorld()
	{
		mainText = "The Thin World is where I traverse, as the Thick World is where you do. I've no domain in your world, and but for me, you've none in mine.";
		Options.Add("Who are you?");
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
			base.terminal.currentScreen = new GritGateTerminalScreenWhoAreYou();
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
