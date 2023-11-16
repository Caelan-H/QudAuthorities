using System;

namespace XRL.World.Parts;

[Serializable]
public class IntPropertyChanger : IPoweredPart
{
	public string _AffectedProperty;

	public string AfterUpdateEvent;

	public string BehaviorDescription;

	public int Amount = 1;

	public int AmountApplied = int.MinValue;

	public int PowerLoadBonusDivisor = 150;

	public bool Applied;

	public string AffectedProperty
	{
		get
		{
			return _AffectedProperty;
		}
		set
		{
			if (NameForStatus == null || NameForStatus == _AffectedProperty)
			{
				NameForStatus = value;
			}
			_AffectedProperty = value;
		}
	}

	public IntPropertyChanger()
	{
		MustBeUnderstood = true;
		WorksOnWearer = true;
	}

	public override bool SameAs(IPart p)
	{
		IntPropertyChanger intPropertyChanger = p as IntPropertyChanger;
		if (intPropertyChanger.AffectedProperty != AffectedProperty)
		{
			return false;
		}
		if (intPropertyChanger.AfterUpdateEvent != AfterUpdateEvent)
		{
			return false;
		}
		if (intPropertyChanger.Amount != Amount)
		{
			return false;
		}
		if (intPropertyChanger.BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeUnequippedEvent.ID && ID != BootSequenceDoneEvent.ID && ID != BootSequenceInitializedEvent.ID && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EquippedEvent.ID)
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
			E.Postfix.AppendRules(BehaviorDescription, base.AddStatusSummary);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckApply(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeUnequippedEvent E)
	{
		Unapply(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckApply();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckApply();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		CheckApply();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		CheckApply();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		CheckApply();
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
		CheckApply(null, !base.OnWorldMap);
	}

	public override void TenTurnTick(long TurnNumber)
	{
		CheckApply(null, !base.OnWorldMap, 10);
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		CheckApply(null, !base.OnWorldMap, 100);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ExamineSuccess");
		base.Register(Object);
	}

	private void Apply(GameObject GO)
	{
		if (Applied)
		{
			return;
		}
		if (GO != null)
		{
			int num = Amount;
			if (IsPowerLoadSensitive)
			{
				int num2 = MyPowerLoadBonus(int.MinValue, 100, PowerLoadBonusDivisor);
				if (num2 != 0)
				{
					num = ((num < 0) ? (num - num2) : (num + num2));
				}
			}
			GO.ModIntProperty(AffectedProperty, num, RemoveIfZero: true);
			AmountApplied = num;
			if (!string.IsNullOrEmpty(AfterUpdateEvent))
			{
				GO.FireEvent(AfterUpdateEvent);
			}
		}
		Applied = true;
	}

	private void Unapply(GameObject GO)
	{
		if (!Applied)
		{
			return;
		}
		if (GO != null)
		{
			int num = AmountApplied;
			if (num == int.MinValue)
			{
				num = Amount;
			}
			GO.ModIntProperty(AffectedProperty, -num, RemoveIfZero: true);
			if (!string.IsNullOrEmpty(AfterUpdateEvent))
			{
				GO.FireEvent(AfterUpdateEvent);
			}
		}
		Applied = false;
	}

	private void CheckApply(GameObject GO = null, bool ConsumeCharge = false, int Turns = 1)
	{
		if (Applied)
		{
			if (IsDisabled(ConsumeCharge, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L))
			{
				Unapply(GO ?? ParentObject.Equipped);
			}
		}
		else if (IsReady(ConsumeCharge, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L))
		{
			Apply(GO ?? ParentObject.Equipped);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ExamineSuccess")
		{
			CheckApply();
		}
		return base.FireEvent(E);
	}
}
