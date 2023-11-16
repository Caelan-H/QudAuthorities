using System.Collections.Generic;
using XRL.World;

namespace XRL.UI;

public class CyberneticsScreenRemove : CyberneticsScreen
{
	private List<GameObject> cybernetic = new List<GameObject>();

	private List<BodyPart> bodypart = new List<BodyPart>();

	protected override void OnUpdate()
	{
		ClearOptions();
		cybernetic.Clear();
		bodypart.Clear();
		foreach (BodyPart part in base.terminal.obj.Body.GetBody().GetParts())
		{
			if (part.Cybernetics != null && !cybernetic.Contains(part.Cybernetics))
			{
				string text = "";
				if (part.Cybernetics.HasTag("CyberneticsNoRemove"))
				{
					text += " {{R|[CANNOT BE UNINSTALLED]}}";
				}
				if (part.Cybernetics.HasTag("CyberneticsDestroyOnRemoval"))
				{
					text += " {{R|[DESTROYED ON UNINSTALL]}}";
				}
				Options.Add(part.Cybernetics.BaseKnownDisplayName + " [" + part.Name.Replace("Worn on ", "") + "]" + text);
				bodypart.Add(part);
				cybernetic.Add(part.Cybernetics);
			}
		}
		mainText = "You are given to whimsy, Aristocrat. Choose an implant to uninstall.";
		if (Options.Count == 0)
		{
			mainText += "\n\n{{R|<NO IMPLANTS INSTALLED>}}";
		}
		Options.Add("Return To Main Menu");
	}

	public override void Back()
	{
		base.terminal.currentScreen = new CyberneticsScreenMainMenu();
	}

	public override void Activate()
	{
		if (base.terminal.nSelected < cybernetic.Count)
		{
			if (cybernetic[base.terminal.nSelected].HasTag("CyberneticsNoRemove"))
			{
				base.terminal.currentScreen = new CyberneticsScreenSimpleText("WHIMSY MUST YIELD TO NECESSITY, ARISTOCRAT.", new CyberneticsScreenRemove(), 1);
			}
			else if (cybernetic[base.terminal.nSelected].HasTag("CyberneticsDestroyOnRemoval"))
			{
				cybernetic[base.terminal.nSelected].Unimplant();
				cybernetic[base.terminal.nSelected].Destroy();
				base.terminal.currentScreen = new CyberneticsRemoveResult(cybernetic[base.terminal.nSelected]);
			}
			else
			{
				cybernetic[base.terminal.nSelected].Unimplant(MoveToInventory: true);
				base.terminal.currentScreen = new CyberneticsRemoveResult(cybernetic[base.terminal.nSelected]);
			}
		}
		else
		{
			base.terminal.checkSecurity(1, new CyberneticsScreenMainMenu());
		}
	}
}
