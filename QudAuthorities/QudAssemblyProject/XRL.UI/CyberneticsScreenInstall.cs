using System.Collections.Generic;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

public class CyberneticsScreenInstall : CyberneticsScreen
{
	private List<GameObject> implants = new List<GameObject>();

	protected override void OnUpdate()
	{
		mainText = "You are becoming, ARISTOCRAT. CHOOSE AN IMPLANT TO INSTALL.";
		implants.Clear();
		ClearOptions();
		if (base.terminal.Implants.Count == 0)
		{
			mainText += "\n\n{{R|<NO IMPLANTS AVAILABLE>}}";
		}
		foreach (GameObject implant in base.terminal.Implants)
		{
			if (!implants.Contains(implant))
			{
				implants.Add(implant);
				string text = implant.BaseDisplayName;
				if (implant.GetPart<CyberneticsBaseItem>().Cost > base.nLicensesRemaining)
				{
					text += " {{R|[NOT ENOUGH POINTS]}}";
				}
				Options.Add(text);
			}
		}
		Options.Add("RETURN TO MAIN MENU");
	}

	public override void Back()
	{
		base.terminal.currentScreen = new CyberneticsScreenMainMenu();
	}

	public override void Activate()
	{
		if (base.terminal.nSelected < implants.Count)
		{
			GameObject gameObject = implants[base.terminal.nSelected];
			if (gameObject.GetPart<CyberneticsBaseItem>().Cost > base.nLicensesRemaining)
			{
				base.terminal.currentScreen = new CyberneticsScreenSimpleText("INSUFFICENT LICENSE POINTS TO INSTALL:\n  -" + gameObject.BaseDisplayName + "\n\nPLEASE UNINSTALL AN IMPLANT OR UPGRADE YOUR LICENSE.", new CyberneticsScreenInstall(), 1);
			}
			else if (gameObject.IsBroken() || gameObject.IsRusted() || gameObject.IsEMPed() || gameObject.IsTemporary)
			{
				base.terminal.currentScreen = new CyberneticsScreenSimpleText("ERROR: CONDITION INADEQUATE FOR INSTALLATION\n  -" + gameObject.BaseDisplayName + "\n\nPLEASE SUPPLY A REPLACEMENT.", new CyberneticsScreenInstall(), 1);
			}
			else
			{
				base.terminal.currentScreen = new CyberneticsScreenInstallLocation(gameObject);
			}
		}
		else
		{
			base.terminal.checkSecurity(1, new CyberneticsScreenMainMenu());
		}
	}
}
