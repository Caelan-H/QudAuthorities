using System;

namespace XRL.World.Parts;

[Serializable]
public class EmitGasOnHit : IActivePart
{
	public int Chance = 100;

	public int ChanceEach = 100;

	public string GasBlueprint = "PoisonGas";

	public string CellDensity = "4d10";

	public string AdjacentDensity = "2d10";

	public string GasLevel = "1";

	public string BehaviorDescription;

	public EmitGasOnHit()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		EmitGasOnHit emitGasOnHit = p as EmitGasOnHit;
		if (emitGasOnHit.Chance != Chance)
		{
			return false;
		}
		if (emitGasOnHit.ChanceEach != ChanceEach)
		{
			return false;
		}
		if (emitGasOnHit.GasBlueprint != GasBlueprint)
		{
			return false;
		}
		if (emitGasOnHit.CellDensity != CellDensity)
		{
			return false;
		}
		if (emitGasOnHit.AdjacentDensity != AdjacentDensity)
		{
			return false;
		}
		if (emitGasOnHit.BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return !string.IsNullOrEmpty(BehaviorDescription);
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(BehaviorDescription))
		{
			E.Postfix.AppendRules(BehaviorDescription);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AttackerHit");
		Object.RegisterPartEvent(this, "ProjectileHit");
		Object.RegisterPartEvent(this, "WeaponHit");
		Object.RegisterPartEvent(this, "WeaponThrowHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ProjectileHit" || E.ID == "WeaponHit" || E.ID == "AttackerHit" || E.ID == "WeaponThrowHit")
		{
			CheckApply(E);
		}
		return base.FireEvent(E);
	}

	public int CheckApply(Event E)
	{
		if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return 0;
		}
		GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
		GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
		GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
		GameObject parentObject = ParentObject;
		GameObject subject = gameObjectParameter2;
		GameObject projectile = gameObjectParameter3;
		if (!GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part EmitGasOnHit Activation Main", Chance, subject, projectile).in100())
		{
			return 0;
		}
		GameObject parentObject2 = ParentObject;
		projectile = gameObjectParameter2;
		subject = gameObjectParameter3;
		int @for = GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject2, "Part EmitGasOnHit Activation Each", ChanceEach, projectile, subject);
		return EmitGas(gameObjectParameter, gameObjectParameter2, @for);
	}

	public int EmitGas(GameObject Creator, GameObject Object, int useChanceEach)
	{
		int result = 0;
		Cell cell = Object?.CurrentCell;
		if (cell != null)
		{
			Event e = Event.New("CreatorModifyGas", "Gas", (object)null);
			EmitGas(cell, Creator, e, CellDensity.RollCached(), GasLevel.RollCached(), useChanceEach);
			{
				foreach (Cell localAdjacentCell in cell.GetLocalAdjacentCells())
				{
					EmitGas(localAdjacentCell, Creator, e, AdjacentDensity.RollCached(), GasLevel.RollCached(), useChanceEach);
				}
				return result;
			}
		}
		return result;
	}

	public bool EmitGas(Cell C, GameObject Creator, Event E, int Density, int Level, int useChanceEach)
	{
		if (!useChanceEach.in100())
		{
			return false;
		}
		GameObject firstObject = C.GetFirstObject(GasBlueprint);
		if (firstObject == null)
		{
			firstObject = GameObject.create(GasBlueprint);
			Gas obj = firstObject.GetPart("Gas") as Gas;
			obj.Density = Density;
			obj.Level = Level;
			obj.Creator = Creator;
			E.SetParameter("Gas", firstObject);
			Creator?.FireEvent(E);
			C.AddObject(firstObject);
		}
		else
		{
			Gas gas = firstObject.GetPart("Gas") as Gas;
			gas.Density += Density;
			if (gas.Level < Level || gas.Density < Density * 2)
			{
				gas.Level = Level;
			}
		}
		return true;
	}
}
