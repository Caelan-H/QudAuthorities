using System;

namespace XRL.World.Parts;

[Serializable]
public class TemperatureAdjuster : IPoweredPart
{
	public int TemperatureAmount = 5;

	public int TemperatureThreshold = 100;

	public bool ThresholdAbove = true;

	public bool AlwaysUseCharge;

	public string BehaviorDescription;

	public bool InactiveOnWorldMap;

	public TemperatureAdjuster()
	{
		WorksOnSelf = true;
		WorksOnHolder = true;
		WorksOnWearer = true;
		WorksOnCarrier = true;
		WorksOnCellContents = true;
		WorksOnAdjacentCellContents = true;
	}

	public override bool SameAs(IPart p)
	{
		TemperatureAdjuster temperatureAdjuster = p as TemperatureAdjuster;
		if (temperatureAdjuster.TemperatureThreshold != TemperatureThreshold)
		{
			return false;
		}
		if (temperatureAdjuster.ThresholdAbove != ThresholdAbove)
		{
			return false;
		}
		if (temperatureAdjuster.AlwaysUseCharge != AlwaysUseCharge)
		{
			return false;
		}
		if (temperatureAdjuster.BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		if (temperatureAdjuster.InactiveOnWorldMap != InactiveOnWorldMap)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (InactiveOnWorldMap)
		{
			if (ActivePartHasMultipleSubjects())
			{
				foreach (GameObject activePartSubject in GetActivePartSubjects())
				{
					if (activePartSubject.OnWorldMap())
					{
						return true;
					}
				}
			}
			else
			{
				GameObject activePartFirstSubject = GetActivePartFirstSubject();
				if (activePartFirstSubject != null && activePartFirstSubject.OnWorldMap())
				{
					return true;
				}
			}
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "Inactive";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != GetShortDescriptionEvent.ID || string.IsNullOrEmpty(BehaviorDescription)) && (ID != RadiatesHeatAdjacentEvent.ID || !WorksOnAdjacentCellContents || TemperatureAmount <= 0))
		{
			if (ID == RadiatesHeatEvent.ID && WorksOnCellContents)
			{
				return TemperatureAmount > 0;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(BehaviorDescription))
		{
			E.Postfix.AppendRules(BehaviorDescription, base.AddStatusSummary);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RadiatesHeatAdjacentEvent E)
	{
		if (WorksOnAdjacentCellContents && TemperatureAmount > 0 && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RadiatesHeatEvent E)
	{
		if (WorksOnCellContents && TemperatureAmount > 0 && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
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
		int num = MyPowerLoadLevel();
		if (!IsReady(AlwaysUseCharge, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
		{
			return;
		}
		bool flag = false;
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				if (AdjustTemperature(activePartSubject, num))
				{
					flag = true;
				}
			}
		}
		else
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (activePartFirstSubject != null && AdjustTemperature(activePartFirstSubject, num))
			{
				flag = true;
			}
		}
		if (flag && !AlwaysUseCharge)
		{
			ConsumeCharge(null, num);
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		int num = MyPowerLoadLevel();
		if (!IsReady(AlwaysUseCharge, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 10, null, UseChargeIfUnpowered: false, 0L, num))
		{
			return;
		}
		bool flag = false;
		int num2 = 1;
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				for (int i = 1; i <= 10; i++)
				{
					if (!AdjustTemperature(activePartSubject, num))
					{
						break;
					}
					flag = true;
					if (i > num2)
					{
						num2 = i;
					}
				}
			}
		}
		else
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (activePartFirstSubject != null)
			{
				for (int j = 1; j <= 10; j++)
				{
					if (!AdjustTemperature(activePartFirstSubject, num))
					{
						break;
					}
					flag = true;
					if (j > num2)
					{
						num2 = j;
					}
				}
			}
		}
		if (flag && !AlwaysUseCharge)
		{
			ConsumeCharge(num2, null, num);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		int num = MyPowerLoadLevel();
		if (!IsReady(AlwaysUseCharge, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 100, null, UseChargeIfUnpowered: false, 0L, num))
		{
			return;
		}
		bool flag = false;
		int num2 = 1;
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				for (int i = 1; i <= 100; i++)
				{
					if (!AdjustTemperature(activePartSubject, num))
					{
						break;
					}
					flag = true;
					if (i > num2)
					{
						num2 = i;
					}
				}
			}
		}
		else
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (activePartFirstSubject != null)
			{
				for (int j = 1; j <= 100; j++)
				{
					if (!AdjustTemperature(activePartFirstSubject, num))
					{
						break;
					}
					flag = true;
					if (j > num2)
					{
						num2 = j;
					}
				}
			}
		}
		if (flag && !AlwaysUseCharge)
		{
			ConsumeCharge(num2, null, num);
		}
	}

	public bool AdjustTemperature(GameObject obj, int PowerLoad = int.MinValue)
	{
		if (obj.pPhysics == null)
		{
			return false;
		}
		if (ThresholdAbove)
		{
			if (obj.pPhysics.Temperature >= TemperatureThreshold)
			{
				return false;
			}
		}
		else if (obj.pPhysics.Temperature <= TemperatureThreshold)
		{
			return false;
		}
		int num = TemperatureAmount;
		if (IsPowerLoadSensitive)
		{
			if (PowerLoad == int.MinValue)
			{
				PowerLoad = MyPowerLoadLevel();
			}
			int num2 = MyPowerLoadBonus(PowerLoad, 100, 10);
			if (num2 != 0)
			{
				num = num * (100 + num2) / 100;
			}
		}
		obj.TemperatureChange(num, obj.Equipped ?? obj);
		return true;
	}
}
