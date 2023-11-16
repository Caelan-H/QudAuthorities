using System;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class LongBladesCore : IPart
{
	public const string SUPPORT_TYPE = "LongBladesCore";

	public static readonly string STR_DEFENSIVE = "defensive";

	public static readonly string STR_AGGRESSIVE = "aggressive";

	public static readonly string STR_DUELIST = "dueling";

	public int Ultmode;

	public string currentStance = "";

	public Guid AggressiveStanceID = Guid.Empty;

	public Guid DefensiveStanceID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == NeedPartSupportEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(NeedPartSupportEvent E)
	{
		if (E.Type == "LongBladesCore" && !PartSupportEvent.Check(E, this))
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "AttackerRollMeleeToHit");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CommandAggressiveStance");
		Object.RegisterPartEvent(this, "CommandDeathblow");
		Object.RegisterPartEvent(this, "CommandDefensiveStance");
		Object.RegisterPartEvent(this, "CommandDuelingStance");
		Object.RegisterPartEvent(this, "CommandLunge");
		Object.RegisterPartEvent(this, "CommandSwipe");
		Object.RegisterPartEvent(this, "EquipperEquipped");
		Object.RegisterPartEvent(this, "EquipperUnequipped");
		Object.RegisterPartEvent(this, "GetAttackerHitDice");
		Object.RegisterPartEvent(this, "GetDefenderDV");
		Object.RegisterPartEvent(this, "LongBlades.UpdateStance");
		Object.RegisterPartEvent(this, "PrimaryLimbRecalculated");
		base.Register(Object);
	}

	public override void Initialize()
	{
		base.Initialize();
		if (AggressiveStanceID == Guid.Empty)
		{
			AggressiveStanceID = AddMyActivatedAbility("Aggressive Stance", "CommandAggressiveStance", "Stances", "+1/2 penetration, -2/-3 to hit while wielding a long blade in your primary hand", "\u009f");
		}
		if (DefensiveStanceID == Guid.Empty)
		{
			DefensiveStanceID = AddMyActivatedAbility("Defensive Stance", "CommandDefensiveStance", "Stances", "+2/3 DV while wielding a long blade in your primary hand", "\u009f");
		}
		if (string.IsNullOrEmpty(currentStance))
		{
			ChangeStance(ParentObject.GetPropertyOrTag("InitialStance") ?? STR_DEFENSIVE);
		}
	}

	public override void Remove()
	{
		RemoveMyActivatedAbility(ref AggressiveStanceID);
		RemoveMyActivatedAbility(ref DefensiveStanceID);
		ParentObject.RemoveEffect("LongbladeStance_Aggressive");
		ParentObject.RemoveEffect("LongbladeStance_Defensive");
		ParentObject.RemoveEffect("LongbladeStance_Dueling");
		ParentObject.RemoveEffect("LongbladeEffect_EnGarde");
		base.Remove();
	}

	public GameObject GetPrimaryBlade()
	{
		return ParentObject.GetPrimaryWeaponOfType("LongBlades");
	}

	public bool IsPrimaryBladeEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("LongBlades");
	}

	public void ChangeStance(string newStance)
	{
		if (currentStance == STR_DEFENSIVE)
		{
			base.StatShifter.RemoveStatShifts();
		}
		if (newStance != currentStance)
		{
			currentStance = newStance;
			if (Visible())
			{
				if (!ParentObject.IsPlayer() || base.MyActivatedAbilities == null || !base.MyActivatedAbilities.Silent)
				{
					DidX("switch", "to " + newStance + " stance", null, null, ParentObject);
				}
				Cell cell = ParentObject.GetCurrentCell();
				if (cell != null)
				{
					int x = cell.X;
					int y = cell.Y;
					string text = "&W";
					if (currentStance == STR_DUELIST)
					{
						text = "&W";
					}
					else if (currentStance == STR_AGGRESSIVE)
					{
						text = "&R";
					}
					else if (currentStance == STR_DEFENSIVE)
					{
						text = "&G";
					}
					for (int i = 0; i < 8; i++)
					{
						The.ParticleManager.AddRadial(text + ".", x, y, (float)(i * 45) / 360f * 6.14f, Stat.Random(2, 4), -0.035f * (float)Stat.Random(8, 12), -0.3f + -0.15f * (float)Stat.Random(1, 3), 40);
					}
				}
			}
		}
		if (currentStance == STR_DEFENSIVE && IsPrimaryBladeEquipped())
		{
			int num = 2;
			if (ParentObject.HasPart("LongBladesImprovedDefensiveStance"))
			{
				num++;
			}
			base.StatShifter.SetStatShift("DV", num);
		}
		if (currentStance == STR_AGGRESSIVE)
		{
			ParentObject.ApplyEffect(new LongbladeStance_Aggressive());
		}
		else if (currentStance == STR_DUELIST)
		{
			ParentObject.ApplyEffect(new LongbladeStance_Dueling());
		}
		else if (currentStance == STR_DEFENSIVE)
		{
			ParentObject.ApplyEffect(new LongbladeStance_Defensive());
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (IsPrimaryBladeEquipped())
			{
				string text = STR_AGGRESSIVE;
				if (ParentObject.isDamaged(0.6))
				{
					text = STR_DEFENSIVE;
				}
				else if (ParentObject.HasPart("LongBladesDuelingStance") && (double)ConTarget() >= 0.8)
				{
					text = STR_DUELIST;
				}
				if (currentStance != text)
				{
					if (text == STR_AGGRESSIVE)
					{
						if (IsMyActivatedAbilityAIUsable(AggressiveStanceID))
						{
							ParentObject.FireEvent("CommandAggressiveStance");
						}
					}
					else if (text == STR_DEFENSIVE)
					{
						if (IsMyActivatedAbilityAIUsable(DefensiveStanceID))
						{
							ParentObject.FireEvent("CommandDefensiveStance");
						}
					}
					else if (text == STR_DUELIST && ParentObject.GetPart("LongBladesDuelingStance") is LongBladesDuelingStance longBladesDuelingStance && longBladesDuelingStance.IsMyActivatedAbilityAIUsable(longBladesDuelingStance.ActivatedAbilityID))
					{
						ParentObject.FireEvent("CommandDuelingStance");
					}
				}
				switch (E.GetIntParameter("Distance"))
				{
				case 1:
					if ((double)ConTarget() >= 0.8 && ParentObject.GetPart("LongBladesDeathblow") is LongBladesDeathblow longBladesDeathblow && longBladesDeathblow.IsMyActivatedAbilityAIUsable(longBladesDeathblow.ActivatedAbilityID))
					{
						E.AddAICommand("CommandDeathblow");
						return true;
					}
					if (currentStance != STR_AGGRESSIVE && ParentObject.GetPart("LongBladesLunge") is LongBladesLunge longBladesLunge2 && longBladesLunge2.IsMyActivatedAbilityAIUsable(longBladesLunge2.ActivatedAbilityID))
					{
						E.AddAICommand("CommandLunge");
					}
					if (ParentObject.GetPart("LongBladesSwipe") is LongBladesSwipe longBladesSwipe && longBladesSwipe.IsMyActivatedAbilityAIUsable(longBladesSwipe.ActivatedAbilityID))
					{
						E.AddAICommand("CommandSwipe");
					}
					break;
				case 2:
					if (currentStance == STR_AGGRESSIVE && ParentObject.GetPart("LongBladesLunge") is LongBladesLunge longBladesLunge && longBladesLunge.IsMyActivatedAbilityAIUsable(longBladesLunge.ActivatedAbilityID))
					{
						E.AddAICommand("CommandLunge");
					}
					break;
				}
			}
		}
		else if (E.ID == "BeginTakeAction")
		{
			if (Ultmode > 0)
			{
				Ultmode--;
				if (Ultmode <= 0)
				{
					ParentObject.RemoveEffect("En garde!");
				}
				else if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(Ultmode.Things("turn remains", "turns remain") + " until your guard is down.");
				}
			}
		}
		else if (E.ID == "GetAttackerHitDice")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
			if (gameObjectParameter != null)
			{
				MeleeWeapon part = gameObjectParameter.GetPart<MeleeWeapon>();
				if (part != null && (part.Skill == "LongBlades" || part.Skill == "ShortBlades") && IsPrimaryBladeEquipped() && currentStance == STR_AGGRESSIVE)
				{
					int num = ((!ParentObject.HasPart("LongBladesImprovedAggressiveStance")) ? 1 : 2);
					E.SetParameter("PenetrationBonus", E.GetIntParameter("PenetrationBonus") + num);
				}
			}
		}
		else if (E.ID == "AttackerRollMeleeToHit")
		{
			string stringParameter = E.GetStringParameter("Skill");
			if ((stringParameter == "LongBlades" || stringParameter == "ShortBlades") && IsPrimaryBladeEquipped())
			{
				int num2 = 0;
				if (currentStance == STR_AGGRESSIVE)
				{
					num2 -= (ParentObject.HasPart("LongBladesImprovedAggressiveStance") ? 3 : 2);
				}
				else if (currentStance == STR_DUELIST)
				{
					num2 += (ParentObject.HasPart("LongBladesImprovedDuelistStance") ? 3 : 2);
				}
				if (num2 != 0)
				{
					E.SetParameter("Result", E.GetIntParameter("Result") + num2);
				}
			}
		}
		else if (E.ID == "CommandAggressiveStance")
		{
			if (!IsPrimaryBladeEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a long blade equipped to switch stances.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			ChangeStance(STR_AGGRESSIVE);
		}
		else if (E.ID == "CommandDefensiveStance")
		{
			if (!IsPrimaryBladeEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a long blade equipped to switch stances.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			ChangeStance(STR_DEFENSIVE);
		}
		else if (E.ID == "CommandDuelingStance")
		{
			if (!IsPrimaryBladeEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a long blade equipped to switch stances.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			ChangeStance(STR_DUELIST);
		}
		else if (E.ID == "LongBlades.UpdateStance" || E.ID == "EquipperEquipped" || E.ID == "EquipperUnequipped" || E.ID == "PrimaryLimbRecalculated")
		{
			ChangeStance(currentStance);
		}
		else if (E.ID == "CommandLunge")
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell == null)
			{
				return false;
			}
			if (cell.OnWorldMap())
			{
				return ParentObject.ShowFailure("You cannot do that on the world map.");
			}
			if (!IsPrimaryBladeEquipped())
			{
				return ParentObject.ShowFailure("You must have a long blade equipped in your primary hand to lunge.");
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			if (currentStance == STR_AGGRESSIVE)
			{
				string direction = PickDirectionS();
				Cell cellFromDirection = ParentObject.GetCurrentCell().GetCellFromDirection(direction);
				Cell cellFromDirection2 = cellFromDirection.GetCellFromDirection(direction);
				if (cellFromDirection == null || cellFromDirection2 == null)
				{
					return false;
				}
				GameObject combatTarget = cellFromDirection.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, AllowInanimate: true, InanimateSolidOnly: true);
				if (combatTarget != null)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You can't aggressively lunge through " + combatTarget.the + combatTarget.ShortDisplayName + ".");
					}
					return false;
				}
				GameObject combatTarget2 = cellFromDirection2.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 5);
				if (combatTarget2 == null)
				{
					if (ParentObject.IsPlayer())
					{
						combatTarget2 = cellFromDirection2.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
						if (combatTarget2 == null)
						{
							Popup.ShowFail("There's nothing there to lunge at.");
						}
						else
						{
							Popup.ShowFail("There's nothing there you can lunge at.");
						}
					}
					return false;
				}
				DidXToY("lunge", "at", combatTarget2, null, null, null, ParentObject);
				E.RequestInterfaceExit();
				if (!ParentObject.DirectMoveTo(cellFromDirection))
				{
					if (ParentObject.IsPlayer())
					{
						Popup.Show("Your lunge is interrupted.");
					}
					else if (Visible())
					{
						IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName) + " lunge is interrupted.");
					}
					return false;
				}
				if (ParentObject.PhaseMatches(combatTarget2))
				{
					Event @event = Event.New("MeleeAttackWithWeapon");
					@event.SetParameter("Attacker", ParentObject);
					@event.SetParameter("Defender", combatTarget2);
					@event.SetParameter("Weapon", GetPrimaryBlade());
					@event.SetParameter("Properties", "Lunging");
					@event.SetParameter("PenBonus", 2);
					@event.SetParameter("PenCapBonus", 2);
					combatTarget2.Bloodsplatter();
					ParentObject.FireEvent(@event);
					ParentObject.FireEvent(Event.New("LungedTarget", "Defender", combatTarget2));
				}
				else if (ParentObject.IsPlayer())
				{
					Popup.Show("Your lunge passes through " + combatTarget2.the + combatTarget2.ShortDisplayName + ".");
				}
				else if (Visible())
				{
					IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName) + " lunge passes through " + combatTarget2.the + combatTarget2.ShortDisplayName + ".");
				}
				ParentObject.UseEnergy(1000, "Skill Lunge");
				if (Ultmode <= 0 && ParentObject.GetPart("LongBladesLunge") is LongBladesLunge longBladesLunge3)
				{
					longBladesLunge3.CooldownMyActivatedAbility(longBladesLunge3.ActivatedAbilityID, 15, null, "Agility");
				}
			}
			else if (currentStance == STR_DEFENSIVE)
			{
				Cell cell2 = PickDirection();
				if (cell2 == null)
				{
					return false;
				}
				GameObject combatTarget3 = cell2.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, null, null, AllowInanimate: false);
				if (combatTarget3 == null)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("There's nothing there to lunge away from.");
					}
					return false;
				}
				string directionFromCell = cell2.GetDirectionFromCell(cell);
				DidXToY("lunge", "away from", combatTarget3, null, null, null, ParentObject);
				E.RequestInterfaceExit();
				if (ParentObject.PhaseAndFlightMatches(combatTarget3))
				{
					Event event2 = Event.New("MeleeAttackWithWeapon");
					event2.SetParameter("Attacker", ParentObject);
					event2.SetParameter("Defender", combatTarget3);
					event2.SetParameter("Weapon", GetPrimaryBlade());
					event2.SetParameter("Properties", "Lunging");
					ParentObject.FireEvent(event2);
					ParentObject.FireEvent(Event.New("LungedTarget", "Defender", combatTarget3));
					combatTarget3.DustPuff();
				}
				int force = ParentObject.GetKineticResistance() * 3 / 2;
				for (int i = 0; i < 2; i++)
				{
					if (!ParentObject.pPhysics.Push(directionFromCell, force, 1, IgnoreGravity: true, Involuntary: false, ParentObject))
					{
						break;
					}
				}
				ParentObject.UseEnergy(1000, "Skill Lunge");
				ParentObject.Gravitate();
				if (Ultmode <= 0 && ParentObject.GetPart("LongBladesLunge") is LongBladesLunge longBladesLunge4)
				{
					longBladesLunge4.CooldownMyActivatedAbility(longBladesLunge4.ActivatedAbilityID, 15, null, "Agility");
				}
			}
			else
			{
				if (!(currentStance == STR_DUELIST))
				{
					if (ParentObject.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You must be in a long blade stance to use that ability.");
					}
					return false;
				}
				Cell cell3 = PickDirection();
				if (cell3 == null)
				{
					return false;
				}
				GameObject combatTarget4 = cell3.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 5);
				if (combatTarget4 == null)
				{
					if (ParentObject.IsPlayer())
					{
						combatTarget4 = cell3.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
						if (combatTarget4 == null)
						{
							Popup.ShowFail("There's nothing there to lunge at.");
						}
						else
						{
							Popup.ShowFail("There's nothing there you can lunge at.");
						}
					}
					return false;
				}
				DidXToY("lunge", "at", combatTarget4, null, null, null, ParentObject);
				E.RequestInterfaceExit();
				if (ParentObject.PhaseMatches(combatTarget4))
				{
					combatTarget4.Bloodsplatter();
					Event event3 = Event.New("MeleeAttackWithWeapon");
					event3.SetParameter("Attacker", ParentObject);
					event3.SetParameter("Defender", combatTarget4);
					event3.SetParameter("Weapon", GetPrimaryBlade());
					event3.SetParameter("Properties", "Autohit,Autopen,Lunging");
					event3.SetParameter("PenBonus", 1);
					event3.SetParameter("PenCapBonus", 1);
					ParentObject.FireEvent(event3);
					ParentObject.FireEvent(Event.New("LungedTarget", "Defender", combatTarget4));
				}
				else if (ParentObject.IsPlayer())
				{
					Popup.Show("Your lunge passes through " + combatTarget4.the + combatTarget4.ShortDisplayName + ".");
				}
				else if (Visible())
				{
					IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName) + " lunge passes through " + combatTarget4.the + combatTarget4.ShortDisplayName + ".");
				}
				ParentObject.UseEnergy(1000, "Skill Lunge");
				if (Ultmode <= 0 && ParentObject.GetPart("LongBladesLunge") is LongBladesLunge longBladesLunge5)
				{
					longBladesLunge5.CooldownMyActivatedAbility(longBladesLunge5.ActivatedAbilityID, 15, null, "Agility");
				}
			}
		}
		else if (E.ID == "CommandSwipe")
		{
			Cell cell4 = ParentObject.GetCurrentCell();
			if (cell4 == null)
			{
				return false;
			}
			if (cell4.OnWorldMap())
			{
				return ParentObject.ShowFailure("You cannot do that on the world map.");
			}
			GameObject primaryBlade = GetPrimaryBlade();
			if (primaryBlade == null)
			{
				return ParentObject.ShowFailure("You must have a long blade equipped in your primary hand to swipe.");
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			if (currentStance == STR_AGGRESSIVE)
			{
				for (int j = 0; j < 1; j++)
				{
					if (ParentObject.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You aggressively swipe your blade in the air.", 'G');
					}
					else if (Visible())
					{
						IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + " aggressively" + ParentObject.GetVerb("swipe") + " " + ParentObject.its + " blade in the air.", 'R');
					}
					string[] directionList = Directions.DirectionList;
					foreach (string direction2 in directionList)
					{
						GameObject gameObject = cell4.GetCellFromDirection(direction2)?.GetCombatTarget(ParentObject);
						if (gameObject != null && (gameObject.pBrain == null || gameObject.pBrain.IsHostileTowards(ParentObject)))
						{
							gameObject.Bloodsplatter();
							Event event4 = Event.New("MeleeAttackWithWeapon");
							event4.SetParameter("Attacker", ParentObject);
							event4.SetParameter("Defender", gameObject);
							event4.SetParameter("Weapon", primaryBlade);
							ParentObject.FireEvent(event4);
						}
					}
				}
				ParentObject.UseEnergy(1000, "Skill Aggressive Swipe");
				if (Ultmode <= 0 && ParentObject.GetPart("LongBladesSwipe") is LongBladesSwipe longBladesSwipe2)
				{
					longBladesSwipe2.CooldownMyActivatedAbility(longBladesSwipe2.ActivatedAbilityID, 15);
				}
			}
			else if (currentStance == STR_DEFENSIVE)
			{
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You swipe your blade in the air, pushing your enemies backward.", 'G');
				}
				else if (Visible())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("swipe") + " " + ParentObject.its + " blade in the air, pushing " + ParentObject.its + " foes backward.", 'R');
				}
				string[] directionList = Directions.DirectionList;
				foreach (string direction3 in directionList)
				{
					GameObject gameObject2 = ParentObject.GetCurrentCell().GetCellFromDirection(direction3)?.GetCombatTarget(ParentObject);
					if (gameObject2 != null && gameObject2.GetMatterPhase() == 1)
					{
						gameObject2.DustPuff();
						gameObject2.pPhysics.Push(direction3, 1000, 4, IgnoreGravity: false, Involuntary: true, ParentObject);
						if (!gameObject2.MakeSave("Agility,Strength", 30, null, null, "LongBlades Blade Swipe Knockdown", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, primaryBlade))
						{
							gameObject2.ApplyEffect(new Prone());
						}
					}
				}
				ParentObject.UseEnergy(1000, "Skill Defensive Swipe");
				if (Ultmode <= 0 && ParentObject.GetPart("LongBladesSwipe") is LongBladesSwipe longBladesSwipe3)
				{
					longBladesSwipe3.CooldownMyActivatedAbility(longBladesSwipe3.ActivatedAbilityID, 15);
				}
			}
			else
			{
				if (!(currentStance == STR_DUELIST))
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You must be in a long blade stance to use that ability.");
					}
					return false;
				}
				Cell cell5 = PickDirection();
				if (cell5 == null)
				{
					return false;
				}
				GameObject combatTarget5 = cell5.GetCombatTarget(ParentObject);
				if (combatTarget5 == null)
				{
					if (ParentObject.IsPlayer())
					{
						combatTarget5 = cell5.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
						if (combatTarget5 == null)
						{
							Popup.ShowFail("There's nothing there to swipe at.");
						}
						else
						{
							Popup.ShowFail("There's nothing there you can swipe at.");
						}
					}
					return false;
				}
				DidXToY("swipe", ParentObject.its + " blade at", combatTarget5, null, null, null, ParentObject);
				Disarming.Disarm(combatTarget5, ParentObject, 25, "Strength", "Agility", primaryBlade);
				Event event5 = Event.New("MeleeAttackWithWeapon");
				event5.SetParameter("Attacker", ParentObject);
				event5.SetParameter("Defender", combatTarget5);
				event5.SetParameter("Weapon", primaryBlade);
				event5.SetParameter("Properties", "Autohit,Autopen");
				ParentObject.FireEvent(event5);
				ParentObject.UseEnergy(1000, "Skill Duelist Swipe");
				if (Ultmode <= 0 && ParentObject.GetPart("LongBladesSwipe") is LongBladesSwipe longBladesSwipe4)
				{
					longBladesSwipe4.CooldownMyActivatedAbility(longBladesSwipe4.ActivatedAbilityID, 15);
				}
			}
		}
		else if (E.ID == "CommandDeathblow")
		{
			if (!IsPrimaryBladeEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a long blade equipped to effectively yell out 'En garde!'");
				}
				return false;
			}
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("En garde!");
				ParentObject.ParticleText("En garde!", 'W');
			}
			else if (ParentObject.IsVisible())
			{
				ParentObject.ParticleText("En garde!", 'W');
			}
			ParentObject.ApplyEffect(new LongbladeEffect_EnGarde());
			Ultmode = 10;
			if (ParentObject.GetPart("LongBladesDeathblow") is LongBladesDeathblow longBladesDeathblow2)
			{
				longBladesDeathblow2.CooldownMyActivatedAbility(longBladesDeathblow2.ActivatedAbilityID, 100, null, "Agility");
			}
			if (ParentObject.GetPart("LongBladesLunge") is LongBladesLunge longBladesLunge6 && longBladesLunge6.IsMyActivatedAbilityCoolingDown(longBladesLunge6.ActivatedAbilityID))
			{
				longBladesLunge6.MyActivatedAbility(longBladesLunge6.ActivatedAbilityID).SetCooldown(0);
			}
			if (ParentObject.GetPart("LongBladesSwipe") is LongBladesSwipe longBladesSwipe5 && longBladesSwipe5.IsMyActivatedAbilityCoolingDown(longBladesSwipe5.ActivatedAbilityID))
			{
				longBladesSwipe5.MyActivatedAbility(longBladesSwipe5.ActivatedAbilityID).SetCooldown(0);
			}
		}
		return base.FireEvent(E);
	}
}
