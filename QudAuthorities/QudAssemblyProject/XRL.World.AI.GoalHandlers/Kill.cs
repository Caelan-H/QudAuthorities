using System;
using System.Collections.Generic;
using System.Reflection;
using XRL.Rules;
using XRL.World.AI.Pathfinding;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Kill : GoalHandler
{
	public GameObject _Target;

	public int ExtinguishSelfTries;

	[NonSerialized]
	public static Event eAIGetDefensiveItemList = new Event("AIGetDefensiveItemList", 3, 0, 1);

	[NonSerialized]
	public static Event eAIGetOffensiveItemList = new Event("AIGetOffensiveItemList", 3, 0, 1);

	[NonSerialized]
	public static Event eAIGetOffensiveMutationList = new Event("AIGetOffensiveMutationList", 3, 0, 1);

	[NonSerialized]
	public static Event eAIGetDefensiveMutationList = new Event("AIGetDefensiveMutationList", 3, 0, 1);

	private int LastSeen;

	private int LastAttacked;

	private int MoveTries;

	[NonSerialized]
	public static List<string> tryMeleeOrder = new List<string> { "items", "missile", "mutations", "abilities", "defensiveItems", "defensiveMutations" };

	[NonSerialized]
	public static List<string> tryMissileOrder = new List<string> { "items", "missile", "mutations", "thrown", "abilities", "defensiveItems", "defensiveMutations" };

	public GameObject Target
	{
		get
		{
			GameObject.validate(ref _Target);
			return _Target;
		}
		set
		{
			_Target = value;
		}
	}

	public Kill(GameObject Target)
	{
		this.Target = Target;
	}

	public override void Create()
	{
		if (Target == null)
		{
			return;
		}
		Think("I'm trying to kill someone!");
		if (base.ParentObject.HasRegisteredEvent("AICreateKill"))
		{
			Event e = Event.New("AICreateKill", "Actor", base.ParentObject, "Target", Target);
			if (!base.ParentObject.FireEvent(e))
			{
				return;
			}
		}
		if (Target.HasRegisteredEvent("AITargetCreateKill"))
		{
			Event e2 = Event.New("AITargetCreateKill", "Actor", base.ParentObject, "Target", Target);
			Target.FireEvent(e2);
		}
	}

	public override string GetDetails()
	{
		if (Target != null)
		{
			if (!_Target.IsPlayer())
			{
				return _Target.DebugName;
			}
			return "Player";
		}
		return null;
	}

	public override bool Finished()
	{
		return false;
	}

	public bool TryThrownWeapon()
	{
		Body body = base.ParentObject.Body;
		if (body == null)
		{
			return false;
		}
		Cell currentCell = base.ParentObject.CurrentCell;
		Cell currentCell2 = Target.CurrentCell;
		if (currentCell == null || currentCell2 == null || currentCell.ParentZone != currentCell2.ParentZone)
		{
			return false;
		}
		foreach (BodyPart item in body.LoopParts())
		{
			if (!(item.Type == "Thrown Weapon"))
			{
				continue;
			}
			GameObject equipped = item.Equipped;
			if (equipped == null || !equipped.IsThrownWeapon)
			{
				continue;
			}
			List<Point> list = Zone.Line(currentCell.X, currentCell.Y, currentCell2.X, currentCell2.Y);
			if (Target != null && equipped.HasPart("StickyOnHit") && Target.HasEffect("Stuck"))
			{
				continue;
			}
			if (list.Count > 6)
			{
				return false;
			}
			for (int i = 1; i < list.Count - 1; i++)
			{
				Cell cell = currentCell2.ParentZone.GetCell(list[i].X, list[i].Y);
				if (cell.IsOccluding())
				{
					return false;
				}
				if (!cell.HasObjectWithPart("Combat"))
				{
					continue;
				}
				foreach (GameObject item2 in cell.LoopObjectsWithPart("Combat"))
				{
					if (!ParentBrain.IsHostileTowards(item2))
					{
						return false;
					}
				}
			}
			Think("I'm going to throw my " + equipped.ShortDisplayName);
			if (base.ParentObject.PerformThrow(equipped, Target.CurrentCell))
			{
				base.ParentObject.FireEvent("AIAfterThrow");
				return true;
			}
			Think("It didn't work, I'll try a different weapon...");
			return false;
		}
		return false;
	}

	public bool TryReload()
	{
		try
		{
			Body body = base.ParentObject.Body;
			if (body == null)
			{
				return false;
			}
			if (base.ParentObject.Inventory == null)
			{
				return false;
			}
			if (!base.ParentObject.InSameZone(Target))
			{
				return false;
			}
			foreach (BodyPart item in body.LoopParts())
			{
				GameObject equipped = item.Equipped;
				if (equipped == null || !(equipped.GetPart("EnergyCellSocket") is EnergyCellSocket))
				{
					continue;
				}
				if (equipped.GetPart("EnergyAmmoLoader") is EnergyAmmoLoader energyAmmoLoader && energyAmmoLoader.GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) == ActivePartStatus.Unpowered)
				{
					CommandReloadEvent.Execute(base.ParentObject, equipped, null, FreeAction: false, FromDialog: false, energyAmmoLoader.GetActiveChargeUse());
					break;
				}
				int charge = 0;
				equipped.ForeachPart(delegate(IPart Part)
				{
					if (Part is IActivePart activePart)
					{
						if (activePart.ChargeUse > charge)
						{
							charge = activePart.ChargeUse;
						}
					}
					else
					{
						Type type = Part.GetType();
						FieldInfo field = type.GetField("ChargeUse");
						if (field?.FieldType == typeof(int))
						{
							int num = (int)field.GetValue(Part);
							if (num > charge)
							{
								charge = num;
							}
						}
						PropertyInfo property = type.GetProperty("ChargeUse");
						if (property?.PropertyType == typeof(int))
						{
							int num2 = (int)property.GetValue(Part, null);
							if (num2 > charge)
							{
								charge = num2;
							}
						}
					}
				});
				if (charge > 0 && !equipped.TestCharge(charge, LiveOnly: false, 0L) && !equipped.HasEffect("ElectromagneticPulsed"))
				{
					CommandReloadEvent.Execute(base.ParentObject, equipped, null, FreeAction: false, FromDialog: false, charge);
					break;
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("TryReload", x);
			return false;
		}
		return false;
	}

	public bool TryMissileWeapon()
	{
		if (Target == null)
		{
			return false;
		}
		if (!Target.HasPart("Combat"))
		{
			return false;
		}
		Cell currentCell = base.ParentObject.CurrentCell;
		if (currentCell == null)
		{
			return false;
		}
		Zone parentZone = currentCell.ParentZone;
		if (parentZone == null)
		{
			return false;
		}
		Cell currentCell2 = Target.CurrentCell;
		if (currentCell2 == null)
		{
			return false;
		}
		if (parentZone != currentCell2.ParentZone)
		{
			return false;
		}
		Body body = base.ParentObject.Body;
		if (body == null)
		{
			return false;
		}
		if (base.ParentObject.GetIntProperty("TurretWarmup") > 0 && (Target.IsPlayerControlled() || Visible()))
		{
			if (base.ParentObject.GetIntProperty("TurretWarmup") == 1)
			{
				if (parentZone.IsActive())
				{
					if (Visible())
					{
						base.ParentObject.pPhysics.DidX("chirp");
						if (AutoAct.IsActive() && GoalHandler.ThePlayer.IsRelevantHostile(base.ParentObject))
						{
							AutoAct.Interrupt(null, null, base.ParentObject);
						}
					}
					else if (base.ParentObject.IsAudible(GoalHandler.ThePlayer))
					{
						GoalHandler.AddPlayerMessage("Something chirps " + The.Player.DescribeDirectionToward(base.ParentObject) + ".");
						if (AutoAct.IsActive())
						{
							AutoAct.Interrupt(null, null, base.ParentObject);
						}
					}
				}
				base.ParentObject.ApplyEffect(new WarmingUp());
				base.ParentObject.UseEnergy(1000, "Warmup");
				base.ParentObject.SetIntProperty("TurretWarmup", 2);
				return true;
			}
			if (base.ParentObject.GetIntProperty("TurretWarmup") == 2)
			{
				base.ParentObject.RemoveEffect("WarmingUp");
				base.ParentObject.SetIntProperty("TurretWarmup", 3);
			}
		}
		if (ParentBrain.NeedToReload)
		{
			ParentBrain.NeedToReload = false;
			if (!CommandReloadEvent.Execute(base.ParentObject))
			{
				return false;
			}
		}
		List<GameObject> missileWeapons = body.GetMissileWeapons();
		if (missileWeapons == null || missileWeapons.Count <= 0)
		{
			return false;
		}
		int num = currentCell2.PathDistanceTo(currentCell);
		if (num > ParentBrain.MaxMissileRange)
		{
			return false;
		}
		int i = 0;
		for (int count = missileWeapons.Count; i < count; i++)
		{
			GameObject gameObject = missileWeapons[i];
			if (!gameObject.FireEvent(Event.New("AIWantUseWeapon", "Object", base.ParentObject)))
			{
				continue;
			}
			List<Point> list = Zone.Line(currentCell.X, currentCell.Y, currentCell2.X, currentCell2.Y);
			MissileWeapon missileWeapon = gameObject.GetPart("MissileWeapon") as MissileWeapon;
			if (missileWeapon != null && num > missileWeapon.MaxRange)
			{
				return false;
			}
			for (int j = 1; j < list.Count - 1; j++)
			{
				Cell cell = parentZone.GetCell(list[j].X, list[j].Y);
				int k = 0;
				for (int count2 = cell.Objects.Count; k < count2; k++)
				{
					GameObject gameObject2 = cell.Objects[k];
					if (gameObject2.HasPart("Combat") && !ParentBrain.IsHostileTowards(gameObject2))
					{
						return false;
					}
				}
				if (cell.IsOccluding() && (missileWeapon == null || !(missileWeapon.Skill == "HeavyWeapons") || !cell.HasObjectWithPart("Forcefield")))
				{
					return false;
				}
			}
			Think("I'm going to fire my " + gameObject.ShortDisplayName);
			Event @event = Event.New("CommandFireMissileWeapon");
			@event.SetParameter("AimLevel", 0);
			@event.SetParameter("Owner", base.ParentObject);
			@event.SetParameter("TargetCell", currentCell2);
			MissilePath value = MissileWeapon.CalculateMissilePath(parentZone, currentCell.X, currentCell.Y, currentCell2.X, currentCell2.Y);
			@event.SetParameter("Path", value);
			if (base.ParentObject.FireEvent(@event))
			{
				base.ParentObject.FireEvent("AIAfterMissile");
				gameObject.FireEvent("AIAfterMissile");
				return true;
			}
			Think("It didn't work, I'll try a different weapon...");
		}
		return false;
	}

	public override void Push(Brain pBrain)
	{
		if (pBrain.ParentObject != Target && pBrain.CanFight())
		{
			base.Push(pBrain);
		}
	}

	public bool TryDefensiveItems()
	{
		try
		{
			if (base.ParentObject.HasStat("Intelligence") && base.ParentObject.Stat("Intelligence") < 7)
			{
				return false;
			}
			if (Target.InSameZone(base.ParentObject))
			{
				List<AICommandList> list;
				if (eAIGetDefensiveItemList.HasParameter("List"))
				{
					list = eAIGetDefensiveItemList.GetParameter<List<AICommandList>>("List");
					list.Clear();
				}
				else
				{
					list = new List<AICommandList>(8);
					eAIGetDefensiveItemList.SetParameter("List", list);
				}
				eAIGetDefensiveItemList.SetParameter("Distance", Target.DistanceTo(base.ParentObject));
				eAIGetDefensiveItemList.SetParameter("User", base.ParentObject);
				eAIGetDefensiveItemList.SetParameter("Target", Target);
				base.ParentObject.FireEvent(eAIGetDefensiveItemList);
				if (AICommandList.HandleCommandList(list, base.ParentObject, Target))
				{
					return true;
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("TryDefensiveItems", x);
			return false;
		}
		return false;
	}

	public bool TryItems()
	{
		if (Target.InSameZone(base.ParentObject))
		{
			List<AICommandList> list;
			if (eAIGetOffensiveItemList.HasParameter("List"))
			{
				list = eAIGetOffensiveItemList.GetParameter("List") as List<AICommandList>;
				list.Clear();
			}
			else
			{
				list = new List<AICommandList>(8);
				eAIGetOffensiveItemList.SetParameter("List", list);
			}
			eAIGetOffensiveItemList.SetParameter("Distance", Target.DistanceTo(base.ParentObject));
			eAIGetOffensiveItemList.SetParameter("Target", Target);
			eAIGetOffensiveItemList.SetParameter("User", base.ParentObject);
			base.ParentObject.FireEvent(eAIGetOffensiveItemList);
			if (AICommandList.HandleCommandList(list, base.ParentObject, Target))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryMutations()
	{
		if (Target.InSameZone(base.ParentObject))
		{
			List<AICommandList> list;
			if (eAIGetOffensiveMutationList.HasParameter("List"))
			{
				list = eAIGetOffensiveMutationList.GetParameter("List") as List<AICommandList>;
				list.Clear();
			}
			else
			{
				list = new List<AICommandList>(8);
				eAIGetOffensiveMutationList.SetParameter("List", list);
			}
			eAIGetOffensiveMutationList.SetParameter("Distance", Target.DistanceTo(base.ParentObject));
			eAIGetOffensiveMutationList.SetParameter("Target", Target);
			eAIGetOffensiveMutationList.SetParameter("User", base.ParentObject);
			base.ParentObject.FireEvent(eAIGetOffensiveMutationList);
			if (AICommandList.HandleCommandList(list, base.ParentObject, Target))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryDefensiveMutations()
	{
		if (Target.InSameZone(base.ParentObject))
		{
			List<AICommandList> list;
			if (eAIGetDefensiveMutationList.HasParameter("List"))
			{
				list = eAIGetDefensiveMutationList.GetParameter("List") as List<AICommandList>;
				list.Clear();
			}
			else
			{
				list = new List<AICommandList>(8);
				eAIGetDefensiveMutationList.SetParameter("List", list);
			}
			eAIGetDefensiveMutationList.SetParameter("Distance", Target.DistanceTo(base.ParentObject));
			eAIGetDefensiveMutationList.SetParameter("User", base.ParentObject);
			eAIGetDefensiveMutationList.SetParameter("Target", Target);
			base.ParentObject.FireEvent(eAIGetDefensiveMutationList);
			if (AICommandList.HandleCommandList(list, base.ParentObject, Target))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryAbilities()
	{
		return false;
	}

	public GameObject FindTargetOfOpportunity(Cell MyCell, Cell TargetCell = null)
	{
		if (TargetCell != null)
		{
			Cell cellFromDirectionOfCell = MyCell.GetCellFromDirectionOfCell(TargetCell);
			if (cellFromDirectionOfCell != null)
			{
				GameObject combatTarget = cellFromDirectionOfCell.GetCombatTarget(base.ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, AllowInanimate: false);
				if (combatTarget != null && base.ParentObject.IsHostileTowards(combatTarget))
				{
					return combatTarget;
				}
			}
		}
		foreach (Cell localAdjacentCell in MyCell.GetLocalAdjacentCells())
		{
			GameObject combatTarget2 = localAdjacentCell.GetCombatTarget(base.ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, AllowInanimate: false);
			if (combatTarget2 != null && base.ParentObject.IsHostileTowards(combatTarget2))
			{
				return combatTarget2;
			}
		}
		return null;
	}

	public GameObject FindTargetOfOpportunity(GameObject Target = null)
	{
		return FindTargetOfOpportunity(base.ParentObject.CurrentCell, Target?.CurrentCell);
	}

	public override void TakeAction()
	{
		if (base.ParentObject.HasTagOrProperty("NoCombat"))
		{
			FailToParent();
			return;
		}
		base.ParentObject.FireEvent("AICombatStart");
		if (Target == null)
		{
			Think("I don't have a target any more!");
			FailToParent();
			return;
		}
		if (Target.pRender == null || !Target.pRender.Visible)
		{
			Think("I can't see my target any more!");
			FailToParent();
			return;
		}
		if (Target.IsNowhere())
		{
			Think("My target has been destroyed!");
			FailToParent();
			return;
		}
		if (Target == ParentBrain.PartyLeader)
		{
			Think("I shouldn't kill my party leader.");
			FailToParent();
			return;
		}
		if (ParentBrain.IsPlayerLed() && Target.IsPlayerLed())
		{
			Think("I shouldn't kill other members of the player's party.");
			FailToParent();
			return;
		}
		Cell currentCell = base.ParentObject.CurrentCell;
		Cell currentCell2 = Target.GetCurrentCell();
		if (currentCell2 == null || currentCell == null)
		{
			return;
		}
		if (currentCell2 == currentCell)
		{
			Think("I should get some distance from my target.");
			PushChildGoal(new Flee(Target, 1));
			return;
		}
		GameObject combatTarget = currentCell2.GetCombatTarget(base.ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, !Target.HasPart("Combat"));
		if (combatTarget != null && combatTarget != Target && !ParentBrain.IsHostileTowards(combatTarget))
		{
			Think("I shouldn't try to kill something if by doing so I would hit somebody else I don't want to.");
			FailToParent();
			return;
		}
		if (base.ParentObject.HasCopyRelationship(Target) && base.ParentObject.GetIntProperty("CopyAllowFeelingAdjust") <= 0 && Target.GetIntProperty("CopyAllowFeelingAdjust") <= 0)
		{
			Think("I shouldn't kill a friendly copy of myself from another time stream.");
			FailToParent();
			return;
		}
		int num = currentCell.DistanceToRespectStairs(currentCell2);
		if (currentCell2.ParentZone == null || currentCell2.ParentZone.ZoneID == null || num > 80)
		{
			LastSeen++;
		}
		if (LastSeen > 5)
		{
			Think("I can't find my target...");
			Target = null;
			FailToParent();
		}
		else if (Target.IsInvalid() || Target.IsInGraveyard())
		{
			Think("My target is dead!");
			Target = null;
			FailToParent();
		}
		else if (base.ParentObject.IsAflame() && (base.ParentObject.isDamaged(50, inclusive: true) || base.ParentObject.pPhysics.Temperature >= base.ParentObject.pPhysics.FlameTemperature * 2) && ++ExtinguishSelfTries < 5)
		{
			Think("I'm on fire!");
			PushChildGoal(new ExtinguishSelf());
		}
		else
		{
			if (!base.ParentObject.FireEvent("AIBeginKill"))
			{
				return;
			}
			if (currentCell2 != null && num == 1)
			{
				if (!base.ParentObject.FireEvent("AIAttackMelee"))
				{
					if (!base.ParentObject.FireEvent("AICanAttackMelee"))
					{
						Target = null;
						FailToParent();
					}
					return;
				}
				bool isFlying = Target.IsFlying;
				bool isFlying2 = base.ParentObject.IsFlying;
				if ((isFlying && !isFlying2) || !base.ParentObject.PhaseMatches(Target))
				{
					GameObject gameObject = FindTargetOfOpportunity(base.ParentObject.CurrentCell);
					if (gameObject != null && base.ParentObject.FireEvent(Event.New("AISwitchToTargetOfOpportunity", "Target", Target, "AltTarget", gameObject)))
					{
						PushChildGoal(new Kill(gameObject));
					}
					else
					{
						PushChildGoal(new Flee(Target, 2));
					}
					return;
				}
				List<string> list = tryMeleeOrder;
				string tagOrStringProperty = base.ParentObject.GetTagOrStringProperty("customMeleeOrder");
				if (tagOrStringProperty != null)
				{
					list = tagOrStringProperty.CachedCommaExpansion();
				}
				else
				{
					list.ShuffleInPlace();
				}
				Think("I'm going to melee my target in melee!");
				using (List<string>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						switch (enumerator.Current)
						{
						case "defensiveItems":
						{
							Think("I'm going to try my defensive items.");
							int num2 = base.ParentObject.Stat("Energy");
							if (TryDefensiveItems())
							{
								if (base.ParentObject.Stat("Energy") == num2)
								{
									base.ParentObject.UseEnergy(1000);
								}
								return;
							}
							break;
						}
						case "missile":
							if ((ParentBrain.PointBlankRange || !Target.FlightMatches(base.ParentObject)) && TryMissileWeapon())
							{
								return;
							}
							break;
						case "mutations":
							if (TryMutations())
							{
								return;
							}
							break;
						case "defensiveMutations":
							if (TryDefensiveMutations())
							{
								return;
							}
							break;
						case "abilities":
							if (TryAbilities())
							{
								return;
							}
							break;
						case "items":
							if (TryItems())
							{
								return;
							}
							break;
						}
					}
				}
				if (isFlying && !isFlying2)
				{
					if (ParentBrain.PartyLeader == null)
					{
						Target = null;
					}
					FailToParent();
					return;
				}
				Cell cell = currentCell2;
				Event e = ((!base.ParentObject.IsFlying || Target.IsFlying) ? Event.New("CommandAttackCell", "Cell", cell) : Event.New("CommandSwoopAttack", "Direction", currentCell.GetDirectionFromCell(cell)));
				if (ParentBrain != null && base.ParentObject != null)
				{
					base.ParentObject.FireEvent(e);
				}
				return;
			}
			if (!base.ParentObject.FireEvent("AIAttackRange"))
			{
				if (!base.ParentObject.FireEvent("AICanAttackRange"))
				{
					Target = null;
					FailToParent();
				}
				return;
			}
			Think("I'm going to try attacking my target at range!");
			List<string> list2 = tryMissileOrder;
			string tagOrStringProperty2 = base.ParentObject.GetTagOrStringProperty("customMissileOrder");
			if (tagOrStringProperty2 != null)
			{
				list2 = tagOrStringProperty2.CachedCommaExpansion();
			}
			else
			{
				list2.ShuffleInPlace();
			}
			using (List<string>.Enumerator enumerator = list2.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current)
					{
					case "defensiveItems":
					{
						int num3 = base.ParentObject.Stat("Energy");
						if (TryDefensiveItems())
						{
							if (base.ParentObject.Stat("Energy") == num3)
							{
								base.ParentObject.UseEnergy(1000);
							}
							return;
						}
						break;
					}
					case "mutations":
						if (TryMutations())
						{
							return;
						}
						break;
					case "defensiveMutations":
						if (TryDefensiveMutations())
						{
							return;
						}
						break;
					case "abilities":
						if (TryAbilities())
						{
							return;
						}
						break;
					case "missile":
						if (TryReload() || TryMissileWeapon())
						{
							return;
						}
						break;
					case "thrown":
						if (TryThrownWeapon())
						{
							return;
						}
						break;
					case "items":
						if (TryItems())
						{
							return;
						}
						break;
					}
				}
			}
			if (!base.ParentObject.FireEvent("AICantAttackRange"))
			{
				return;
			}
			if (ParentBrain.isMobile())
			{
				LastAttacked++;
				if (LastAttacked > 8 && Math.Max(1, 6 - base.ParentObject.StatMod("Intelligence") * 3 / 2).in100())
				{
					Think("I'm going to stop pursuing my target.");
					ParentBrain.Goals.Clear();
					if (!base.ParentObject.IsPlayerControlled())
					{
						ParentBrain.Hibernating = true;
						ParentBrain.PushGoal(new Wait(2));
					}
				}
				else if (num <= 1)
				{
					base.ParentObject.UseEnergy(1000);
					Think("I'm close enough to my target.");
				}
				else if (base.ParentObject.FireEvent(Event.New("AIMovingTowardsTarget", "Target", Target)))
				{
					Think("I'm going to move towards my target.");
					bool pathGlobal = Target.IsPlayer();
					Cell currentCell3 = base.ParentObject.CurrentCell;
					Cell currentCell4 = Target.CurrentCell;
					Zone parentZone = currentCell3.ParentZone;
					Zone parentZone2 = currentCell4.ParentZone;
					if (parentZone2.IsWorldMap())
					{
						Think("Target's on the world map, can't follow!");
						Target = null;
						FailToParent();
						return;
					}
					if (parentZone2 != currentCell3.ParentZone && base.ParentObject.HasTagOrProperty("StaysOnZLevel") && parentZone2 != null && currentCell3.ParentZone != null && parentZone2.Z != currentCell3.ParentZone.Z)
					{
						Think("Target's on another Z level, can't follow!");
						Target = null;
						FailToParent();
						return;
					}
					if (parentZone.ZoneID == null || parentZone2.ZoneID == null)
					{
						Target = null;
						FailToParent();
						return;
					}
					FindPath findPath = new FindPath(parentZone.ZoneID, currentCell3.X, currentCell3.Y, parentZone2.ZoneID, currentCell4.X, currentCell4.Y, pathGlobal, PathUnlimited: false, base.ParentObject);
					if (findPath.bFound && findPath.Directions.Count > 0)
					{
						Think("I found a step to take toward my target, I'm going to try it.");
						MoveTries = 0;
						parentZone.MarkActive(parentZone2);
						PushChildGoal(new Step(findPath.Directions[0], careful: false, overridesCombat: false, wandering: false, juggernaut: false, Target));
						return;
					}
					GameObject gameObject2 = FindTargetOfOpportunity(currentCell3, currentCell4);
					if (gameObject2 != null && base.ParentObject.FireEvent(Event.New("AISwitchToTargetOfOpportunity", "Target", Target, "AltTarget", gameObject2)))
					{
						parentZone.MarkActive(parentZone2);
						PushChildGoal(new Kill(gameObject2));
						return;
					}
					if (ParentBrain.limitToAquatic())
					{
						PushChildGoal(new Flee(Target, 10));
						return;
					}
					Think("I can't find a path.");
					if (!base.ParentObject.FireEvent(Event.New("AIFailCombatPathfind", "Target", Target)))
					{
						FailToParent();
						return;
					}
					if (MoveTries > 1)
					{
						if (MoveTries > 2)
						{
							FailToParent();
						}
						else
						{
							PushChildGoal(new Wait(Stat.Random(1, 3)));
						}
					}
					MoveTries++;
				}
				else
				{
					base.ParentObject.UseEnergy(1000);
					Think("I'm not allowed to move.");
				}
			}
			else
			{
				base.ParentObject.UseEnergy(1000);
				Think("My target is too far and I'm immobile.");
				FailToParent();
			}
		}
	}
}
