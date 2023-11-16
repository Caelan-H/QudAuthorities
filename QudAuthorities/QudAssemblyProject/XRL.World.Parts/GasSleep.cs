using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasSleep : IPart
{
	public string GasType = "Sleep";

	public override bool SameAs(IPart p)
	{
		if ((p as GasSleep).GasType != GasType)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetNavigationWeightEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (E.Smart)
		{
			E.Uncacheable = true;
			Gas gas = ParentObject.GetPart("Gas") as Gas;
			if (IsAffectable(E.Actor, gas))
			{
				int num = gas.Level * 5;
				E.MinWeight(StepValue(gas.Density) / 2 + num, Math.Min(50 + num, 70));
			}
		}
		else
		{
			E.MinWeight(2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		ApplySleep(E.Object);
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
		ApplySleep();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		ApplySleep();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		ApplySleep();
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "DensityChange");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DensityChange" && StepValue(E.GetIntParameter("OldValue")) != StepValue(E.GetIntParameter("NewValue")))
		{
			FlushNavigationCaches();
		}
		return base.FireEvent(E);
	}

	public void ApplySleep()
	{
		ApplySleep(ParentObject.CurrentCell);
	}

	public void ApplySleep(Cell C)
	{
		if (C != null)
		{
			for (int i = 0; i < C.Objects.Count; i++)
			{
				ApplySleep(C.Objects[i]);
			}
		}
	}

	public bool IsAffectable(GameObject Object, Gas Gas = null)
	{
		if (!CheckGasCanAffectEvent.Check(Object, ParentObject, Gas))
		{
			return false;
		}
		if (Object == null)
		{
			return true;
		}
		if (Object.FireEvent("CanApplySleep") && Object.FireEvent("CanApplySleepGas") && Object.FireEvent("CanApplyInvoluntarySleep") && CanApplyEffectEvent.Check(Object, "Sleep"))
		{
			return Object.PhaseMatches(ParentObject);
		}
		return false;
	}

	public bool ApplySleep(GameObject GO)
	{
		if (GO == ParentObject)
		{
			return false;
		}
		if (GO.pBrain == null)
		{
			return false;
		}
		if (!GO.Respires)
		{
			return false;
		}
		if (GO.HasEffect("Asleep"))
		{
			return false;
		}
		Gas gas = ParentObject.GetPart("Gas") as Gas;
		if (!IsAffectable(GO, gas))
		{
			return false;
		}
		int @for = GetRespiratoryAgentPerformanceEvent.GetFor(GO, ParentObject, gas, null, 0, 0, WillAllowSave: true);
		if (@for > 0 && !GO.MakeSave("Toughness", 5 + gas.Level + @for / 10, null, null, "Sleep Inhaled Gas", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
		{
			return GO.ApplyEffect(new Asleep("4d6".RollCached() + gas.Level));
		}
		return false;
	}
}
