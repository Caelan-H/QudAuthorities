using System.Collections.Generic;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

public class CyberneticsScreenInstallLocation : CyberneticsScreen
{
	private GameObject implant;

	private List<BodyPart> slots = new List<BodyPart>();

	public CyberneticsScreenInstallLocation(GameObject implant)
	{
		this.implant = implant;
	}

	protected override void OnUpdate()
	{
		slots.Clear();
		ClearOptions();
		mainText = "PLEASE CHOOSE A TARGET BODY PART.";
		CyberneticsBaseItem part = implant.GetPart<CyberneticsBaseItem>();
		Body body = base.terminal.obj.Body;
		List<string> list = part.Slots.CachedCommaExpansion();
		int num = 1;
		foreach (BodyPart part2 in body.GetParts())
		{
			if (!list.Contains(part2.Type) || !part2.CanReceiveCyberneticImplant())
			{
				continue;
			}
			if (part2.Cybernetics != null)
			{
				if (!part2.Cybernetics.HasTag("CyberneticsNoRemove"))
				{
					string text = "";
					if (part2.Cybernetics.HasTag("CyberneticsDestroyOnRemoval"))
					{
						text = "{{R|[DESTROYED ON UNINSTALL]}}";
					}
					slots.Add(part2);
					Options.Add(part2.Name + " [WILL REPLACE " + part2.Cybernetics.BaseKnownDisplayName + "]" + text);
				}
			}
			else
			{
				slots.Add(part2);
				Options.Add(part2.Name);
			}
			num++;
		}
		Options.Add("<cancel operation, return to main menu>");
	}

	public override void Back()
	{
		base.terminal.checkSecurity(1, new CyberneticsScreenMainMenu());
	}

	public override void Activate()
	{
		if (base.terminal.nSelected < slots.Count)
		{
			base.terminal.currentScreen = new CyberneticsInstallResult(implant, slots, base.terminal.nSelected);
		}
		else
		{
			Back();
		}
	}
}
