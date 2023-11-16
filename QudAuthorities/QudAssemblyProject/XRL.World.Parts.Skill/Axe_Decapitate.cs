using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe_Decapitate : BaseSkill
{
	public Guid ActivatedAbilityID;

	public override void Attach()
	{
		if (ActivatedAbilityID == Guid.Empty)
		{
			AddAbility();
		}
		base.Attach();
	}

	public static bool ShouldDecapitate(GameObject who)
	{
		if (!(who?.GetPart("Axe_Decapitate") is Axe_Decapitate axe_Decapitate))
		{
			return false;
		}
		return axe_Decapitate.ShouldDecapitate();
	}

	public bool ShouldDecapitate()
	{
		if (!IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			return false;
		}
		return true;
	}

	public static bool Decapitate(GameObject Attacker, GameObject Defender, Cell Where = null, BodyPart LostPart = null, GameObject Weapon = null, GameObject Projectile = null, bool weaponActing = false)
	{
		Body body = Defender.Body;
		if (body == null)
		{
			return false;
		}
		if (!Defender.CanBeDismembered(Weapon))
		{
			return false;
		}
		if (!Defender.FireEvent("BeforeDecapitate"))
		{
			return false;
		}
		if (LostPart == null)
		{
			List<BodyPart> list = new List<BodyPart>(2);
			foreach (BodyPart part in body.GetParts())
			{
				if (part.IsSeverable() && part.SeverRequiresDecapitate())
				{
					list.Add(part);
				}
			}
			LostPart = list.GetRandomElement();
			if (LostPart == null)
			{
				return false;
			}
		}
		if (weaponActing && Weapon != null && Attacker != null)
		{
			IComponent<GameObject>.XDidYToZ(Weapon, "lop", "off", Defender, LostPart.GetOrdinalName(), "!", null, null, Defender, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true, Attacker);
		}
		else
		{
			IComponent<GameObject>.XDidYToZ(Attacker, "lop", "off", Defender, LostPart.GetOrdinalName(), "!", null, null, Defender, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true);
		}
		if (LostPart.Type == "Head")
		{
			Defender.ParticleText("*decapitated!*", IComponent<GameObject>.ConsequentialColorChar(null, Defender));
			if (Defender.IsPlayer())
			{
				AchievementManager.SetAchievement("ACH_GET_DECAPITATED");
			}
		}
		else
		{
			Defender.ParticleText("*dismembered!*", IComponent<GameObject>.ConsequentialColorChar(null, Defender));
		}
		body.Dismember(LostPart, Where);
		Defender.ApplyEffect(new Bleeding("1d2+1", 35, Attacker));
		Defender.RemoveAllEffects("Hooked");
		if (((Attacker != null && Attacker.IsPlayer()) || (Defender != null && Defender.IsPlayer())) && CombatJuice.enabled)
		{
			CombatJuice.cameraShake(0.5f);
		}
		if (body.AnyDismemberedMortalParts() && !body.AnyMortalParts())
		{
			Defender.Die(Attacker, null, "You were " + ((LostPart.Type == "Head") ? "decapitated" : "relieved of your vital anatomy") + " by " + Attacker.a + Attacker.ShortDisplayName + ".", Defender.It + Defender.GetVerb("were") + " @@" + ((LostPart.Type == "Head") ? "decapitated" : ("relieved of " + Defender.its + " vital anatomy")) + " by " + Attacker.a + Attacker.ShortDisplayName + ".", Accidental: false, Weapon, Projectile);
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == CommandEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandToggleDecapitate")
		{
			ToggleMyActivatedAbility(ActivatedAbilityID);
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject Object)
	{
		AddAbility();
		return base.AddSkill(Object);
	}

	public override bool RemoveSkill(GameObject Object)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(Object);
	}

	public void AddAbility()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Decapitate", "CommandToggleDecapitate", "Skill", "Toggles whether you will attempt to decapitate opponents when dismembering them.", "ö", null, Toggleable: true, DefaultToggleState: true);
	}
}
