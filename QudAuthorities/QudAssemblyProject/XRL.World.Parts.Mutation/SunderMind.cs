using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SunderMind : BaseMutation
{
	public bool RealityDistortionBased;

	public new Guid ActivatedAbilityID = Guid.Empty;

	public string SuccessSound = "sunder_mind";

	public string FailureSound = "sunder_mind_fail";

	public GameObjectReference activeTarget;

	public int activeRounds;

	public int totalDamage;

	public int accruedDamage;

	public SunderMind()
	{
		DisplayName = "Sunder Mind";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != GetEnergyCostEvent.ID)
		{
			return ID == TookDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (activeRounds > 0 && activeTarget.go() != null)
		{
			try
			{
				Tick();
			}
			catch (Exception x)
			{
				MetricsManager.LogException("SunderMind::BeforeTakeAction", x);
			}
		}
		else
		{
			EndSunder();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (activeRounds > 0 && E.Type != "Pass")
		{
			CancelSunder();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (activeRounds > 0 && (E.Damage.HasAttribute("Mental") || E.Damage.HasAttribute("Psionic")))
		{
			CancelSunder();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandSunderMind");
		base.Register(Object);
	}

	public string GetDamageDice(int Level)
	{
		if (Level < 3)
		{
			if (Level % 2 == 1)
			{
				return "1d3";
			}
			return "1d4";
		}
		if (Level % 2 == 1)
		{
			return "1d3+" + Level / 2;
		}
		return "1d4+" + (Level - 1) / 2;
	}

	public void CancelSunder()
	{
		if (ParentObject.IsPlayer() && activeTarget != null && activeTarget.go() != null)
		{
			IComponent<GameObject>.AddPlayerMessage("Your concentration slips and the channel between you and " + activeTarget.go().t() + " dissipates into aether.");
		}
		else if (ParentObject.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Your concentration slips and the channel dissipates.");
		}
		EndSunder();
	}

	public void EndSunder()
	{
		activeRounds = 0;
		if (activeTarget != null)
		{
			GameObject gameObject = activeTarget.go();
			if (gameObject != null)
			{
				MemberOfPsychicBattle effect = gameObject.GetEffect<MemberOfPsychicBattle>();
				if (effect != null && !effect.isAttacker)
				{
					gameObject.RemoveEffect("MemberOfPsychicBattle");
				}
			}
			activeTarget.free();
			activeTarget = null;
		}
		MemberOfPsychicBattle effect2 = ParentObject.GetEffect<MemberOfPsychicBattle>();
		if (effect2 != null && effect2.isAttacker)
		{
			ParentObject.RemoveEffect("MemberOfPsychicBattle");
		}
	}

	public int GetCooldown(int Level)
	{
		return 80;
	}

	public override string GetDescription()
	{
		return "You sunder the mind of an enemy, leaving them reeling in pain.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("" + "For up to 10 rounds, you engage in psychic combat with an opponent, dealing damage each round.\n", "Taking any action other than passing the turn will break the connection.\n"), "Each round you make a mental attack vs mental armor (MA).\n"), "Damage increment: {{rules|", GetDamageDice(Level), "}}\n"), "After the tenth round, you deal bonus damage equal to the total amount of damage you've done so far.\n"), "Range: sight\n"), "Cooldown: ", GetCooldown(Level).ToString(), " rounds");
	}

	public void BeginSunder(GameObject target)
	{
		accruedDamage = 0;
		if (target != null && target.HasPart("Brain") && !target.HasEffect("MemberOfPsychicBattle") && target.ApplyEffect(new MemberOfPsychicBattle(ParentObject, isAttacker: false)))
		{
			activeRounds = 10;
			activeTarget = target.takeReference();
			ParentObject.ApplyEffect(new MemberOfPsychicBattle(target, isAttacker: true));
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You burrow a channel through the psychic aether to " + target.t() + " and begin to sunder " + target.its + " mind!");
			}
			else if (target.IsPlayer())
			{
				Popup.Show(ParentObject.T() + " " + The.Player.DescribeDirectionToward(ParentObject) + ParentObject.GetVerb("burrow") + " a channel through the psychic aether and" + ParentObject.GetVerb("begin") + " to sunder your mind!");
				AutoAct.Interrupt(null, null, ParentObject);
			}
			Tick();
		}
	}

	public void Tick()
	{
		List<Effect> effects = ParentObject.Effects;
		if (effects != null && effects.Any((Effect e) => e.IsOfTypes(33554434)))
		{
			CancelSunder();
			return;
		}
		MemberOfPsychicBattle effect = ParentObject.GetEffect<MemberOfPsychicBattle>();
		if (effect != null)
		{
			effect.turnsRemaining = activeRounds;
			Nosebleed(ParentObject, effect);
			GameObject gameObject = activeTarget.go();
			if (gameObject == null || gameObject.IsInvalid() || !gameObject.InSameZone(ParentObject))
			{
				CancelSunder();
				return;
			}
			MemberOfPsychicBattle effect2 = ParentObject.GetEffect<MemberOfPsychicBattle>();
			if (effect2 != null)
			{
				effect2.turnsRemaining = activeRounds;
				Nosebleed(gameObject, effect2);
				if (activeRounds > 1)
				{
					if (ParentObject.IsPlayer() || Visible())
					{
						ParentObject.ParticleBlip("&Y~", (10 - activeRounds) / 2);
					}
					PerformMentalAttack(Blast, ParentObject, gameObject, null, "SunderMind Blast", null, 8388609, 100, int.MinValue, Math.Max(ParentObject.StatMod("Ego"), base.Level - 2));
				}
				else
				{
					int num = accruedDamage;
					if (Options.SifrahPsychicCombat)
					{
						if (ParentObject.IsPlayer())
						{
							PsychicCombatSifrah psychicCombatSifrah = new PsychicCombatSifrah(gameObject, "SunderMind", base.Level, gameObject.Stat("Level") / 5 + gameObject.StatMod("Willpower"), "sundering the mind of " + gameObject.an());
							psychicCombatSifrah.Play(gameObject);
							num = num * (psychicCombatSifrah.Performance + 100) / 100;
						}
						else if (gameObject.IsPlayer())
						{
							PsychicCombatSifrah psychicCombatSifrah2 = new PsychicCombatSifrah(ParentObject, "SunderMind", gameObject.Stat("Level") / 5 + gameObject.StatMod("Willpower"), base.Level, "defending against having your mind sundered by " + gameObject.an());
							psychicCombatSifrah2.Play(ParentObject);
							num = num * (100 - psychicCombatSifrah2.Performance) / 100;
						}
					}
					if (num > gameObject.hitpoints)
					{
						if (gameObject.IsPlayer())
						{
							Popup.Show("Your sense of self is pulled apart by what feels like a billion years of geologic pressure.");
							Popup.Show("Your head explodes!");
							AchievementManager.SetAchievement("ACH_HEAD_EXPLODE");
						}
						else if (gameObject.IsVisible())
						{
							IComponent<GameObject>.AddPlayerMessage(gameObject.Poss("head") + " explodes!");
						}
						gameObject.BigBloodsplatter();
					}
					if (num > 0)
					{
						gameObject.TakeDamage(num, "from the cumulative trauma of %t mental assault!", "Mental Psionic Unavoidable", "Your head was exploded by " + ParentObject.an() + ".", gameObject.Its + " head was exploded by " + ParentObject.an() + ".", Attacker: ParentObject, Owner: ParentObject, Source: ParentObject);
					}
				}
				activeRounds--;
				if (activeRounds <= 0)
				{
					EndSunder();
				}
			}
			else
			{
				CancelSunder();
			}
		}
		else
		{
			CancelSunder();
		}
	}

	public void Nosebleed(GameObject Target, MemberOfPsychicBattle TargetEffect)
	{
		if (!Target.IsValid() || Target.HasPart("MentalShield") || TargetEffect.turnsRemaining > 5)
		{
			return;
		}
		bool flag = Target.CanHaveNosebleed();
		string text = (flag ? "nosebleed" : "hemorrhage");
		if (TargetEffect.turnsRemaining == 5)
		{
			if (flag)
			{
				IComponent<GameObject>.EmitMessage(Target, Target.Poss("nose begins to bleed."));
			}
			else
			{
				IComponent<GameObject>.EmitMessage(Target, Target.Poss("brain begins to hemorrhage."));
			}
		}
		Target.TakeDamage(1, DeathReason: "You died from a " + text + ".", Attacker: ParentObject, Owner: ParentObject, Message: "from a " + text + ".", Attributes: "Physical Circulatory Unavoidable");
	}

	public int RollDamage(GameObject GO, int Penetrations)
	{
		int num = 0;
		string damageDice = GetDamageDice(base.Level);
		for (int i = 0; i < Penetrations; i++)
		{
			num += damageDice.RollCached();
		}
		return num;
	}

	public void PenetrationFailure(GameObject GO)
	{
		GO.pPhysics?.PlayWorldSound(FailureSound, 0.5f, 0f, combat: true);
		if (ParentObject.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Your attack fails to penetrate " + GO.poss("mental defenses."), 'r');
		}
	}

	public bool Blast(MentalAttackEvent E)
	{
		GameObject defender = E.Defender;
		int penetrations = E.Penetrations;
		if (penetrations == 0)
		{
			PenetrationFailure(defender);
			return false;
		}
		int num = RollDamage(defender, penetrations) * 100 / E.Magnitude;
		if (num <= 0)
		{
			PenetrationFailure(defender);
			return false;
		}
		accruedDamage += num;
		Damage damage = new Damage(num);
		damage.AddAttribute("Mental");
		damage.AddAttribute("Psionic");
		string resultColor = Stat.GetResultColor(penetrations);
		resultColor = resultColor.Replace("&", "");
		Cell cell = defender.CurrentCell;
		Event @event = Event.New("TakeDamage");
		@event.SetParameter("Damage", damage);
		@event.SetParameter("Owner", ParentObject);
		@event.SetParameter("Attacker", ParentObject);
		@event.SetParameter("Perspective", ParentObject);
		@event.SetParameter("Phase", 5);
		@event.SetParameter("NoDamageMessage", 1);
		@event.SetParameter("SilentIfNoDamage", 1);
		if (E.IsPlayerInvolved())
		{
			@event.SetParameter("Message", Event.NewStringBuilder("{{").Append(ColorCoding.ConsequentialColorChar(E.Attacker, E.Defender)).Append('|')
				.Append(E.Attacker.IsPlayer() ? "You" : E.Attacker.T())
				.Append(' ')
				.Append(E.Attacker.GetVerb("sunder", PrependSpace: false))
				.Append(' ')
				.Append(E.Defender.poss("mind"))
				.Append("{{")
				.Append(resultColor)
				.Append("|(x")
				.Append(penetrations)
				.Append(")}} for %d damage!}}")
				.ToString());
		}
		if (!defender.FireEvent(@event) || damage.Amount == 0)
		{
			cell?.PlayWorldSound(FailureSound, 0.5f, 0f, Combat: true);
			if (E.IsPlayerInvolved())
			{
				IComponent<GameObject>.XDidYToZ(E.Attacker, "attempt", "to burrow a channel through the psychic aether and sunder", E.Defender, "mind, but the attack has no effect.", ".", null, E.Defender, E.Attacker, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true);
			}
		}
		else if (cell != null)
		{
			cell.PlayWorldSound(SuccessSound, 0.5f, 0f, Combat: true);
			cell.PsychicPulse();
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			if (intParameter <= 80 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
				if (activeRounds <= 0 && gameObjectParameter != null && (!gameObjectParameter.HasPart("SunderMind") || gameObjectParameter.GetPart<SunderMind>().activeRounds <= 0) && !gameObjectParameter.HasEffect("MemberOfPsychicBattle") && gameObjectParameter.HasPart("Brain") && !gameObjectParameter.HasPart("MentalShield") && (ParentObject.HasLOSTo(gameObjectParameter, IncludeSolid: false) || (ParentObject.HasPart("SensePsychic") && intParameter < ParentObject.GetPart<SensePsychic>().Radius)) && (!RealityDistortionBased || (ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)) && gameObjectParameter.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)))))
				{
					E.AddAICommand("CommandSunderMind");
				}
			}
		}
		else if (E.ID == "CommandSunderMind")
		{
			if (RealityDistortionBased && !ParentObject.IsRealityDistortionUsable())
			{
				RealityStabilized.ShowGenericInterdictMessage(ParentObject);
				return false;
			}
			Cell cell = PickDestinationCell(80, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, PickTarget.PickStyle.EmptyCell, null, Snap: true, HasOurSensePsychic);
			if (cell == null)
			{
				return false;
			}
			GameObject gameObject = cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
			if (gameObject != null && gameObject.pBrain == null)
			{
				gameObject = null;
			}
			if (gameObject == null)
			{
				gameObject = cell.GetFirstObjectWithPart("Brain");
			}
			bool flag = false;
			if (gameObject != null && !gameObject.IsVisible())
			{
				SensePsychicEffect ourSensePsychic = GetOurSensePsychic(gameObject);
				if (ourSensePsychic == null)
				{
					gameObject = null;
				}
				else if (!ourSensePsychic.Identified)
				{
					flag = true;
					gameObject = null;
				}
			}
			if (gameObject == ParentObject && gameObject.IsPlayer() && Popup.ShowYesNo("Are you sure you want to sunder your own mind?") == DialogResult.No)
			{
				return false;
			}
			if (gameObject == null)
			{
				if (ParentObject.IsPlayer())
				{
					if (flag)
					{
						Popup.ShowFail("You cannot grasp the mind there sufficiently to sunder it.");
					}
					else
					{
						Popup.ShowFail("There's no target with a mind there.");
					}
				}
				return false;
			}
			EndSunder();
			if (gameObject.HasEffect("MemberOfPsychicBattle") || (gameObject.HasPart("SunderMind") && gameObject.GetPart<SunderMind>().activeRounds > 0))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("That target is already locked in psychic battle!");
				}
				return false;
			}
			if (RealityDistortionBased)
			{
				if (cell != ParentObject.CurrentCell)
				{
					Event e = Event.New("InitiateRealityDistortionTransit", "Object", ParentObject, "Mutation", this, "Cell", cell);
					if (!ParentObject.FireEvent(e, E) || !cell.FireEvent(e, E))
					{
						return false;
					}
				}
				else if (!ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this), E))
				{
					return false;
				}
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(base.Level));
			UseEnergy(1000, "Mental Mutation Sunder Mind");
			BeginSunder(gameObject);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Sunder Mind", "CommandSunderMind", "Mental Mutation", null, "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}

	private bool IsOurSensePsychic(Effect FX)
	{
		if (FX is SensePsychicEffect sensePsychicEffect)
		{
			return sensePsychicEffect.Listener == ParentObject;
		}
		return false;
	}

	private bool HasOurSensePsychic(GameObject GO)
	{
		return GO.HasEffect("SensePsychicEffect", IsOurSensePsychic);
	}

	private SensePsychicEffect GetOurSensePsychic(GameObject GO)
	{
		return GO.GetEffect("SensePsychicEffect", IsOurSensePsychic) as SensePsychicEffect;
	}
}
