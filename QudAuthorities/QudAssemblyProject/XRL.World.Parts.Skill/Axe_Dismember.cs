using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe_Dismember : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public bool bDismembering;

	[NonSerialized]
	private static List<BodyPart> DismemberableBodyParts = new List<BodyPart>(8);

	public Axe_Dismember()
	{
	}

	public Axe_Dismember(GameObject ParentObject)
		: this()
	{
		this.ParentObject = ParentObject;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AttackerAfterDamage");
		Object.RegisterPartEvent(this, "CommandDismember");
		base.Register(Object);
	}

	public static bool Dismember(GameObject Attacker, GameObject Defender, Cell Where = null, BodyPart LostPart = null, GameObject Weapon = null, GameObject Projectile = null, bool assumeDecapitate = false, bool suppressDecapitate = false, bool weaponActing = false)
	{
		if (LostPart == null)
		{
			LostPart = GetDismemberableBodyPart(Defender, Attacker, Weapon, assumeDecapitate, suppressDecapitate);
			if (LostPart == null)
			{
				return false;
			}
		}
		if (Where != null && (!GameObject.validate(ref Defender) || Defender.CurrentCell != Where) && Where.IsOccluding())
		{
			Where = Where.GetFirstNonOccludingAdjacentCell() ?? Where;
		}
		if (LostPart.SeverRequiresDecapitate())
		{
			return Axe_Decapitate.Decapitate(Attacker, Defender, Where, LostPart, Weapon, Projectile, weaponActing);
		}
		if (Defender.Body.Dismember(LostPart, Where) == null)
		{
			return false;
		}
		if (weaponActing && Weapon != null && Attacker != null)
		{
			IComponent<GameObject>.XDidYToZ(Weapon, "chop", "off", Defender, LostPart.GetOrdinalName(), "!", null, null, Defender, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true, Attacker);
		}
		else
		{
			IComponent<GameObject>.XDidYToZ(Attacker, "chop", "off", Defender, LostPart.GetOrdinalName(), "!", null, null, Defender, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true);
		}
		Defender.ParticleText("*dismembered!*", IComponent<GameObject>.ConsequentialColorChar(null, Defender));
		Defender.ApplyEffect(new Bleeding("1d2", 35, Attacker));
		Defender.RemoveAllEffects("Hooked");
		if (((Attacker != null && Attacker.IsPlayer()) || (Defender != null && Defender.IsPlayer())) && CombatJuice.enabled)
		{
			CombatJuice.cameraShake(0.25f);
		}
		return true;
	}

	public GameObject GetPrimaryAxe()
	{
		return ParentObject.GetPrimaryWeaponOfType("Axe");
	}

	public bool IsPrimaryAxeEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("Axe");
	}

	public static bool BodyPartIsDismemberable(BodyPart Part, GameObject Actor = null, bool assumeDecapitate = false, bool suppressDecapitate = false)
	{
		if (!Part.IsSeverable())
		{
			return false;
		}
		if (Part.SeverRequiresDecapitate())
		{
			if (suppressDecapitate)
			{
				return false;
			}
			if (!assumeDecapitate && !Axe_Decapitate.ShouldDecapitate(Actor))
			{
				return false;
			}
		}
		return true;
	}

	public static BodyPart GetDismemberableBodyPart(GameObject obj, GameObject Actor = null, GameObject Weapon = null, bool assumeDecapitate = false, bool suppressDecapitate = false)
	{
		Body body = obj.Body;
		if (body == null)
		{
			return null;
		}
		if (!obj.CanBeDismembered(Weapon))
		{
			return null;
		}
		DismemberableBodyParts.Clear();
		foreach (BodyPart part in body.GetParts())
		{
			if (BodyPartIsDismemberable(part, Actor, assumeDecapitate, suppressDecapitate))
			{
				DismemberableBodyParts.Add(part);
			}
		}
		return DismemberableBodyParts.GetRandomElement();
	}

	public static bool HasAnyDismemberableBodyPart(GameObject obj, GameObject Actor = null, GameObject Weapon = null, bool assumeDecapitate = false, bool suppressDecapitate = false)
	{
		Body body = obj.Body;
		if (body == null)
		{
			return false;
		}
		if (!obj.CanBeDismembered(Weapon))
		{
			return false;
		}
		foreach (BodyPart part in body.GetParts())
		{
			if (BodyPartIsDismemberable(part, Actor, assumeDecapitate, suppressDecapitate))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CastForceSuccess(GameObject attacker, Axe_Dismember skill = null, GameObject weapon = null)
	{
		if (skill == null)
		{
			skill = new Axe_Dismember(attacker);
		}
		if (weapon == null)
		{
			weapon = skill.GetPrimaryAxe();
		}
		Cell cell = skill.PickDirection();
		if (cell != null)
		{
			GameObject combatTarget = cell.GetCombatTarget(attacker);
			if (combatTarget != null)
			{
				Dismember(attacker, combatTarget, null, null, weapon);
			}
		}
		return true;
	}

	public static bool Cast(GameObject attacker, Axe_Dismember skill = null, GameObject weapon = null)
	{
		bool flag = false;
		if (skill == null)
		{
			skill = new Axe_Dismember(attacker);
			attacker.RegisterPartEvent(skill, "AttackerAfterDamage");
			flag = true;
		}
		if (weapon == null)
		{
			weapon = skill.GetPrimaryAxe();
		}
		if (attacker.CanMoveExtremities("Dismember", ShowMessage: true) && attacker.CanChangeBodyPosition("Dismember", ShowMessage: true))
		{
			Cell cell = skill.PickDirection();
			if (cell != null)
			{
				GameObject combatTarget = cell.GetCombatTarget(attacker);
				if (combatTarget == null)
				{
					if (attacker.IsPlayer())
					{
						if (cell.HasObjectWithPart("Combat"))
						{
							Popup.Show("There's nothing there you can dismember.");
						}
						else
						{
							Popup.Show("There's nothing there to dismember.");
						}
					}
				}
				else
				{
					try
					{
						skill.bDismembering = true;
						if (combatTarget == attacker && attacker.IsPlayer() && Popup.ShowYesNo("Are you sure you want to dismember " + attacker.itself + "?") != 0)
						{
							return true;
						}
						Event @event = Event.New("MeleeAttackWithWeapon");
						@event.SetParameter("Attacker", attacker);
						@event.SetParameter("Defender", combatTarget);
						@event.SetParameter("Weapon", weapon);
						attacker.FireEvent(@event);
						attacker.UseEnergy(1000, "Skill Axe Dismember");
						if (!combatTarget.IsHostileTowards(IComponent<GameObject>.ThePlayer))
						{
							combatTarget.GetAngryAt(IComponent<GameObject>.ThePlayer);
						}
						skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, 30);
					}
					catch (Exception ex)
					{
						XRLCore.LogError("Dismember", ex);
					}
					finally
					{
						skill.bDismembering = false;
					}
				}
			}
		}
		if (flag)
		{
			attacker.UnregisterPartEvent(skill, "AttackerAfterDamage");
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerAfterDamage")
		{
			if (E.GetIntParameter("Penetrations") > 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				GameObject obj = E.GetGameObjectParameter("Weapon");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
				if (GameObject.validate(ref obj) && obj.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon && meleeWeapon.Skill == "Axe")
				{
					Cell where = E.GetParameter("Cell") as Cell;
					if (bDismembering || gameObjectParameter.HasEffect("Berserk"))
					{
						return Dismember(gameObjectParameter, gameObjectParameter2, where, null, obj);
					}
					int num = 3;
					if ((E.GetStringParameter("Properties", "") ?? "").Contains("Charging") && gameObjectParameter.HasSkill("Axe_ChargingStrike"))
					{
						num *= 2;
					}
					if (obj.pPhysics != null && obj.pPhysics.UsesTwoSlots)
					{
						num *= 2;
					}
					GameObject @object = obj;
					GameObject subject = gameObjectParameter2;
					num = GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, @object, "Skill Dismember", num, subject);
					if (num.in100())
					{
						return Dismember(gameObjectParameter, gameObjectParameter2);
					}
				}
			}
		}
		else if (E.ID == "CommandDismember")
		{
			GameObject primaryAxe = GetPrimaryAxe();
			if (primaryAxe == null)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("You must have an axe equipped in your primary hand to dismember.");
				}
				return true;
			}
			if (!ParentObject.CheckFrozen())
			{
				return true;
			}
			return Cast(ParentObject, this, primaryAxe);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Dismember", "CommandDismember", "Skill", "You make an attack with your axe at an adjacent opponent. If you hit and penetrate at least once, you dismember one of their limbs and they start bleeding (1-2 damage per turn. toughness save; difficulty 35). Additionally, your axe attacks that penetrate have a percentage chance to dismember: 3% for one-handed axes and 6% for two-handed axes.", "-");
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
