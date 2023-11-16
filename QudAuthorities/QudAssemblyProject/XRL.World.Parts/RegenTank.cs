using System;

namespace XRL.World.Parts;

[Serializable]
public class RegenTank : IPoweredPart
{
	public const int STATUS_READY = 1;

	public const int STATUS_TANK_NOT_FULL = 2;

	public const int STATUS_WRONG_MIXTURE = 3;

	public const int STATUS_MISSING_REGEN_LIQUID = 4;

	public const int STATUS_WRONG_SUBJECT = 5;

	public string RulesDescription = "Rejuvenates health and regenerates lost body parts.\nMust be filled with at least 100 drams of a liquid mixture that's at least 2/3rds convalessence to be fully functional.\nIf 1+ dram of cloning draught is present in the mixture, all lost body parts are regenerated over time and 1 dram of cloning draught is consumed. Excess cloning draught is not consumed.\nMust be entered and waited inside for regeneration to take effect.";

	public string RejuvenationStatusLine = "{{C|Rejuvenation status: }}";

	public string LimbRegenerationStatusLine = "{{C|Limb regeneration status: }}";

	public string RejuvenationTriggerNotify = "RegenTankRejuvenation";

	public string LimbRegenerationTriggerNotify = "RegenTankLimbRegeneration";

	public string BaseLiquid = "convalessence";

	public string RegenLiquid = "cloning";

	public string RejuvenationHealing = "1d3";

	public string RejuvenationEvent = "Recuperating";

	public string RejuvenationQuery;

	public string RejuvenationNotify;

	public string LimbRegenerationHealing = "5d8";

	public string LimbRegenerationEvent;

	public string LimbRegenerationQuery = "AnyRegenerableLimbs";

	public string LimbRegenerationNotify = "RegenerateAllLimbs";

	public int MinTotalDrams = 100;

	[FieldSaveVersion(236)]
	public int BaseLiquidPermillageNeeded = 666;

	public int RegenLiquidMilliDramsNeeded = 850;

	public int RegenLiquidDramsUsed = 1;

	public int RejuvenationRegeneraLevel = 4;

	public int LimbRegenerationRegeneraLevel = 4;

	public bool RequiresAlive = true;

