using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: the effective PushLevel and PushDistance are
///             increased by the standard power load bonus, i.e. 2 for the standard
///             overload power load of 400, which means an additional 1000 lbs. of
///             push force and two cells of push distance, and reality stabilization
///             penetration is increased by ((power load - 100) / 10), i.e. 30 for
///             the standard overload power load of 400 (affecting only the capacity
///             of the device to activate within reality stabilization, not the
///             ability of emitted force fields to resist reality stabilization).
///             </remarks>
[Serializable]
public class ForceEmitter : IPoweredPart
{
	public string Blueprint = "Forcefield";

	public int PushLevel = 5;

	[FieldSaveVersion(259)]
	public int PushDistance = 4;

	public bool StartActive;

	public int RenewChance = 10;

	public Guid ActivatedAbilityID;

	[NonSerialized]
	public Dictionary<string, GameObject> CurrentField = new Dictionary<string, GameObject>(8);

	[NonSerialized]
	public static List<string> toRemove = new List<string>();

	public ForceEmitter()
	{
		ChargeUse = 500;
		IsPowerLoadSensitive = true;
		IsRealityDistortionBased = true;
		WorksOnHolder = true;
		WorksOnWearer = true;
	}

	public override void Attach()
	{
		IsPowerLoadSensitive = true;
		base.Attach();
	}

