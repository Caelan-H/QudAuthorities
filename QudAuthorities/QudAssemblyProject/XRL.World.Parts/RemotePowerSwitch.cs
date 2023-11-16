using System;

namespace XRL.World.Parts;

[Serializable]
public class RemotePowerSwitch : IPoweredPart
{
	public string FrequencyCode;

	public RemotePowerSwitch()
	{
		ChargeUse = 0;
		IsPowerSwitchSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as RemotePowerSwitch).FrequencyCode != FrequencyCode)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		PowerSwitch powerSwitch = Connected();
		if (powerSwitch != null && ParentObject.Understood() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (powerSwitch.Active)
			{
				E.AddAction("Deactivate", "deactivate", "RemotePowerSwitchOff", null, 'a', FireOnActor: false, powerSwitch.DeactivateActionPriority, 0, Override: false, WorksAtDistance: false, powerSwitch.FlippableKinetically);
			}
			else
			{
				E.AddAction("Activate", "activate", "RemotePowerSwitchOn", null, 'a', FireOnActor: false, powerSwitch.ActivateActionPriority, 0, Override: false, WorksAtDistance: false, powerSwitch.FlippableKinetically);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "RemotePowerSwitchOn")
		{
			PowerSwitch powerSwitch = Connected();
			if (powerSwitch != null && E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				IComponent<GameObject>.XDidYToZ(E.Actor, powerSwitch.ActivateVerb, powerSwitch.ActivatePreposition, ParentObject, powerSwitch.ActivateExtra, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
				if (powerSwitch.KeylessActivation || powerSwitch.AccessCheck(E.Actor))
				{
					string text = GameText.VariableReplace(powerSwitch.ActivateSuccessMessage, ParentObject);
					string text2 = GameText.VariableReplace(powerSwitch.ActivateFailureMessage, ParentObject);
					if (ParentObject.FireEvent(Event.New("RemotePowerSwitchActivate", "Actor", E.Actor)))
					{
						if (!string.IsNullOrEmpty(text))
						{
							IComponent<GameObject>.EmitMessage(E.Actor, text, FromDialog: true);
						}
					}
					else if (!string.IsNullOrEmpty(text2))
					{
						IComponent<GameObject>.EmitMessage(E.Actor, text2, FromDialog: true);
					}
				}
				E.Actor.UseEnergy(1000, "Item Activate");
				E.RequestInterfaceExit();
			}
		}
		else if (E.Command == "RemotePowerSwitchOff")
		{
			PowerSwitch powerSwitch2 = Connected();
			if (powerSwitch2 != null && E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				IComponent<GameObject>.XDidYToZ(E.Actor, powerSwitch2.DeactivateVerb, powerSwitch2.DeactivatePreposition, ParentObject, powerSwitch2.DeactivateExtra, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
				if (powerSwitch2.KeylessDeactivation || powerSwitch2.AccessCheck(E.Actor))
				{
					string text3 = GameText.VariableReplace(powerSwitch2.DeactivateSuccessMessage, ParentObject);
					string text4 = GameText.VariableReplace(powerSwitch2.DeactivateFailureMessage, ParentObject);
					if (ParentObject.FireEvent(Event.New("RemotePowerSwitchDeactivate", "Actor", E.Actor)))
					{
						if (!string.IsNullOrEmpty(text3))
						{
							IComponent<GameObject>.EmitMessage(E.Actor, text3, FromDialog: true);
						}
					}
					else if (!string.IsNullOrEmpty(text4))
					{
						IComponent<GameObject>.EmitMessage(E.Actor, text4, FromDialog: true);
					}
				}
				E.Actor.UseEnergy(1000, "Item Deactivate");
				E.RequestInterfaceExit();
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanSmartUse");
		Object.RegisterPartEvent(this, "CommandSmartUseEarly");
		Object.RegisterPartEvent(this, "RemotePowerSwitchActivate");
		Object.RegisterPartEvent(this, "RemotePowerSwitchDeactivate");
		base.Register(Object);
	}

	public PowerSwitch Connected()
	{
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return null;
		}
		Zone parentZone = cell.ParentZone;
		if (parentZone == null)
		{
			return null;
		}
		for (int i = 0; i < parentZone.Width; i++)
		{
			for (int j = 0; j < parentZone.Height; j++)
			{
				Cell cell2 = parentZone.GetCell(i, j);
				if (cell2 == null)
				{
					continue;
				}
				foreach (GameObject @object in cell2.Objects)
				{
					if (@object.GetPart("PowerSwitch") is PowerSwitch powerSwitch && powerSwitch.FrequencyCode == FrequencyCode)
					{
						return powerSwitch;
					}
				}
			}
		}
		return null;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			if (Connected() != null && E.GetGameObjectParameter("User").IsPlayer() && ParentObject.Understood())
			{
				return false;
			}
		}
		else if (E.ID == "CommandSmartUseEarly")
		{
			if (Connected() != null && E.GetGameObjectParameter("User").IsPlayer() && ParentObject.Understood())
			{
				ParentObject.Twiddle();
				return false;
			}
		}
		else if (E.ID == "RemotePowerSwitchActivate")
		{
			PowerSwitch powerSwitch = Connected();
			if (powerSwitch == null)
			{
				return false;
			}
			if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
			Event @event = Event.New("PowerSwitchActivate");
			@event.SetParameter("Actor", E.GetGameObjectParameter("Actor"));
			@event.SetFlag("Forced", E.HasFlag("Forced"));
			if (!powerSwitch.FireEvent(@event))
			{
				return false;
			}
		}
		else if (E.ID == "RemotePowerSwitchDeactivate")
		{
			PowerSwitch powerSwitch2 = Connected();
			if (powerSwitch2 == null)
			{
				return false;
			}
			if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
			Event event2 = Event.New("PowerSwitchDectivate");
			event2.SetParameter("Actor", E.GetGameObjectParameter("Actor"));
			event2.SetFlag("Forced", E.HasFlag("Forced"));
			if (!powerSwitch2.FireEvent(event2))
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
