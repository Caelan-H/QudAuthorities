using System;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class RocketSkates : IActivePart
{
	public int PlumeLevel = 3;

	private FlamingHands flamingHands;

	public RocketSkates()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as RocketSkates).PlumeLevel != PlumeLevel)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CellChangedEvent.ID && ID != EquippedEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != GetRunningBehaviorEvent.ID && ID != GetShortDescriptionEvent.ID && ID != PartSupportEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PartSupportEvent E)
	{
		if (E.Skip != this && E.Type == "Run" && IsObjectActivePartSubject(E.Actor) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetRunningBehaviorEvent E)
	{
		if (E.Priority < 20 && IsObjectActivePartSubject(E.Actor) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.AbilityName = "Power Skate";
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("Move at high speed while leaving behind a fiery exhaust.");
			if (!E.Actor.HasSkill("Tactics_Hurdle"))
			{
				stringBuilder.Compound("-5 DV.", ' ');
			}
			if (!Running.IsEnhanced(E.Actor))
			{
				if (E.Actor.HasSkill("Pistol_SlingAndRun"))
				{
					stringBuilder.Compound("Reduced accuracy with missile weapons (except pistols).", ' ');
				}
				else
				{
					stringBuilder.Compound("Reduced accuracy with missile weapons.", ' ');
				}
				stringBuilder.Compound("-10 to hit in melee combat.", ' ').Compound("Is ended by attacking in melee, by effects that interfere with movement, and by most other actions that have action costs, other than using physical mutations.", ' ');
			}
			E.AbilityDescription = stringBuilder.ToString();
			E.Verb = "power skate";
			E.EffectDisplayName = "{{K-y-Y-M-m-K-m-M-Y-y-K-y-Y sequence|power skating}}";
			E.EffectMessageName = "power skating";
			E.EffectDuration = 9999;
			E.SpringingEffective = false;
			E.Priority = 20;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Replaces Sprint with Power Skate (unlimited duration).");
		E.Postfix.AppendRules("Emits plumes of fire when the wearer moves while power skating.", base.AddStatusSummary);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		SyncAbility();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		SyncAbility();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		SyncAbility();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "AfterMoved");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "AfterMoved");
		NeedPartSupportEvent.Send(E.Actor, "Run", this);
		Run.SyncAbility(E.Actor);
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
		if (IsSkating() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ParentObject.Smoke();
		}
		SyncAbility();
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterMoved")
		{
			Cell cell = E.GetParameter("FromCell") as Cell;
			Cell cell2 = ParentObject.GetCurrentCell();
			if (cell != null && cell.ParentZone == cell2.ParentZone && PlumeLevel > 0 && !cell2.OnWorldMap() && IsSkating() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				string directionFromCell = cell2.GetDirectionFromCell(cell);
				Cell cell3 = cell.GetCellFromDirection(directionFromCell);
				if (cell3 == null || cell3.ParentZone != cell2.ParentZone)
				{
					cell3 = cell;
				}
				if (flamingHands == null)
				{
					flamingHands = new FlamingHands();
				}
				flamingHands.Level = PlumeLevel;
				flamingHands.ParentObject = XRLCore.Core.Game.Player.Body;
				cell3?.ParticleBlip("&r^W" + (char)(219 + Stat.Random(0, 4)), 6);
				cell?.ParticleBlip("&R^W" + (char)(219 + Stat.Random(0, 4)), 3);
				flamingHands.Flame(cell3, null, doEffect: false);
			}
		}
		return base.FireEvent(E);
	}

	public bool IsSkating()
	{
		if (ParentObject.Equipped?.GetEffect("Running") is Running running)
		{
			return running.MessageName == "power skating";
		}
		return false;
	}

	public override bool IsActivePartEngaged()
	{
		if (!IsSkating())
		{
			return false;
		}
		return base.IsActivePartEngaged();
	}

	private void SyncAbility(GameObject who = null)
	{
		if (who == null)
		{
			who = ParentObject.Equipped;
			if (who == null)
			{
				return;
			}
		}
		if (IsObjectActivePartSubject(who) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			who.RequirePart<Run>();
		}
		Run.SyncAbility(who);
	}
}
