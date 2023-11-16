using System;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCathedra : IPart, IFlightSource
{
	public const string ABL_CMD = "CommandActivateCathedra";

	public const string ABL_CLASS = "Cybernetics";

	public int BaseLevel = 10;

	public int BaseCooldown = 100;

	public Guid ActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private GameObject _User;

	public int FlightLevel => GetLevel();

	public int FlightBaseFallChance => 6;

	public bool FlightRequiresOngoingEffort => true;

	public string FlightEvent => "CommandActivateCathedraFlight";

	public string FlightActivatedAbilityClass => "Cybernetics";

	public string FlightSourceDescription => "Cathedra";

	public bool FlightFlying { get; set; }

	public Guid FlightActivatedAbilityID { get; set; }

	public GameObject User => _User ?? (_User = ParentObject.Implantee);

	public override void SaveData(SerializationWriter Writer)
	{
		base.SaveData(Writer);
		Writer.Write(FlightFlying);
		Writer.Write(FlightActivatedAbilityID);
	}

	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
		FlightFlying = Reader.ReadBoolean();
		FlightActivatedAbilityID = Reader.ReadGuid();
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AttemptToLandEvent.ID && ID != BodyPositionChangedEvent.ID && ID != CommandEvent.ID && ID != EndTurnEvent.ID && ID != GetMaxCarriedWeightEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != MovementModeChangedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		OnImplanted(E.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		OnUnimplanted(E.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandActivateCathedra")
		{
			if (!E.Actor.OnWorldMap())
			{
				Activate(E.Actor);
			}
		}
		else if (E.Command == FlightEvent)
		{
			if (E.Actor.IsActivatedAbilityToggledOn(FlightActivatedAbilityID))
			{
				if (E.Actor.IsPlayer() && E.Actor.CurrentCell != null && E.Actor.GetEffectCount("Flying") <= 1)
				{
					foreach (GameObject @object in E.Actor.CurrentCell.Objects)
					{
						if (@object.GetPart("StairsDown") is StairsDown stairsDown && stairsDown.IsLongFall() && Popup.ShowYesNo("It looks like a long way down " + @object.the + @object.ShortDisplayName + " you're above. Are you sure you want to stop flying?") != 0)
						{
							return false;
						}
					}
				}
				Flight.StopFlying(ParentObject, E.Actor, this);
			}
			else
			{
				Flight.StartFlying(ParentObject, E.Actor, this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (FlightFlying)
		{
			Flight.MaintainFlight(ParentObject, User, this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BodyPositionChangedEvent E)
	{
		if (FlightFlying && E.To != "Flying")
		{
			if (E.Involuntary)
			{
				Flight.FailFlying(ParentObject, E.Object, this);
			}
			else
			{
				Flight.StopFlying(ParentObject, E.Object, this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(MovementModeChangedEvent E)
	{
		if (FlightFlying && E.To != "Flying")
		{
			if (E.Involuntary)
			{
				Flight.FailFlying(ParentObject, E.Object, this);
			}
			else
			{
				Flight.StopFlying(ParentObject, E.Object, this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AttemptToLandEvent E)
	{
		if (FlightFlying && Flight.StopFlying(ParentObject, E.Actor, this))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaxCarriedWeightEvent E)
	{
		E.Weight += 100.0;
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			Flight.CheckFlight(ParentObject, User, this);
		}
		return base.FireEvent(E);
	}

	public virtual void OnImplanted(GameObject Object)
	{
		_User = Object;
		Object.RegisterPartEvent(this, "EnteredCell");
		Flight.AbilitySetup(ParentObject, Object, this);
		CarryingCapacityChangedEvent.Send(Object);
	}

	public virtual void OnUnimplanted(GameObject Object)
	{
		_User = null;
		Object.UnregisterPartEvent(this, "EnteredCell");
		Object.RemoveActivatedAbility(ref ActivatedAbilityID);
		Flight.FailFlying(ParentObject, Object, this);
		Flight.AbilityTeardown(ParentObject, Object, this);
		CarryingCapacityChangedEvent.Send(Object);
	}

	public virtual void Activate(GameObject Actor)
	{
		Actor.CooldownActivatedAbility(ActivatedAbilityID, GetCooldown(Actor));
	}

	public int GetLevel(GameObject Actor = null)
	{
		return GetAvailableComputePowerEvent.AdjustUp(Actor ?? User, BaseLevel);
	}

	public int GetCooldown(GameObject Actor = null)
	{
		return GetAvailableComputePowerEvent.AdjustDown(Actor ?? User, BaseCooldown);
	}
}
