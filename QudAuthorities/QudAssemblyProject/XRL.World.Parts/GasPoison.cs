using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasPoison : IPart
{
	public string GasType = "Poison";

	public int GasLevel = 1;

	public override bool SameAs(IPart p)
	{
		GasPoison gasPoison = p as GasPoison;
		if (gasPoison.GasType != GasType)
		{
			return false;
		}
		if (gasPoison.GasLevel != GasLevel)
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
			if (IsAffectable(E.Actor))
			{
				E.MinWeight(GasDensityStepped() / 2 + GasLevel * 10, Math.Min(50 + GasLevel * 10, 80));
			}
		}
		else
		{
			E.MinWeight(8);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		ApplyPoisonGas(E.Object);
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
		ApplyPoisonGas();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		ApplyPoisonGas();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		ApplyPoisonGas();
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

	public void ApplyPoisonGas()
	{
		ApplyPoisonGas(ParentObject.CurrentCell);
	}

	public void ApplyPoisonGas(Cell C)
	{
		if (C == null)
		{
			return;
		}
		List<GameObject> list = C.Objects;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (ApplyPoisonGas(list[i]) && list == C.Objects)
			{
				list = Event.NewGameObjectList();
				list.AddRange(C.Objects);
				count = C.Objects.Count;
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
		if (Object.FireEvent("CanApplyPoisonGasPoison") && CanApplyEffectEvent.Check(Object, "PoisonGasPoison"))
		{
			return Object.PhaseMatches(ParentObject);
		}
		return false;
	}

	public bool ApplyPoisonGas(GameObject GO)
	{
		if (GO == ParentObject)
		{
			return false;
		}
		if (!GO.Respires)
		{
			return false;
		}
		if (!GO.HasTag("Creature"))
		{
			return false;
		}
		Gas gas = ParentObject.GetPart("Gas") as Gas;
		if (!IsAffectable(GO, gas))
		{
			return false;
		}
		int @for = GetRespiratoryAgentPerformanceEvent.GetFor(GO, ParentObject, gas);
		if (@for <= 0)
		{
			return false;
		}
		GO.RemoveEffect("PoisonGasPoison");
		PoisonGasPoison poisonGasPoison = new PoisonGasPoison(Stat.Random(1, 10), gas.Creator);
		poisonGasPoison.Damage = GasLevel * 2;
		GO.ApplyEffect(poisonGasPoison);
		int amount = (int)Math.Max(Math.Floor((double)(@for + 1) / 20.0), 1.0);
		return GO.TakeDamage(amount, "from %t {{g|poison}}!", "Poison Gas", null, null, null, gas.Creator);
	}
}
