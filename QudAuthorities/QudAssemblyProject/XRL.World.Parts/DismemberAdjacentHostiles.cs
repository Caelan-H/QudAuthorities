using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class DismemberAdjacentHostiles : IPoweredPart
{
	public int ChanceToActivate = 100;

	public int ChancePerHostile = 10;

	public bool CanAlwaysDecapitate;

	public bool UsesChargeToActivate;

	public bool UsesChargePerHostile;

	public bool UsesChargePerDismemberment = true;

	public DismemberAdjacentHostiles()
	{
		ChargeUse = 1000;
		IsPowerLoadSensitive = true;
		WorksOnEquipper = true;
		NameForStatus = "AntipersonnelSystems";
	}

	public override bool SameAs(IPart p)
	{
		DismemberAdjacentHostiles dismemberAdjacentHostiles = p as DismemberAdjacentHostiles;
		if (dismemberAdjacentHostiles.ChanceToActivate != ChanceToActivate)
		{
			return false;
		}
		if (dismemberAdjacentHostiles.ChancePerHostile != ChancePerHostile)
		{
			return false;
		}
		if (dismemberAdjacentHostiles.CanAlwaysDecapitate != CanAlwaysDecapitate)
		{
			return false;
		}
		if (dismemberAdjacentHostiles.UsesChargeToActivate != UsesChargeToActivate)
		{
			return false;
		}
		if (dismemberAdjacentHostiles.UsesChargePerHostile != UsesChargePerHostile)
		{
			return false;
		}
		if (dismemberAdjacentHostiles.UsesChargePerDismemberment != UsesChargePerDismemberment)
		{
			return false;
		}
		return base.SameAs(p);
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
		CheckDismemberment();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GenericQueryEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GenericQueryEvent E)
	{
		if (E.Query == "PhaseHarmonicEligible" && base.IsTechScannable)
		{
			E.Result = true;
		}
		return base.HandleEvent(E);
	}

	public void CheckDismemberment()
	{
		GameObject gameObject = ParentObject.Equipped ?? ParentObject.Implantee;
		if (gameObject == null)
		{
			return;
		}
		Cell cell = gameObject.CurrentCell;
		if (cell == null || cell.OnWorldMap())
		{
			return;
		}
		int num = MyPowerLoadLevel();
		int num2 = ChanceToActivate;
		if (num2 < 100 && IsPowerLoadSensitive)
		{
			num2 = num2 * (100 + IComponent<GameObject>.PowerLoadBonus(num, 100, 10)) / 100;
		}
		if (!num2.in100() || !IsReady(UsesChargeToActivate, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
		{
			return;
		}
		CheckDismemberment(cell, gameObject, num);
		foreach (Cell localAdjacentCell in cell.GetLocalAdjacentCells())
		{
			CheckDismemberment(localAdjacentCell, gameObject, num);
		}
	}

	public void CheckDismemberment(Cell C, GameObject user = null, int? load = null)
	{
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			GameObject gameObject = C.Objects[i];
			if (gameObject.pBrain == null)
			{
				continue;
			}
			CheckDismemberment(gameObject, user, load);
			if (count != C.Objects.Count)
			{
				count = C.Objects.Count;
				if (i < count && C.Objects[i] != gameObject)
				{
					i--;
				}
			}
		}
	}

	public void CheckDismemberment(GameObject who, GameObject user = null, int? load = null)
	{
		if (!GameObject.validate(ref who))
		{
			return;
		}
		if (user == null)
		{
			user = ParentObject.Equipped ?? ParentObject.Implantee;
		}
		if (!who.IsHostileTowards(user) || who.Body == null || !user.FlightMatches(who) || (UsesChargePerHostile && !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, load)))
		{
			return;
		}
		int num = ChancePerHostile;
		if (num < 100 && IsPowerLoadSensitive)
		{
			num = num * (100 + IComponent<GameObject>.PowerLoadBonus(load.Value, 100, 10)) / 100;
		}
		if (!num.in100())
		{
			return;
		}
		int @for = GetActivationPhaseEvent.GetFor(ParentObject, user.GetPhase());
		if (who.PhaseMatches(@for))
		{
			if (!load.HasValue)
			{
				load = MyPowerLoadLevel();
			}
			if (IsReady(UsesChargePerDismemberment, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, load))
			{
				Axe_Dismember.Dismember(user, who, null, null, ParentObject, null, CanAlwaysDecapitate, suppressDecapitate: false, weaponActing: true);
			}
		}
	}
}
