using System;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Robot : IPart
{
	public bool EMPable = true;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ApplyEffectEvent.ID && ID != BeforeDeathRemovalEvent.ID && ID != BeforeApplyDamageEvent.ID && ID != CanApplyEffectEvent.ID && ID != GetItemElementsEvent.ID && ID != GetMaximumLiquidExposureEvent.ID && ID != GetMutationTermEvent.ID && ID != GetScanTypeEvent.ID && ID != IsMutantEvent.ID && ID != IsSensableAsPsychicEvent.ID && ID != RespiresEvent.ID)
		{
			return ID == TransparentToEMPEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetScanTypeEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.ScanType = Scanning.Scan.Tech;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsMutantEvent E)
	{
		E.IsMutant = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		E.Sensable = false;
		return false;
	}

	public override bool HandleEvent(GetMutationTermEvent E)
	{
		E.Term = "module";
		E.Color = "C";
		return false;
	}

	public override bool HandleEvent(RespiresEvent E)
	{
		return false;
	}

	public override bool HandleEvent(TransparentToEMPEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CanApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction += 25;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject && (E.Damage.HasAttribute("Poison") || E.Damage.HasAttribute("Asphyxiation")))
		{
			E.Damage.Amount = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		GameObject gameObject = null;
		int chance = 5;
		if (!ParentObject.HasPart("ExtradimensionalLoot"))
		{
			if (IComponent<GameObject>.ThePlayer.HasSkill("Tinkering_Scavenger"))
			{
				chance = 35;
			}
			if (chance.in100())
			{
				gameObject = GameObjectFactory.create(PopulationManager.RollOneFrom("Scrap " + Tier.Constrain(ParentObject.Stat("Level") / 5)).Blueprint);
			}
		}
		if (gameObject != null && gameObject.pPhysics != null && gameObject.pPhysics.IsReal)
		{
			Cell dropCell = ParentObject.GetDropCell();
			if (dropCell != null)
			{
				gameObject.pPhysics.InInventory = null;
				dropCell.AddObject(gameObject);
				gameObject.HandleEvent(DroppedEvent.FromPool(null, gameObject));
			}
			else
			{
				gameObject.Obliterate();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("circuitry", 2);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIMovingTowardsTarget");
		Object.RegisterPartEvent(this, "ApplyEMP");
		Object.RegisterPartEvent(this, "ApplyingTonic");
		Object.RegisterPartEvent(this, "CanApplyAshPoison");
		Object.RegisterPartEvent(this, "CanApplySpores");
		Object.RegisterPartEvent(this, "CanApplyTonic");
		Object.RegisterPartEvent(this, "HasPowerConnectors");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyingTonic" || E.ID == "CanApplyTonic" || E.ID == "CanApplyAshPoison" || E.ID == "CanApplySpores")
		{
			return false;
		}
		if (E.ID == "HasPowerConnectors")
		{
			return false;
		}
		if (E.ID == "AIMovingTowardsTarget")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			if (gameObjectParameter != null && gameObjectParameter.HasProperty("RobotStop") && !ParentObject.MakeSave("Willpower", 18, null, null, "RobotStop Restraint", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, gameObjectParameter))
			{
				if (Visible())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("come") + " to a complete stop.");
					ParentObject.ParticleBlip("&rX");
				}
				return false;
			}
		}
		else if (E.ID == "ApplyEMP" && EMPable)
		{
			Brain pBrain = ParentObject.pBrain;
			pBrain.Goals.Clear();
			pBrain.PushGoal(new Dormant(E.GetIntParameter("Duration")));
		}
		return base.FireEvent(E);
	}

	private bool Check(IEffectCheckEvent E)
	{
		if (E.Name == "AshPoison" || E.Name == "CardiacArrest" || E.Name == "Confusion" || E.Name == "CyberneticRejectionSyndrome" || E.Name == "Disease" || E.Name == "DiseaseOnset" || E.Name == "Poison" || E.Name == "PoisonGasPoison" || E.Name == "ShatterMentalArmor")
		{
			return false;
		}
		return true;
	}
}
