using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasPlasma : IPart
{
	public string GasType = "Plasma";

	public int GasLevel = 1;

	public override bool SameAs(IPart p)
	{
		GasPlasma gasPlasma = p as GasPlasma;
		if (gasPlasma.GasType != GasType)
		{
			return false;
		}
		if (gasPlasma.GasLevel != GasLevel)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != GetNavigationWeightEvent.ID)
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
			if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || (E.Actor.FireEvent("CanApplyCoatedInPlasma") && E.Actor.PhaseMatches(ParentObject))))
			{
				E.MinWeight(GasDensityStepped() / 3 + 40, 90);
			}
		}
		else
		{
			E.MinWeight(70);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		ApplyPlasma(E.Object);
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
		ApplyPlasma();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		ApplyPlasma();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		ApplyPlasma();
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

	public void ApplyPlasma()
	{
		ApplyPlasma(ParentObject.CurrentCell);
	}

	public void ApplyPlasma(Cell C)
	{
		if (C == null)
		{
			return;
		}
		List<GameObject> list = C.Objects;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (ApplyPlasma(list[i]) && list == C.Objects)
			{
				list = Event.NewGameObjectList();
				list.AddRange(C.Objects);
				count = C.Objects.Count;
			}
		}
	}

	public bool ApplyPlasma(GameObject GO)
	{
		if (GO == ParentObject)
		{
			return false;
		}
		Gas gas = ParentObject.GetPart("Gas") as Gas;
		if (!CheckGasCanAffectEvent.Check(GO, ParentObject, gas))
		{
			return false;
		}
		if (!GO.FireEvent("CanApplyCoatedInPlasma"))
		{
			return false;
		}
		if (!CanApplyEffectEvent.Check(GO, "CoatedInPlasma"))
		{
			return false;
		}
		if (!GO.PhaseMatches(ParentObject))
		{
			return false;
		}
		if (gas.Density <= 0)
		{
			return false;
		}
		int num = Stat.Random(gas.Density * 2 / 5, gas.Density * 3 / 5);
		if (num <= 0)
		{
			return false;
		}
		if (GO.GetEffect("CoatedInPlasma") is CoatedInPlasma coatedInPlasma)
		{
			if (coatedInPlasma.Duration < num)
			{
				coatedInPlasma.Duration = num;
			}
			if (!GameObject.validate(ref coatedInPlasma.Owner) && GameObject.validate(gas.Creator))
			{
				coatedInPlasma.Owner = gas.Creator;
			}
			return true;
		}
		CoatedInPlasma e = new CoatedInPlasma(num, gas.Creator);
		return GO.ApplyEffect(e);
	}
}
