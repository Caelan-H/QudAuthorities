using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Rifle_SuppressiveFire : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandSuppressiveFire");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandSuppressiveFire")
		{
			if (!ParentObject.CheckFrozen())
			{
				return true;
			}
			List<GameObject> missileWeapons = ParentObject.GetMissileWeapons();
			bool flag = false;
			bool flag2 = false;
			string text = null;
			if (missileWeapons != null && missileWeapons.Count > 0)
			{
				foreach (GameObject item in missileWeapons)
				{
					if (item.GetPart("MissileWeapon") is MissileWeapon missileWeapon && (missileWeapon.Skill == "Rifle" || missileWeapon.Skill == "Bow"))
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
					Popup.Show("You do not have a bow or rifle equipped!");
				}
			}
			else if (!flag)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show(text ?? ("You need to reload! (" + ControlManager.getCommandInputDescription("CmdReload", Options.ModernUI) + ")"));
				}
			}
			else
			{
				ParentObject.FireEvent(Event.New("CommandFireMissileWeapon", "FireType", FireType.SuppressingFire));
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		return base.RemoveSkill(GO);
	}
}
