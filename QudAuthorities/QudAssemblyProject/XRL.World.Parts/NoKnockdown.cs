using System;

namespace XRL.World.Parts;

[Serializable]
public class NoKnockdown : IActivePart
{
	public NoKnockdown()
	{
		WorksOnSelf = true;
		IsBreakageSensitive = false;
		IsRustSensitive = false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyProne");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyProne" && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
