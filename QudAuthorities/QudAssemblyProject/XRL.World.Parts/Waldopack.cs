using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Waldopack : IPoweredPart
{
	public string ManagerID => ParentObject.id + "::Waldopack";

	public Waldopack()
	{
		WorksOnEquipper = true;
		NameForStatus = "ServoSystems";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Actor.RegisterPartEvent(this, "AttackerQueryWeaponSecondaryAttackChance");
			E.Actor.RegisterPartEvent(this, "Dismember");
			AddArm(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "AttackerQueryWeaponSecondaryAttackChance");
		E.Actor.UnregisterPartEvent(this, "Dismember");
		RemoveArm(E.Actor);
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
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational();
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 10);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 100);
		}
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Dismember");
		base.Register(Object);
	}

	public void AddArm(GameObject Wearer = null)
	{
		if (Wearer == null)
		{
			Wearer = ParentObject.Equipped;
			if (Wearer == null)
			{
				return;
			}
		}
		Body body = Wearer.Body;
		if (body != null)
		{
			body.GetBody().AddPartAt("Servo-Arm", 0, null, null, null, null, ManagerID, null, null, null, null, null, null, null, true, null, null, null, null, "Arm", new string[4] { "Hands", "Feet", "Roots", "Thrown Weapon" }).AddPart("Servo-Claw", 0, null, null, null, null, Extrinsic: true, Manager: ManagerID);
		}
	}

	public void RemoveArm(GameObject Wearer = null)
	{
		if (Wearer == null)
		{
			Wearer = ParentObject.Equipped;
			if (Wearer == null)
			{
				return;
			}
		}
		Wearer.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Dismember")
		{
			if (E.GetParameter("Part") is BodyPart bodyPart && bodyPart.Manager != null && bodyPart.Manager == ManagerID)
			{
				ParentObject.ApplyEffect(new Broken());
				return false;
			}
		}
		else if (E.ID == "AttackerQueryWeaponSecondaryAttackChance" && E.GetParameter("Part") is BodyPart bodyPart2 && bodyPart2.Manager != null && bodyPart2.Manager == ManagerID)
		{
			E.SetParameter("Chance", 8);
		}
		return base.FireEvent(E);
	}
}
