using System;
using System.Text;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Wings : BaseDefaultEquipmentMutation, IFlightSource
{
	public GameObject WingsObject;

	public string BodyPartType = "Back";

	public int BaseFallChance = 6;

	public bool _FlightFlying;

	public Guid _FlightActivatedAbilityID = Guid.Empty;

	public int appliedChargeBonus;

	public int appliedJumpBonus;

	public int FlightLevel => base.Level;

	public int FlightBaseFallChance => BaseFallChance;

	public bool FlightRequiresOngoingEffort => true;

	public string FlightEvent => "CommandFlight";

	public string FlightActivatedAbilityClass => "Physical Mutation";

	public string FlightSourceDescription => null;

	public bool FlightFlying
	{
		get
		{
			return _FlightFlying;
		}
		set
		{
			_FlightFlying = value;
		}
	}

	public Guid FlightActivatedAbilityID
	{
		get
		{
			return _FlightActivatedAbilityID;
		}
		set
		{
			_FlightActivatedAbilityID = value;
		}
	}

	public string ManagerID => ParentObject.id + "::Wings";

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Wings obj = base.DeepCopy(Parent, MapInv) as Wings;
		obj.WingsObject = null;
		return obj;
	}

	public Wings()
	{
		DisplayName = "Wings";
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AttemptToLandEvent.ID && ID != BodyPositionChangedEvent.ID && ID != EndTurnEvent.ID && ID != EnteredCellEvent.ID && ID != GetLostChanceEvent.ID && ID != GetItemElementsEvent.ID && ID != MovementModeChangedEvent.ID && ID != ReplicaCreatedEvent.ID)
		{
			return ID == TravelSpeedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetLostChanceEvent E)
	{
		E.PercentageBonus += 36 + 4 * base.Level;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			Flight.SyncFlying(ParentObject, ParentObject, this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TravelSpeedEvent E)
	{
		E.PercentageBonus += 50 + 50 * base.Level;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BodyPositionChangedEvent E)
	{
		if (FlightFlying && E.To != "Flying")
		{
			if (E.Involuntary)
			{
				Flight.FailFlying(ParentObject, ParentObject, this);
			}
			else
			{
				Flight.StopFlying(ParentObject, ParentObject, this);
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
				Flight.FailFlying(ParentObject, ParentObject, this);
			}
			else
			{
				Flight.StopFlying(ParentObject, ParentObject, this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AttemptToLandEvent E)
	{
		if (FlightFlying && Flight.StopFlying(ParentObject, ParentObject, this))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Flight.MaintainFlight(ParentObject, ParentObject, this);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		Flight.CheckFlight(ParentObject, ParentObject, this);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("travel", 1);
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You fly.";
	}

	public float SprintingMoveSpeedBonus(int Level)
	{
		return 0.1f + 0.1f * (float)Level;
	}

	public int GetJumpDistanceBonus(int Level)
	{
		return 1 + Level / 3;
	}

	public int GetChargeDistanceBonus(int Level)
	{
		return 2 + Level / 3;
	}

	public override string GetLevelText(int Level)
	{
		int num = Math.Max(0, FlightBaseFallChance - Level);
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("You travel on the world map at {{rules|").Append(1.5 + 0.5 * (double)Level).Append("x}} speed.\n");
		stringBuilder.Append("{{rules|" + (36 + Level * 4)).Append("%}} reduced chance of becoming lost\n");
		stringBuilder.Append("While outside, you may fly. You cannot be hit in melee by grounded creatures while flying.\n");
		stringBuilder.Append("{{rules|" + num).Append("%}} chance of falling clumsily to the ground\n");
		stringBuilder.Append("{{rules|+" + SprintingMoveSpeedBonus(Level) * 100f + "%}} move speed while sprinting\n");
		stringBuilder.Append("You can jump {{rules|" + GetJumpDistanceBonus(Level) + ((GetJumpDistanceBonus(Level) == 1) ? "}} square" : "}} squares") + " farther.\n");
		stringBuilder.Append("You can charge {{rules|" + GetChargeDistanceBonus(Level) + ((GetChargeDistanceBonus(Level) == 1) ? "}} square" : "}} squares") + " farther.\n");
		stringBuilder.Append("+300 reputation with {{w|birds}} and {{w|winged mammals}}");
		return stringBuilder.ToString();
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "AIGetPassiveMutationList");
		Object.RegisterPartEvent(this, "CommandFlight");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandFlight")
		{
			if (IsMyActivatedAbilityToggledOn(FlightActivatedAbilityID))
			{
				if (ParentObject.IsPlayer() && base.currentCell != null && ParentObject.GetEffectCount("Flying") <= 1)
				{
					int i = 0;
					for (int count = base.currentCell.Objects.Count; i < count; i++)
					{
						GameObject gameObject = base.currentCell.Objects[i];
						if (gameObject.GetPart("StairsDown") is StairsDown stairsDown && stairsDown.IsLongFall() && Popup.ShowYesNo("It looks like a long way down " + gameObject.t() + " you're above. Are you sure you want to stop flying?") != 0)
						{
							return false;
						}
					}
				}
				Flight.StopFlying(ParentObject, ParentObject, this);
			}
			else
			{
				Flight.StartFlying(ParentObject, ParentObject, this);
			}
		}
		else if ((E.ID == "AIGetOffensiveMutationList" || E.ID == "AIGetPassiveMutationList") && !FlightFlying && Flight.EnvironmentAllowsFlight(ParentObject) && Flight.IsAbilityAIUsable(this, ParentObject))
		{
			E.AddAICommand(FlightEvent);
		}
		return base.FireEvent(E);
	}

	public override void OnRegenerateDefaultEquipment(Body body)
	{
		BodyPart bodyPart;
		if (!HasRegisteredSlot(BodyPartType))
		{
			bodyPart = body.GetFirstPart(BodyPartType, EvenIfDismembered: true) ?? AddBodyPart(body);
			if (bodyPart != null)
			{
				RegisterSlot(BodyPartType, bodyPart);
			}
		}
		else
		{
			bodyPart = GetRegisteredSlot(BodyPartType, evenIfDismembered: false);
		}
		if (bodyPart != null)
		{
			bodyPart.Description = "Worn around Wings";
			bodyPart.DescriptionPrefix = null;
			bodyPart.DefaultBehavior = GameObject.create("Wings");
		}
	}

	public BodyPart AddBodyPart(Body Body)
	{
		BodyPart body = Body.GetBody();
		return body.AddPartAt(BodyPartType, 0, null, null, null, null, Category: body.Category, Manager: ManagerID, RequiresLaterality: null, Mobility: null, Appendage: null, Integral: null, Mortal: null, Abstract: null, Extrinsic: null, Plural: null, Mass: null, Contact: null, IgnorePosition: null, InsertAfter: "Head", OrInsertBefore: new string[3] { "Arm", "Missile Weapon", "Hands" });
	}

	public override bool ChangeLevel(int NewLevel)
	{
		if (appliedChargeBonus > 0)
		{
			ParentObject.ModIntProperty("ChargeRangeModifier", -appliedChargeBonus);
		}
		if (appliedJumpBonus > 0)
		{
			ParentObject.ModIntProperty("JumpRangeModifier", -appliedJumpBonus);
		}
		appliedChargeBonus = GetChargeDistanceBonus(NewLevel);
		appliedJumpBonus = GetJumpDistanceBonus(NewLevel);
		ParentObject.ModIntProperty("ChargeRangeModifier", appliedChargeBonus);
		ParentObject.ModIntProperty("JumpRangeModifier", appliedJumpBonus);
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		Flight.AbilitySetup(GO, GO, this);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.ModIntProperty("ChargeRangeModifier", -appliedChargeBonus, RemoveIfZero: true);
		GO.ModIntProperty("JumpRangeModifier", -appliedJumpBonus, RemoveIfZero: true);
		GO.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		appliedChargeBonus = 0;
		appliedJumpBonus = 0;
		Flight.AbilityTeardown(GO, GO, this);
		return base.Unmutate(GO);
	}
}
