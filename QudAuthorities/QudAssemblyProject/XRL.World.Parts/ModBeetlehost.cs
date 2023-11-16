using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;

namespace XRL.World.Parts;

[Serializable]
public class ModBeetlehost : IModification
{
	public int Chance = 100;

	public ModBeetlehost()
	{
	}

	public ModBeetlehost(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnHolder = true;
		ChargeUse = 1000;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		base.IsTechScannable = true;
		NameForStatus = "AutomataFab";
	}

	public override void TierConfigure()
	{
		ChargeUse = Math.Max(1000 - Tier * 150, 100);
	}

	public override void ApplyModification(GameObject Object)
	{
		if (ChargeUse > 0 && !Object.IsCreature)
		{
			Object.RequirePart<EnergyCellSocket>();
			IncreaseDifficultyAndComplexity(3, 2);
		}
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModBeetlehost).Chance != Chance)
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
			E.Postfix.AppendRules("When powered, discharges a clockwork beetle friend on hit. Drains cell power quickly.", base.AddStatusSummary);
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
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Weapon");
			GameObject gameObjectParameter4 = E.GetGameObjectParameter("Projectile");
			Cell cell = gameObjectParameter2.CurrentCell;
			if (cell != null)
			{
				List<Cell> localEmptyAdjacentCells = cell.GetLocalEmptyAdjacentCells();
				if (localEmptyAdjacentCells.Count > 0 && (ParentObject.IsCombatObject() || IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L)))
				{
					GameObject subject = gameObjectParameter2;
					GameObject projectile = gameObjectParameter4;
					if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter3, "Modification ModBeetlehost Activation", Chance, subject, projectile).in100())
					{
						gameObjectParameter2.Bloodsplatter();
						Algorithms.RandomShuffleInPlace(localEmptyAdjacentCells);
						GameObject gameObject = GameObject.create("ClockworkBeetle");
						gameObject.MakeActive();
						localEmptyAdjacentCells[0].AddObject(gameObject);
						if (gameObject.pBrain != null)
						{
							gameObject.pBrain.PartyLeader = gameObjectParameter;
							gameObject.pBrain.Target = gameObjectParameter2;
							gameObject.IsTrifling = true;
						}
						gameObjectParameter2.Target = gameObject;
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
