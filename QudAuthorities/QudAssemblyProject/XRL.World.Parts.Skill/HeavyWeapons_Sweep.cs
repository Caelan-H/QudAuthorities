using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class HeavyWeapons_Sweep : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandHeavyWeaponsSweep");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandHeavyWeaponsSweep" && ParentObject.CheckFrozen())
		{
			List<GameObject> missileWeapons = ParentObject.GetMissileWeapons();
			bool flag = false;
			bool flag2 = false;
			string text = null;
			if (missileWeapons != null && missileWeapons.Count > 0)
			{
				foreach (GameObject item in missileWeapons)
				{
					if (item.GetPart("MissileWeapon") is MissileWeapon missileWeapon && missileWeapon.Skill == "HeavyWeapons")
					{
						flag2 = true;
						if (missileWeapon.ReadyToFire())
						{
							flag = true;
							break;
						}
						if (text == null)
						{
							text = missileWeapon.GetNotReadyToFireMessage();
						}
					}
				}
			}
			if (!flag2)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You do not have a heavy missile weapon equipped.");
				}
			}
			else if (!flag)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail(text ?? ("You need to reload! (" + ControlManager.getCommandInputDescription("CmdReload", Options.ModernUI) + ")"));
				}
			}
			else if (ParentObject.FireEvent(Event.New("CommandFireMissileWeapon", "Sweep", 1)))
			{
				CooldownMyActivatedAbility(ActivatedAbilityID, 250);
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Sweep", "CommandHeavyWeaponsSweep", "Skill", null, "®");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
