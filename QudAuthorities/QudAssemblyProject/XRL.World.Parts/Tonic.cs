using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Tonic : IPart
{
	public bool CausesOverdose = true;

	public bool Eat;

	public string BehaviorDescription = "This item is a tonic. Applying one tonic while under the effects of another may produce undesired results.";

	public override bool SameAs(IPart p)
	{
		Tonic tonic = p as Tonic;
		if (tonic.CausesOverdose != CausesOverdose)
		{
			return false;
		}
		if (tonic.Eat != Eat)
		{
			return false;
		}
		if (tonic.BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsAlwaysEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(BehaviorDescription);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
	{
		if (!E.Object.IsBroken() && !E.Object.IsRusted())
		{
			int @default = 0;
			if (E.Object.HasPart("Empty_Tonic_Applicator"))
			{
				@default = -100;
			}
			else if (E.Object.Equipped == E.Actor || E.Object.InInventory == E.Actor)
			{
				@default = ((!E.Object.IsImportant()) ? 100 : (-1));
			}
			if (Eat)
			{
				E.AddAction("Eat", "eat", "Apply", null, 'e', FireOnActor: false, @default, 0, Override: true);
				E.AddAction("Feed To", "feed to", "ApplyTo", null, 'f', FireOnActor: false, -1, 0, Override: true);
			}
			else
			{
				E.AddAction("Apply", "apply", "Apply", null, 'a', FireOnActor: false, @default);
				E.AddAction("Apply To", "apply to", "ApplyTo", null, 'a', FireOnActor: false, -1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Apply" || E.Command == "ApplyInvoluntarily" || E.Command == "ApplyExternally")
		{
			bool flag = E.Command == "ApplyInvoluntarily" || E.Command == "ApplyExternally";
			bool flag2 = E.Command == "ApplyInvoluntarily";
			if (E.Item.IsBroken())
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail(E.Item.Itis + " broken...");
				}
			}
			else if (E.Item.IsRusted())
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail(E.Item.Itis + " rusted...");
				}
			}
			else
			{
				if (!flag2 && !flag && !E.Item.ConfirmUseImportant(E.Actor, "use", "up", 1))
				{
					return false;
				}
				if (E.Actor.IsPlayer() || IComponent<GameObject>.Visible(E.Actor))
				{
					ParentObject.MakeUnderstood();
				}
				Event @event = Event.New("ApplyingTonic");
				@event.SetParameter("Actor", E.ObjectTarget ?? E.Actor);
				@event.SetParameter("Subject", E.Actor);
				@event.SetParameter("Tonic", ParentObject);
				@event.SetFlag("External", flag);
				@event.SetFlag("Involuntary", flag2);
				if (E.Actor.FireEvent(@event))
				{
					List<Effect> tonicEffects = E.Actor.GetTonicEffects();
					int tonicCapacity = E.Actor.GetTonicCapacity();
					if (tonicEffects.Count >= tonicCapacity && CausesOverdose)
					{
						foreach (Effect item in tonicEffects)
						{
							if (!E.Actor.MakeSave("Toughness", 16 + 3 * (tonicEffects.Count - tonicCapacity), null, null, "Overdose", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
							{
								Event event2 = Event.New("Overdose");
								event2.SetParameter("Actor", E.ObjectTarget ?? E.Actor);
								event2.SetParameter("Subject", E.Actor);
								event2.SetParameter("Tonic", ParentObject);
								event2.SetFlag("External", flag);
								event2.SetFlag("Involuntary", flag2);
								item.FireEvent(event2);
							}
						}
					}
					int chance = 5;
					string value = "No";
					if (E.Actor.HasPart("TonicAllergy"))
					{
						chance = 33;
					}
					bool flag3 = false;
					if (E.Actor.IsMutant() && chance.in100())
					{
						E.Actor.SetLongProperty("Overdosing", 1L);
						flag3 = true;
					}
					try
					{
						Event event3 = Event.New("ApplyTonic");
						event3.SetParameter("Actor", E.ObjectTarget ?? E.Actor);
						event3.SetParameter("Subject", E.Actor);
						event3.SetParameter("Target", E.Actor);
						event3.SetParameter("Owner", E.Actor);
						event3.SetParameter("Attacker", E.ObjectTarget);
						event3.SetParameter("Overdose", value);
						event3.SetFlag("External", flag);
						event3.SetFlag("Involuntary", flag2);
						if (ParentObject.FireEvent(event3))
						{
							if (Eat)
							{
								E.Actor.FireEvent(Event.New("Eating", "Food", ParentObject));
							}
							if (!flag && !E.Actor.IsPlayer() && IComponent<GameObject>.Visible(E.Actor))
							{
								IComponent<GameObject>.AddPlayerMessage(E.Actor.T() + E.Actor.GetVerb(Eat ? "eat" : "apply") + " " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
							}
							ParentObject.Destroy(null, Silent: true);
						}
					}
					finally
					{
						if (flag3)
						{
							E.Actor.SetLongProperty("Overdosing", 0L);
						}
					}
				}
			}
		}
		else if (E.Command == "ApplyTo")
		{
			if (E.Item.IsBroken())
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail(E.Item.Itis + " broken...");
				}
				return false;
			}
			if (E.Item.IsRusted())
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail(E.Item.Itis + " rusted...");
				}
				return false;
			}
			if (!E.Item.ConfirmUseImportant(E.Actor, "use", "up", 1))
			{
				return false;
			}
			Cell cell = PickDirection(ForAttack: false, null, E.Actor);
			if (cell == null)
			{
				return false;
			}
			GameObject combatTarget = cell.GetCombatTarget(E.Actor);
			if (combatTarget == null)
			{
				combatTarget = cell.GetCombatTarget(E.Actor, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
				if (combatTarget != null)
				{
					if (cell.GetCombatTarget(E.Actor, IgnoreFlight: true) == null)
					{
						Popup.ShowFail("You are out of phase with " + combatTarget.t() + ".");
					}
					else
					{
						Popup.ShowFail("You cannot reach " + combatTarget.t() + ".");
					}
				}
				else
				{
					Popup.ShowFail("There is no one there you can " + (Eat ? "feed" : "apply") + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " to.");
				}
				return false;
			}
			if (combatTarget == E.Actor)
			{
				if (Eat)
				{
					Popup.ShowFail("If you want to eat " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " " + E.Actor.itself + ", you can do so through the eat action.");
				}
				else
				{
					Popup.ShowFail("If you want to apply " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " to " + E.Actor.itself + ", you can do so through the apply action.");
				}
				return false;
			}
			if (combatTarget.IsHostileTowards(E.Actor) || (!combatTarget.IsLedBy(E.Actor) && GetUtilityScoreEvent.GetFor(combatTarget, ParentObject, null, ForPermission: true) <= 0))
			{
				if (Eat)
				{
					Popup.ShowFail(combatTarget.T() + combatTarget.GetVerb("do") + " not want to eat " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				}
				else
				{
					Popup.ShowFail(combatTarget.T() + combatTarget.GetVerb("do") + " not want " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " applied to " + combatTarget.them + ". You'll need to equip " + ParentObject.them + " as a weapon and attack with " + ParentObject.them + ".");
				}
				return false;
			}
			ParentObject.SplitFromStack();
			IComponent<GameObject>.WDidXToYWithZ(E.Actor, Eat ? "feed" : "apply", ParentObject, "to", combatTarget, null, null, null, combatTarget);
			InventoryActionEvent.Check(ParentObject, combatTarget, ParentObject, "ApplyExternally", Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, 0, 0, E.Actor);
			E.Actor.UseEnergy(1000, "Item ApplyTo");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ProjectileHit");
		Object.RegisterPartEvent(this, "ThrownProjectileHit");
		Object.RegisterPartEvent(this, "WeaponAfterDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ProjectileHit" || E.ID == "ThrownProjectileHit" || E.ID == "WeaponAfterDamage")
		{
			GameObject obj = E.GetGameObjectParameter("Defender");
			if (!Eat && GameObject.validate(ref obj))
			{
				if (E.GetIntParameter("Penetrations") > 0 && !IsBroken() && !IsRusted())
				{
					if (obj.HasPart("Stomach"))
					{
						GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
						InventoryActionEvent.Check(ParentObject, obj, ParentObject, "ApplyInvoluntarily", Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, 0, 0, gameObjectParameter);
					}
				}
				else
				{
					if (Visible())
					{
						IComponent<GameObject>.AddPlayerMessage(ParentObject.T() + ParentObject.GetVerb("fail") + " to penetrate " + Grammar.MakePossessive(obj.t()) + " armor and" + ParentObject.Is + " destroyed.");
					}
					ParentObject.Destroy(null, Silent: true);
				}
			}
		}
		return base.FireEvent(E);
	}
}
