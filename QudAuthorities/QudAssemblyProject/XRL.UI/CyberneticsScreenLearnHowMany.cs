namespace XRL.UI;

public class CyberneticsScreenLearnHowMany : CyberneticsScreen
{
	protected override void OnUpdate()
	{
		mainText = "Insightful question, Aristocrat.\n\nEach implant has a point cost, and the total point cost of your installed implants can't exceed your license tier (displayed at the bottom of this screen). You can upgrade your license at a nook by spending cybernetic credits.";
		ClearOptions();
		Options.Add("<back>");
		Options.Add("Can I uninstall implants?");
		Options.Add("Return To Main Menu");
	}

	public override void Back()
	{
		base.terminal.checkSecurity(1, new CyberneticsScreenMainMenu());
	}

	public override void Activate()
	{
		switch (base.terminal.nSelected)
		{
		case 0:
			base.terminal.checkSecurity(1, new CyberneticsScreenLearn());
			break;
		case 1:
			base.terminal.checkSecurity(1, new CyberneticsScreenLearnUninstall());
			break;
		case 2:
			Back();
			break;
		}
	}
}
