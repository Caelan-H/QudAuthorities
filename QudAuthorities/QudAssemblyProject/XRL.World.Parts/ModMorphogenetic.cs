using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ModMorphogenetic : IModification
{
	public const int DEFAULT_SAVE_BASE_DIFFICULTY = 10;

	public const float DEFAULT_SAVE_DAMAGE_DIFFICULTY_FACTOR = 0.2f;

	public const string DEFAULT_SAVE_ATTRIBUTE = "Willpower";

	public const string DEFAULT_SAVE_VS = "MorphicShock Daze";

	public const string DEFAULT_DAZE_DURATION = "2-3";

	public int SaveBaseDifficulty = 10;

	public float SaveDamageDifficultyFactor = 0.2f;

	public string SaveAttribute = "Willpower";

	public string SaveVs = "MorphicShock Daze";

	public string DazeDuration = "2-3";

	[NonSerialized]
	private Event eCanApplyMorphicShock = new Event("CanApplyMorphicShock");

	[NonSerialized]
	private Event eApplyMorphicShock = new Event("ApplyMorphicShock");

	public ModMorphogenetic()
	{
	}

	public ModMorphogenetic(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		ChargeUse = 200;
		WorksOnSelf = true;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		IsPowerLoadSensitive = true;
		base.IsTechScannable = true;
		NameForStatus = "MorphogeneticResonator";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart("MeleeWeapon") && !Object.HasPart("MissileWeapon") && !Object.HasPart("ThrownWeapon"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<EnergyCellSocket>();
		IncreaseDifficultyAndComplexity(2, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetItemElementsEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{m|morphogenetic}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription(), base.AddStatusSummary);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("circuitry", 2);
		E.Add("might", 1);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "LauncherProjectileHit");
		Object.RegisterPartEvent(this, "WeaponDealDamage");
		Object.RegisterPartEvent(this, "WeaponPseudoThrowHit");
		Object.RegisterPartEvent(this, "WeaponThrowHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "WeaponDealDamage" || E.ID == "LauncherProjectileHit" || E.ID == "WeaponThrowHit" || E.ID == "WeaponPseudoThrowHit") && E.GetParameter("Damage") is Damage damage && damage.Amount > 0)
		{
			GameObject defender = E.GetGameObjectParameter("Defender");
			if (defender != null && defender.pBrain != null)
			{
				string species = defender.GetSpecies();
				Cell cell = defender.CurrentCell;
				if (!string.IsNullOrEmpty(species) && cell != null && cell.ParentZone != null && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					GameObject attacker = E.GetGameObjectParameter("Attacker");
					Event @event = Event.New("InitiateRealityDistortionLocal");
					@event.SetParameter("Object", attacker);
					@event.SetParameter("Device", ParentObject);
					@event.SetParameter("Operator", attacker);
					@event.SetParameter("Cell", cell);
					if (defender.FireEvent(@event, E))
					{
						List<GameObject> list = Event.NewGameObjectList();
						cell.ParentZone.FindObjects(list, (GameObject obj) => obj != defender && MorphicShockMatch(obj, species));
						if (list.Count > 0)
						{
							int num = MyPowerLoadLevel();
							if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
							{
								if (list.Count > 1)
								{
									list.Sort((GameObject a, GameObject b) => a.DistanceTo(attacker).CompareTo(b.DistanceTo(attacker)));
								}
								int i = 0;
								for (int count = list.Count; i < count; i++)
								{
									GameObject gameObject = list[i];
									if (gameObject != null && gameObject.IsValid())
									{
										ApplyMorphicShock(gameObject, damage.Amount, attacker, num);
									}
								}
							}
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Morphogenetic: When powered and used to perform a successful, damaging hit, this weapon attempts to daze all other creatures of the same species as your target on the local map. Compute power on the local lattice increases the strength of this effect.";
	}

	public string GetInstanceDescription()
	{
		return GetDescription(Tier);
	}

	public bool ApplyMorphicShock(GameObject who, int damage, GameObject owner, int PowerLoad = 100)
	{
		if (who.pBrain == null)
		{
			return false;
		}
		if (!who.LocalEvent("CheckRealityDistortionAccessibility"))
		{
			return false;
		}
		int num = SaveBaseDifficulty + IComponent<GameObject>.PowerLoadBonus(PowerLoad) + (int)((float)damage * SaveDamageDifficultyFactor) + GetAvailableComputePowerEvent.GetFor(who) / 3;
		if (who.HasPart("Analgesia"))
		{
			num -= 5;
		}
		if (num <= 0)
		{
			return false;
		}
		eCanApplyMorphicShock.SetParameter("Attacker", owner);
		eCanApplyMorphicShock.SetParameter("Defender", who);
		eCanApplyMorphicShock.SetParameter("Difficulty", num);
		if (!who.FireEvent(eCanApplyMorphicShock))
		{
			return false;
		}
		if (who.MakeSave(SaveAttribute, num, null, null, SaveVs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
		{
			return false;
		}
		eApplyMorphicShock.SetParameter("Attacker", owner);
		eApplyMorphicShock.SetParameter("Defender", who);
		eApplyMorphicShock.SetParameter("Difficulty", num);
		if (!who.FireEvent(eApplyMorphicShock))
		{
			return false;
		}
		if (who.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("A weird" + (who.HasPart("Analgesia") ? "" : ", painful") + " shock reverberates through you.");
		}
		if (!who.ApplyEffect(new Dazed(DazeDuration.RollCached())))
		{
			return false;
		}
		who.TelekinesisBlip();
		return true;
	}

	public static bool MorphicShockMatch(GameObject who, string species)
	{
		if (who.pBrain == null)
		{
			return false;
		}
		if (species == "*")
		{
			return true;
		}
		string species2 = who.GetSpecies();
		if (species2 == "*")
		{
			return true;
		}
		return species2 == species;
	}
}
