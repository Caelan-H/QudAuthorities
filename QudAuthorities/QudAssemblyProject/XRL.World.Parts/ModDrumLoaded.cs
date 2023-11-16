using System;

namespace XRL.World.Parts;

[Serializable]
public class ModDrumLoaded : IModification
{
	public ModDrumLoaded()
	{
	}

	public ModDrumLoaded(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart("MagazineAmmoLoader"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (Object.GetPart("MagazineAmmoLoader") is MagazineAmmoLoader magazineAmmoLoader)
		{
			magazineAmmoLoader.MaxAmmo = (int)Math.Round((float)magazineAmmoLoader.MaxAmmo * 1.2f);
			if (magazineAmmoLoader.MaxAmmo == 1)
			{
				magazineAmmoLoader.MaxAmmo = 2;
			}
			else if (Object.GetPart("MissileWeapon") is MissileWeapon missileWeapon)
			{
				int num = magazineAmmoLoader.MaxAmmo % missileWeapon.AmmoPerAction;
				if (num != 0)
				{
					if ((double)num < (double)missileWeapon.AmmoPerAction * 0.5)
					{
						magazineAmmoLoader.MaxAmmo -= num;
					}
					else
					{
						magazineAmmoLoader.MaxAmmo += missileWeapon.AmmoPerAction - num;
					}
				}
			}
		}
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("drum-loaded");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Drum-loaded: This weapon may hold 20% additional ammo.");
		return base.HandleEvent(E);
	}
}
