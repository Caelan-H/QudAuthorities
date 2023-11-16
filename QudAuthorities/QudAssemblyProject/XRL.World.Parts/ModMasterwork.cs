using System;

namespace XRL.World.Parts;

[Serializable]
public class ModMasterwork : IModification
{
	public int Bonus = 1;

	public ModMasterwork()
	{
	}

	public ModMasterwork(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart("MeleeWeapon") && !Object.HasPart("MissileWeapon"))
		{
			return false;
		}
		if (Object.HasPart("GeomagneticDisc"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModMasterwork).Bonus != Bonus)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetCriticalThresholdEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetItemElementsEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{Y|masterwork}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.Append("\n{{rules|Masterwork").Append(": This weapon is ").Append(Bonus * 5)
			.Append("% ")
			.Append((Bonus >= 0) ? "more" : "less")
			.Append(" likely to score critical hits (standard chance is 5%).")
			.Append("}}");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCriticalThresholdEvent E)
	{
		if (E.Weapon == ParentObject || E.Projectile == ParentObject)
		{
			E.Threshold -= Bonus;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("might", 1);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
