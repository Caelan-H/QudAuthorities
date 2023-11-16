using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasAsh : IPart
{
	public string GasType = "Ash";

	public int GasLevel = 1;

	public override bool SameAs(IPart p)
	{
		GasAsh gasAsh = p as GasAsh;
		if (gasAsh.GasType != GasType)
		{
			return false;
		}
		if (gasAsh.GasLevel != GasLevel)
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
			if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || (E.Actor.FireEvent("CanApplyAshPoison") && E.Actor.PhaseMatches(ParentObject))))
			{
				E.MinWeight(GasDensityStepped() / 2 + 5, 70);
			}
		}
		else
		{
			E.MinWeight(30);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		UpdateOpacity();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		ApplyAsh(E.Object);
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
		ApplyAsh();
		UpdateOpacity();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		ApplyAsh();
		UpdateOpacity();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		ApplyAsh();
		UpdateOpacity();
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

	public void ApplyAsh()
	{
		ApplyAsh(ParentObject.CurrentCell);
	}

	public void ApplyAsh(Cell C)
	{
		if (C == null)
		{
			return;
		}
		List<GameObject> list = C.Objects;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (ApplyAsh(list[i]) && list == C.Objects)
			{
				list = Event.NewGameObjectList();
				list.AddRange(C.Objects);
				count = C.Objects.Count;
			}
		}
	}

	public bool ApplyAsh(GameObject GO)
	{
		if (GO == ParentObject)
		{
			return false;
		}
		if (!GO.Respires)
		{
			return false;
		}
		Gas gas = ParentObject.GetPart("Gas") as Gas;
		if (!CheckGasCanAffectEvent.Check(GO, ParentObject, gas))
		{
			return false;
		}
		if (!GO.FireEvent("CanApplyAshPoison"))
		{
			return false;
		}
		if (!CanApplyEffectEvent.Check(GO, "AshPoison"))
		{
			return false;
		}
		if (!GO.PhaseMatches(ParentObject))
		{
			return false;
		}
		int @for = GetRespiratoryAgentPerformanceEvent.GetFor(GO, ParentObject, gas);
		if (@for <= 0)
		{
			return false;
		}
		GO.RemoveEffect("AshPoison");
		AshPoison ashPoison = new AshPoison(Stat.Random(1, 10), gas.Creator);
		ashPoison.Damage = GasLevel * 2;
		GO.ApplyEffect(ashPoison);
		int amount = (int)Math.Max(Math.Floor((double)(@for + 1) / 10.0), 1.0);
		return GO.TakeDamage(amount, "from %t {{K|choking ash}}!", "Asphyxiation Gas", null, null, null, gas.Creator);
	}

	public void UpdateOpacity()
	{
		bool flag = GasDensity() >= 40;
		if (ParentObject.pRender.Occluding != flag)
		{
			ParentObject.pRender.Occluding = flag;
			ParentObject.CurrentCell?.ClearOccludeCache();
		}
	}
}
