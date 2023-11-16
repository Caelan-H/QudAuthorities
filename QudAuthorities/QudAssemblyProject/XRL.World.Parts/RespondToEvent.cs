using System;

namespace XRL.World.Parts;

[Serializable]
public class RespondToEvent : IPoweredPart
{
	public string EventHandled;

	public RespondToEvent()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as RespondToEvent).EventHandled != EventHandled)
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

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		if (!string.IsNullOrEmpty(EventHandled))
		{
			Object.RegisterPartEvent(this, EventHandled);
		}
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == EventHandled && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
