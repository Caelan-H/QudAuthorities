using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasShame : IPart
{
	public string GasType = "Shame";

	public override bool SameAs(IPart p)
	{
		if ((p as GasShame).GasType != GasType)
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
				E.MinWeight(StepValue(gas.Density) / 5 + 5, 60);
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
		ApplyShame(E.Object);
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
		ApplyShame();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		ApplyShame();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		ApplyShame();
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

	public void ApplyShame()
	{
		ApplyShame(ParentObject.CurrentCell);
	}

	public void ApplyShame(Cell C)
	{
		if (C != null)
		{
			int i = 0;
			for (int count = C.Objects.Count; i < count; i++)
			{
				ApplyShame(C.Objects[i]);
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
		if (Object.FireEvent("CanApplyShamed") && Object.FireEvent("CanApplyShameGas") && CanApplyEffectEvent.Check(Object, "Shamed"))
		{
			return Object.PhaseMatches(ParentObject);
		}
		return false;
	}

	public void ApplyShame(GameObject GO)
	{
		if (GO == ParentObject || GO.pBrain == null || !GO.Respires)
		{
			return;
		}
		Gas gas = ParentObject.GetPart("Gas") as Gas;
		if (IsAffectable(GO, gas))
		{
			int @for = GetRespiratoryAgentPerformanceEvent.GetFor(GO, ParentObject, gas, null, 0, 0, WillAllowSave: true);
			if (@for > 0 && !GO.MakeSave("Willpower", 5 + gas.Level + @for / 10, null, null, "Shame Inhaled Gas", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				GO.ApplyEffect(new Shamed("2d6".RollCached() + gas.Level * 2));
			}
		}
	}
}
