using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class BootSequence : IPoweredPart
{
	public int BootTime = 100;

	public string VariableBootTime;

	public int BootTimeLeft;

	public bool ReadoutInName;

	public bool ReadoutInDescription;

	public bool AlwaysObvious;

	public bool ObviousIfUnderstood;

	public string TextInDescription;

	public string _VerbOnBootInitialized = "beep";

	public string _VerbOnBootDone = "ding";

	public string _VerbOnBootAborted = "bloop";

	public string SoundOnBootInitialized = "startup";

	public string SoundOnBootDone = "completion";

	public string SoundOnBootAborted = "shutdown";

	[FieldSaveVersion(256)]
	public float SoundVolumeOnBootInitialized = 0.6f;

	[FieldSaveVersion(256)]
	public float SoundVolumeOnBootDone = 0.5f;

	[FieldSaveVersion(256)]
	public float SoundVolumeOnBootAborted = 1f;

	public float ComputePowerFactor = 1.5f;

	public bool Sensitive;

	public bool PartWasReady;

	public string VerbOnBootInitialized
	{
		get
		{
			return _VerbOnBootInitialized;
		}
		set
		{
			_VerbOnBootInitialized = value;
			if (SoundOnBootInitialized == "startup" && _VerbOnBootInitialized != "beep")
			{
				SoundOnBootInitialized = null;
				SoundVolumeOnBootInitialized = 1f;
			}
		}
	}

	public string VerbOnBootDone
	{
		get
		{
			return _VerbOnBootDone;
		}
		set
		{
			_VerbOnBootDone = value;
			if (SoundOnBootDone == "completion" && _VerbOnBootDone != "ding")
			{
				SoundOnBootDone = null;
				SoundVolumeOnBootDone = 1f;
			}
		}
	}

	public string VerbOnBootAborted
	{
		get
		{
			return _VerbOnBootAborted;
		}
		set
		{
			_VerbOnBootAborted = value;
			if (SoundOnBootAborted == "shutdown" && _VerbOnBootAborted != "bloop")
			{
				SoundOnBootAborted = null;
				SoundVolumeOnBootAborted = 1f;
			}
		}
	}

	public BootSequence()
	{
		IsBootSensitive = false;
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		BootSequence bootSequence = p as BootSequence;
		if (bootSequence.BootTime != BootTime)
		{
			return false;
		}
		if (bootSequence.VariableBootTime != VariableBootTime)
		{
			return false;
		}
		if (bootSequence.ReadoutInName != ReadoutInName)
		{
			return false;
		}
		if (bootSequence.ReadoutInDescription != ReadoutInDescription)
		{
			return false;
		}
		if (bootSequence.AlwaysObvious != AlwaysObvious)
		{
			return false;
		}
		if (bootSequence.ObviousIfUnderstood != ObviousIfUnderstood)
		{
			return false;
		}
		if (bootSequence.TextInDescription != TextInDescription)
		{
			return false;
		}
		if (bootSequence.VerbOnBootInitialized != VerbOnBootInitialized)
		{
			return false;
		}
		if (bootSequence.VerbOnBootDone != VerbOnBootDone)
		{
			return false;
		}
		if (bootSequence.VerbOnBootAborted != VerbOnBootAborted)
		{
			return false;
		}
		if (bootSequence.SoundOnBootInitialized != SoundOnBootInitialized)
		{
			return false;
		}
		if (bootSequence.SoundOnBootDone != SoundOnBootDone)
		{
			return false;
		}
		if (bootSequence.SoundOnBootAborted != SoundOnBootAborted)
		{
			return false;
		}
		if (bootSequence.ComputePowerFactor != ComputePowerFactor)
		{
			return false;
		}
		if (bootSequence.Sensitive != Sensitive)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public bool IsObvious()
	{
		if (AlwaysObvious)
		{
			return true;
		}
		if ((ObviousIfUnderstood || ReadoutInName || ReadoutInDescription) && ParentObject.Understood())
		{
			return true;
		}
		return false;
	}

	private void BootUI(string Verb, string Sound)
	{
		if (!string.IsNullOrEmpty(Verb))
		{
			DidX(Verb);
		}
		if (!string.IsNullOrEmpty(Sound))
		{
			PlayWorldSound(Sound);
		}
	}

	private void ResetBootTime()
	{
		BootTimeLeft = GetAvailableComputePowerEvent.AdjustDown(this, (VariableBootTime != null) ? Math.Max(VariableBootTime.RollCached(), BootTime) : BootTime, ComputePowerFactor);
	}

	private void InitBoot()
	{
		ResetBootTime();
		BootSequenceInitializedEvent.Send(ParentObject);
		SyncRenderEvent.Send(ParentObject);
		BootUI(VerbOnBootInitialized, SoundOnBootInitialized);
		ConsumeCharge();
	}

	private void AbortBoot()
	{
		BootSequenceAbortedEvent.Send(ParentObject);
		SyncRenderEvent.Send(ParentObject);
		BootUI(VerbOnBootAborted, SoundOnBootAborted);
	}

	private void SyncBoot()
	{
		if (PartWasReady && Sensitive)
		{
			AbortBoot();
		}
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (PartWasReady)
			{
				if (!Sensitive)
				{
					AbortBoot();
				}
				PartWasReady = false;
			}
		}
		else
		{
			if (!PartWasReady || Sensitive)
			{
				InitBoot();
			}
			PartWasReady = true;
		}
	}

	public void Reboot()
	{
		if (PartWasReady)
		{
			AbortBoot();
		}
		PartWasReady = IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (PartWasReady)
		{
			InitBoot();
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EndTurnEvent.ID && ID != EnteredCellEvent.ID && ID != EquippedEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != LeftCellEvent.ID && ID != TakenEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		Reboot();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (PartWasReady && !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, (BootTimeLeft > 0) ? ChargeUse : 0, UseChargeIfUnpowered: false, 0L))
		{
			AbortBoot();
			PartWasReady = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		SyncBoot();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		SyncBoot();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		SyncBoot();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		SyncBoot();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		SyncBoot();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		SyncBoot();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, (BootTimeLeft > 0) ? ChargeUse : 0, UseChargeIfUnpowered: false, 0L))
		{
			if (!PartWasReady)
			{
				InitBoot();
			}
			else if (BootTimeLeft > 0)
			{
				if (--BootTimeLeft <= 0)
				{
					BootSequenceDoneEvent.Send(ParentObject);
					BootUI(VerbOnBootDone, SoundOnBootDone);
				}
				ConsumeCharge();
			}
			PartWasReady = true;
		}
		else if (PartWasReady)
		{
			AbortBoot();
			PartWasReady = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (BootTimeLeft > 0 && ReadoutInName && PartWasReady && E.Understood())
		{
			E.AddTag("[{{K|" + BootTimeLeft + " sec}}]", 40);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (BootTimeLeft > 0 && PartWasReady)
		{
			if (!string.IsNullOrEmpty(TextInDescription))
			{
				E.Postfix.Append('\n').Append(TextInDescription);
			}
			if (ReadoutInDescription)
			{
				E.Postfix.Append('\n').Append(ParentObject.Its).Append(" readout indicates that ")
					.Append(ParentObject.its)
					.Append(" startup sequence will take an estimated ")
					.Append(Grammar.Cardinal(BootTimeLeft))
					.Append(" more ")
					.Append((BootTimeLeft == 1) ? "round" : "rounds")
					.Append(".");
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "GameStart");
		Object.RegisterPartEvent(this, "LiquidFueledPowerPlantFueled");
		Object.RegisterPartEvent(this, "ObjectEntered");
		Object.RegisterPartEvent(this, "ObjectExited");
		Object.RegisterPartEvent(this, "Reboot");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Reboot")
		{
			Reboot();
		}
		else if (E.ID == "GameStart")
		{
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, 0, UseChargeIfUnpowered: false, 0L))
			{
				BootTimeLeft = 0;
				BootSequenceDoneEvent.Send(ParentObject);
				PartWasReady = true;
			}
			else
			{
				PartWasReady = false;
			}
		}
		else if (E.ID == "ObjectEntered" || E.ID == "ObjectExited")
		{
			SyncBoot();
		}
		else if (E.ID == "LiquidFueledPowerPlantFueled" && !PartWasReady && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			InitBoot();
			PartWasReady = true;
		}
		return base.FireEvent(E);
	}
}
