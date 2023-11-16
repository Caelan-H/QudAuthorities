using System.Collections.Generic;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

public class CyberneticsInstallResult : CyberneticsScreen
{
	public GameObject implant;

	public List<BodyPart> slots;

	public int nSlot;

	public int licensePointsInstalled;

	public bool installed;

	public CyberneticsInstallResult(GameObject SelectedImplant, List<BodyPart> slots, int nSlot)
	{
		this.nSlot = nSlot;
		implant = SelectedImplant;
		this.slots = slots;
		mainText += "..................................................\n";
		mainText += "................................\n";
		mainText = mainText + "Installing " + SelectedImplant.BaseDisplayName + ".............\n";
		mainText += "Interfacing with nervous system...................\n";
		mainText += "..................................................\n";
		mainText += "..................................................\n";
		mainText += "..................................................\n";
		mainText += "..................................................\n";
		mainText += "..................................................\n";
		mainText += ".....Complete!\n\n";
		mainText += "Congratulations! Your cybernetic implant was successfully installed.\n";
		mainText += "You are becoming.";
		Options.Add("RETURN TO MAIN MENU");
	}

	public override void TextComplete()
	{
		if (installed)
		{
			return;
		}
		installed = true;
		GameObject obj = slots[nSlot].Cybernetics;
		if (implant.HasTag("CyberneticsUsesEqSlot") && slots[nSlot].Equipped != null && !slots[nSlot].TryUnequip())
		{
			base.terminal.currentScreen = new CyberneticsScreenSimpleText("!ERROR: CANNOT UNEQUIP THAT LIMB", this);
			return;
		}
		if (GameObject.validate(ref obj))
		{
			if (obj.HasTag("CyberneticsDestroyOnRemoval"))
			{
				obj.Unimplant();
				obj.Destroy();
			}
			else
			{
				obj.Unimplant(MoveToInventory: true);
			}
		}
		implant.RemoveFromContext();
		implant.MakeUnderstood();
		slots[nSlot].Implant(implant);
		licensePointsInstalled = (implant?.GetPart<CyberneticsBaseItem>()?.Cost).GetValueOrDefault();
		Body parentBody = slots[nSlot].ParentBody;
		if (!parentBody.ParentObject.IsPlayer())
		{
			return;
		}
		AchievementManager.SetAchievement("ACH_INSTALL_IMPLANT");
		bool flag = true;
		foreach (string item in new List<string> { "Head", "Face", "Body", "Hands", "Feet", "Arm", "Back" })
		{
			List<BodyPart> part = parentBody.GetPart(item);
			if (part.IsNullOrEmpty())
			{
				flag = false;
				break;
			}
			foreach (BodyPart item2 in part)
			{
				if (item2.Cybernetics == null && item2.CanReceiveCyberneticImplant())
				{
					flag = false;
					goto end_IL_0226;
				}
			}
			continue;
			end_IL_0226:
			break;
		}
		if (flag)
		{
			AchievementManager.SetAchievement("ACH_INSTALL_IMPLANT_EVERY_SLOT");
		}
	}

	public override void Back()
	{
		base.terminal.checkSecurity(25, licensePointsInstalled, new CyberneticsScreenMainMenu());
	}

	public override void Activate()
	{
		Back();
	}
}