	public override bool SameAs(IPart p)
	{
		return false;
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
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			item.Value.Obliterate();
		}
		CurrentField.Clear();
		SyncActivatedAbilityName();
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
		int powerLoad = MyPowerLoadLevel();
		int pushForce = GetPushForce(powerLoad);
		int pushDistance = GetPushDistance(powerLoad);
		Event @event = (IsRealityDistortionBased ? Event.New("CheckRealityDistortionAccessibility") : null);
		string[] directionList = Directions.DirectionList;
		foreach (string text in directionList)
		{
			Cell cellFromDirection = cell.GetCellFromDirection(text, BuiltOnly: false);
			if (cellFromDirection == null)
			{
				continue;
			}
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
			if ((@event != null && !cellFromDirection.FireEvent(@event)) || (Renew && !RenewChance.in100()))
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
			cellFromDirection.AddObject(gameObject2);
			if (gameObject2.CurrentCell == cellFromDirection)
			{
				CurrentField[text] = gameObject2;
				num++;
				foreach (GameObject item in cellFromDirection.GetObjectsWithPartReadonly("Physics"))
				{
					if (item != gameObject2 && item.pPhysics.Solid && (forcefield == null || !forcefield.CanPass(item)) && !item.HasPart("Forcefield") && !item.HasPart("HologramMaterial") && item.PhaseMatches(gameObject2))
					{
						item.pPhysics.Push(text, pushForce, pushDistance);
					}
				}
				foreach (GameObject item2 in cellFromDirection.GetObjectsWithPartReadonly("Combat"))
				{
					if (item2 != gameObject2 && item2.pPhysics != null && (forcefield == null || !forcefield.CanPass(item2)) && !item2.HasPart("HologramMaterial") && item2.PhaseMatches(gameObject2))
					{
						item2.pPhysics.Push(text, pushForce, pushDistance);
					}
				}
			}
			else
			{
				gameObject2.Obliterate();
			}
		}
		return num;
	}

	public int GetPushForce(int PowerLoad)
	{
		return 5000 + (PushLevel + MyPowerLoadBonus(PowerLoad)) * 500;
	}

	public int GetPushDistance(int PowerLoad)
	{
		return PushDistance + MyPowerLoadBonus(PowerLoad);
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
		if (cell == null || cell.ParentZone == null || cell.OnWorldMap())
		{
			DestroyBubble();
			return;
		}
		toRemove.Clear();
		int powerLoad = MyPowerLoadLevel();
		int pushForce = GetPushForce(powerLoad);
		int pushDistance = GetPushDistance(powerLoad);
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
				foreach (GameObject item2 in cellFromDirection.GetObjectsWithPartReadonly("Physics"))
				{
					if (item2 != value && item2.pPhysics.Solid && (part == null || !part.CanPass(item2)) && !item2.HasPart("Forcefield") && !item2.HasPart("HologramMaterial") && item2.PhaseMatches(value))
					{
						item2.pPhysics.Push(key, pushForce, pushDistance);
					}
				}
				foreach (GameObject item3 in cellFromDirection.GetObjectsWithPartReadonly("Combat"))
				{
					if (item3 != value && item3.pPhysics != null && (part == null || !part.CanPass(item3)) && !item3.HasPart("HologramMaterial") && item3.PhaseMatches(value))
					{
						item3.pPhysics.Push(key, pushForce, pushDistance);
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
			GameObject value = Reader.ReadGameObject("forcebubble");
			CurrentField.Add(key, value);
		}
		base.LoadData(Reader);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CheckExistenceSupportEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EndTurnEvent.ID && ID != EquippedEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetRealityStabilizationPenetrationEvent.ID && ID != InventoryActionEvent.ID && ID != ObjectCreatedEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (WorksOnSelf)
		{
			SetUpActivatedAbility(ParentObject);
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

	public override bool HandleEvent(EffectRemovedEvent E)
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

	public override bool HandleEvent(GetRealityStabilizationPenetrationEvent E)
	{
		E.Penetration += MyPowerLoadBonus(int.MinValue, 100, 10);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "BeginMove");
		E.Actor.RegisterPartEvent(this, "EffectApplied");
		E.Actor.RegisterPartEvent(this, "EffectRemoved");
		E.Actor.RegisterPartEvent(this, "EnteredCell");
		E.Actor.RegisterPartEvent(this, "MoveFailed");
		E.Actor.RegisterPartEvent(this, "ToggleForceEmitter");
		if (ParentObject.Understood())
		{
			SetUpActivatedAbility(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "BeginMove");
		E.Actor.UnregisterPartEvent(this, "EffectApplied");
		E.Actor.UnregisterPartEvent(this, "EffectRemoved");
		E.Actor.UnregisterPartEvent(this, "EnteredCell");
		E.Actor.UnregisterPartEvent(this, "MoveFailed");
		E.Actor.UnregisterPartEvent(this, "ToggleForceEmitter");
		E.Actor.RemoveActivatedAbility(ref ActivatedAbilityID);
		DestroyBubble();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood())
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (activePartFirstSubject != null && activePartFirstSubject.IsPlayer())
			{
				if (IsActive())
				{
					E.AddAction("Deactivate", "deactivate", "ToggleForceEmitter", null, 'a', FireOnActor: false, 10);
				}
				else
				{
					E.AddAction("Activate", "activate", "ToggleForceEmitter", null, 'a', FireOnActor: false, 10);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ToggleForceEmitter" && ActivateForceEmitter(E))
		{
			E.RequestInterfaceExit();
			E.Actor.UseEnergy(1000, "Item Force Bracelet");
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
		Object.RegisterPartEvent(this, "EnterCell");
		Object.RegisterPartEvent(this, "ExamineSuccess");
		if (WorksOnSelf)
		{
			Object.RegisterPartEvent(this, "BeginMove");
			Object.RegisterPartEvent(this, "EnteredCell");
			Object.RegisterPartEvent(this, "MoveFailed");
			Object.RegisterPartEvent(this, "ToggleForceEmitter");
		}
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetDefensiveItemList")
		{
			if (!IsActive() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && ForceFieldAdvisable())
			{
				if (ParentObject.pPhysics.CurrentCell.AnyAdjacentCell((Cell c) => c.GetCombatObject() != null && ParentObject.pBrain != null && ParentObject.pBrain.GetOpinion(c.GetCombatObject()) == Brain.CreatureOpinion.allied))
				{
					return true;
				}
				E.AddAICommand("ToggleForceEmitter", 1, ParentObject, Inv: true);
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
		else if (E.ID == "EffectApplied")
		{
			if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				DestroyBubble();
			}
		}
		else if (E.ID == "EnterCell")
		{
			if (StartActive && !IsActive())
			{
				ActivateForceEmitter();
			}
			ParentObject.UnregisterPartEvent(this, "EnterCell");
		}
		else if (E.ID == "ToggleForceEmitter")
		{
			if (ActivateForceEmitter(E))
			{
				E.RequestInterfaceExit();
				IComponent<GameObject>.ThePlayer.UseEnergy(1000, "Item Force Bracelet");
			}
		}
		else if (E.ID == "ExamineSuccess")
		{
			SetUpActivatedAbility(ParentObject.Equipped);
		}
		return base.FireEvent(E);
	}

	private bool ForceFieldAdvisable()
	{
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		return activePartFirstSubject?.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Device", ParentObject, "Operator", activePartFirstSubject)) ?? false;
	}

	public bool ActivateForceEmitter(IEvent E = null)
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
		if (!cell.BroadcastEvent(Event.New("InitiateForceBubble", "Object", activePartFirstSubject, "Device", ParentObject, "Operator", activePartFirstSubject), E))
		{
			return false;
		}
		if (!cell.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", activePartFirstSubject, "Device", ParentObject, "Operator", activePartFirstSubject), E))
		{
			return false;
		}
		if (IsActive())
		{
			if (IsSuspended())
			{
				if (activePartFirstSubject.IsPlayer())
				{
					Popup.Show(ParentObject.Does("vibrate") + " slightly.");
				}
			}
			else if (activePartFirstSubject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("The {{B|force bubble}} snaps off.");
			}
			else if (IComponent<GameObject>.Visible(activePartFirstSubject))
			{
				IComponent<GameObject>.AddPlayerMessage("The {{B|force bubble}} around " + activePartFirstSubject.t() + " snaps off.");
			}
			DestroyBubble();
			return true;
		}
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus == ActivePartStatus.Unpowered)
		{
			if (activePartFirstSubject.IsPlayer())
			{
				Popup.Show(ParentObject.Does("don't") + " have enough charge to sustain the field!");
			}
		}
		else
		{
			if (activePartStatus == ActivePartStatus.Operational && CreateBubble() > 0)
			{
				if (activePartFirstSubject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("A {{B|force bubble}} pops into being around you!");
				}
				else if (IComponent<GameObject>.Visible(activePartFirstSubject))
				{
					IComponent<GameObject>.AddPlayerMessage("A {{B|force bubble}} pops into being around " + activePartFirstSubject.t() + ".");
				}
				SyncActivatedAbilityName(activePartFirstSubject);
				return true;
			}
			if (activePartFirstSubject.IsPlayer())
			{
				Popup.Show("Nothing happens.");
			}
		}
		return false;
	}

	public void SetUpActivatedAbility(GameObject who)
	{
		if (who != null)
		{
			ActivatedAbilityID = who.AddActivatedAbility(GetActivatedAbilityName(who), "ToggleForceEmitter", (who == ParentObject) ? "Maneuvers" : "Items", null, "è");
		}
	}

	public string GetActivatedAbilityName(GameObject who = null)
	{
		if (who == null)
		{
			who = ParentObject.Equipped ?? ParentObject;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(IsActive() ? "Deactivate" : "Activate").Append(' ').Append((who == null || who == ParentObject) ? "Force Emitter" : Grammar.MakeTitleCase(ParentObject.BaseDisplayNameStripped));
		return stringBuilder.ToString();
	}

	public void SyncActivatedAbilityName(GameObject who = null)
	{
		if (!(ActivatedAbilityID == Guid.Empty))
		{
			if (who == null)
			{
				who = ParentObject.Equipped ?? ParentObject;
			}
			who.SetActivatedAbilityDisplayName(ActivatedAbilityID, GetActivatedAbilityName(who));
		}
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
