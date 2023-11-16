using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasStun : IPart
{
	public string GasType = "Stun";

	public override bool SameAs(IPart p)
	{
		if ((p as GasStun).GasType != GasType)
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
				E.MinWeight(StepValue(gas.Density) / 2 + 1, 51);
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
		if (E.Object != ParentObject && E.Object.PhaseMatches(ParentObject))
		{
			ApplyStun(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "DensityChange");
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
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
		if (Object.FireEvent("CanApplyStunGasStun") && CanApplyEffectEvent.Check(Object, "StunGasStun"))
		{
			return Object.PhaseMatches(ParentObject);
		}
		return false;
	}

	public void ApplyStun(GameObject GO)
	{
		if (GameObject.validate(ref GO) && GO.Respires && IsAffectable(GO))
		{
			int @for = GetRespiratoryAgentPerformanceEvent.GetFor(GO, ParentObject);
			if (@for > 0)
			{
				GO.RemoveEffect("StunGasStun");
				GO.ApplyEffect(new StunGasStun(@for));
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				foreach (GameObject item in cell.GetObjectsWithPartReadonly("Brain"))
				{
					if (item != ParentObject && item.PhaseMatches(ParentObject))
					{
						ApplyStun(item);
					}
				}
			}
		}
		else if (E.ID == "DensityChange" && StepValue(E.GetIntParameter("OldValue")) != StepValue(E.GetIntParameter("NewValue")))
		{
			FlushNavigationCaches();
		}
		return base.FireEvent(E);
	}
}
