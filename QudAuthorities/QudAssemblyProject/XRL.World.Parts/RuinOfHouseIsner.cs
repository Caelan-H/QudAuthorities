using System;

namespace XRL.World.Parts;

[Serializable]
public class RuinOfHouseIsner : IPart
{
	public int EgoBonus;

	public int ShotCount;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "MagazineAmmoLoaderReloaded");
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		Object.RegisterPartEvent(this, "WeaponMissileWeaponHit");
		Object.RegisterPartEvent(this, "WeaponMissileWeaponShot");
		Object.RegisterPartEvent(this, "WeaponMissleWeaponFiring");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponMissileWeaponShot")
		{
			if (ShotCount == 7)
			{
				E.SetParameter("AimVariance", 0);
				E.SetParameter("FlatVariance", 0);
				E.SetParameter("WeaponAccuracy", 0);
			}
		}
		else if (E.ID == "MagazineAmmoLoaderReloaded")
		{
			ShotCount = 0;
		}
		else if (E.ID == "WeaponMissleWeaponFiring")
		{
			ShotCount++;
		}
		else if (E.ID == "WeaponMissileWeaponHit")
		{
			if (ShotCount == 7 && E.GetIntParameter("Critical") == 1)
			{
				E.SetParameter("Critical", 1);
			}
			if (E.GetIntParameter("Critical") == 1)
			{
				E.SetParameter("Penetrations", E.GetIntParameter("Penetrations") + 4);
				E.SetParameter("PenetrationCap", E.GetIntParameter("PenetrationCap") + 4);
			}
		}
		else if (E.ID == "Equipped")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("EquippingObject");
			gameObjectParameter.RegisterPartEvent(this, "BeginTakeAction");
			if (EgoBonus == 0 && gameObjectParameter.HasStat("Ego"))
			{
				gameObjectParameter.GetStat("Ego").Bonus++;
				EgoBonus = 1;
			}
		}
		else if (E.ID == "Unequipped")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("UnequippingObject");
			gameObjectParameter2.UnregisterPartEvent(this, "BeginTakeAction");
			if (EgoBonus > 0 && gameObjectParameter2.HasStat("Ego"))
			{
				gameObjectParameter2.GetStat("Ego").Bonus--;
				EgoBonus = 0;
			}
		}
		return base.FireEvent(E);
	}
}
