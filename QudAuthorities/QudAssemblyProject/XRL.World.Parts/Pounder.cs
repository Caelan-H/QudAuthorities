using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class Pounder : IActivePart
{
	public int TurnsPerBonus = 1;

	public int TurnsNextToTarget;

	public Pounder()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
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
		CheckTargetAdjacency();
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		int pounderBonus = GetPounderBonus();
		E.Postfix.AppendRules("Pounder: Receives +1 to its to-hit and penetration rolls for every " + TurnsPerBonus.Things("turn") + " " + (ParentObject.IsCreature ? ParentObject.itis : (ParentObject.its + " wielder is")) + " next to " + (ParentObject.IsCreature ? ParentObject.its : "their") + " target. (currently " + ((pounderBonus > 0) ? ("+" + pounderBonus) : (pounderBonus.ToString() ?? "")) + ")", base.AddStatusSummary);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AttackerGetWeaponPenModifier");
		Object.RegisterPartEvent(this, "AttackerRollMeleeToHit");
		Object.RegisterPartEvent(this, "GetWeaponPenModifier");
		Object.RegisterPartEvent(this, "RollMeleeToHit");
		base.Register(Object);
	}

	public int GetPounderBonus()
	{
		return TurnsNextToTarget / TurnsPerBonus;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "RollMeleeToHit" || E.ID == "AttackerRollMeleeToHit")
		{
			if (WasReady())
			{
				E.SetParameter("Result", E.GetIntParameter("Result") + GetPounderBonus());
			}
		}
		else if ((E.ID == "GetWeaponPenModifier" || E.ID == "AttackerGetWeaponPenModifier") && WasReady())
		{
			E.SetParameter("Penetrations", E.GetIntParameter("Penetrations") + GetPounderBonus());
		}
		return base.FireEvent(E);
	}

	public void CheckTargetAdjacency()
	{
		GameObject gameObject = ParentObject.Equipped ?? ParentObject;
		GameObject target = gameObject.Target;
		if (target == null || gameObject.DistanceTo(target) > 1 || !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			TurnsNextToTarget = 0;
		}
		else
		{
			TurnsNextToTarget++;
		}
	}
}
