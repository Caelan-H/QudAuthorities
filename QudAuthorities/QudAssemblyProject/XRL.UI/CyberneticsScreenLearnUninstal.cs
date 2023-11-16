namespace XRL.UI;

public class CyberneticsScreenLearnUninstall : CyberneticsScreen
{
	protected override void OnUpdate()
	{
		mainText = "Aristocrat, your question was most finely uttered.\n\nYou may freely install and uninstall implants at a nook, though some implants are destroyed when uninstalled or can't be uninstalled at all.";
		ClearOptions();
		Options.Add("<back>");
		Options.Add("How many implants can I install?");
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
			base.terminal.checkSecurity(1, new CyberneticsScreenLearnHowMany());
			break;
		case 2:
			Back();
			break;
		}
	}
}
