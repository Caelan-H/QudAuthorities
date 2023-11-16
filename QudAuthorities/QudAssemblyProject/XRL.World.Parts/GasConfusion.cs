using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasConfusion : IPart
{
	public string GasType = "Confusion";

	public override bool SameAs(IPart p)
	{
		if ((p as GasConfusion).GasType != GasType)
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
				E.MinWeight(StepValue(gas.Density) / 2 + 20, 60);
			}
		}
		else
		{
			E.MinWeight(3);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		ApplyConfusion(E.Object);
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
		ApplyConfusion();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		ApplyConfusion();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		ApplyConfusion();
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

	public void ApplyConfusion()
	{
		ApplyConfusion(ParentObject.CurrentCell);
	}

	public void ApplyConfusion(Cell C)
	{
		if (C != null)
		{
			int i = 0;
			for (int count = C.Objects.Count; i < count; i++)
			{
				ApplyConfusion(C.Objects[i]);
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
		if (Object.FireEvent("CanApplyConfusion") && Object.FireEvent("CanApplyConfusionGas") && CanApplyEffectEvent.Check(Object, "Confusion"))
		{
			return Object.PhaseMatches(ParentObject);
		}
		return false;
	}

	public void ApplyConfusion(GameObject GO)
	{
		if (GO == ParentObject || GO.pBrain == null || !GO.Respires)
		{
			return;
		}
		Gas gas = ParentObject.GetPart("Gas") as Gas;
		if (IsAffectable(GO, gas))
		{
			int @for = GetRespiratoryAgentPerformanceEvent.GetFor(GO, ParentObject, gas, null, 0, 0, WillAllowSave: true);
			if (@for > 0 && !GO.MakeSave("Toughness", 5 + gas.Level + @for / 10, null, null, "Confusion Inhaled Gas", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				GO.ApplyEffect(new Confused(Stat.Roll("4d6") + gas.Level, gas.Level, gas.Level + 2));
			}
		}
	}
}
