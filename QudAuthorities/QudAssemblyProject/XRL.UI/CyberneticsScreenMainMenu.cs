namespace XRL.UI;

public class CyberneticsScreenMainMenu : CyberneticsScreen
{
	public CyberneticsScreenMainMenu()
	{
		mainText = "Welcome, Aristocrat, to a becoming nook. You are one step closer to the Grand Unification. Please choose from the following options.";
		Options.Add("Learn About Cybernetics");
		Options.Add("Install Cybernetics");
		Options.Add("Uninstall Cybernetics");
		Options.Add("Upgrade Your License");
	}

	public override void Activate()
	{
		switch (base.terminal.nSelected)
		{
		case 0:
			base.terminal.currentScreen = new CyberneticsScreenLearn();
			break;
		case 1:
			base.terminal.currentScreen = new CyberneticsScreenInstall();
			break;
		case 2:
			base.terminal.currentScreen = new CyberneticsScreenRemove();
			break;
		case 3:
			base.terminal.currentScreen = new CyberneticsScreenUpgrade();
			break;
		}
	}
}
