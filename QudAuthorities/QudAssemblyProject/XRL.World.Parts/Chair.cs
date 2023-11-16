using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Chair : IActivePart
{
	public int Level;

	public int LevelWhenDisabled = -1;

	public string DamageAttributes;

	public bool NoSmartUse;

	public Chair()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		Chair chair = p as Chair;
		if (chair.Level != Level)
		{
			return false;
		}
		if (chair.LevelWhenDisabled != LevelWhenDisabled)
		{
			return false;
		}
		if (chair.DamageAttributes != DamageAttributes)
		{
			return false;
		}
		if (chair.NoSmartUse != NoSmartUse)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AddedToInventoryEvent.ID && ID != BeforeDestroyObjectEvent.ID && (ID != CanSmartUseEvent.ID || NoSmartUse) && (ID != CommandSmartUseEvent.ID || NoSmartUse) && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EndTurnEvent.ID && ID != GetInventoryActionsEvent.ID && ID != IdleQueryEvent.ID && ID != InventoryActionEvent.ID && ID != LeftCellEvent.ID && ID != PollForHealingLocationEvent.ID)
		{
			return ID == UseHealingLocationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		DetachEffects((Cell)null, (IEvent)E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		DetachEffects((Cell)null, (IEvent)E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		DetachEffects(E.Cell, E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		SyncEffects();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		SyncEffects();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (ChargeUse > 0)
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null && cell.HasObjectWithEffect("Sitting", IsSittingOnThis))
			{
				ConsumeChargeIfOperational();
				SyncEffects();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood())
		{
			if (E.Actor.GetEffect("Sitting") is XRL.World.Effects.Sitting sitting && sitting.SittingOn == ParentObject)
			{
				E.AddAction("Stand", "stand", "StandUpFromChair", null, 's', FireOnActor: false, 5);
			}
			else
			{
				E.AddAction("Sit", "sit", "SitOnChair", null, 's', FireOnActor: false, 5);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "SitOnChair")
		{
			SitDown(E.Actor, E);
		}
		else if (E.Command == "StandUpFromChair")
		{
			StandUp(E.Actor, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (!NoSmartUse && E.Actor != ParentObject && ParentObject.Understood() && E.Actor.PhaseMatches(ParentObject) && E.Actor.FlightCanReach(ParentObject))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (!NoSmartUse && E.Actor != ParentObject && ParentObject.Understood() && E.Actor.PhaseMatches(ParentObject) && E.Actor.FlightCanReach(ParentObject))
		{
			if (E.Actor.GetEffect("Sitting") is XRL.World.Effects.Sitting sitting && sitting.SittingOn == ParentObject)
			{
				StandUp(E.Actor, E);
			}
			else
			{
				SitDown(E.Actor, E);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (Level <= -10)
		{
			return base.HandleEvent(E);
		}
		if (E.Actor == ParentObject)
		{
			return base.HandleEvent(E);
		}
		if (ParentObject.DistanceTo(E.Actor) > 0)
		{
			return base.HandleEvent(E);
		}
		if (!1.in100())
		{
			return base.HandleEvent(E);
		}
		if (E.Actor.GetEffect("Sitting") is XRL.World.Effects.Sitting sitting)
		{
			if (sitting.SittingOn == ParentObject && StandUp(E.Actor))
			{
				return false;
			}
		}
		else if (SitDown(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PollForHealingLocationEvent E)
	{
		if (E.Actor != ParentObject && E.Actor.PhaseMatches(ParentObject) && E.Actor.FlightCanReach(ParentObject))
		{
			int num = EffectiveLevel();
			if (num > -10)
			{
				E.Value = Math.Max(E.Value, Math.Min(Math.Max(1, num), 9));
				if (E.First)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseHealingLocationEvent E)
	{
		if (EffectiveLevel() > -10 && !E.Actor.HasEffect("Sitting"))
		{
			SitDown(E.Actor, E);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public int EffectiveLevel()
	{
		if (!IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return Level;
		}
		return LevelWhenDisabled;
	}

	private void SyncObjectEffect(GameObject GO)
	{
		if (GO.GetEffect("Sitting") is XRL.World.Effects.Sitting sitting && sitting.SittingOn == ParentObject)
		{
			sitting.Level = EffectiveLevel();
			sitting.DamageAttributes = DamageAttributes;
		}
	}

	public void SyncEffects()
	{
		ParentObject.CurrentCell?.ForeachObject((Action<GameObject>)SyncObjectEffect);
	}

	private void DetachEffects(GameObject GO, IEvent Event)
	{
		if (GO.GetEffect("Sitting") is XRL.World.Effects.Sitting sitting && sitting.SittingOn == ParentObject)
		{
			GO.RemoveEffect(sitting);
			GO.ApplyEffect(new Prone());
			Event?.RequestInterfaceExit();
		}
	}

	private void DetachEffects(Cell C = null, IEvent Event = null)
	{
		if (C == null)
		{
			C = ParentObject.GetCurrentCell();
		}
		C?.SafeForeachObject(delegate(GameObject o)
		{
			DetachEffects(o, Event);
		});
	}

	public bool SitDown(GameObject who, IEvent E = null)
	{
		if (who == ParentObject)
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You cannot sit on " + who.itself + ".");
			}
			return false;
		}
		if (who.HasEffect("Sitting"))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You are already sitting down.");
			}
			return false;
		}
		if (who.HasEffect("Enclosed"))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You cannot do that while enclosed.");
			}
			return false;
		}
		if (who.HasEffect("Burrowed"))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You cannot do that while burrowed.");
			}
			return false;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			cell = who.CurrentCell;
			if (cell == null)
			{
				return false;
			}
			if (ParentObject.Equipped == who && !InventoryActionEvent.Check(ParentObject.Equipped, who, ParentObject, "Unequip"))
			{
				if (who.IsPlayer())
				{
					Popup.ShowFail("You cannot unequip " + ParentObject.t() + ".");
				}
				return false;
			}
			if (ParentObject.InInventory == who && !who.FireEvent(Event.New("CommandDropObject", "Object", ParentObject)))
			{
				if (who.IsPlayer())
				{
					Popup.ShowFail("You cannot set " + ParentObject.t() + " down!");
				}
				return false;
			}
			if (ParentObject.CurrentCell != cell)
			{
				return false;
			}
		}
		if (!who.CanChangeBodyPosition("Sitting", ShowMessage: true))
		{
			return false;
		}
		if (!who.FlightCanReach(ParentObject))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You cannot reach " + ParentObject.the + ParentObject.ShortDisplayName + ".");
			}
			return false;
		}
		if (!who.PhaseMatches(ParentObject))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You are out of phase with " + ParentObject.the + ParentObject.ShortDisplayName + ".");
			}
			return false;
		}
		if (who.CurrentCell != cell && (!who.DirectMoveTo(cell, 0, forced: false, ParentObject.IsAlliedTowards(who)) || who.CurrentCell != cell))
		{
			E?.RequestInterfaceExit();
			return false;
		}
		IComponent<GameObject>.XDidYToZ(who, "sit", "down on", ParentObject);
		who.ApplyEffect(new XRL.World.Effects.Sitting(ParentObject, EffectiveLevel(), DamageAttributes));
		if (who.IsPlayer())
		{
			ParentObject.SetIntProperty("DroppedByPlayer", 1);
			MetricsManager.LogEvent("ChairsSat");
		}
		who.UseEnergy(1000, "Position");
		E?.RequestInterfaceExit();
		who.FireEvent(Event.New("SatIn", "Object", ParentObject));
		ParentObject.FireEvent(Event.New("BeingSatOn", "Object", who));
		return true;
	}

	public bool StandUp(GameObject who, IEvent E = null, XRL.World.Effects.Sitting S = null)
	{
		if (S == null)
		{
			S = who.GetEffect("Sitting") as XRL.World.Effects.Sitting;
			if (S == null)
			{
				if (who.IsPlayer())
				{
					Popup.ShowFail("You are not sitting down.");
				}
				return false;
			}
		}
		if (S.SittingOn != ParentObject)
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("It is not " + ParentObject.the + ParentObject.DisplayNameOnly + " that you are sitting on.");
			}
			return false;
		}
		IComponent<GameObject>.XDidY(who, "stand", "up");
		who.RemoveEffect(S);
		who.UseEnergy(1000, "Position");
		E?.RequestInterfaceExit();
		return true;
	}

	private bool IsSittingOnThis(GameObject obj)
	{
		if (obj.GetEffect("Sitting") is XRL.World.Effects.Sitting sitting)
		{
			return sitting.SittingOn == ParentObject;
		}
		return false;
	}
}
