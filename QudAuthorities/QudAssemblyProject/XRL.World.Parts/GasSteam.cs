using System;

namespace XRL.World.Parts;

[Serializable]
public class GasSteam : IPart
{
	public string GasType = "Steam";

	public override bool SameAs(IPart p)
	{
		if ((p as GasSteam).GasType != GasType)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID && ID != GeneralAmnestyEvent.ID && ID != GetNavigationWeightEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null)
		{
			foreach (GameObject item in Event.NewGameObjectList(cell.Objects))
			{
				ApplySteam(item);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (E.Smart)
		{
			E.Uncacheable = true;
			if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || E.Actor.PhaseMatches(ParentObject)))
			{
				int num = GasDensityStepped() / 2 + 15;
				if (E.Actor != null)
				{
					int num2 = E.Actor.Stat("HeatResistance");
					if (num2 != 0)
					{
						num = num * (100 - num2) / 100;
					}
				}
				E.MinWeight(num, 65);
			}
		}
		else
		{
			E.MinWeight(5);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		ApplySteam(E.Object);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "DensityChange");
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public void ApplySteam(GameObject GO)
	{
		if (GameObject.validate(ref GO) && GO != ParentObject)
		{
			Gas gas = ParentObject.GetPart("Gas") as Gas;
			if (CheckGasCanAffectEvent.Check(GO, ParentObject, gas) && GO.PhaseAndFlightMatches(ParentObject) && GO.GetIntProperty("Inorganic") == 0 && (GO.HasTag("Creature") || GO.HasPart("Food")))
			{
				Damage damage = new Damage((int)Math.Max(Math.Ceiling(0.18f * (float)gas.Density), 1.0));
				damage.AddAttribute("Heat");
				damage.AddAttribute("Steam");
				damage.AddAttribute("NoBurn");
				Event @event = Event.New("TakeDamage");
				@event.SetParameter("Damage", damage);
				@event.SetParameter("Owner", gas.Creator);
				@event.SetParameter("Attacker", gas.Creator);
				@event.SetParameter("Message", "from %o scalding steam!");
				GO.FireEvent(@event);
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DensityChange" && StepValue(E.GetIntParameter("OldValue")) != StepValue(E.GetIntParameter("NewValue")))
		{
			FlushNavigationCaches();
		}
		return base.FireEvent(E);
	}
}
