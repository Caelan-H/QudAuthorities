using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ReflectProjectiles : IPoweredPart
{
	public int Chance = 100;

	public string RetroVariance = "0";

	public string Verb = "reflect";

	public ReflectProjectiles()
	{
		ChargeUse = 0;
		IsEMPSensitive = true;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "ApplyEMP");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyEMP" && IsEMPSensitive && ParentObject.HasEffect("ProjectileReflectionShield"))
		{
			ParentObject.RemoveEffect("ProjectileReflectionShield");
			DidX("have", ParentObject.its + " reflective shield deactivated", null, null, null, ParentObject);
		}
		if (E.ID == "BeginTakeAction")
		{
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				if (!ParentObject.HasEffect("ProjectileReflectionShield"))
				{
					ParentObject.ApplyEffect(new ProjectileReflectionShield(Chance, RetroVariance, Verb));
					DidX("activate", ParentObject.its + " reflective shield", null, null, ParentObject);
				}
			}
			else if (ParentObject.HasEffect("ProjectileReflectionShield"))
			{
				ParentObject.RemoveEffect("ProjectileReflectionShield");
				DidX("have", ParentObject.its + " reflective shield deactivated", null, null, null, ParentObject);
			}
		}
		return base.FireEvent(E);
	}
}
