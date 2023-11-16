using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class DischargeOnStep : IActivePart
{
	public int CooldownLeft;

	public string Cooldown = "1d6";

	public int Chance = 100;

	public string SaveStat;

	public string SaveDifficultyStat;

	public int SaveTarget = 15;

	public string SaveVs;

	public bool TriggerOnSaveSuccess = true;

	public string Voltage = "1d2";

	public string Damage = "4d4";

	public string CooldownColorString = "&w";

	public new string ReadyColorString = "&W";

	public string CooldownTileColor = "&w";

	public string ReadyTileColor = "&W";

	public string CooldownDetailColor = "w";

	public new string ReadyDetailColor = "w";

	public DischargeOnStep()
	{
		WorksOnCellContents = true;
	}

	public override bool SameAs(IPart p)
	{
		DischargeOnStep dischargeOnStep = p as DischargeOnStep;
		if (dischargeOnStep.Chance != Chance)
		{
			return false;
		}
		if (dischargeOnStep.SaveStat != SaveStat)
		{
			return false;
		}
		if (dischargeOnStep.SaveDifficultyStat != SaveDifficultyStat)
		{
			return false;
		}
		if (dischargeOnStep.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (dischargeOnStep.SaveVs != SaveVs)
		{
			return false;
		}
		if (dischargeOnStep.CooldownLeft != CooldownLeft)
		{
			return false;
		}
		if (dischargeOnStep.TriggerOnSaveSuccess != TriggerOnSaveSuccess)
		{
			return false;
		}
		if (dischargeOnStep.Cooldown != Cooldown)
		{
			return false;
		}
		if (dischargeOnStep.Voltage != Voltage)
		{
			return false;
		}
		if (dischargeOnStep.Damage != Damage)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (CooldownLeft > 0)
		{
			CooldownLeft--;
			if (CooldownLeft <= 0)
			{
				SyncColor();
			}
		}
		CheckActivate();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetNavigationWeightEvent.ID && ID != InterruptAutowalkEvent.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		GameObject actor = E.Actor;
		if (actor == null || actor.PhaseAndFlightMatches(ParentObject))
		{
			E.MinWeight(98);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		return false;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		SyncColor();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		CheckActivate();
		return base.HandleEvent(E);
	}

	public void CheckActivate()
	{
		if (CooldownLeft > 0 || GetActivePartFirstSubject(ValidStepTarget) == null || !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) || !Chance.in100())
		{
			return;
		}
		foreach (GameObject activePartSubject in GetActivePartSubjects(ValidStepTarget))
		{
			if (string.IsNullOrEmpty(SaveStat) || activePartSubject.MakeSave(SaveStat, SaveTarget, ParentObject, SaveDifficultyStat) == TriggerOnSaveSuccess)
			{
				Discharge();
				break;
			}
		}
	}

	public void SyncColor()
	{
		if (CooldownLeft > 0)
		{
			if (!string.IsNullOrEmpty(CooldownColorString))
			{
				ParentObject.pRender.ColorString = CooldownColorString;
			}
			if (!string.IsNullOrEmpty(CooldownTileColor))
			{
				ParentObject.pRender.TileColor = CooldownTileColor;
			}
			if (!string.IsNullOrEmpty(CooldownDetailColor))
			{
				ParentObject.pRender.DetailColor = CooldownDetailColor;
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(ReadyColorString))
			{
				ParentObject.pRender.ColorString = ReadyColorString;
			}
			if (!string.IsNullOrEmpty(ReadyTileColor))
			{
				ParentObject.pRender.TileColor = ReadyTileColor;
			}
			if (!string.IsNullOrEmpty(ReadyDetailColor))
			{
				ParentObject.pRender.DetailColor = ReadyDetailColor;
			}
		}
	}

	public bool ValidStepTarget(GameObject obj)
	{
		if (obj.IsCombatObject())
		{
			return obj.PhaseAndFlightMatches(ParentObject);
		}
		return false;
	}

	public void Discharge()
	{
		if (CooldownLeft > 0)
		{
			return;
		}
		CooldownLeft = Stat.Roll(Cooldown);
		List<Cell> adjacentCells = ParentObject.CurrentCell.GetAdjacentCells();
		adjacentCells.Add(ParentObject.CurrentCell);
		adjacentCells.ForEach(delegate(Cell c)
		{
			ParentObject.Discharge(c, Voltage.RollCached(), Damage, ParentObject);
		});
		adjacentCells.ForEach(delegate(Cell c)
		{
			c.GetObjectsWithPart("DischargeOnStep").ForEach(delegate(GameObject o)
			{
				o.GetPart<DischargeOnStep>().Discharge();
			});
		});
		ConsumeChargeIfOperational();
		SyncColor();
	}
}
