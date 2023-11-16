using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Stopsvaalinn : IPoweredPart
{
	public string Blueprint = "Forcefield";

	public int PushLevel = 5;

	public int RenewChance = 10;

	public Guid ActivatedAbilityID = Guid.Empty;

	public string FieldDirection;

	public bool RobotStopApplied;

	[NonSerialized]
	public Dictionary<string, GameObject> CurrentField = new Dictionary<string, GameObject>(3);

	[NonSerialized]
	public static List<string> toRemove = new List<string>();

	public Stopsvaalinn()
	{
		ChargeUse = 500;
		IsRealityDistortionBased = true;
		WorksOnWearer = true;
		WorksOnEquipper = true;
		NameForStatus = "ForceEmitter";
	}

	private void ApplyRobotStop(GameObject Subject = null)
	{
		if (!RobotStopApplied)
		{
			if (Subject == null)
			{
				Subject = GetActivePartFirstSubject();
			}
			if (Subject != null)
			{
				Subject.SetStringProperty("RobotStop", "true");
				RobotStopApplied = true;
			}
		}
	}

	private void UnapplyRobotStop(GameObject Subject = null)
	{
		if (RobotStopApplied)
		{
			if (Subject == null)
			{
				Subject = GetActivePartFirstSubject();
			}
			if (Subject != null)
			{
				Subject.RemoveStringProperty("RobotStop");
				RobotStopApplied = false;
			}
		}
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
		if (toRemove.Count <= 0)
		{
			return;
		}
		foreach (string item2 in toRemove)
		{
			CurrentField.Remove(item2);
		}
		if (CurrentField.Count == 0)
		{
			UnapplyRobotStop();
		}
	}

	public bool IsActive()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			if (CurrentField.Count > 0)
			{
				return true;
			}
		}
		UnapplyRobotStop();
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
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			item.Value.Obliterate();
		}
		CurrentField.Clear();
		UnapplyRobotStop();
		FieldDirection = null;
	}

	public int CreateBubble(bool Renew = false)
	{
		Validate();
		int num = 0;
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (activePartFirstSubject == null)
		{
			return num;
		}
		Cell cell = activePartFirstSubject.CurrentCell;
		if (cell == null)
		{
			return num;
		}
		if (cell.ParentZone.IsWorldMap())
		{
			return num;
		}
		if (Renew && RenewChance <= 0)
		{
			return num;
		}
		Event @event = (IsRealityDistortionBased ? Event.New("CheckRealityDistortionAccessibility") : null);
		if (string.IsNullOrEmpty(FieldDirection) || (CurrentField.Count == 0 && !Renew))
		{
			FieldDirection = PickDirectionS();
			if (string.IsNullOrEmpty(FieldDirection))
			{
				return num;
			}
			if (FieldDirection == ".")
			{
				FieldDirection = Directions.GetRandomDirection();
			}
		}
		foreach (KeyValuePair<string, Cell> item in cell.GetAdjacentDirectionCellMap(FieldDirection, BuiltOnly: false))
		{
			string key = item.Key;
			Cell value = item.Value;
			if (value == null || value == cell)
			{
				continue;
			}
			if (CurrentField.ContainsKey(key))
			{
				GameObject gameObject = CurrentField[key];
				if (gameObject.CurrentCell == value)
				{
					continue;
				}
				gameObject.Obliterate();
				CurrentField.Remove(key);
			}
			if ((@event != null && !value.FireEvent(@event)) || (Renew && !RenewChance.in100()))
			{
				continue;
			}
			GameObject gameObject2 = GameObject.create(Blueprint);
			Forcefield forcefield = gameObject2.GetPart("Forcefield") as Forcefield;
			if (forcefield != null)
			{
				forcefield.Creator = activePartFirstSubject;
				forcefield.MovesWithOwner = true;
				forcefield.RejectOwner = false;
			}
			gameObject2.RequirePart<ExistenceSupport>().SupportedBy = ParentObject;
			Phase.carryOver(activePartFirstSubject, gameObject2);
			AnimatedMaterialForcefield part = gameObject2.GetPart<AnimatedMaterialForcefield>();
			if (part != null)
			{
				part.Color = "Red";
			}
			value.AddObject(gameObject2);
			if (gameObject2.CurrentCell == value)
			{
				CurrentField.Add(key, gameObject2);
				num++;
				foreach (GameObject item2 in value.GetObjectsWithPart("Physics"))
				{
					if (item2 != gameObject2 && item2 != activePartFirstSubject && item2.pPhysics.Solid && (forcefield == null || !forcefield.CanPass(item2)) && !item2.HasPart("Forcefield") && !item2.HasPart("HologramMaterial") && item2.PhaseMatches(gameObject2))
					{
						item2.pPhysics.Push(key, 5000 + 500 * PushLevel, 4);
					}
				}
				foreach (GameObject item3 in value.GetObjectsWithPart("Combat"))
				{
					if (item3 != gameObject2 && item3 != activePartFirstSubject && item3.pPhysics != null && (forcefield == null || !forcefield.CanPass(item3)) && !item3.HasPart("HologramMaterial") && item3.PhaseMatches(gameObject2))
					{
						item3.pPhysics.Push(key, 5000 + 500 * PushLevel, 4);
					}
				}
			}
			else
			{
				gameObject2.Obliterate();
			}
		}
		if (num > 0)
		{
			ApplyRobotStop(activePartFirstSubject);
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
		Cell cell = GetActivePartFirstSubject()?.CurrentCell;
		if (cell == null || cell.ParentZone == null || cell.ParentZone.IsWorldMap())
		{
			DestroyBubble();
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
				value.Obliterate();
				toRemove.Add(key);
				continue;
			}
			cellFromDirection.AddObject(value);
			Forcefield part = value.GetPart<Forcefield>();
			if (value.CurrentCell == cellFromDirection)
			{
				foreach (GameObject item2 in cellFromDirection.GetObjectsWithPart("Physics"))
				{
					if (item2 != value && item2.pPhysics.Solid && (part == null || !part.CanPass(item2)) && !item2.HasPart("Forcefield") && !item2.HasPart("HologramMaterial") && item2.PhaseMatches(value))
					{
						item2.pPhysics.Push(key, 5000 + 500 * PushLevel, 4);
					}
				}
				foreach (GameObject item3 in cellFromDirection.GetObjectsWithPart("Combat"))
				{
					if (item3 != value && item3.pPhysics != null && (part == null || !part.CanPass(item3)) && !item3.HasPart("HologramMaterial") && item3.PhaseMatches(value))
					{
						item3.pPhysics.Push(key, 5000 + 500 * PushLevel, 4);
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
			GameObject value = Reader.ReadGameObject("stopsvalinn");
			CurrentField.Add(key, value);
		}
		base.LoadData(Reader);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CheckExistenceSupportEvent.ID && ID != CommandEvent.ID && ID != EffectAppliedEvent.ID && ID != EndTurnEvent.ID && ID != EquippedEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "ActivateStopsvalinn" && E.Actor == ParentObject.Equipped && ActivateStopsvalinn(E))
		{
			E.RequestInterfaceExit();
			E.Actor?.UseEnergy(1000, "Item Stopsvalinn");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			DestroyBubble();
		}
		return base.HandleEvent(E);
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
			if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				DestroyBubble();
			}
			else if (RenewChance > 0 && CurrentField.Count < 8)
			{
				CreateBubble(Renew: true);
			}
			else
			{
				MaintainBubble();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckApplyPowers(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		UnapplyPowers(E.Actor);
		return base.HandleEvent(E);
	}

	private void CheckApplyPowers(GameObject who = null)
	{
		if (ActivatedAbilityID == Guid.Empty)
		{
			if (who == null)
			{
				who = ParentObject.Equipped;
			}
			if (who != null && (!who.IsPlayer() || ParentObject.Understood()))
			{
				ApplyPowers(who);
			}
		}
	}

	private void ApplyPowers(GameObject who)
	{
		base.StatShifter.SetStatShift(who, "Ego", 1);
		who.RegisterPartEvent(this, "BeginMove");
		who.RegisterPartEvent(this, "EnteredCell");
		who.RegisterPartEvent(this, "MoveFailed");
		ActivatedAbilityID = who.AddActivatedAbility("Activate Stopsvalinn", "ActivateStopsvalinn", "Items", null, "é", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased);
	}

	private void CheckUnapplyPowers(GameObject who = null)
	{
		if (ActivatedAbilityID != Guid.Empty)
		{
			UnapplyPowers(who);
		}
	}

	private void UnapplyPowers(GameObject who)
	{
		base.StatShifter.RemoveStatShifts(who);
		who.UnregisterPartEvent(this, "BeginMove");
		who.UnregisterPartEvent(this, "EnteredCell");
		who.UnregisterPartEvent(this, "MoveFailed");
		who.RemoveActivatedAbility(ref ActivatedAbilityID);
		DestroyBubble();
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood() && IsObjectActivePartSubject(E.Actor))
		{
			if (IsActive())
			{
				E.AddAction("Deactivate", "deactivate", "ActivateStopsvalinn", null, 'a');
			}
			else
			{
				E.AddAction("Activate", "activate", "ActivateStopsvalinn", null, 'a');
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateStopsvalinn" && ActivateStopsvalinn(E))
		{
			E.RequestInterfaceExit();
			E.Actor.UseEnergy(1000, "Item Stopsvalinn");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		DestroyBubble();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetDefensiveItemList");
		Object.RegisterPartEvent(this, "ExamineSuccess");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetDefensiveItemList")
		{
			if (!IsActive() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && ForceFieldAdvisable())
			{
				GameObject equipped = ParentObject.Equipped;
				if (equipped != null && equipped.IsActivatedAbilityAIUsable(ActivatedAbilityID))
				{
					E.AddAICommand("ActivateStopsvalinn", 1, ParentObject, Inv: true);
				}
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
		else if (E.ID == "ExamineSuccess")
		{
			CheckApplyPowers();
		}
		return base.FireEvent(E);
	}

	private bool ForceFieldAdvisable()
	{
		return ((ParentObject.pPhysics.Equipped != null) ? ParentObject.pPhysics.Equipped : ParentObject).FireEvent(Event.New("CheckRealityDistortionAdvisability", "Device", ParentObject));
	}

	public bool ActivateStopsvalinn(IEvent E = null)
	{
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (activePartFirstSubject == null)
		{
			return false;
		}
		Cell cell = activePartFirstSubject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		if (!cell.BroadcastEvent(Event.New("InitiateForceBubble", "Object", activePartFirstSubject, "Device", ParentObject), E))
		{
			return false;
		}
		if (!cell.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", activePartFirstSubject, "Device", ParentObject), E))
		{
			return false;
		}
		if (IsActive())
		{
			if (IsSuspended())
			{
				if (activePartFirstSubject.IsPlayer())
				{
					Popup.Show(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("vibrate") + " slightly.");
				}
			}
			else if (activePartFirstSubject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("The {{R|force bubble}} snaps off.");
			}
			else if (IComponent<GameObject>.Visible(activePartFirstSubject))
			{
				IComponent<GameObject>.AddPlayerMessage("The {{R|force bubble}} in front of " + activePartFirstSubject.the + activePartFirstSubject.ShortDisplayName + " snaps off.");
			}
			DestroyBubble();
			return true;
		}
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus == ActivePartStatus.Unpowered)
		{
			if (activePartFirstSubject.IsPlayer())
			{
				Popup.Show(ParentObject.The + ParentObject.DisplayNameOnly + ParentObject.GetVerb("don't") + " have enough charge to sustain the field!");
			}
		}
		else
		{
			if (activePartStatus == ActivePartStatus.Operational && CreateBubble() > 0)
			{
				if (activePartFirstSubject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("A {{R|force bubble}} pops into being in front of you!");
				}
				else if (IComponent<GameObject>.Visible(activePartFirstSubject))
				{
					IComponent<GameObject>.AddPlayerMessage("A {{R|force bubble}} pops into being in front of " + activePartFirstSubject.the + activePartFirstSubject.ShortDisplayName + ".");
				}
				return true;
			}
			if (activePartFirstSubject.IsPlayer())
			{
				Popup.Show("Nothing happens.");
			}
		}
		return false;
	}

	public void MaintainBubble()
	{
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (activePartFirstSubject == null)
		{
			return;
		}
		foreach (GameObject value in CurrentField.Values)
		{
			Phase.sync(activePartFirstSubject, value);
		}
	}
}
