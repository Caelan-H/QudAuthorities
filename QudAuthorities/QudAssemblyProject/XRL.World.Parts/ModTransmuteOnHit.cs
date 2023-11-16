using System;

namespace XRL.World.Parts;

[Serializable]
public class ModTransmuteOnHit : IModification
{
	public int ChancePerThousand = 1;

	public string Table = "Gemstones";

	public ModTransmuteOnHit()
	{
	}

	public ModTransmuteOnHit(int Tier)
		: base(Tier)
	{
	}

	public ModTransmuteOnHit(int ChancePerThousand, string Table)
		: this()
	{
		this.ChancePerThousand = ChancePerThousand;
		this.Table = Table;
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart("MeleeWeapon"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification()
	{
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool SameAs(IPart p)
	{
		ModTransmuteOnHit modTransmuteOnHit = p as ModTransmuteOnHit;
		if (modTransmuteOnHit.ChancePerThousand != ChancePerThousand)
		{
			return false;
		}
		if (modTransmuteOnHit.Table != Table)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!ParentObject.HasTag("Creature"))
		{
			E.Postfix.AppendRules("Small chance to transmute an enemy into a gemstone on hit.");
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponHit");
		Object.RegisterPartEvent(this, "AttackerAfterDamage");
		Object.RegisterPartEvent(this, "DealingMissileDamage");
		Object.RegisterPartEvent(this, "WeaponMissileWeaponHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "AttackerAfterDamage" || E.ID == "DealingMissileDamage" || E.ID == "WeaponMissileWeaponHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter != null && gameObjectParameter2 != null && gameObjectParameter2.IsHostileTowards(gameObjectParameter))
			{
				GameObject parentObject = ParentObject;
				GameObject subject = gameObjectParameter2;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Modification ModTransmuteOnHit Activation", ChancePerThousand, subject, null, ConstrainToPercentage: false, ConstrainToPermillage: true).in1000() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					Cell cell = gameObjectParameter2.GetCurrentCell();
					if (cell != null)
					{
						GameObject gameObject = GameObject.create(PopulationManager.RollOneFrom(Table).Blueprint);
						if (gameObject != null)
						{
							gameObjectParameter2.Splatter("&B!");
							gameObjectParameter2.Splatter("&b?");
							if (gameObjectParameter2.IsPlayer())
							{
								AchievementManager.SetAchievement("ACH_TRANSMUTED_GEM");
							}
							if (gameObjectParameter2.Die(gameObjectParameter, null, "You were transmuted into " + gameObject.an() + "."))
							{
								cell.AddObject(gameObject);
							}
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
