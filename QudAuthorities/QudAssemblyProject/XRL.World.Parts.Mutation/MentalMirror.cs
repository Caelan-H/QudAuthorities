using System;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class MentalMirror : BaseMutation
{
	public new Guid ActivatedAbilityID;

	[NonSerialized]
	public bool Active;

	public MentalMirror()
	{
		DisplayName = "Mental Mirror";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetItemElementsEvent.ID)
		{
			return ID == BeforeMentalDefendEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("glass", 1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeMentalDefendEvent E)
	{
		if (!CheckActive() || E.Attacker == ParentObject)
		{
			return true;
		}
		if (E.Reflectable && E.Penetrations <= 0)
		{
			E.Penetrations = RollReflection(E);
			if (E.Penetrations > 0)
			{
				ReflectMessage(E.Attacker);
				E.Reflected = true;
				E.Defender = E.Attacker;
			}
		}
		else if (E.Penetrations > 0)
		{
			ShatterMessage(E.Attacker);
		}
		Activate();
		return base.HandleEvent(E);
	}

	public int RollReflection(IMentalAttackEvent E)
	{
		int combatMA = Stats.GetCombatMA(E.Attacker);
		int num = Math.Max(ParentObject.StatMod("Willpower"), base.Level);
		return Stat.RollPenetratingSuccesses("1d8", combatMA, num + E.Modifier);
	}

	public override string GetDescription()
	{
		return "You reflect mental attacks back at your attackers.";
	}

	public int GetMABonus(int Level)
	{
		return 3 + Level;
	}

	public int GetCooldown(int Level)
	{
		return 50;
	}

	public bool UpdateStatShifts()
	{
		if (Active)
		{
			base.StatShifter.SetStatShift("MA", GetMABonus(base.Level));
		}
		else
		{
			base.StatShifter.RemoveStatShifts();
		}
		return true;
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("When you suffer a mental attack while Mental Mirror is off cooldown, you gain +{{rules|" + GetMABonus(Level) + "}} mental armor (MA).\n", "If the attack then fails to penetrate your MA, it's reflected back at your attacker.\n"), "Cooldown: ", GetCooldown(Level).ToString());
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "DefenderAfterAttack");
		Object.RegisterPartEvent(this, "ReflectProjectile");
		base.Register(Object);
	}

	public bool CheckActive()
	{
		bool flag = GetMyActivatedAbilityCooldown(ActivatedAbilityID) <= 0;
		if (flag != Active)
		{
			Active = flag;
			UpdateStatShifts();
		}
		return flag;
	}

	public void Activate()
	{
		CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(base.Level));
		CheckActive();
	}

	public void ReflectMessage(GameObject Actor)
	{
		if (ParentObject.IsPlayer() || Actor.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage(ParentObject.Poss("mental mirror reflects the attack!"));
		}
	}

	public void ShatterMessage(GameObject Actor)
	{
		if (ParentObject.IsPlayer() || Actor.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage(ParentObject.Poss("mental mirror shatters!"));
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DefenderAfterAttack" && CheckActive())
		{
			if (E.GetParameter("Damage") is Damage damage && damage.HasAttribute("Mental"))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				if (gameObjectParameter == null)
				{
					return true;
				}
				if (E.GetIntParameter("Penetrations") <= 0)
				{
					GameObject gameObjectParameter2 = E.GetGameObjectParameter("Reflector");
					Event @event = new Event("MeleeAttackWithWeapon");
					@event.SetParameter("Attacker", gameObjectParameter);
					@event.SetParameter("Defender", gameObjectParameter2 ?? gameObjectParameter);
					@event.SetParameter("Reflector", ParentObject);
					@event.SetParameter("Weapon", E.GetGameObjectParameter("Weapon"));
					@event.SetParameter("Properties", E.GetStringParameter("Properties"));
					ReflectMessage(gameObjectParameter);
					gameObjectParameter.FireEvent(@event);
				}
				else
				{
					ShatterMessage(gameObjectParameter);
				}
				Activate();
			}
		}
		else if (E.ID == "ReflectProjectile" && CheckActive() && E.GetGameObjectParameter("Projectile")?.GetPart("Projectile") is Projectile projectile && projectile.Attributes.Contains("Mental"))
		{
			Activate();
			if (80.in100())
			{
				float num = (float)E.GetParameter("Angle");
				E.SetParameter("Direction", (int)num + 180);
				E.SetParameter("By", ParentObject);
				E.SetParameter("Verb", "reflect");
				return false;
			}
			ShatterMessage(E.GetGameObjectParameter("Attacker"));
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		UpdateStatShifts();
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Mental Mirror", "CommandMentalMirror", "Mental Mutation", null, "m");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		base.StatShifter.RemoveStatShifts();
		return base.Unmutate(GO);
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
		if (!Active)
		{
			CheckActive();
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (!Active)
		{
			CheckActive();
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (!Active)
		{
			CheckActive();
		}
	}
}
