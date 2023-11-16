using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
///             which it is by default, a chance to target a given projectile that
///             is over 0 will be increased by ((power load - 100) / 30), i.e.
///             10 for the standard overload power load of 400.
///             </remarks>
[Serializable]
public class PointDefense : IPoweredPart
{
	public int MinRange = 2;

	public int MaxRange = 10;

	public int TargetExplosives = 100;

	public int TargetThrownWeapons = 100;

	public int TargetArrows = 100;

	public int TargetSlugs;

	public int TargetEnergy;

	public bool UsesSelfEquipment;

	public bool UsesEquipperEquipment;

	public bool ShowComputeMessage = true;

	public string EquipmentEvent = "UseForPointDefense";

	public float ComputePowerFactor = 1f;

	[NonSerialized]
	private Cell TargetCell;

	[NonSerialized]
	private bool TargetCellHit;

	[NonSerialized]
	private GameObject WeaponSystem;

	public PointDefense()
	{
		WorksOnEquipper = true;
		IsPowerLoadSensitive = true;
		NameForStatus = "PointDefenseTracking";
	}

	public override bool SameAs(IPart p)
	{
		PointDefense pointDefense = p as PointDefense;
		if (pointDefense.MinRange != MinRange)
		{
			return false;
		}
		if (pointDefense.MaxRange != MaxRange)
		{
			return false;
		}
		if (pointDefense.TargetExplosives != TargetExplosives)
		{
			return false;
		}
		if (pointDefense.TargetThrownWeapons != TargetThrownWeapons)
		{
			return false;
		}
		if (pointDefense.TargetArrows != TargetArrows)
		{
			return false;
		}
		if (pointDefense.TargetSlugs != TargetSlugs)
		{
			return false;
		}
		if (pointDefense.TargetEnergy != TargetEnergy)
		{
			return false;
		}
		if (pointDefense.UsesSelfEquipment != UsesSelfEquipment)
		{
			return false;
		}
		if (pointDefense.UsesEquipperEquipment != UsesEquipperEquipment)
		{
			return false;
		}
		if (pointDefense.ShowComputeMessage != ShowComputeMessage)
		{
			return false;
		}
		if (pointDefense.EquipmentEvent != EquipmentEvent)
		{
			return false;
		}
		if (pointDefense.ComputePowerFactor != ComputePowerFactor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ProjectileMovingEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowComputeMessage)
		{
			if (ComputePowerFactor > 0f)
			{
				E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
			}
			else if (ComputePowerFactor < 0f)
			{
				E.Postfix.AppendRules("Compute power on the local lattice decreases this item's effectiveness.");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ProjectileMovingEvent E)
	{
		if (E.Launcher == WeaponSystem && WeaponSystem != null)
		{
			if (E.Cell == TargetCell)
			{
				TargetCellHit = true;
				TargetCell = null;
				return false;
			}
		}
		else if (E.Defender == null)
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell == null)
			{
				return true;
			}
			int num = cell.PathDistanceTo(E.Cell);
			if (num < MinRange || num > MaxRange)
			{
				return true;
			}
			GameObject gameObject = GetActivePartFirstSubject() ?? ParentObject;
			if (gameObject == E.Attacker)
			{
				return true;
			}
			int? PowerLoad = null;
			if (!ChanceToTargetProjectile(E.Projectile, ref PowerLoad, E.Throw).in100())
			{
				return true;
			}
			if (E.Attacker != null && !E.Attacker.IsHostileTowards(gameObject) && !IsCellOnPath(cell, E.Path))
			{
				return true;
			}
			if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, PowerLoad))
			{
				return true;
			}
			if (!PathClear(cell, E.Cell, gameObject))
			{
				return true;
			}
			if (UsesSelfEquipment || UsesEquipperEquipment)
			{
				WeaponSystem = null;
				if (UsesSelfEquipment)
				{
					Body body = ParentObject.Body;
					if (body != null)
					{
						WeaponSystem = body.FindEquipmentByEvent(EquipmentEvent);
					}
				}
				if (UsesEquipperEquipment && WeaponSystem == null && ParentObject.Equipped == null)
				{
					Body body2 = ParentObject.Equipped.Body;
					if (body2 != null)
					{
						WeaponSystem = body2.FindEquipmentByEvent(EquipmentEvent);
					}
				}
				if (WeaponSystem == null)
				{
					return true;
				}
			}
			else
			{
				WeaponSystem = ParentObject;
			}
			TargetCell = E.Cell;
			TargetCellHit = false;
			try
			{
				Event @event = Event.New("CommandFireMissile");
				@event.SetParameter("Owner", gameObject);
				@event.SetParameter("TargetCell", TargetCell);
				@event.SetParameter("ScreenBuffer", E.ScreenBuffer);
				@event.SetParameter("MessageAsFrom", WeaponSystem);
				WeaponSystem.FireEvent(@event);
				if (TargetCellHit)
				{
					WeaponSystem?.pPhysics?.DidXToY("intercept", E.Projectile, null, "!", null, gameObject);
					return false;
				}
			}
			finally
			{
				WeaponSystem = null;
				TargetCell = null;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		ConsumeChargeIfOperational();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 10);
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 100);
	}

	public bool PathClear(Cell CC, Cell TargetCell, GameObject Actor)
	{
		Zone parentZone = CC.ParentZone;
		List<Point> list = Zone.Line(CC.X, CC.Y, TargetCell.X, TargetCell.Y);
		for (int i = 1; i < list.Count - 1; i++)
		{
			Cell cell = parentZone.GetCell(list[i].X, list[i].Y);
			if (cell.IsOccluding())
			{
				return false;
			}
			if (!cell.HasObjectWithPart("Combat"))
			{
				continue;
			}
			foreach (GameObject item in cell.LoopObjectsWithPart("Combat"))
			{
				if (!Actor.IsHostileTowards(item))
				{
					return false;
				}
			}
		}
		return true;
	}

	public int ChanceToTargetProjectile(GameObject obj, ref int? PowerLoad, bool? Thrown = null)
	{
		int num = 0;
		if (TargetThrownWeapons > 0 && num < TargetThrownWeapons)
		{
			if (Thrown == true)
			{
				num = TargetThrownWeapons;
			}
			else if (!Thrown.HasValue && obj.HasPart("ThrownWeapon"))
			{
				num = TargetThrownWeapons;
			}
		}
		if (TargetExplosives > 0 && num < TargetExplosives && IsExplosiveEvent.Check(obj))
		{
			num = TargetExplosives;
		}
		if (TargetArrows > 0 && num < TargetArrows && (obj.HasPart("AmmoArrow") || obj.HasTag("Arrow")))
		{
			num = TargetArrows;
		}
		if (TargetSlugs > 0 && num < TargetSlugs && (obj.HasPart("AmmoSlug") || obj.HasPart("AmmoShotgunShell") || obj.HasTag("Slug")))
		{
			num = TargetSlugs;
		}
		if (TargetEnergy > 0 && num < TargetEnergy && ProjectileIsEnergy(obj))
		{
			num = TargetEnergy;
		}
		if (num > 0)
		{
			num = GetAvailableComputePowerEvent.AdjustUp(this, num, ComputePowerFactor);
			if (IsPowerLoadSensitive)
			{
				if (!PowerLoad.HasValue)
				{
					PowerLoad = MyPowerLoadLevel();
				}
				num += IComponent<GameObject>.PowerLoadBonus(PowerLoad.Value, 100, 30);
			}
		}
		return num;
	}

	public int ChanceToTargetProjectile(GameObject obj, bool? Thrown = null)
	{
		int? PowerLoad = 100;
		return ChanceToTargetProjectile(obj, ref PowerLoad, Thrown);
	}

	private bool ProjectileIsEnergy(GameObject obj)
	{
		if (obj.GetPart("Projectile") is Projectile projectile && !string.IsNullOrEmpty(projectile.Attributes))
		{
			if (projectile.Attributes.Contains("Light"))
			{
				return true;
			}
			if (projectile.Attributes.Contains("Heat"))
			{
				return true;
			}
			if (projectile.Attributes.Contains("Cold"))
			{
				return true;
			}
			if (projectile.Attributes.Contains("Disintegrate"))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCellOnPath(Cell C, List<Point> Path)
	{
		int i = 0;
		for (int count = Path.Count; i < count; i++)
		{
			if (Path[i].X == C.X && Path[i].Y == C.Y)
			{
				return true;
			}
		}
		return false;
	}
}
