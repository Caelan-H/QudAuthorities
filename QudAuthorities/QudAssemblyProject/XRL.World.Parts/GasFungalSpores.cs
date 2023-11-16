using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasFungalSpores : IPart
{
	public string Infection = "LuminousInfection";

	public string GasType = "FungalSpores";

	public int GasLevel = 1;

	public override bool SameAs(IPart p)
	{
		GasFungalSpores gasFungalSpores = p as GasFungalSpores;
		if (gasFungalSpores.Infection != Infection)
		{
			return false;
		}
		if (gasFungalSpores.GasType != GasType)
		{
			return false;
		}
		if (gasFungalSpores.GasLevel != GasLevel)
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
			if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || (E.Actor != ParentObject.GetPart<Gas>()?.Creator && E.Actor.FireEvent("CanApplySpores") && !E.Actor.HasEffect("FungalSporeInfection") && E.Actor.PhaseMatches(ParentObject))))
			{
				E.MinWeight(GasDensityStepped() / 2 + 20, 80);
			}
		}
		else
		{
			E.MinWeight(10);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		ApplyFungalSpores(E.Object);
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
		ApplyFungalSpores();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		ApplyFungalSpores();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		ApplyFungalSpores();
	}

	public override bool AllowStaticRegistration()
	{
		return true;
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

	public void ApplyFungalSpores()
	{
		ApplyFungalSpores(ParentObject.CurrentCell);
	}

	public void ApplyFungalSpores(Cell C)
	{
		if (C == null)
		{
			return;
		}
		List<GameObject> list = C.Objects;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (ApplyFungalSpores(list[i]) && list == C.Objects)
			{
				list = Event.NewGameObjectList();
				list.AddRange(C.Objects);
				count = C.Objects.Count;
			}
		}
	}

	public bool ApplyFungalSpores(GameObject GO)
	{
		if (GO == ParentObject)
		{
			return false;
		}
		GameObject gameObject = null;
		Gas gas = ParentObject.GetPart("Gas") as Gas;
		if (gas != null)
		{
			gameObject = gas.Creator;
		}
		if (GO == gameObject)
		{
			return false;
		}
		if (!CheckGasCanAffectEvent.Check(GO, ParentObject, gas))
		{
			return false;
		}
		if (!GO.FireEvent("CanApplySpores"))
		{
			return false;
		}
		if (!GO.FireEvent("ApplySpores"))
		{
			return false;
		}
		if (GO.HasTagOrProperty("ImmuneToFungus"))
		{
			return false;
		}
		if (!GO.PhaseMatches(ParentObject))
		{
			return false;
		}
		GO.RemoveEffect("SporeCloudPoison");
		if (!GO.HasStat("Toughness"))
		{
			return false;
		}
		if (GO.HasEffect("FungalSporeInfection"))
		{
			return false;
		}
		bool result = false;
		SporeCloudPoison sporeCloudPoison = GO.GetEffect("SporeCloudPoison") as SporeCloudPoison;
		if (Infection == "PaxInfection")
		{
			GasLevel = 8;
		}
		else
		{
			int num = Stat.Random(2, 5);
			bool num2 = sporeCloudPoison != null;
			if (sporeCloudPoison == null)
			{
				sporeCloudPoison = new SporeCloudPoison(num, gas?.Creator);
			}
			else
			{
				if (sporeCloudPoison.Duration < num)
				{
					sporeCloudPoison.Duration = num;
				}
				if (!GameObject.validate(ref sporeCloudPoison.Owner) && gas != null && gas.Creator != null)
				{
					sporeCloudPoison.Owner = gas.Creator;
				}
			}
			sporeCloudPoison.Damage = Math.Min(1, GasLevel);
			if (num2 || GO.ApplyEffect(sporeCloudPoison))
			{
				result = true;
			}
		}
		int difficulty = 10 + GasLevel / 3;
		if (!GO.MakeSave("Toughness", difficulty, null, null, "Fungal Disease Contact Gas", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
		{
			if (GO.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your skin itches.");
			}
			Event @event = new Event("BeforeApplyFungalInfection");
			bool flag = false;
			if (!GO.FireEvent(@event) || @event.HasParameter("Cancelled"))
			{
				flag = true;
			}
			FungalSporeInfection fungalSporeInfection;
			if (Infection == "PaxInfection")
			{
				fungalSporeInfection = new FungalSporeInfection(3, Infection);
			}
			else
			{
				fungalSporeInfection = new FungalSporeInfection(flag ? Stat.Random(8, 10) : (Stat.Random(20, 30) * 120), Infection);
				fungalSporeInfection.Fake = flag;
			}
			if (GO.ApplyEffect(fungalSporeInfection))
			{
				result = true;
			}
		}
		return result;
	}
}
