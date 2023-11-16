using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class SlottedCellCharger : IPoweredPart
{
	public int ChargeRate;

	public long ActiveTurn;

	public int UsedOnTurn;

	public SlottedCellCharger()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
		NameForStatus = "ChargingSystem";
	}

	public GameObject GetChargeableCell()
	{
		if (!(ParentObject.GetPart("EnergyCellSocket") is EnergyCellSocket energyCellSocket))
		{
			return null;
		}
		if (energyCellSocket.Cell == null)
		{
			return null;
		}
		if (!(energyCellSocket.Cell.GetPart("EnergyCell") is EnergyCell energyCell))
		{
			return null;
		}
		if (energyCell.Charge >= energyCell.MaxCharge)
		{
			return null;
		}
		return energyCellSocket.Cell;
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		return GetChargeableCell() == null;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		if (!(ParentObject.GetPart("EnergyCellSocket") is EnergyCellSocket energyCellSocket))
		{
			return "NoSocket";
		}
		if (energyCellSocket.Cell == null)
		{
			return "NoCell";
		}
		if (!(energyCellSocket.Cell.GetPart("EnergyCell") is EnergyCell energyCell))
		{
			return "InappropriateCell";
		}
		if (energyCell.Charge >= energyCell.MaxCharge)
		{
			return "CellFull";
		}
		return "Error";
	}

	public override bool SameAs(IPart p)
	{
		if ((p as SlottedCellCharger).ChargeRate != ChargeRate)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ChargeAvailableEvent.ID)
		{
			return ID == QueryChargeStorageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ChargeAvailableEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			CheckTurn();
			GameObject chargeableCell = GetChargeableCell();
			if (chargeableCell != null)
			{
				int num = ((ChargeRate == 0 || E.Forced) ? (E.Amount - ChargeUse * E.Multiple) : Math.Min(ChargeRate * E.Multiple - UsedOnTurn, E.Amount - ChargeUse * E.Multiple));
				if (num > 0)
				{
					int num2 = RechargeAvailableEvent.Send(chargeableCell, E, num);
					if (num2 != 0)
					{
						if (E.Multiple > 1)
						{
							UsedOnTurn = num2 / E.Multiple;
						}
						else
						{
							UsedOnTurn += num2;
						}
						E.Amount -= num2 + ChargeUse * E.Multiple;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeStorageEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			CheckTurn();
			if (ChargeRate <= 0 || ChargeRate > UsedOnTurn)
			{
				GameObject chargeableCell = GetChargeableCell();
				if (chargeableCell != null)
				{
					int num = QueryRechargeStorageEvent.Retrieve(chargeableCell, E);
					if (num > 0)
					{
						if (ChargeRate > 0)
						{
							num = Math.Min(num, ChargeRate - UsedOnTurn);
						}
						E.Amount += num;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public void CheckTurn()
	{
		if (XRLCore.Core != null && XRLCore.Core.Game != null && ActiveTurn < XRLCore.Core.Game.Turns)
		{
			UsedOnTurn = 0;
			ActiveTurn = XRLCore.Core.Game.Turns;
		}
	}
}