	public RegenTank()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		RegenTank regenTank = p as RegenTank;
		if (regenTank.RulesDescription != RulesDescription)
		{
			return false;
		}
		if (regenTank.RejuvenationStatusLine != RejuvenationStatusLine)
		{
			return false;
		}
		if (regenTank.LimbRegenerationStatusLine != LimbRegenerationStatusLine)
		{
			return false;
		}
		if (regenTank.RejuvenationTriggerNotify != RejuvenationTriggerNotify)
		{
			return false;
		}
		if (regenTank.LimbRegenerationTriggerNotify != LimbRegenerationTriggerNotify)
		{
			return false;
		}
		if (regenTank.BaseLiquid != BaseLiquid)
		{
			return false;
		}
		if (regenTank.RegenLiquid != RegenLiquid)
		{
			return false;
		}
		if (regenTank.RejuvenationHealing != RejuvenationHealing)
		{
			return false;
		}
		if (regenTank.RejuvenationEvent != RejuvenationEvent)
		{
			return false;
		}
		if (regenTank.RejuvenationQuery != RejuvenationQuery)
		{
			return false;
		}
		if (regenTank.RejuvenationNotify != RejuvenationNotify)
		{
			return false;
		}
		if (regenTank.LimbRegenerationHealing != LimbRegenerationHealing)
		{
			return false;
		}
		if (regenTank.LimbRegenerationEvent != LimbRegenerationEvent)
		{
			return false;
		}
		if (regenTank.LimbRegenerationQuery != LimbRegenerationQuery)
		{
			return false;
		}
		if (regenTank.LimbRegenerationNotify != LimbRegenerationNotify)
		{
			return false;
		}
		if (regenTank.MinTotalDrams != MinTotalDrams)
		{
			return false;
		}
		if (regenTank.BaseLiquidPermillageNeeded != BaseLiquidPermillageNeeded)
		{
			return false;
		}
		if (regenTank.RegenLiquidMilliDramsNeeded != RegenLiquidMilliDramsNeeded)
		{
			return false;
		}
		if (regenTank.RegenLiquidDramsUsed != RegenLiquidDramsUsed)
		{
			return false;
		}
		if (regenTank.RejuvenationRegeneraLevel != RejuvenationRegeneraLevel)
		{
			return false;
		}
		if (regenTank.LimbRegenerationRegeneraLevel != LimbRegenerationRegeneraLevel)
		{
			return false;
		}
		if (regenTank.RequiresAlive != RequiresAlive)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GenericNotifyEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GenericNotifyEvent E)
	{
		if (E.Notify == RejuvenationTriggerNotify)
		{
			if (E.Subject != null && GetRejuvenationStatus(E.Subject) == 1 && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && (string.IsNullOrEmpty(RejuvenationQuery) || GenericQueryEvent.Check(E.Subject, RejuvenationQuery, null, ParentObject, RejuvenationRegeneraLevel)))
			{
				if (!string.IsNullOrEmpty(RejuvenationHealing))
				{
					int num = RejuvenationHealing.RollCached();
					if (num > 0)
					{
						E.Subject.Heal(num, Message: false, FloatText: true, RandomMinimum: true);
					}
				}
				if (!string.IsNullOrEmpty(RejuvenationEvent))
				{
					E.Subject.FireEvent(RejuvenationEvent);
				}
				if (RejuvenationRegeneraLevel != 0)
				{
					E.Subject.FireEvent(Event.New("Regenera", "Source", ParentObject, "Level", RejuvenationRegeneraLevel));
				}
				if (!string.IsNullOrEmpty(RejuvenationNotify))
				{
					GenericNotifyEvent.Send(E.Subject, RejuvenationNotify, null, ParentObject, RejuvenationRegeneraLevel);
				}
			}
		}
		else if (E.Notify == LimbRegenerationTriggerNotify && E.Subject != null && GetLimbRegenerationStatus(E.Subject) == 1 && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && (string.IsNullOrEmpty(LimbRegenerationQuery) || GenericQueryEvent.Check(E.Subject, LimbRegenerationQuery, null, ParentObject, LimbRegenerationRegeneraLevel)))
		{
			if (!string.IsNullOrEmpty(LimbRegenerationHealing))
			{
				int num2 = LimbRegenerationHealing.RollCached();
				if (num2 > 0)
				{
					E.Subject.Heal(num2, Message: false, FloatText: true, RandomMinimum: true);
				}
			}
			if (!string.IsNullOrEmpty(LimbRegenerationEvent))
			{
				E.Subject.FireEvent(LimbRegenerationEvent);
			}
			if (LimbRegenerationRegeneraLevel != 0)
			{
				E.Subject.FireEvent(Event.New("Regenera", "Source", ParentObject, "Level", LimbRegenerationRegeneraLevel));
			}
			if (!string.IsNullOrEmpty(LimbRegenerationNotify))
			{
				GenericNotifyEvent.Send(E.Subject, LimbRegenerationNotify, null, ParentObject, LimbRegenerationRegeneraLevel);
			}
			if (RegenLiquidDramsUsed > 0)
			{
				ParentObject.LiquidVolume?.UseDrams(RegenLiquid, RegenLiquidDramsUsed);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(RejuvenationStatusLine) || !string.IsNullOrEmpty(LimbRegenerationStatusLine))
		{
			E.Base.Append('\n');
			if (!string.IsNullOrEmpty(RejuvenationStatusLine))
			{
				E.Base.Append('\n').Append(RejuvenationStatusLine).Append(GetRejuvenationStatusString());
			}
			if (!string.IsNullOrEmpty(LimbRegenerationStatusLine))
			{
				E.Base.Append('\n').Append(LimbRegenerationStatusLine).Append(GetLimbRegenerationStatusString());
			}
		}
		if (!string.IsNullOrEmpty(RulesDescription))
		{
			E.Postfix.AppendRules(RulesDescription);
		}
		return base.HandleEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		return GetRejuvenationStatus() != 1;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return GetRejuvenationStatus() switch
		{
			2 => "TankNotFull", 
			3 => "ImproperMixture", 
			5 => "ImproperSubject", 
			_ => null, 
		};
	}

	public int GetRejuvenationStatus(GameObject who = null)
	{
		if (RequiresAlive && who != null && !who.IsAlive)
		{
			return 5;
		}
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume == null || liquidVolume.Volume < MinTotalDrams)
		{
			return 2;
		}
		if (liquidVolume.Proportion(BaseLiquid) < BaseLiquidPermillageNeeded)
		{
			return 3;
		}
		return 1;
	}

	public int GetLimbRegenerationStatus(GameObject who = null)
	{
		if (RequiresAlive && who != null && !who.IsAlive)
		{
			return 5;
		}
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume == null || liquidVolume.Volume < MinTotalDrams)
		{
			return 2;
		}
		if (liquidVolume.Proportion(BaseLiquid) < BaseLiquidPermillageNeeded)
		{
			return 3;
		}
		liquidVolume.MilliAmount(RegenLiquid);
		if (liquidVolume.MilliAmount(RegenLiquid) < RegenLiquidMilliDramsNeeded)
		{
			return 4;
		}
		return 1;
	}

	public string GetRejuvenationStatusString()
	{
		int rejuvenationStatus = GetRejuvenationStatus();
		if (rejuvenationStatus == 1)
		{
			string statusSummary = GetStatusSummary();
			if (statusSummary != null)
			{
				return statusSummary;
			}
		}
		return GetStatusString(rejuvenationStatus);
	}

	public string GetLimbRegenerationStatusString()
	{
		int limbRegenerationStatus = GetLimbRegenerationStatus();
		if (limbRegenerationStatus == 1)
		{
			string statusSummary = GetStatusSummary();
			if (statusSummary != null)
			{
				return statusSummary;
			}
		}
		return GetStatusString(limbRegenerationStatus);
	}

	public string GetStatusString(int status)
	{
		return status switch
		{
			1 => "{{G|ready}}", 
			2 => "{{K|tank not full}}", 
			3 => "{{R|improper mixture}}", 
			4 => "{{K|insufficient " + LiquidVolume.getLiquid(RegenLiquid).GetName().Strip() + "}}", 
			5 => "{{R|improper subject}}", 
			_ => "{{K|unknown failure}}", 
		};
	}
}
