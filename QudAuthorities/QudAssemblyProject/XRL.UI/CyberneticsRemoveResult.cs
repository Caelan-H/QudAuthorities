using XRL.World;

namespace XRL.UI;

public class CyberneticsRemoveResult : CyberneticsScreen
{
	public CyberneticsRemoveResult(GameObject SelectedImplant)
	{
		mainText += "..................................................\n";
		mainText += "................................\n";
		mainText = mainText + "Uninstalling " + SelectedImplant.BaseKnownDisplayName + "...........\n";
		mainText += "Interfacing with nervous system...................\n";
		mainText += "..................................................\n";
		mainText += "..................................................\n";
		mainText += "..................................................\n";
		mainText += "..................................................\n";
		mainText += "..................................................\n";
		mainText += ".....Complete!\n\n";
		mainText += "Congratulations! ";
		mainText += "Your cybernetic implant was successfully uninstalled.";
		Options.Add("RETURN TO MAIN MENU");
	}

	public override void Back()
	{
		base.terminal.checkSecurity(25, new CyberneticsScreenMainMenu());
	}

	public override void Activate()
	{
		Back();
	}
}
