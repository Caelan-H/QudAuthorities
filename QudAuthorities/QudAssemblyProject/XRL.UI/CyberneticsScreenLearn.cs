namespace XRL.UI;

public class CyberneticsScreenLearn : CyberneticsScreen
{
	public CyberneticsScreenLearn()
	{
		mainText = "Your curiosity is admirable, aristocrat.\n\nCybernetics are bionic augmentations implanted in your body to assist in your self-actualization. You can have implants installed at becoming nooks such as this one. Either load them in the rack or carry them on your person.";
		Options.Add("How many implants can I install?");
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
			base.terminal.checkSecurity(1, new CyberneticsScreenLearnHowMany());
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
