using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Language;
using XRL.UI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class Teleprojector : IPart
{
	public int InitialChargeUse = 1000;

	public int MaintainChargeUse = 10;

	public Guid ActivatedAbilityID = Guid.Empty;

	public GameObject Target;

	public string pinnedZone;

	public override bool SameAs(IPart p)
	{
		Teleprojector teleprojector = p as Teleprojector;
		if (teleprojector.InitialChargeUse != InitialChargeUse)
		{
			return false;
		}
		if (teleprojector.MaintainChargeUse != MaintainChargeUse)
		{
			return false;
		}
		if (teleprojector.ActivatedAbilityID != ActivatedAbilityID)
		{
			return false;
		}
		if (teleprojector.Target != Target)
		{
			return false;
		}
		if (teleprojector.pinnedZone != pinnedZone)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BootSequenceDoneEvent.ID && ID != BootSequenceInitializedEvent.ID && ID != EquippedEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != InventoryActionEvent.ID && ID != IsOverloadableEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ActivatedAbilityID != Guid.Empty)
		{
			E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "ActivateTeleprojector");
		E.Actor.RegisterPartEvent(this, "BeginTakeAction");
		E.Actor.RegisterPartEvent(this, "ChainInterruptDomination");
		E.Actor.RegisterPartEvent(this, "DominationBroken");
		E.Actor.RegisterPartEvent(this, "EarlyBeforeDeathRemoval");
		E.Actor.RegisterPartEvent(this, "TookDamage");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		EndDomination(E.Actor);
		E.Actor.UnregisterPartEvent(this, "ActivateTeleprojector");
		E.Actor.UnregisterPartEvent(this, "BeginTakeAction");
		E.Actor.UnregisterPartEvent(this, "ChainInterruptDomination");
		E.Actor.UnregisterPartEvent(this, "DominationBroken");
		E.Actor.UnregisterPartEvent(this, "EarlyBeforeDeathRemoval");
		E.Actor.UnregisterPartEvent(this, "TookDamage");
		RemoveAbility(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		if (ParentObject.Equipped != null)
		{
			if (ParentObject.Equipped.IsPlayer())
			{
				Popup.Show(ParentObject.The + ParentObject.DisplayNameOnly + ParentObject.GetVerb("attune") + " to your physiology.");
			}
			ActivatedAbilityID = ParentObject.Equipped.AddActivatedAbility("Activate " + Grammar.MakeTitleCase(ParentObject.BaseDisplayNameStripped), "ActivateTeleprojector", "Items", null, "÷");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		RemoveAbility();
		EndDomination();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsOverloadableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsActive())
		{
			E.AddAction("Activate", "activate", "ActivateTeleprojector", null, 'a', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: false);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateTeleprojector" && ActivateTeleprojector())
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ActivateTeleprojector");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TookDamage")
		{
			InterruptDomination();
		}
		else if (E.ID == "DominationBroken")
		{
			EndDomination();
		}
		else if (E.ID == "EffectApplied")
		{
			if (E.GetParameter("Effect") is Effect effect && effect.IsOfType(33554432) && !effect.IsOfType(16777216))
			{
				InterruptDomination();
			}
		}
		else if (E.ID == "ActivateTeleprojector")
		{
			if (ActivateTeleprojector())
			{
				E.RequestInterfaceExit();
			}
		}
		else if (E.ID == "BeginTakeAction")
		{
			if (GameObject.validate(ref Target) && MaintainChargeUse > 0 && !ParentObject.UseCharge(MaintainChargeUse * MyPowerLoadLevel() / 100, LiveOnly: false, 0L))
			{
				InterruptDomination();
			}
		}
		else if (E.ID == "ChainInterruptDomination")
		{
			if (GameObject.validate(ref Target) && !Target.FireEvent("InterruptDomination"))
			{
				return false;
			}
		}
		else if (E.ID == "EarlyBeforeDeathRemoval")
		{
			PerformMetempsychosis();
		}
		return base.FireEvent(E);
	}

	private bool ActivateTeleprojector()
	{
		if (!IsActive())
		{
			return false;
		}
		if (ActivatedAbilityID == Guid.Empty)
		{
			return false;
		}
		GameObject who = ParentObject.Equipped;
		if (!who.IsPlayer())
		{
			return false;
		}
		Cell cell = PickDirection(ForAttack: true);
		if (who.IsActivatedAbilityUsable(ActivatedAbilityID) && cell != null)
		{
			bool flag = false;
			using (List<GameObject>.Enumerator enumerator = cell.GetObjectsWithPart("Robot").GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					GameObject current = enumerator.Current;
					flag = true;
					if (current.HasCopyRelationship(who))
					{
						if (who.IsPlayer())
						{
							Popup.ShowFail("You can't dominate " + who.itself + "!");
						}
						return false;
					}
					if (current.GetEffect((Dominated e) => e.Dominator == who) != null)
					{
						if (who.IsPlayer())
						{
							Popup.ShowFail("You can't dominate someone you are already dominating.");
						}
						return false;
					}
					if (current.HasEffect("Dominated"))
					{
						if (who.IsPlayer())
						{
							Popup.ShowFail("You can't dominate someone who is already being dominated.");
						}
						return false;
					}
					if (!current.CheckInfluence("Domination", who))
					{
						return false;
					}
					int num = MyPowerLoadLevel();
					if (ParentObject.UseCharge(InitialChargeUse * num / 100, LiveOnly: false, 0L, IncludeTransient: true, IncludeBiological: true, num))
					{
						current.GetAngryAt(who, -20);
						int attackModifier = IComponent<GameObject>.PowerLoadBonus(num) + who.Stat("Level") + GetAvailableComputePowerEvent.GetFor(who) / 5;
						PerformMentalAttack(RoboDom, who, current, null, "Domination Teleprojector", "1d8+4", 0, 400, int.MinValue, attackModifier, current.Stat("Level"));
						who.CooldownActivatedAbility(ActivatedAbilityID, 200);
						return true;
					}
					if (who.IsPlayer())
					{
						Popup.ShowFail(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("don't") + " have enough charge to function.");
					}
					return false;
				}
			}
			if (!flag)
			{
				Popup.ShowFail("There is nothing there that " + ParentObject.the + ParentObject.ShortDisplayName + " can uplink with.");
			}
		}
		return false;
	}

	public bool RoboDom(MentalAttackEvent E)
	{
		GameObject attacker = E.Attacker;
		GameObject defender = E.Defender;
		if (E.Penetrations > 0)
		{
			int duration = GetAvailableComputePowerEvent.AdjustUp(attacker, E.Magnitude);
			Dominated e = new Dominated(attacker, RoboDom: true, duration);
			if (defender.ApplyEffect(e))
			{
				Target = defender;
				defender.Sparksplatter();
				Popup.Show("You take control of " + defender.the + defender.ShortDisplayName + "!");
				attacker.pBrain.PushGoal(new Dormant(-1));
				Pin();
				XRLCore.Core.Game.Player.Body = defender;
				IComponent<GameObject>.ThePlayer.Target = null;
				return true;
			}
		}
		IComponent<GameObject>.XDidY(defender, "resist", "domination", "!", null, defender, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup: true);
		return false;
	}

	public void Pin()
	{
		Unpin();
		pinnedZone = ParentObject.GetCurrentCell().ParentZone.ZoneID;
		if (!XRLCore.Core.Game.ZoneManager.PinnedZones.CleanContains(pinnedZone))
		{
			XRLCore.Core.Game.ZoneManager.PinnedZones.Add(pinnedZone);
		}
	}

	public void Unpin()
	{
		if (pinnedZone != null)
		{
			XRLCore.Core.Game.ZoneManager.PinnedZones.Remove(pinnedZone);
			pinnedZone = null;
		}
	}

	private bool IsActive()
	{
		if (ParentObject.Equipped == null)
		{
			return false;
		}
		if (ParentObject.GetPart("BootSequence") is BootSequence bootSequence && bootSequence.BootTimeLeft > 0)
		{
			return false;
		}
		return true;
	}

	private void RemoveAbility(GameObject GO = null)
	{
		if (GO == null)
		{
			GO = ParentObject.Equipped;
		}
		GO?.RemoveActivatedAbility(ref ActivatedAbilityID);
		ActivatedAbilityID = Guid.Empty;
	}

	private bool EndDomination(GameObject who = null)
	{
		if (!GameObject.validate(ParentObject))
		{
			return false;
		}
		if (!GameObject.validate(ref Target))
		{
			return false;
		}
		if (who == null)
		{
			who = ParentObject.Equipped;
		}
		Dominated effect = Target.GetEffect((Dominated e) => e.Dominator == who);
		if (effect != null && !effect.BeingRemovedBySource)
		{
			effect.BeingRemovedBySource = true;
			Target.RemoveEffect(effect);
		}
		if (Target.OnWorldMap())
		{
			Target.PullDown();
		}
		Target.UpdateVisibleStatusColor();
		Target = null;
		if (who != null)
		{
			XRLCore.Core.Game.Player.Body = who;
			IComponent<GameObject>.ThePlayer.Target = null;
			if (who.IsPlayer())
			{
				Popup.Show("{{r|Your domination is broken!}}");
			}
			who.Sparksplatter();
			who.pBrain.Goals.Clear();
			who.UpdateVisibleStatusColor();
		}
		Sidebar.UpdateState();
		Unpin();
		return true;
	}

	public void InterruptDomination()
	{
		if (GameObject.validate(ref Target))
		{
			Target.FireEvent("InterruptDomination");
		}
	}

	public bool PerformMetempsychosis(GameObject who = null)
	{
		if (!GameObject.validate(ref Target))
		{
			return false;
		}
		if (who == null)
		{
			who = ParentObject.Equipped;
		}
		Dominated effect = Target.GetEffect((Dominated e) => e.Dominator == who);
		if (effect != null && !effect.BeingRemovedBySource)
		{
			effect.BeingRemovedBySource = true;
			effect.Metempsychosis = true;
			Target.RemoveEffect(effect);
			Domination.Metempsychosis(Target, effect.FromOriginalPlayerBody);
		}
		Target = null;
		Unpin();
		return true;
	}
}
