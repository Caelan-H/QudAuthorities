using System;

namespace XRL.World.Parts;

[Serializable]
public class GasCryo : IPart
{
	public string GasType = "Cryo";

	public override bool SameAs(IPart p)
	{
		if ((p as GasCryo).GasType != GasType)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (E.Smart)
		{
			E.Uncacheable = true;
			if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || E.Actor.PhaseMatches(ParentObject)))
			{
				int num = GasDensityStepped() / 2 + 3;
				if (E.Actor != null)
				{
					int num2 = E.Actor.Stat("ColdResistance");
					if (num2 != 0)
					{
						num = Math.Max(num * (100 - num2) / 100, 0);
					}
				}
				if (num > 0)
				{
					E.MinWeight(num, 53);
				}
			}
		}
		else
		{
			E.MinWeight(3);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "DensityChange");
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "ObjectEnteredCell");
		base.Register(Object);
	}

	public void ApplyCryo(GameObject GO)
	{
		Gas gas = ParentObject.GetPart("Gas") as Gas;
		if (CheckGasCanAffectEvent.Check(GO, ParentObject, gas) && GO.PhaseMatches(ParentObject))
		{
			int num = 0;
			num = (int)Math.Ceiling(2.5f * (float)gas.Density);
			if (GO.pPhysics.Temperature > -num)
			{
				GO.TemperatureChange(-num, ParentObject, Radiant: false, MinAmbient: false, MaxAmbient: false, ParentObject.GetPhase());
			}
			if (GO.IsPlayer() || (GO.pPhysics != null && GO.pPhysics.Temperature > GO.pPhysics.BrittleTemperature))
			{
				Damage damage = new Damage(1);
				damage.AddAttribute("Cold");
				Event @event = Event.New("TakeDamage");
				@event.AddParameter("Damage", damage);
				@event.AddParameter("Owner", ParentObject);
				@event.AddParameter("Attacker", ParentObject);
				@event.AddParameter("Message", "from the {{icy|cryogenic mist}}.");
				GO.FireEvent(@event);
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (ParentObject.CurrentCell != null)
			{
				foreach (GameObject item in ParentObject.CurrentCell.GetObjectsWithPartReadonly("Physics"))
				{
					if (item.pRender != null && item.pRender.RenderLayer > 0 && item != ParentObject)
					{
						ApplyCryo(item);
					}
				}
			}
		}
		else if (E.ID == "DensityChange")
		{
			if (StepValue(E.GetIntParameter("OldValue")) != StepValue(E.GetIntParameter("NewValue")))
			{
				FlushNavigationCaches();
			}
		}
		else if (E.ID == "ObjectEnteredCell")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter != ParentObject)
			{
				ApplyCryo(gameObjectParameter);
			}
		}
		return base.FireEvent(E);
	}
}
