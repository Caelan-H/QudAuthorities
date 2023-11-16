using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.Skills;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class AutomatedExternalDefibrillator : IPoweredPart
{
	public int Chance = 50;

	public string Damage = "1d4";

	public string DamageAttributes = "Electric";

	public string RequireSkill = "Firstaid";

	public AutomatedExternalDefibrillator()
	{
		ChargeUse = 500;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		AutomatedExternalDefibrillator automatedExternalDefibrillator = p as AutomatedExternalDefibrillator;
		if (automatedExternalDefibrillator.Chance != Chance)
		{
			return false;
		}
		if (automatedExternalDefibrillator.Damage != Damage)
		{
			return false;
		}
		if (automatedExternalDefibrillator.DamageAttributes != DamageAttributes)
		{
			return false;
		}
		if (automatedExternalDefibrillator.RequireSkill != RequireSkill)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Defibrillate", "activate", "Defibrillate", null, 'a');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Defibrillate")
		{
			AttemptDefibrillate(E.Actor, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(RequireSkill))
		{
			if (Chance >= 100)
			{
				E.Postfix.AppendRules("Stops cardiac arrest.");
			}
			else
			{
				E.Postfix.AppendRules(Chance + "% chance to stop cardiac arrest.");
			}
			if (!string.IsNullOrEmpty(Damage))
			{
				E.Postfix.AppendRules("Does " + Damage + ((!string.IsNullOrEmpty(DamageAttributes)) ? (" " + DamageAttributes.ToLower()) : "") + " damage per application.");
			}
			E.Postfix.AppendRules("Requires training in " + SkillFactory.GetSkillOrPowerName(RequireSkill) + " to use.");
		}
		return base.HandleEvent(E);
	}

	public bool AttemptDefibrillate(GameObject who, MinEvent E = null)
	{
		if (!who.CanMoveExtremities("Defibrillate", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(RequireSkill) && !who.HasSkill(RequireSkill))
		{
			if (who.IsPlayer())
			{
				Popup.Show("You don't know how to use " + ParentObject.t() + ".");
			}
			return false;
		}
		Cell cell = who.PickDirection();
		if (cell == null)
		{
			return false;
		}
		GameObject obj = FindOptimalTarget(cell, who);
		if (obj == null)
		{
			obj = FindAlternateTarget(cell, who);
			if (obj == null)
			{
				if (who.IsPlayer())
				{
					if (cell.GetCombatTarget(who, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5) == null)
					{
						Popup.Show("There is no one there to use " + ParentObject.t() + " on.");
					}
					else
					{
						Popup.Show("There is no one there you can use " + ParentObject.t() + " on.");
					}
				}
				return false;
			}
			string message = ((obj != who) ? (obj.T() + obj.Is + " not in cardiac arrest. Do you want to use " + ParentObject.t() + " on " + obj.them + " anyway?") : ("You are not in cardiac arrest. Do you want to use " + ParentObject.t() + " on " + who.itself + " anyway?"));
			if (Popup.ShowYesNo(message) != 0)
			{
				return false;
			}
		}
		E.RequestInterfaceExit();
		who.UseEnergy(1000, "Item Medical Defibrillator");
		if (obj != who && obj.IsHostileTowards(who) && obj.IsMobile())
		{
			int combatDV = Stats.GetCombatDV(obj);
			if (Stat.Random(1, 20) + who.StatMod("Agility") < combatDV)
			{
				IComponent<GameObject>.WDidXToYWithZ(who, "try", "to use", ParentObject, "on", obj, ", but " + obj.it + obj.GetVerb("dodge", PrependSpace: true, PronounAntecedent: true), "!", null, null, who, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: false, indefiniteIndirectObject: false, indefiniteDirectObjectForOthers: false, indefiniteIndirectObjectForOthers: false, possessiveDirectObject: false, possessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
				return false;
			}
		}
		IComponent<GameObject>.WDidXToYWithZ(who, "use", ParentObject, "on", obj, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: false, indefiniteIndirectObject: false, indefiniteDirectObjectForOthers: false, indefiniteIndirectObjectForOthers: false, possessiveDirectObject: false, possessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		if (!string.IsNullOrEmpty(Damage))
		{
			GameObject gameObject = obj;
			int amount = Damage.RollCached();
			string damageAttributes = DamageAttributes;
			bool accidental = obj.HasEffect("CardiacArrest");
			gameObject.TakeDamage(amount, "from the device.", damageAttributes, null, null, null, who, null, null, accidental);
		}
		if (GameObject.validate(ref obj) && !obj.IsInGraveyard() && obj.HasEffect("CardiacArrest") && Chance.in100())
		{
			obj.RemoveEffect("CardiacArrest");
		}
		return true;
	}

	private static GameObject FindOptimalTarget(Cell C, GameObject who)
	{
		foreach (GameObject @object in C.Objects)
		{
			if (@object.HasEffect("CardiacArrest") && who.PhaseAndFlightMatches(@object))
			{
				return @object;
			}
		}
		return null;
	}

	private static GameObject FindAlternateTarget(Cell C, GameObject who)
	{
		return C.GetCombatTarget(who);
	}
}
