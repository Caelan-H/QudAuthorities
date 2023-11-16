using System;
using XRL.World.Parts;

namespace XRL.UI;

public class CyberneticsScreenUpgrade : CyberneticsScreen
{
	public int GetUpgradeCost()
	{
		int num = base.terminal.nLicenses - base.terminal.nFreeLicenses;
		if (num < 8)
		{
			return 1;
		}
		if (num < 16)
		{
			return 2;
		}
		if (num < 24)
		{
			return 3;
		}
		return 4;
	}

	protected override void OnUpdate()
	{
		ClearOptions();
		mainText = "BECOME A FINER ARISTOCRAT. UPGRADE YOUR LICENSE TIER WITH CYBERNETICS CREDITS.\n\n{{C|1}} CREDIT FOR LICENSE TIERS 1-8\n{{C|2}} CREDITS FOR LICENSE TIERS 9-16\n{{C|3}} CREDITS FOR LICENSE TIERS 17-24\n{{C|4}} CREDITS FOR LICENSE TIERS 25+\n";
		if (base.terminal.nFreeLicenses > 0)
		{
			mainText = mainText + "\nREMEMBER, ARISTOCRAT, YOUR BASE LICENSE TIER IS {{C|" + (base.terminal.nLicenses - base.terminal.nFreeLicenses) + "}}.";
		}
		int upgradeCost = GetUpgradeCost();
		string text = "Upgrade Your License [{{C|" + GetUpgradeCost() + "}} " + ((upgradeCost == 1) ? "credit" : "credits") + "]";
		if (base.terminal.nCredits < GetUpgradeCost())
		{
			text += " {{R|INSUFFICENT CREDITS}}";
		}
		Options.Add(text);
		Options.Add("Return To Main Menu");
	}

	public override void Back()
	{
		base.terminal.checkSecurity(1, new CyberneticsScreenMainMenu());
	}

	public override void Activate()
	{
		if (base.terminal.nSelected == 0)
		{
			if (base.terminal.nCredits < GetUpgradeCost())
			{
				base.terminal.currentScreen = new CyberneticsScreenSimpleText("{{R|INSUFFICIENT CREDITS TO UPGRADE}}", new CyberneticsScreenUpgrade());
				return;
			}
			int num = GetUpgradeCost();
			int i = 0;
			for (int count = base.terminal.Wedges.Count; i < count; i++)
			{
				if (num <= 0)
				{
					break;
				}
				CyberneticsCreditWedge cyberneticsCreditWedge = base.terminal.Wedges[i];
				if (cyberneticsCreditWedge.ParentObject == null || !cyberneticsCreditWedge.ParentObject.IsValid())
				{
					continue;
				}
				int count2 = cyberneticsCreditWedge.ParentObject.Count;
				int credits = cyberneticsCreditWedge.Credits;
				int num2 = credits * count2;
				if (num2 > num)
				{
					int num3 = 0;
					int num4 = 0;
					while (num >= credits && num3 < count2)
					{
						cyberneticsCreditWedge.ParentObject.Destroy();
						num -= credits;
						num3++;
						if (++num4 >= 10000)
						{
							throw new Exception("infinite loop in license upgrade wedge use");
						}
					}
					if (num > 0 && num3 < count2)
					{
						cyberneticsCreditWedge.ParentObject.SplitStack(1);
						cyberneticsCreditWedge.Credits -= num;
						cyberneticsCreditWedge.ParentObject.CheckStack();
						num = 0;
					}
				}
				else
				{
					num -= num2;
					cyberneticsCreditWedge.ParentObject.Obliterate();
				}
			}
			base.terminal.obj.ModIntProperty("CyberneticsLicenses", 1);
			base.terminal.currentScreen = new CyberneticsScreenSimpleText("YOU ARE BECOMING, ARISTOCRAT.", new CyberneticsScreenUpgrade(), 5);
		}
		else
		{
			base.terminal.checkSecurity(1, new CyberneticsScreenMainMenu());
		}
	}
}
