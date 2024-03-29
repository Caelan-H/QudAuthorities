using System;

namespace XRL.World.Parts;

[Serializable]
public class ModPsionic : IModification
{
	public int SwingsRemaining = -1;

	public string StartingSwings = "126-500";

	public int SwingThreshold1 = 125;

	public int SwingThreshold2 = 250;

	public int SwingThreshold3 = 375;

	public ModPsionic()
	{
	}

	public ModPsionic(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart("MeleeWeapon");
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<MeleeWeapon>().Stat = "Ego";
		Object.RemovePart("TinkerItem");
		IncreaseComplexityIfComplex(1);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanBeModdedEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetMaximumLiquidExposureEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ModificationAppliedEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 100;
		return false;
	}

	public override bool HandleEvent(CanBeModdedEvent E)
	{
		return false;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E?.Object != null)
		{
			if (!E.Understood() || !E.Object.HasProperName)
			{
				E.AddAdjective("{{psionic|psionic}}");
			}
			E.AddTag("(" + GetPsionicModSwingsRemainingDescription() + ")", -40);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModificationAppliedEvent E)
	{
		if (E.Modification == this)
		{
			SwingsRemaining = StartingSwings.RollCached();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		SwingsRemaining = StartingSwings.RollCached();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponAfterAttack");
		Object.RegisterPartEvent(this, "WeaponAfterAttackMissed");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponAfterAttack" || E.ID == "WeaponAfterAttackMissed")
		{
			SwingsRemaining--;
			if (SwingsRemaining <= 0)
			{
				GameObject equipped = ParentObject.Equipped;
				DidX("disappear");
				ParentObject.ForceUnequipRemoveAndRemoveContents(Silent: true);
				ParentObject.Destroy();
				if (equipped != null && equipped.IsValid() && !equipped.IsPlayer() && equipped.pBrain != null)
				{
					equipped.pBrain.PerformReequip();
				}
			}
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Psionic: This weapon uses the wielder's Ego modifier for penetration bonus instead of Strength mod and attacks MA instead of AV. It will dissipate from the corporeal realm after some use.";
	}

	public string GetPsionicModSwingsRemainingDescription()
	{
		if (SwingsRemaining > SwingThreshold3)
		{
			return "substantial";
		}
		if (SwingsRemaining > SwingThreshold2)
		{
			return "lambent";
		}
		if (SwingsRemaining > SwingThreshold1)
		{
			return "tenuous";
		}
		return "insubstantial";
	}
}
