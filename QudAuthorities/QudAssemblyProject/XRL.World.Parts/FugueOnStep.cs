using System;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class FugueOnStep : IActivePart
{
	public int Chance = 100;

	public string SaveStat;

	public string SaveDifficultyStat;

	public int SaveTarget = 15;

	public string SaveVs;

	public bool TriggerOnSaveSuccess = true;

	public string Duration = "2d6+20";

	public string Copies = "1d4+1";

	public string Cooldown = "100-200";

	public int HostileCopyChance;

	public string HostileCopyColorString;

	public string HostileCopyPrefix;

	public string FriendlyCopyColorString;

	public string FriendlyCopyPrefix;

	public new string ReadyColorString = "&G";

	public string ReadyTileColor = "&G";

	public new string ReadyDetailColor = "M";

	public string CooldownColorString = "&g";

	public string CooldownTileColor = "&g";

	public string CooldownDetailColor = "m";

	public bool TreatAsPsychic = true;

	public int CooldownLeft;

	public FugueOnStep()
	{
		WorksOnCellContents = true;
	}

	public override bool SameAs(IPart p)
	{
		FugueOnStep fugueOnStep = p as FugueOnStep;
		if (fugueOnStep.Chance != Chance)
		{
			return false;
		}
		if (fugueOnStep.SaveStat != SaveStat)
		{
			return false;
		}
		if (fugueOnStep.SaveDifficultyStat != SaveDifficultyStat)
		{
			return false;
		}
		if (fugueOnStep.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (fugueOnStep.SaveVs != SaveVs)
		{
			return false;
		}
		if (fugueOnStep.TriggerOnSaveSuccess != TriggerOnSaveSuccess)
		{
			return false;
		}
		if (fugueOnStep.Duration != Duration)
		{
			return false;
		}
		if (fugueOnStep.Copies != Copies)
		{
			return false;
		}
		if (fugueOnStep.Cooldown != Cooldown)
		{
			return false;
		}
		if (fugueOnStep.HostileCopyChance != HostileCopyChance)
		{
			return false;
		}
		if (fugueOnStep.HostileCopyColorString != HostileCopyColorString)
		{
			return false;
		}
		if (fugueOnStep.HostileCopyPrefix != HostileCopyPrefix)
		{
			return false;
		}
		if (fugueOnStep.FriendlyCopyColorString != FriendlyCopyColorString)
		{
			return false;
		}
		if (fugueOnStep.FriendlyCopyPrefix != FriendlyCopyPrefix)
		{
			return false;
		}
		if (fugueOnStep.ReadyColorString != ReadyColorString)
		{
			return false;
		}
		if (fugueOnStep.ReadyTileColor != ReadyTileColor)
		{
			return false;
		}
		if (fugueOnStep.ReadyDetailColor != ReadyDetailColor)
		{
			return false;
		}
		if (fugueOnStep.CooldownColorString != CooldownColorString)
		{
			return false;
		}
		if (fugueOnStep.CooldownTileColor != CooldownTileColor)
		{
			return false;
		}
		if (fugueOnStep.CooldownDetailColor != CooldownDetailColor)
		{
			return false;
		}
		if (fugueOnStep.CooldownLeft != CooldownLeft)
		{
			return false;
		}
		if (fugueOnStep.TreatAsPsychic != TreatAsPsychic)
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
		if (!base.WantEvent(ID, cascade) && ID != InterruptAutowalkEvent.ID && (ID != IsSensableAsPsychicEvent.ID || !TreatAsPsychic))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		return false;
	}

	public override bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		if (TreatAsPsychic)
		{
			E.Sensable = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		SyncColor();
		return base.HandleEvent(E);
	}

	public void CheckActivate()
	{
		if (CooldownLeft > 0 || GetActivePartFirstSubject(ValidStepTarget) == null || !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) || !Chance.in100())
		{
			return;
		}
		bool flag = false;
		foreach (GameObject activePartSubject in GetActivePartSubjects(ValidStepTarget))
		{
			if ((string.IsNullOrEmpty(SaveStat) || activePartSubject.MakeSave(SaveStat, SaveTarget, ParentObject, SaveDifficultyStat) == TriggerOnSaveSuccess) && Activate(activePartSubject))
			{
				flag = true;
			}
		}
		if (flag && !string.IsNullOrEmpty(Cooldown))
		{
			ConsumeChargeIfOperational();
			CooldownLeft = Stat.Roll(Cooldown);
			SyncColor();
		}
	}

	public bool Activate(GameObject who)
	{
		if (!TemporalFugue.PerformTemporalFugue(who, null, null, ParentObject, Involuntary: true, Stat.Roll(Duration), Stat.Roll(Copies), HostileCopyChance, HostileCopyColorString: HostileCopyColorString, HostileCopyPrefix: HostileCopyPrefix, FriendlyCopyColorString: FriendlyCopyColorString, FriendlyCopyPrefix: FriendlyCopyPrefix))
		{
			return false;
		}
		if (Visible())
		{
			if (who.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You step on " + ParentObject.t() + " and vibrate through spacetime.");
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage(who.Does("step") + " on " + ParentObject.t() + " and" + who.GetVerb("vibrate") + " through spacetime.");
			}
		}
		return true;
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
			return obj.FlightMatches(ParentObject);
		}
		return false;
	}
}
