using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ForceBubble : BaseMutation
{
	public string Blueprint = "Forcefield";

	public bool IsRealityDistortionBased = true;

	public new Guid ActivatedAbilityID;

	public int Duration;

	[NonSerialized]
	public Dictionary<string, GameObject> CurrentField = new Dictionary<string, GameObject>(8);

	[NonSerialized]
	public static List<string> toRemove = new List<string>();

	public ForceBubble()
	{
		DisplayName = "Force Bubble";
		Type = "Mental";
	}

	public void Validate()
	{
		toRemove.Clear();
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			if (item.Value == null || item.Value.IsInvalid() || item.Value.IsInGraveyard())
			{
				toRemove.Add(item.Key);
			}
		}
		foreach (string item2 in toRemove)
		{
			CurrentField.Remove(item2);
		}
	}

	public bool IsActive()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			return CurrentField.Count > 0;
		}
		return false;
	}

	public bool IsSuspended()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			if (CurrentField.Count <= 0)
			{
				return false;
			}
			foreach (KeyValuePair<string, GameObject> item in CurrentField)
			{
				if (item.Value.CurrentCell != null)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public bool IsAnySuspended()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			if (CurrentField.Count <= 0)
			{
				return false;
			}
			foreach (KeyValuePair<string, GameObject> item in CurrentField)
			{
				if (item.Value.CurrentCell == null)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public void DestroyBubble()
	{
		Validate();
		MyActivatedAbility(ActivatedAbilityID).ToggleState = false;
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			item.Value.Obliterate();
		}
		CurrentField.Clear();
	}

	public int GetPushForce()
	{
		return 5000 + base.Level * 500;
	}

	public int CreateBubble()
	{
		Validate();
		int num = 0;
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return num;
		}
		if (cell.ParentZone.IsWorldMap())
		{
			return num;
		}
		Event @event = (IsRealityDistortionBased ? Event.New("CheckRealityDistortionAccessibility") : null);
		string[] directionList = Directions.DirectionList;
		foreach (string text in directionList)
		{
			Cell cellFromDirection = cell.GetCellFromDirection(text, BuiltOnly: false);
			if (CurrentField.ContainsKey(text))
			{
				GameObject gameObject = CurrentField[text];
				if (gameObject.CurrentCell == cellFromDirection)
				{
					continue;
				}
				gameObject.Obliterate();
				CurrentField.Remove(text);
			}
			if (cellFromDirection != null && @event != null && !cellFromDirection.FireEvent(@event))
			{
				continue;
			}
			GameObject gameObject2 = GameObject.create(Blueprint);
			Forcefield forcefield = gameObject2.GetPart("Forcefield") as Forcefield;
			if (forcefield != null)
			{
				forcefield.Creator = ParentObject;
				forcefield.MovesWithOwner = true;
				forcefield.RejectOwner = false;
			}
			gameObject2.RequirePart<ExistenceSupport>().SupportedBy = ParentObject;
			Phase.carryOver(ParentObject, gameObject2);
			CurrentField.Add(text, gameObject2);
			cellFromDirection?.AddObject(gameObject2);
			if (cellFromDirection == null || gameObject2.CurrentCell != cellFromDirection)
			{
				continue;
			}
			num++;
			foreach (GameObject item in cellFromDirection.GetObjectsWithPartReadonly("Physics"))
			{
				if (item != gameObject2 && item.pPhysics.Solid && (forcefield == null || !forcefield.CanPass(item)) && !item.HasPart("Forcefield") && !item.HasPart("HologramMaterial") && item.PhaseMatches(gameObject2))
				{
					item.pPhysics.Push(text, GetPushForce(), 4);
				}
			}
			foreach (GameObject item2 in cellFromDirection.GetObjectsWithPartReadonly("Combat"))
			{
				if (item2 != gameObject2 && item2.pPhysics != null && (forcefield == null || !forcefield.CanPass(item2)) && !item2.HasPart("HologramMaterial") && item2.PhaseMatches(gameObject2))
				{
					item2.pPhysics.Push(text, GetPushForce(), 4);
				}
			}
		}
		return num;
	}

	public void SuspendBubble()
	{
		Validate();
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			item.Value.RemoveFromContext();
		}
	}

	public void DesuspendBubble(bool Validated = false)
	{
		if (!Validated)
		{
			Validate();
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			DestroyBubble();
		}
		else
		{
			if (cell.ParentZone != null && cell.ParentZone.IsWorldMap())
			{
				return;
			}
			toRemove.Clear();
			foreach (KeyValuePair<string, GameObject> item in CurrentField)
			{
				string key = item.Key;
				GameObject value = item.Value;
				if (value.CurrentCell != null)
				{
					continue;
				}
				Cell cellFromDirection = cell.GetCellFromDirection(key, BuiltOnly: false);
				if (cellFromDirection == null)
				{
					continue;
				}
				cellFromDirection.AddObject(value);
				Forcefield part = value.GetPart<Forcefield>();
				if (value.CurrentCell == cellFromDirection)
				{
					foreach (GameObject item2 in cellFromDirection.GetObjectsWithPartReadonly("Physics"))
					{
						if (item2 != value && item2.pPhysics.Solid && (part == null || !part.CanPass(item2)) && !item2.HasPart("Forcefield") && !item2.HasPart("HologramMaterial") && item2.PhaseMatches(value))
						{
							item2.Push(key, GetPushForce(), 4);
						}
					}
					foreach (GameObject item3 in cellFromDirection.GetObjectsWithPartReadonly("Combat"))
					{
						if (item3 != value && item3.pPhysics != null && (part == null || !part.CanPass(item3)) && !item3.HasPart("HologramMaterial") && item3.PhaseMatches(value))
						{
							item3.Push(key, GetPushForce(), 4);
						}
					}
				}
				else
				{
					value.Obliterate();
					toRemove.Add(key);
				}
			}
			foreach (string item4 in toRemove)
			{
				CurrentField.Remove(item4);
			}
		}
	}

	public void MaintainBubble()
	{
		foreach (GameObject value in CurrentField.Values)
		{
			Phase.sync(ParentObject, value);
		}
	}

	public override void SaveData(SerializationWriter Writer)
	{
		Writer.Write(CurrentField.Count);
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			Writer.Write(item.Key);
			Writer.WriteGameObject(item.Value);
		}
		base.SaveData(Writer);
	}

	public override void LoadData(SerializationReader Reader)
	{
		CurrentField.Clear();
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string key = Reader.ReadString();
			GameObject value = Reader.ReadGameObject("forcebubble");
			CurrentField.Add(key, value);
		}
		base.LoadData(Reader);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CheckExistenceSupportEvent.ID && ID != EndTurnEvent.ID)
		{
			return ID == OnDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (CurrentField.ContainsValue(E.Object))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (IsActive())
		{
			DesuspendBubble(Validated: true);
			Duration--;
			if (Duration <= 0)
			{
				DestroyBubble();
			}
			else
			{
				MaintainBubble();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		DestroyBubble();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeginMove");
		Object.RegisterPartEvent(this, "CommandForceBubble");
		Object.RegisterPartEvent(this, "EnteredCell");
		Object.RegisterPartEvent(this, "MoveFailed");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You generate a force field around your person.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("Creates a 3x3 force field centered around yourself\n" + "Duration: {{rules|" + (9 + Level * 3) + "}} rounds\n", "Cooldown: 100 rounds\n"), "You may fire missile weapons through the force field.");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && !IsMyActivatedAbilityToggledOn(ActivatedAbilityID) && (!IsRealityDistortionBased || ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this))))
			{
				if (ParentObject.pPhysics.CurrentCell.AnyAdjacentCell((Cell c) => c.GetCombatObject() != null && ParentObject.pBrain != null && ParentObject.pBrain.GetOpinion(c.GetCombatObject()) == Brain.CreatureOpinion.allied))
				{
					return true;
				}
				E.AddAICommand("CommandForceBubble");
			}
		}
		else if (E.ID == "CommandForceBubble")
		{
			if (IsActive())
			{
				DestroyBubble();
			}
			else
			{
				if (IsRealityDistortionBased && !ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this), E))
				{
					return false;
				}
				CooldownMyActivatedAbility(ActivatedAbilityID, 100);
				MyActivatedAbility(ActivatedAbilityID).ToggleState = true;
				Duration = 9 + base.Level * 3 + 1;
				CreateBubble();
				UseEnergy(1000, "Mental Mutation Force Bubble");
			}
		}
		else if (E.ID == "BeginMove")
		{
			SuspendBubble();
		}
		else if (E.ID == "EnteredCell" || E.ID == "MoveFailed")
		{
			DesuspendBubble();
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Force Bubble", "CommandForceBubble", "Mental Mutation", null, "\t", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: true, IsAttack: false, IsRealityDistortionBased);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
