using System;
using System.Collections.Generic;
using System.Text;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class Combat : IPart
{
	[NonSerialized]
	public MissileWeapon LastFired;

	[NonSerialized]
	public static int TrackShieldBlock;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != GetAdjacentNavigationWeightEvent.ID && ID != GetDefenderHitDiceEvent.ID)
		{
			return ID == GetNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		ParentObject.ClearShieldBlocks();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDefenderHitDiceEvent E)
	{
		GameObject shield = ParentObject.GetShield(CanBlockWithShield, E.Attacker);
		if (shield == null)
		{
			return true;
		}
		if (!ParentObject.CanMoveExtremities())
		{
			return true;
		}
		if (!(shield.GetPart("Shield") is Shield shield2))
		{
			return true;
		}
		BlockedWithShield(shield);
		int num;
		if (ParentObject.HasEffect("ShieldWall") && shield2.WornOn == "Hand")
		{
			num = 100;
		}
		else
		{
			num = 25 * (1 + ParentObject.GetIntProperty("ImprovedBlock"));
			if (ParentObject.HasSkill("Shield_Block"))
			{
				num += 25;
			}
			if (ParentObject.HasSkill("Shield_DeftBlocking"))
			{
				num += 25;
			}
		}
		if (num.in100())
		{
			E.ShieldBlocked = true;
			if (ParentObject.HasRegisteredEvent("ShieldBlock"))
			{
				ParentObject.FireEvent(Event.New("ShieldBlock", "Shield", shield));
			}
			if (TrackShieldBlock > 0)
			{
				TrackShieldBlock++;
			}
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You block with your shield! (+" + shield2.AV + " AV)", 'g');
			}
			ParentObject.ParticleText("*block (+" + shield2.AV + " AV)", IComponent<GameObject>.ConsequentialColorChar(ParentObject));
			if (ParentObject.HasTagOrProperty("BlockSound"))
			{
				ParentObject.PlayWorldSound(ParentObject.GetTagOrStringProperty("BlockSound"), 0.5f, 0f, combat: false, ParentObject.HasIntProperty("BlockSoundDelay") ? ((float)ParentObject.GetIntProperty("BlockSoundDelay") / 1000f) : 0f);
			}
			E.AV += shield2.AV;
			int chance = 20;
			if (ParentObject.HasStat("Strength"))
			{
				chance = ParentObject.Stat("Strength") * 2 - 35;
			}
			if (ParentObject.HasSkill("Shield_StaggeringBlock") && E.Attacker != null && chance.in100())
			{
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You stagger " + E.Attacker.t() + " with your shield block!", 'g');
				}
				else if (E.Attacker.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You are staggered by " + ParentObject.poss("block") + " block!", 'r');
				}
				E.Attacker.ApplyEffect(new Stun(Stat.Random(1, 2), 12));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!ParentObject.CanBePositionSwapped() && !E.Flying)
		{
			E.MinWeight(100);
		}
		else
		{
			E.Uncacheable = true;
			if (E.Actor != null && ParentObject.IsHostileTowards(E.Actor) && ParentObject != E.Actor.Target)
			{
				if (E.Flying && !ParentObject.IsFlying)
				{
					E.MinWeight(5);
				}
				else
				{
					E.MinWeight(80);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(1);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandAttackCell");
		Object.RegisterPartEvent(this, "CommandAttackObject");
		Object.RegisterPartEvent(this, "CommandAttackDirection");
		Object.RegisterPartEvent(this, "CommandFireMissileWeapon");
		Object.RegisterPartEvent(this, "CommandSwoopAttack");
		Object.RegisterPartEvent(this, "CommandThrowWeapon");
		Object.RegisterPartEvent(this, "MeleeAttackWithWeapon");
		Object.RegisterPartEvent(this, "PerformMeleeAttack");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandThrowWeapon")
		{
			if (!ParentObject.CheckFrozen())
			{
				return false;
			}
			if (ParentObject.HasEffect("Paralyzed"))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You are paralyzed!");
				}
				return false;
			}
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot throw things on the world map.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			GameObject equipped = ParentObject.Body.GetFirstPart("Thrown Weapon").Equipped;
			MissilePath missilePath = null;
			Cell cell = E.GetParameter("TargetCell") as Cell;
			if (ParentObject.IsPlayer() && cell == null && equipped != null)
			{
				Physics physics = ((Sidebar.CurrentTarget == null || ParentObject.GetFuriousConfusion() > 0 || ParentObject.GetConfusion() > 0) ? ParentObject.pPhysics : Sidebar.CurrentTarget.pPhysics);
				FireType FireType = FireType.Normal;
				physics.CurrentCell.ParentZone.CalculateMissileMap(ParentObject);
				missilePath = MissileWeapon.ShowPicker(physics.CurrentCell.X, physics.CurrentCell.Y, Locked: true, AllowVis.Any, 999, BowOrRifle: false, equipped, ref FireType);
				if (missilePath == null)
				{
					return false;
				}
				cell = physics.CurrentCell.ParentZone.GetCell((int)(missilePath.x1 / 3f), (int)(missilePath.y1 / 3f));
			}
			if (cell != null && !ParentObject.PerformThrow(equipped, cell, missilePath))
			{
				return false;
			}
		}
		else if (E.ID == "CommandFireMissileWeapon")
		{
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot fire missile weapons on the world map.");
				}
				return false;
			}
			if (!ParentObject.FireEvent("CanFireMissileWeapon"))
			{
				return false;
			}
			ParentObject.CurrentZone.CalculateMissileMap(ParentObject);
			List<GameObject> missileWeapons = ParentObject.GetMissileWeapons();
			List<MissileWeapon> list = new List<MissileWeapon>(missileWeapons.Count);
			foreach (GameObject item2 in missileWeapons)
			{
				MissileWeapon item = item2.GetPart("MissileWeapon") as MissileWeapon;
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
			if (list.Count > 1 && !CanFireAllMissileWeaponsEvent.Check(ParentObject, missileWeapons))
			{
				if (list.Contains(LastFired))
				{
					list.Remove(LastFired);
				}
				list = new List<MissileWeapon>(1) { list.GetRandomElement() };
			}
			Cell cell2 = E.GetParameter("TargetCell") as Cell;
			MissilePath missilePath2 = null;
			FireType FireType2 = FireType.Normal;
			if (E.HasParameter("FireType"))
			{
				FireType2 = (FireType)E.GetParameter("FireType");
			}
			if (cell2 == null)
			{
				if (!ParentObject.IsPlayer())
				{
					return true;
				}
				cell2 = ((ParentObject.Target == null || ParentObject.GetTotalConfusion() > 0) ? ParentObject.CurrentCell : ParentObject.Target.CurrentCell);
				if (cell2 == null)
				{
					return true;
				}
				bool bowOrRifle = false;
				foreach (MissileWeapon item3 in list)
				{
					if (item3.Skill == "Rifle" || item3.Skill == "Bow")
					{
						bowOrRifle = true;
					}
				}
				GameObject Projectile = null;
				GameObject gameObject = null;
				string Blueprint = null;
				GetMissileWeaponProjectileEvent.GetFor(missileWeapons[0], ref Projectile, ref Blueprint);
				if (Projectile == null && !string.IsNullOrEmpty(Blueprint))
				{
					gameObject = GameObject.create(Blueprint);
					if (gameObject != null)
					{
						MissileWeapon.SetupProjectile(gameObject, ParentObject);
						Projectile = gameObject;
					}
				}
				try
				{
					missilePath2 = MissileWeapon.ShowPicker(cell2.X, cell2.Y, Locked: true, AllowVis.Any, 999, bowOrRifle, Projectile ?? ParentObject, ref FireType2);
					if (missilePath2 == null)
					{
						return false;
					}
					cell2 = missilePath2.Path[missilePath2.Path.Count - 1];
					if (FireType2 == FireType.Mark)
					{
						Rifle_DrawABead obj = ParentObject.GetPart("Rifle_DrawABead") as Rifle_DrawABead;
						GameObject combatTarget = cell2.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, Projectile);
						obj.SetMark(combatTarget);
						Sidebar.CurrentTarget = combatTarget;
						ParentObject.UseEnergy(1000, "Physical Skill");
						FireType2 = FireType.Normal;
						AutoAct.Setting = "ReopenMissileUI";
						return true;
					}
				}
				finally
				{
					gameObject?.Obliterate();
				}
			}
			else
			{
				missilePath2 = MissileWeapon.CalculateMissilePath(ParentObject.CurrentZone, ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, cell2.X, cell2.Y);
				if (ParentObject.CurrentZone != cell2.ParentZone)
				{
					return false;
				}
			}
			if (missilePath2 == null)
			{
				return false;
			}
			int num = 0;
			if (cell2 != null)
			{
				foreach (MissileWeapon item4 in list)
				{
					float num2 = 1f;
					if (list.Count > 1)
					{
						num2 /= (float)list.Count;
					}
					num = 0;
					foreach (GameObject item5 in cell2.LoopObjectsWithPart("Combat"))
					{
						if (item5.GetEffect("RifleMark") is RifleMark rifleMark && rifleMark.Marker == ParentObject)
						{
							num += 2;
						}
					}
					GameObject combatTarget2 = cell2.GetCombatTarget(ParentObject);
					if (combatTarget2 != null)
					{
						Event @event = Event.New("TargetedForMissileWeapon");
						@event.SetParameter("Attacker", ParentObject);
						@event.SetParameter("Defender", combatTarget2);
						@event.SetParameter("Weapon", item4.ParentObject);
						if (!combatTarget2.FireEvent(@event))
						{
							return false;
						}
					}
					if (ParentObject.HasSkill("Rifle_Kickback") && ParentObject.GetBodyPartCountEquippedOn(item4.ParentObject) >= 2 && cell2.IsAdjacentTo(ParentObject.CurrentCell) && combatTarget2 != null && combatTarget2.IsCombatObject() && combatTarget2.IsPotentiallyMobile() && ParentObject.FlightCanReach(combatTarget2))
					{
						if (!ParentObject.PhaseMatches(combatTarget2) || combatTarget2.GetMatterPhase() != 1)
						{
							if (ParentObject.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage("You kick at " + combatTarget2.t() + ", but the kick passes through " + combatTarget2.them + ".");
							}
							else if (combatTarget2.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("kick") + " at you, but the kick passes through you.");
							}
							else if (combatTarget2.IsVisible() && ParentObject.IsVisible())
							{
								IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("kick") + " at " + combatTarget2.t() + ", but the kick passes through " + combatTarget2.them + ".");
							}
						}
						else if (!combatTarget2.CanBeInvoluntarilyMoved() || combatTarget2.MakeSave("Strength", 15, ParentObject, null, "Rifle Kickback Knockback") || !combatTarget2.Push(ParentObject.CurrentCell.GetDirectionFromCell(combatTarget2.CurrentCell), ParentObject.Stat("Strength") * 50, 4))
						{
							if (ParentObject.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage("You kick at " + combatTarget2.t() + ", but " + combatTarget2.does("hold") + " " + combatTarget2.its + " ground.");
							}
							else if (combatTarget2.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("kick") + " at you, but you hold your ground.");
							}
							else if (combatTarget2.IsVisible() && ParentObject.IsVisible())
							{
								IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("kick") + " at " + combatTarget2.t() + ", but " + combatTarget2.it + combatTarget2.GetVerb("hold", PrependSpace: true, PronounAntecedent: true) + " " + combatTarget2.its + " ground.");
							}
						}
						else
						{
							combatTarget2.DustPuff();
							if (ParentObject.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage("You kick " + combatTarget2.t() + " backwards.");
							}
							else if (combatTarget2.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("kick") + " you backwards.");
							}
							else if (combatTarget2.IsVisible() && ParentObject.IsVisible())
							{
								IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("kick") + " " + combatTarget2.t() + " backwards.");
							}
						}
					}
					ParentObject.FireEvent("FiringMissile");
					if (E.HasParameter("EnergyMul"))
					{
						num2 = E.GetIntParameter("EnergyMul");
					}
					Event event2 = Event.New("CommandFireMissile", "EnergyMultiplier", num2, "AimLevel", num);
					event2.SetParameter("Owner", ParentObject);
					event2.SetParameter("TargetCell", cell2);
					event2.SetParameter("Path", missilePath2);
					event2.SetParameter("FireType", FireType2);
					if (E.HasParameter("Rapid"))
					{
						event2.SetParameter("EnergyMultiplier", 0f);
						int i = 0;
						for (int intParameter = E.GetIntParameter("Rapid"); i < intParameter; i++)
						{
							item4.ParentObject.FireEvent(event2);
						}
					}
					else if (E.HasParameter("Sweep"))
					{
						event2.SetParameter("EnergyMultiplier", 0f);
						event2.SetParameter("FlatVariance", -45);
						item4.ParentObject.FireEvent(event2);
						event2.SetParameter("FlatVariance", -22);
						item4.ParentObject.FireEvent(event2);
						event2.SetParameter("FlatVariance", 0);
						item4.ParentObject.FireEvent(event2);
						event2.SetParameter("FlatVariance", 22);
						item4.ParentObject.FireEvent(event2);
						event2.SetParameter("FlatVariance", 45);
						item4.ParentObject.FireEvent(event2);
					}
					else
					{
						event2.SetParameter("FlatVariance", 0);
						item4.ParentObject.FireEvent(event2);
					}
					LastFired = item4;
				}
				ParentObject.FireEvent("FiredMissileWeapon");
			}
		}
		else if (E.ID == "MeleeAttackWithWeapon")
		{
			The.Game.ActionTicks++;
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Weapon");
			string text = E.GetStringParameter("Properties", "") ?? "";
			int value = 0;
			string text2 = "1d1";
			string text3 = "Strength";
			Damage damage = new Damage(0);
			damage.AddAttribute("Melee");
			MeleeWeapon meleeWeapon = gameObjectParameter3?.GetPart("MeleeWeapon") as MeleeWeapon;
			if (meleeWeapon != null)
			{
				text2 = meleeWeapon.BaseDamage;
				value = meleeWeapon.MaxStrengthBonus;
				text3 = meleeWeapon.Stat;
				if (text3.Contains(","))
				{
					string[] array = text3.Split(',');
					int num3 = -999;
					string[] array2 = array;
					foreach (string text4 in array2)
					{
						int num4 = gameObjectParameter.Stat(text4);
						if (num4 > num3)
						{
							text3 = text4;
							num3 = num4;
						}
					}
				}
				if (meleeWeapon.HasTag("WeaponUnarmed"))
				{
					damage.AddAttribute("Unarmed");
				}
				if (!string.IsNullOrEmpty(meleeWeapon.Attributes))
				{
					damage.AddAttributes(meleeWeapon.Attributes);
				}
				int intParameter2 = E.GetIntParameter("AdjustDamageResult");
				if (intParameter2 != 0)
				{
					text2 = DieRoll.AdjustResult(text2, intParameter2);
				}
				int intParameter3 = E.GetIntParameter("AdjustDamageDieSize");
				if (intParameter3 != 0)
				{
					text2 = DieRoll.AdjustDieSize(text2, intParameter3);
				}
			}
			else
			{
				damage.AddAttribute("Unarmed");
			}
			bool flag = false;
			damage.AddAttribute(text3);
			if (Statistic.IsMental(text3))
			{
				damage.AddAttribute("Mental");
				flag = true;
			}
			if (base.juiceEnabled)
			{
				CombatJuice.playWorldSound(gameObjectParameter, gameObjectParameter3?.GetTagOrStringProperty("SwingSound") ?? "Swing_Default");
			}
			int num5 = Stat.Random(1, 20);
			int value2 = num5;
			num5 += E.GetIntParameter("HitBonus") + gameObjectParameter.GetIntProperty("HitBonus");
			if (meleeWeapon != null)
			{
				num5 += meleeWeapon.HitBonus;
			}
			MeleeWeapon meleeWeapon2 = gameObjectParameter3?.GetPart<MeleeWeapon>();
			string text5 = ((meleeWeapon2 == null) ? "Unarmed" : meleeWeapon2.Skill);
			num5 += gameObjectParameter.StatMod("Agility");
			Event event3 = Event.New("RollMeleeToHit");
			event3.SetParameter("Weapon", gameObjectParameter3);
			event3.SetParameter("Damage", damage);
			event3.SetParameter("Defender", gameObjectParameter2);
			event3.SetParameter("Result", num5);
			event3.SetParameter("Skill", text5);
			event3.SetParameter("Stat", text3);
			gameObjectParameter3?.FireEvent(event3);
			event3.ID = "AttackerRollMeleeToHit";
			gameObjectParameter?.FireEvent(event3);
			num5 = event3.GetIntParameter("Result");
			Event event4 = Event.New("GetDefenderDV");
			event4.SetParameter("Weapon", gameObjectParameter3);
			event4.SetParameter("Damage", damage);
			event4.SetParameter("Attacker", gameObjectParameter);
			event4.SetParameter("Defender", gameObjectParameter2);
			event4.SetParameter("NaturalHitResult", value2);
			event4.SetParameter("Result", num5);
			event4.SetParameter("Skill", text5);
			event4.SetParameter("Stat", text3);
			event4.SetParameter("DV", Stats.GetCombatDV(gameObjectParameter2));
			gameObjectParameter2.FireEvent(event4);
			event4.ID = "WeaponGetDefenderDV";
			gameObjectParameter3?.FireEvent(event4);
			num5 = event4.GetIntParameter("Result");
			value2 = event4.GetIntParameter("NaturalHitResult");
			bool flag2 = text.Contains("Critical");
			if (!flag2)
			{
				int num6 = GetCriticalThresholdEvent.GetFor(gameObjectParameter, gameObjectParameter2, gameObjectParameter3, null, text5);
				int @for = GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter3, "Melee Critical", 5, gameObjectParameter2);
				if (@for != 5)
				{
					num6 -= (@for - 5) / 5;
				}
				if (value2 >= num6)
				{
					flag2 = true;
				}
			}
			bool flag3 = true;
			bool flag4 = flag2 || num5 > event4.GetIntParameter("DV") || text.Contains("Autohit");
			if (flag4)
			{
				event4.ID = "DefenderBeforeHit";
				if (!gameObjectParameter2.FireEvent(event4))
				{
					flag4 = false;
				}
			}
			if (event4.HasIntParameter("NoMissMessage"))
			{
				flag3 = false;
			}
			if (!flag4)
			{
				if (base.juiceEnabled)
				{
					CombatJuice.playPrefabAnimation(gameObjectParameter2, "CombatJuice/CombatJuiceMissAnimationPrefab");
					CombatJuice.playWorldSound(gameObjectParameter, gameObjectParameter3?.GetTagOrStringProperty("MissSound") ?? "Miss_Default");
					CombatJuice.punch(gameObjectParameter, gameObjectParameter2);
				}
				else
				{
					gameObjectParameter2.ParticleBlip("&K\t");
				}
				if (IsPlayer())
				{
					if (flag3)
					{
						if (IComponent<GameObject>.TerseMessages)
						{
							IComponent<GameObject>.AddPlayerMessage("You miss!", 'r');
						}
						else if (gameObjectParameter3 != null)
						{
							IComponent<GameObject>.AddPlayerMessage("{{r|You miss with " + gameObjectParameter.its_(gameObjectParameter3) + "!}} [" + num5 + " vs " + event4.GetIntParameter("DV") + "]");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage("{{r|You miss!}} [" + num5 + " vs " + event4.GetIntParameter("DV") + "]");
						}
					}
				}
				else if (gameObjectParameter2.IsPlayer())
				{
					if (AutoAct.IsInterruptable())
					{
						AutoAct.Interrupt(null, null, gameObjectParameter);
					}
					if (flag3)
					{
						if (IComponent<GameObject>.TerseMessages)
						{
							IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("miss") + " you!");
						}
						else if (gameObjectParameter3 != null)
						{
							IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("miss") + " you with " + gameObjectParameter.its_(gameObjectParameter3) + "! [" + num5 + " vs " + event4.GetIntParameter("DV") + "]");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("miss") + " you! [" + num5 + " vs " + event4.GetIntParameter("DV") + "]");
						}
					}
				}
				else if (AutoAct.IsInterruptable())
				{
					if (gameObjectParameter.IsPlayerLedAndPerceptible() && !gameObjectParameter.IsTrifling)
					{
						AutoAct.Interrupt("you " + gameObjectParameter.GetPerceptionVerb() + " " + gameObjectParameter.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: true) + " fighting" + (gameObjectParameter.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(gameObjectParameter))), null, gameObjectParameter);
					}
					else if (gameObjectParameter2.IsPlayerLedAndPerceptible() && !gameObjectParameter2.IsTrifling)
					{
						AutoAct.Interrupt("you " + gameObjectParameter2.GetPerceptionVerb() + " " + gameObjectParameter2.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: true) + " fighting" + (gameObjectParameter2.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(gameObjectParameter2))), null, gameObjectParameter2);
					}
				}
				Event event5 = Event.New("AttackerMeleeMiss");
				event5.SetParameter("Weapon", gameObjectParameter3);
				event5.SetParameter("Attacker", gameObjectParameter);
				event5.SetParameter("Defender", gameObjectParameter2);
				gameObjectParameter.FireEvent(event5);
				Event event6 = Event.New("DefenderAfterAttackMissed");
				event6.SetParameter("Attacker", gameObjectParameter);
				event6.SetParameter("Defender", gameObjectParameter2);
				event6.SetParameter("Weapon", gameObjectParameter3);
				gameObjectParameter2.FireEvent(event6);
				if (gameObjectParameter3 != null)
				{
					event6.ID = "WeaponAfterAttackMissed";
					gameObjectParameter3.FireEvent(event6);
				}
			}
			else
			{
				gameObjectParameter2.FireEvent("DefenderAttackHit");
				if (!gameObjectParameter2.HasStat("AV"))
				{
					return false;
				}
				DefendMeleeHitEvent.Send(gameObjectParameter, gameObjectParameter2, gameObjectParameter3, damage, event3.GetIntParameter("Result"));
				int AV = (flag ? Stats.GetCombatMA(gameObjectParameter2) : Stats.GetCombatAV(gameObjectParameter2));
				int PenetrationBonus = 0;
				bool ShieldBlocked = false;
				GetAttackerHitDiceEvent.Process(ref PenetrationBonus, ref AV, ref ShieldBlocked, gameObjectParameter, gameObjectParameter2, gameObjectParameter3);
				GetWeaponHitDiceEvent.Process(ref PenetrationBonus, ref AV, ref ShieldBlocked, gameObjectParameter, gameObjectParameter2, gameObjectParameter3);
				GetDefenderHitDiceEvent.Process(ref PenetrationBonus, ref AV, ref ShieldBlocked, gameObjectParameter, gameObjectParameter2, gameObjectParameter3);
				int value3 = ((meleeWeapon == null || !meleeWeapon.HasTag("WeaponIgnoreStrength")) ? gameObjectParameter.StatMod(text3) : 0);
				int num7 = E.GetIntParameter("PenBonus");
				int num8 = E.GetIntParameter("PenCapBonus");
				if (meleeWeapon != null)
				{
					num7 += meleeWeapon.PenBonus;
					num8 += meleeWeapon.PenBonus;
				}
				Event event7 = Event.New("GetWeaponPenModifier");
				event7.SetParameter("Penetrations", value3);
				event7.SetParameter("MaxStrengthBonus", value);
				event7.SetParameter("Attacker", gameObjectParameter);
				event7.SetParameter("Defender", gameObjectParameter2);
				event7.SetParameter("PenBonus", num7);
				event7.SetParameter("CapBonus", num8);
				event7.SetParameter("Weapon", gameObjectParameter3);
				event7.SetParameter("AV", AV);
				event7.SetParameter("Hand", E.GetParameter("Hand"));
				event7.SetParameter("Properties", text);
				event7.SetFlag("Critical", flag2);
				gameObjectParameter3?.FireEvent(event7);
				event7.ID = "AttackerGetWeaponPenModifier";
				gameObjectParameter?.FireEvent(event7);
				int intParameter4 = event7.GetIntParameter("Penetrations");
				int intParameter5 = event7.GetIntParameter("MaxStrengthBonus");
				num7 = event7.GetIntParameter("PenBonus");
				num8 = event7.GetIntParameter("CapBonus");
				BaseSkill baseSkill = null;
				bool flag5 = false;
				int num9 = 0;
				int num10 = 0;
				if (flag2)
				{
					num9 = 1;
					num10 = 1;
					flag5 = true;
					if (baseSkill == null)
					{
						baseSkill = Skills.GetGenericSkill(text5, gameObjectParameter);
					}
					if (baseSkill != null)
					{
						int weaponCriticalModifier = baseSkill.GetWeaponCriticalModifier(gameObjectParameter, gameObjectParameter2, gameObjectParameter3);
						if (weaponCriticalModifier != 0)
						{
							num9 += weaponCriticalModifier;
							num10 += weaponCriticalModifier;
						}
					}
					Event event8 = Event.New("WeaponCriticalModifier");
					event8.SetParameter("Attacker", gameObjectParameter);
					event8.SetParameter("Defender", gameObjectParameter2);
					event8.SetParameter("Weapon", gameObjectParameter3);
					event8.SetParameter("Skill", text5);
					event8.SetParameter("Stat", text3);
					event8.SetParameter("PenBonus", num9);
					event8.SetParameter("CapBonus", num10);
					event8.SetFlag("AutoPen", flag5);
					gameObjectParameter3?.FireEvent(event8);
					event8.ID = "AttackerCriticalModifier";
					gameObjectParameter.FireEvent(event8);
					num9 = event8.GetIntParameter("PenBonus");
					num10 = event8.GetIntParameter("CapBonus");
					flag5 = event8.HasFlag("AutoPen");
				}
				int value4 = Stat.RollDamagePenetrations(AV, intParameter4 + num7 + PenetrationBonus + num9, intParameter5 + num8 + PenetrationBonus + num10);
				E.SetParameter("DidHit", 1);
				bool flag6 = false;
				Event event9 = Event.New("AttackerHit", 5, 0, 2);
				event9.SetParameter("Penetrations", value4);
				event9.SetParameter("Damage", damage);
				event9.SetParameter("Attacker", gameObjectParameter);
				event9.SetParameter("Defender", gameObjectParameter2);
				event9.SetParameter("Weapon", gameObjectParameter3);
				event9.SetParameter("Properties", text);
				event9.SetFlag("Critical", flag2);
				if (!gameObjectParameter.FireEvent(event9))
				{
					return false;
				}
				if (event9.HasFlag("DidSpecialEffect"))
				{
					flag6 = true;
				}
				text = event9.GetStringParameter("Properties", "") ?? text;
				Event event10 = Event.New("DefenderHit", 4, 0, 2);
				event10.SetParameter("Penetrations", event9.GetIntParameter("Penetrations"));
				event10.SetParameter("Damage", damage);
				event10.SetParameter("Attacker", gameObjectParameter);
				event10.SetParameter("Defender", gameObjectParameter2);
				event10.SetParameter("Weapon", gameObjectParameter3);
				event10.SetFlag("Critical", flag2);
				if (!gameObjectParameter2.FireEvent(event10))
				{
					return false;
				}
				if (event10.HasFlag("DidSpecialEffect"))
				{
					flag6 = true;
				}
				Event event11 = Event.New("WeaponHit", 4, 0, 2);
				event11.SetParameter("Penetrations", event9.GetIntParameter("Penetrations"));
				event11.SetParameter("Damage", damage);
				event11.SetParameter("Attacker", gameObjectParameter);
				event11.SetParameter("Defender", gameObjectParameter2);
				event11.SetParameter("Weapon", gameObjectParameter3);
				event11.SetFlag("Critical", flag2);
				event11.SetParameter("Properties", text);
				if (gameObjectParameter3 != null && !gameObjectParameter3.FireEvent(event11))
				{
					return false;
				}
				if (event11.HasFlag("DidSpecialEffect"))
				{
					flag6 = true;
				}
				text = event11.GetStringParameter("Properties", "") ?? text;
				bool defenderIsCreature = gameObjectParameter2.HasTag("Creature");
				string blueprint = gameObjectParameter2.Blueprint;
				WeaponUsageTracking.TrackMeleeWeaponHit(gameObjectParameter, gameObjectParameter3, defenderIsCreature, blueprint);
				if (gameObjectParameter.HasRegisteredEvent("WieldedWeaponHit"))
				{
					Event event12 = Event.New("WieldedWeaponHit", 4, 0, 2);
					event12.SetParameter("Penetrations", event11.GetIntParameter("Penetrations"));
					event12.SetParameter("Damage", damage);
					event12.SetParameter("Attacker", gameObjectParameter);
					event12.SetParameter("Defender", gameObjectParameter2);
					event12.SetParameter("Weapon", gameObjectParameter3);
					event12.SetFlag("Critical", flag2);
					if (!gameObjectParameter.FireEvent(event12))
					{
						return false;
					}
					if (event12.HasFlag("DidSpecialEffect"))
					{
						flag6 = true;
					}
				}
				if (!event11.HasParameter("Penetrations"))
				{
					return false;
				}
				value4 = event11.GetIntParameter("Penetrations");
				if (value4 <= 0 && text.Contains("Autopen"))
				{
					value4 = 1;
				}
				else if (value4 > 1 && text.Contains("MaxPens1"))
				{
					value4 = 1;
				}
				else if (value4 <= 0 && flag5 && gameObjectParameter != null && gameObjectParameter.IsPlayer())
				{
					value4 = 1;
				}
				Cell value5 = gameObjectParameter2.CurrentCell;
				if (value4 > 0)
				{
					damage.AddAttribute(text5);
					if (gameObjectParameter3 != null && flag2)
					{
						damage.AddAttribute("Critical");
						Event event13 = Event.New("CriticalHit");
						event13.SetParameter("Attacker", gameObjectParameter);
						event13.SetParameter("Defender", gameObjectParameter2);
						event13.SetParameter("BaseDamage", text2);
						event13.SetParameter("Weapon", gameObjectParameter3);
						event13.SetParameter("Skill", text5);
						event13.SetParameter("Stat", text3);
						event13.ID = "AttackerCriticalHit";
						gameObjectParameter.FireEvent(event13);
						event13.ID = "WeaponCriticalHit";
						if (gameObjectParameter3 != null)
						{
							gameObjectParameter3.FireEvent(event13);
						}
						else
						{
							gameObjectParameter.FireEvent(event13);
						}
						event13.ID = "DefenderCriticalHit";
						gameObjectParameter2.FireEvent(event13);
						text2 = event13.GetStringParameter("BaseDamage");
						if (event13.HasFlag("DidSpecialEffect"))
						{
							flag6 = true;
						}
					}
					int num11 = 0;
					if (damage.HasAttribute("Mental") && gameObjectParameter2.pBrain == null)
					{
						if (gameObjectParameter.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("Your mental attack does not affect " + gameObjectParameter2.t() + ".");
						}
						flag6 = true;
					}
					else
					{
						DieRoll cachedDieRoll = text2.GetCachedDieRoll();
						for (int k = 0; k < value4; k++)
						{
							num11 += cachedDieRoll.Resolve();
						}
					}
					if (num11 > 0 || flag6)
					{
						if (flag2)
						{
							gameObjectParameter.ParticleText("*critical hit*", IComponent<GameObject>.ConsequentialColorChar(gameObjectParameter));
						}
						string resultColor = Stat.GetResultColor(value4);
						if (!base.juiceEnabled && Options.ShowMonsterHPHearts)
						{
							gameObjectParameter2.ParticleBlip(resultColor + "\u0003");
						}
						damage.Amount += num11;
						Event event14 = Event.New("DealDamage");
						event14.SetParameter("Penetrations", value4);
						event14.SetParameter("Damage", damage);
						event14.SetParameter("Attacker", gameObjectParameter);
						event14.SetParameter("Defender", gameObjectParameter2);
						event14.SetParameter("Weapon", gameObjectParameter3);
						event14.SetParameter("Properties", text);
						event14.SetParameter("Cell", value5);
						event14.SetFlag("Critical", flag2);
						if (event14.HasFlag("DidSpecialEffect"))
						{
							flag6 = true;
						}
						Event event15 = Event.New("WeaponDealDamage");
						event15.SetParameter("Penetrations", value4);
						event15.SetParameter("Damage", damage);
						event15.SetParameter("Attacker", gameObjectParameter);
						event15.SetParameter("Defender", gameObjectParameter2);
						event15.SetParameter("Weapon", gameObjectParameter3);
						event15.SetParameter("Cell", value5);
						event15.SetFlag("Critical", flag2);
						if (event15.HasFlag("DidSpecialEffect"))
						{
							flag6 = true;
						}
						Event event16 = Event.New("TakeDamage");
						event16.SetParameter("Penetrations", value4);
						event16.SetParameter("Damage", damage);
						event16.SetParameter("Owner", gameObjectParameter);
						event16.SetParameter("Attacker", gameObjectParameter);
						event16.SetParameter("Defender", gameObjectParameter2);
						event16.SetParameter("Weapon", gameObjectParameter3);
						event16.SetParameter("Message", "");
						event16.SetParameter("NoDamageMessage", 1);
						event16.SetParameter("Cell", value5);
						event16.SetFlag("Critical", flag2);
						if (base.juiceEnabled)
						{
							CombatJuice.playPrefabAnimation(gameObjectParameter2, "CombatJuice/CombatJuiceSlashAnimationPrefab");
							CombatJuice.playWorldSound(gameObjectParameter, gameObjectParameter3.GetTagOrStringProperty("HitSound") ?? "Hit_Default", 0.5f, 0f, 0f, gameObjectParameter3.HasIntProperty("HitSoundDelay") ? ((float)gameObjectParameter3.GetIntProperty("HitSoundDelay") / 1000f) : 0.135f);
							CombatJuice.punch(gameObjectParameter, gameObjectParameter2);
						}
						bool flag7 = false;
						if (gameObjectParameter.FireEvent(event14) || flag6)
						{
							WeaponUsageTracking.TrackMeleeWeaponDamage(gameObjectParameter, gameObjectParameter3, defenderIsCreature, blueprint, damage);
							if (gameObjectParameter3 == null || gameObjectParameter3.FireEvent(event15))
							{
								StringBuilder stringBuilder = Event.NewStringBuilder();
								if (IsPlayer())
								{
									stringBuilder.Append("{{g|You");
									if (flag2)
									{
										stringBuilder.Append(" critically");
									}
									stringBuilder.Append(" hit");
									if (!IComponent<GameObject>.TerseMessages)
									{
										stringBuilder.Append(" {{").Append(resultColor).Append("|(x")
											.Append(value4)
											.Append(")}}");
									}
									if (damage.Amount > 0)
									{
										stringBuilder.Append(" for ").Append(damage.Amount).Append(" damage");
									}
									if (!IComponent<GameObject>.TerseMessages)
									{
										stringBuilder.Append(" with ");
										if (gameObjectParameter3 == null)
										{
											stringBuilder.Append("your bare hands");
										}
										else
										{
											gameObjectParameter.its_(gameObjectParameter3, stringBuilder);
										}
										stringBuilder.Append("! [").Append(num5).Append(']');
									}
									stringBuilder.Append("}}");
								}
								else if (gameObjectParameter2.IsPlayer())
								{
									stringBuilder.Append("%T");
									if (flag2)
									{
										stringBuilder.Append(" critically");
									}
									stringBuilder.Append(gameObjectParameter.GetVerb("hit"));
									if (!IComponent<GameObject>.TerseMessages)
									{
										stringBuilder.Append(" {{").Append(resultColor).Append("|(x")
											.Append(value4)
											.Append(")}}");
									}
									if (damage.Amount > 0)
									{
										stringBuilder.Append(" for ").Append(damage.Amount).Append(" damage");
									}
									if (!IComponent<GameObject>.TerseMessages)
									{
										if (gameObjectParameter3 == null)
										{
											stringBuilder.Append(" barehanded");
										}
										else
										{
											stringBuilder.Append(" with ");
											gameObjectParameter.its_(gameObjectParameter3, stringBuilder);
										}
										stringBuilder.Append('.');
										stringBuilder.Append(" [").Append(num5).Append(']');
									}
								}
								event16.SetParameter("Message", stringBuilder.ToString());
								if (flag2 && gameObjectParameter2.GetIntProperty("Bleeds") > 0)
								{
									gameObjectParameter2.Bloodsplatter();
								}
								if (gameObjectParameter2.FireEvent(event16))
								{
									flag7 = true;
									if (event16.HasFlag("DidSpecialEffect"))
									{
										flag6 = true;
									}
									if (gameObjectParameter2.IsValid())
									{
										if (flag2)
										{
											if (baseSkill == null)
											{
												baseSkill = Skills.GetGenericSkill(text5, gameObjectParameter);
											}
											baseSkill?.WeaponMadeCriticalHit(gameObjectParameter, gameObjectParameter2, gameObjectParameter3, text);
											Event event17 = Event.New("AfterCriticalHit");
											event17.SetParameter("Attacker", gameObjectParameter);
											event17.SetParameter("Defender", gameObjectParameter);
											event17.SetParameter("Weapon", gameObjectParameter3);
											event17.SetParameter("Skill", text5);
											event17.SetParameter("Stat", text3);
											event17.SetParameter("Properties", text);
											event17.SetParameter("Cell", value5);
											event17.ID = "AttackerAfterCriticalHit";
											gameObjectParameter.FireEvent(event17);
											event17.ID = "WeaponAfterCriticalHit";
											if (gameObjectParameter3 != null)
											{
												gameObjectParameter3.FireEvent(event17);
											}
											else
											{
												gameObjectParameter.FireEvent(event17);
											}
											event17.ID = "DefenderAfterCriticalHit";
											gameObjectParameter2.FireEvent(event17);
											if (event17.HasFlag("DidSpecialEffect"))
											{
												flag6 = true;
											}
										}
										Event event18 = Event.New("AttackerAfterDamage");
										event18.SetParameter("Penetrations", value4);
										event18.SetParameter("Damage", damage);
										event18.SetParameter("Attacker", gameObjectParameter);
										event18.SetParameter("Defender", gameObjectParameter2);
										event18.SetParameter("Weapon", gameObjectParameter3);
										event18.SetParameter("Message", "");
										event18.SetParameter("Properties", text);
										event18.SetParameter("Cell", value5);
										event18.SetFlag("Critical", flag2);
										gameObjectParameter.FireEvent(event18);
										event18.ID = "WeaponAfterDamage";
										gameObjectParameter3?.FireEvent(event18);
										if (event18.HasFlag("DidSpecialEffect"))
										{
											flag6 = true;
										}
									}
								}
								if (event16.HasFlag("DidSpecialEffect"))
								{
									flag6 = true;
								}
							}
						}
						if (!flag7 && !flag6)
						{
							if (!damage.SuppressionMessageDone && gameObjectParameter.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage("You fail to deal damage with your attack! [" + num5 + "]", 'r');
							}
							if (gameObjectParameter2.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage(gameObjectParameter.Does("fail") + " to deal damage with " + ParentObject.its + " attack! [" + num5 + "]");
							}
						}
						if (!base.juiceEnabled && Options.ShowMonsterHPHearts)
						{
							gameObjectParameter2.ParticleBlip(gameObjectParameter2.GetHPColor() + "\u0003");
						}
					}
				}
				else
				{
					if (base.juiceEnabled)
					{
						CombatJuice.playPrefabAnimation(gameObjectParameter2, "CombatJuice/CombatJuiceBlockAnimationPrefab");
						CombatJuice.punch(gameObjectParameter, gameObjectParameter2);
					}
					if (IsPlayer())
					{
						if (IComponent<GameObject>.TerseMessages)
						{
							IComponent<GameObject>.AddPlayerMessage("You don't penetrate " + gameObjectParameter2.poss("armor") + ".", 'r');
						}
						else if (gameObjectParameter3 != null)
						{
							IComponent<GameObject>.AddPlayerMessage("You don't penetrate " + gameObjectParameter2.poss("armor") + " with " + gameObjectParameter.its_(gameObjectParameter3) + ". {{y|[" + num5 + "]}}", 'r');
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage("You don't penetrate " + gameObjectParameter2.poss("armor") + ". {{y|[" + num5 + "]}}", 'r');
						}
					}
					else if (gameObjectParameter2.IsPlayer())
					{
						if (IComponent<GameObject>.TerseMessages)
						{
							IComponent<GameObject>.AddPlayerMessage(gameObjectParameter.Does("don't") + " penetrate your armor.");
						}
						else if (gameObjectParameter3 != null)
						{
							IComponent<GameObject>.AddPlayerMessage(gameObjectParameter.Does("don't") + " penetrate your armor with " + gameObjectParameter.its_(gameObjectParameter3) + "! [" + num5 + "]");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage(gameObjectParameter.Does("don't") + " penetrate your armor! [" + num5 + "]");
						}
					}
					if (ShieldBlocked)
					{
						if (!base.juiceEnabled)
						{
							gameObjectParameter2.ParticleBlip("&G\a");
						}
						gameObjectParameter2?.pPhysics.PlayWorldSound("ShieldBlockWood", 0.5f, 0.2f, combat: true);
						gameObjectParameter?.pPhysics.PlayWorldSound(gameObjectParameter3?.GetTagOrStringProperty("BlockedSound") ?? "Miss_Default", 0.5f, 0.2f, combat: true);
					}
					else
					{
						if (!base.juiceEnabled)
						{
							gameObjectParameter2.ParticleBlip("&K\a");
						}
						gameObjectParameter?.pPhysics.PlayWorldSound(gameObjectParameter3?.GetTagOrStringProperty("BlockedSound") ?? "Miss_Default", 0.5f, 0.2f, combat: true);
					}
				}
				Event event19 = Event.New("AttackerAfterAttack");
				event19.SetParameter("Penetrations", value4);
				event19.SetParameter("Damage", damage);
				event19.SetParameter("Attacker", gameObjectParameter);
				event19.SetParameter("Defender", gameObjectParameter2);
				event19.SetParameter("Weapon", gameObjectParameter3);
				event19.SetParameter("Skill", text5);
				event19.SetParameter("Stat", text3);
				event19.SetParameter("Properties", text);
				event19.SetParameter("Cell", value5);
				event19.SetFlag("Critical", flag2);
				gameObjectParameter.FireEvent(event19);
				event19.ID = "DefenderAfterAttack";
				gameObjectParameter2.FireEvent(event19);
				if (gameObjectParameter3 != null)
				{
					event19.ID = "WeaponAfterAttack";
					gameObjectParameter3.FireEvent(event19);
				}
			}
		}
		else if (E.ID == "CommandAttackDirection")
		{
			if (ParentObject.pPhysics == null)
			{
				return false;
			}
			string stringParameter = E.GetStringParameter("Direction");
			if (!ParentObject.CheckFrozen())
			{
				return false;
			}
			if (ParentObject.HasEffect("Paralyzed"))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You are paralyzed!");
				}
				return false;
			}
			Cell cellFromDirection = ParentObject.CurrentCell.GetCellFromDirection(stringParameter, BuiltOnly: false);
			if (cellFromDirection != null)
			{
				ParentObject.FireEvent(Event.New("CommandAttackCell", "Cell", cellFromDirection));
			}
		}
		else if (E.ID == "CommandSwoopAttack")
		{
			string text6 = E.GetStringParameter("Direction");
			if (text6 == null)
			{
				if (ParentObject.IsPlayer())
				{
					text6 = PickDirectionS();
				}
				if (text6 == null)
				{
					return false;
				}
			}
			DidX("swoop", "down to attack", null, null, ParentObject);
			Flight.SuspendFlight(ParentObject);
			TrackShieldBlock = 1;
			bool flag8 = false;
			try
			{
				Event e = Event.New("CommandAttackDirection", "Direction", text6);
				bool num12 = FireEvent(e);
				bool flag9 = TrackShieldBlock > 1;
				Flight.DesuspendFlight(ParentObject);
				TrackShieldBlock = 0;
				flag8 = true;
				if (!num12)
				{
					return false;
				}
				int num13 = Flight.GetSwoopFallChance(ParentObject);
				if (flag9)
				{
					num13 *= 2;
				}
				if (num13.in100())
				{
					Flight.Fall(ParentObject);
				}
				else
				{
					ParentObject.UseEnergy(1000, "Swoop Return");
				}
			}
			finally
			{
				if (!flag8)
				{
					Flight.DesuspendFlight(ParentObject);
					TrackShieldBlock = 0;
					flag8 = true;
				}
			}
		}
		else if (E.ID == "CommandAttackCell")
		{
			Cell cell3 = E.GetParameter("Cell") as Cell;
			Cell cell4 = ParentObject.CurrentCell;
			Zone parentZone = cell3.ParentZone;
			Zone parentZone2 = cell4.ParentZone;
			GameObject combatTarget3 = cell3.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
			if (combatTarget3 == null)
			{
				return false;
			}
			if (!combatTarget3.HasStat("Hitpoints"))
			{
				return false;
			}
			parentZone2?.MarkActive(parentZone);
			Event event20 = Event.New("BeginAttack");
			event20.SetParameter("TargetObject", combatTarget3);
			event20.SetParameter("TargetCell", cell3);
			if (ParentObject.FireEvent(event20) && cell3 != null && ParentObject.pPhysics != null)
			{
				Event event21 = Event.New("ObjectAttacking");
				event21.SetParameter("Object", ParentObject);
				event21.SetParameter("TargetObject", combatTarget3);
				event21.SetParameter("TargetCell", cell3);
				if (ParentObject.IsPlayer() && IComponent<GameObject>.Visible(combatTarget3))
				{
					Sidebar.CurrentTarget = combatTarget3;
				}
				if (ParentObject.PhaseAndFlightMatches(combatTarget3) && cell3.FireEvent(event21))
				{
					Event event22 = Event.New("PerformMeleeAttack");
					event22.SetParameter("Attacker", ParentObject);
					event22.SetParameter("TargetCell", cell3);
					event22.SetParameter("Defender", combatTarget3);
					event22.SetParameter("Properties", E.GetParameter("Properties"));
					if (E.HasParameter("PenBonus"))
					{
						event22.SetParameter("PenBonus", E.GetIntParameter("PenBonus"));
					}
					if (E.HasParameter("HitBonus"))
					{
						event22.SetParameter("HitBonus", E.GetIntParameter("HitBonus"));
					}
					if (E.HasParameter("PenCapBonus"))
					{
						event22.SetParameter("PenCapBonus", E.GetIntParameter("PenCapBonus"));
					}
					ParentObject.FireEvent(event22);
				}
			}
		}
		else if (E.ID == "CommandAttackObject")
		{
			GameObject gameObjectParameter4 = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter4 == null)
			{
				return false;
			}
			if (!gameObjectParameter4.HasStat("Hitpoints"))
			{
				return false;
			}
			Cell cell5 = gameObjectParameter4.CurrentCell;
			Cell cell6 = ParentObject.CurrentCell;
			Zone parentZone3 = cell5.ParentZone;
			cell6.ParentZone?.MarkActive(parentZone3);
			Event event23 = Event.New("BeginAttack");
			event23.SetParameter("TargetObject", gameObjectParameter4);
			event23.SetParameter("TargetCell", cell5);
			if (ParentObject.FireEvent(event23) && cell5 != null && ParentObject.pPhysics != null)
			{
				Event event24 = Event.New("ObjectAttacking");
				event24.SetParameter("Object", ParentObject);
				event24.SetParameter("TargetObject", gameObjectParameter4);
				event24.SetParameter("TargetCell", cell5);
				if (ParentObject.IsPlayer() && IComponent<GameObject>.Visible(gameObjectParameter4))
				{
					Sidebar.CurrentTarget = gameObjectParameter4;
				}
				if (ParentObject.PhaseAndFlightMatches(gameObjectParameter4) && cell5.FireEvent(event24))
				{
					Event event25 = Event.New("PerformMeleeAttack");
					event25.SetParameter("Attacker", ParentObject);
					event25.SetParameter("TargetCell", cell5);
					event25.SetParameter("Defender", gameObjectParameter4);
					event25.SetParameter("Properties", E.GetParameter("Properties"));
					if (E.HasParameter("PenBonus"))
					{
						event25.SetParameter("PenBonus", E.GetIntParameter("PenBonus"));
					}
					if (E.HasParameter("HitBonus"))
					{
						event25.SetParameter("HitBonus", E.GetIntParameter("HitBonus"));
					}
					if (E.HasParameter("PenCapBonus"))
					{
						event25.SetParameter("PenCapBonus", E.GetIntParameter("PenCapBonus"));
					}
					ParentObject.FireEvent(event25);
				}
			}
		}
		else if (E.ID == "PerformMeleeAttack")
		{
			GameObject gameObjectParameter5 = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter6 = E.GetGameObjectParameter("Defender");
			if (!gameObjectParameter5.PhaseAndFlightMatches(gameObjectParameter6))
			{
				return false;
			}
			List<GameObject> list2 = new List<GameObject>(8);
			if (gameObjectParameter6 != null)
			{
				gameObjectParameter5.FireEvent(Event.New("PerformingMeleeAttack", "Defender", gameObjectParameter6));
			}
			Body body = gameObjectParameter5.Body;
			BodyPart primaryWeaponPart = null;
			int PossibleWeapons;
			GameObject mainWeapon = body.GetMainWeapon(out PossibleWeapons, out primaryWeaponPart, NeedPrimary: true, FailDownFromPrimary: true);
			if (mainWeapon != null && mainWeapon.FireEvent("CanMeleeAttack"))
			{
				list2.Add(mainWeapon);
			}
			if (PossibleWeapons > 1)
			{
				int value6 = (E.HasParameter("AlwaysOffhand") ? E.GetIntParameter("AlwaysOffhand") : (gameObjectParameter5.HasProperty("AlwaysOffhand") ? 100 : (gameObjectParameter5.HasSkill("Dual_Wield_Two_Weapon_Fighting") ? GlobalConfig.GetIntSetting("TwoWeaponFightingSecondaryAttackChance", 75) : (gameObjectParameter5.HasSkill("Dual_Wield_Ambidexterity") ? GlobalConfig.GetIntSetting("AmbidexteritySecondaryAttackChance", 55) : ((!gameObjectParameter5.HasSkill("Dual_Wield_Offhand_Strikes")) ? GlobalConfig.GetIntSetting("BaseSecondaryAttackChance", 15) : GlobalConfig.GetIntSetting("OffhandStrikesSecondaryAttackChance", 35))))));
				Event event26 = Event.New("QuerySecondaryAttackChance", "Properties", E.GetParameter("Properties"));
				gameObjectParameter5.FireEvent(event26);
				if (event26.HasParameter("Return"))
				{
					value6 = event26.GetIntParameter("Return");
				}
				bool flag10 = gameObjectParameter5.HasPropertyOrTag("AttackWithEverything");
				foreach (BodyPart item6 in body.LoopParts())
				{
					GameObject gameObject2 = item6.Equipped ?? item6.DefaultBehavior;
					if (gameObject2 == null)
					{
						continue;
					}
					MeleeWeapon meleeWeapon3 = gameObject2.GetPart("MeleeWeapon") as MeleeWeapon;
					if (meleeWeapon3 == null)
					{
						if (gameObject2 != item6.Equipped)
						{
							continue;
						}
						gameObject2 = item6.DefaultBehavior;
						if (gameObject2 == null)
						{
							continue;
						}
						meleeWeapon3 = gameObject2.GetPart("MeleeWeapon") as MeleeWeapon;
						if (meleeWeapon3 == null)
						{
							continue;
						}
					}
					if (list2.Contains(gameObject2) || !gameObject2.FireEvent("CanMeleeAttack"))
					{
						continue;
					}
					BodyPart bodyPart = item6;
					GameObject gameObject3 = gameObject2;
					if (primaryWeaponPart != null && bodyPart.DefaultPrimary && bodyPart != primaryWeaponPart && primaryWeaponPart.Primary)
					{
						bodyPart = primaryWeaponPart;
						gameObject3 = bodyPart.Equipped ?? bodyPart.DefaultBehavior ?? gameObject2;
					}
					if (!meleeWeapon3.AttackFromPart(bodyPart))
					{
						continue;
					}
					int num14 = 0;
					if (flag10)
					{
						num14 = 100;
					}
					else
					{
						Event event27 = Event.New("QueryWeaponSecondaryAttackChance");
						event27.SetParameter("Weapon", gameObject3);
						event27.SetParameter("BodyPart", bodyPart);
						event27.SetParameter("Chance", value6);
						event27.SetParameter("Properties", E.GetParameter("Properties"));
						gameObject3.FireEvent(event27);
						if (gameObjectParameter5 != null)
						{
							event27.ID = "AttackerQueryWeaponSecondaryAttackChance";
							gameObjectParameter5.FireEvent(event27);
							event27.ID = "AttackerQueryWeaponSecondaryAttackChanceMultiplier";
							gameObjectParameter5.FireEvent(event27);
						}
						num14 = event27.GetIntParameter("Chance");
						if (E.HasParameter("AlwaysOffhand"))
						{
							num14 = E.GetIntParameter("AlwaysOffhand");
						}
					}
					while (num14 > 0)
					{
						if (num14.in100())
						{
							list2.Add(gameObject2);
						}
						num14 -= 100;
					}
				}
			}
			for (int l = 0; l < list2.Count; l++)
			{
				GameObject gameObject4 = list2[l];
				if (gameObject4 == null || gameObject4.IsInvalid() || (gameObject4.Equipped != gameObjectParameter5 && !gameObjectParameter5.IsADefaultBehavior(gameObject4)))
				{
					continue;
				}
				Event event28 = Event.New("MeleeAttackWithWeapon");
				event28.SetParameter("Attacker", gameObjectParameter5);
				event28.SetParameter("Defender", gameObjectParameter6);
				event28.SetParameter("Weapon", gameObject4);
				event28.SetParameter("Properties", E.GetParameter("Properties"));
				if (E.HasParameter("HitBonus"))
				{
					int intParameter6 = E.GetIntParameter("HitBonus");
					event28.SetParameter("HitBonus", intParameter6);
				}
				if (E.HasParameter("PenBonus"))
				{
					int intParameter7 = E.GetIntParameter("PenBonus");
					event28.SetParameter("PenBonus", intParameter7);
				}
				if (E.HasParameter("PenCapBonus"))
				{
					int intParameter8 = E.GetIntParameter("PenCapBonus");
					event28.SetParameter("PenCapBonus", intParameter8);
				}
				if (l == 0)
				{
					event28.SetParameter("Hand", "Primary");
				}
				else
				{
					event28.SetParameter("Hand", "Secondary");
				}
				if (ParentObject != null)
				{
					ParentObject.FireEvent(event28);
					if (gameObjectParameter6 == null || gameObjectParameter6.IsInvalid() || gameObjectParameter6.IsInGraveyard() || (gameObjectParameter5 != null && gameObjectParameter5.IsInvalid()) || ParentObject == null || ParentObject.IsInvalid())
					{
						break;
					}
				}
			}
			int num15 = E.GetIntParameter("EnergyCost", 1000);
			if (mainWeapon != null && mainWeapon.GetWeaponSkill() == "ShortBlades" && gameObjectParameter5.HasSkill("ShortBlades_Expertise"))
			{
				num15 = num15 * 3 / 4;
			}
			gameObjectParameter5.UseEnergy(num15, "Combat Melee");
			if (list2.Count > 0)
			{
				Event event29 = Event.New("AttackerAfterMelee", "Weapons", list2);
				event29.SetParameter("Attacker", gameObjectParameter5);
				event29.SetParameter("Defender", gameObjectParameter6);
				gameObjectParameter5.FireEvent(event29);
			}
			Event event30 = Event.New("AIMessage");
			event30.SetParameter("Message", "Attacked");
			event30.SetParameter("By", gameObjectParameter5);
			gameObjectParameter6.FireEvent(event30);
		}
		return base.FireEvent(E);
	}

	public static int GetShieldBlocksPerTurn(GameObject who)
	{
		if (!who.HasSkill("Shield_SwiftBlocking"))
		{
			return 1;
		}
		return 2;
	}

	public int GetShieldBlocksPerTurn()
	{
		return GetShieldBlocksPerTurn(ParentObject);
	}

	public bool CanBlockWithShield(GameObject shield)
	{
		return CanBlockWithShield(ParentObject, shield);
	}

	public static bool CanBlockWithShield(GameObject who, GameObject shield)
	{
		if (who.HasEffect("ShieldWall"))
		{
			return true;
		}
		if (shield.GetPart("Shield") is Shield shield2)
		{
			return shield2.Blocks < GetShieldBlocksPerTurn(who);
		}
		return false;
	}

	public void BlockedWithShield(GameObject shield)
	{
		BlockedWithShield(ParentObject, shield);
	}

	public static void BlockedWithShield(GameObject who, GameObject shield)
	{
		if (shield.GetPart("Shield") is Shield shield2)
		{
			shield2.Blocks++;
		}
	}
}
