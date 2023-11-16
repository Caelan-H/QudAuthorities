using System;
using System.Text;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Cloneling : IPart
{
	public const int CLONES_PER_DRAM = 40;

	public const string INITIAL_CLONES = "10-40";

	public const string REPLICATION_CONTEXT = "Cloneling";

	public int ClonesLeft;

	public Guid ActivatedAbilityID;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ClonesLeft = "10-40".Roll();
		SyncAbility();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (20.in100() && ClonesLeft.in100())
		{
			Cell dropCell = ParentObject.GetDropCell();
			if (dropCell != null)
			{
				GameObject gameObject = GameObject.create("Phial");
				LiquidVolume liquidVolume = gameObject.LiquidVolume;
				if (liquidVolume != null)
				{
					liquidVolume.InitialLiquid = "cloning";
					liquidVolume.Volume = 1;
				}
				dropCell.AddObject(gameObject);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!ParentObject.IsHostileTowards(E.Actor))
		{
			E.AddAction("ResupplyForCloning", "resupply [1 dram of " + ColorUtility.StripFormatting(LiquidVolume.getLiquid("cloning").GetName()) + "]", "ResupplyForCloning", null, 'r', FireOnActor: false, -1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ResupplyForCloning")
		{
			if (E.Actor.UseDrams(1, "cloning"))
			{
				ClonesLeft = 40;
				SyncAbility();
				IComponent<GameObject>.XDidYToZ(E.Actor, "resupply", ParentObject, "with " + LiquidVolume.getLiquid("cloning").GetName());
				E.Actor.UseEnergy(1000, "Action Resupply");
				E.RequestInterfaceExit();
			}
			else if (E.Actor.IsPlayer())
			{
				Popup.ShowFail("You do not have 1 dram of " + LiquidVolume.getLiquid("cloning").GetName() + ".");
			}
		}
		return base.HandleEvent(E);
	}

	public override void Initialize()
	{
		base.Initialize();
		ActivatedAbilityID = AddMyActivatedAbility("Clone", "CommandClone", "Onboard Systems", null, "\u0013");
	}

	public override void Remove()
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		base.Remove();
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CommandClone");
		Object.RegisterPartEvent(this, "DrinkingFrom");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			ConsiderCloning();
		}
		else if (E.ID == "CommandClone")
		{
			AttemptCloning();
		}
		else if (E.ID == "DrinkingFrom")
		{
			int num = Math.Min(E.GetGameObjectParameter("Container").LiquidVolume.MilliAmount("cloning") * 40 / 1000, 40);
			if (num > ClonesLeft)
			{
				ClonesLeft = num;
				SyncAbility();
			}
		}
		return base.FireEvent(E);
	}

	public void SyncAbility()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Clone [").Append(ClonesLeft).Append(" left]");
		SetMyActivatedAbilityDisplayName(ActivatedAbilityID, stringBuilder.ToString());
		if (ClonesLeft <= 0)
		{
			DisableMyActivatedAbility(ActivatedAbilityID);
		}
		else
		{
			EnableMyActivatedAbility(ActivatedAbilityID);
		}
	}

	public bool ConsiderWandering()
	{
		if (ParentObject.IsPlayerControlled())
		{
			return false;
		}
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return false;
		}
		ParentObject.pBrain.PushGoal(new WanderRandomly(5));
		return true;
	}

	public bool ConsiderCloning()
	{
		if (ParentObject.IsPlayer())
		{
			return false;
		}
		if (ClonesLeft <= 0)
		{
			ConsiderWandering();
			return false;
		}
		if (!IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			ConsiderWandering();
			return false;
		}
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return false;
		}
		for (int i = 0; i < ParentObject.pBrain.Goals.Items.Count; i++)
		{
			if (ParentObject.pBrain.Goals.Items[i].GetType().FullName.Contains("Cloneling"))
			{
				return false;
			}
		}
		GameObject randomElement = ParentObject.CurrentZone.FastCombatSquareVisibility(ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, 12, ParentObject, CanBeCloned).GetRandomElement();
		if (randomElement == null)
		{
			ConsiderWandering();
			return false;
		}
		if (ParentObject.pBrain.Staying && ParentObject.DistanceTo(randomElement) > 1)
		{
			return false;
		}
		ParentObject.pBrain.Goals.Clear();
		ParentObject.pBrain.PushGoal(new ClonelingGoal(randomElement));
		return true;
	}

	public bool CanBeCloned(GameObject Target)
	{
		if (!Cloning.CanBeCloned(Target, ParentObject, "Cloneling"))
		{
			return false;
		}
		if (!Target.PhaseAndFlightMatches(ParentObject))
		{
			return false;
		}
		return true;
	}

	public bool PerformCloning(GameObject Target)
	{
		if (ClonesLeft <= 0)
		{
			return false;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		Cell randomElement = cell.GetLocalEmptyAdjacentCells().GetRandomElement();
		if (randomElement == null)
		{
			return false;
		}
		GameObject gameObject = Target.DeepCopy();
		gameObject.StripContents(KeepNatural: true, Silent: true);
		Statistic stat = gameObject.GetStat("XPValue");
		if (stat != null)
		{
			stat.BaseValue /= 2;
		}
		gameObject.RestorePristineHealth();
		gameObject.RemoveIntProperty("ProperNoun");
		gameObject.RemoveIntProperty("Renamed");
		gameObject.ModIntProperty("IsClone", 1);
		gameObject.ModIntProperty("IsClonelingClone", 1);
		gameObject.SetStringProperty("CloneOf", Target.id);
		if (gameObject.pRender != null && !Target.HasPropertyOrTag("CloneNoNameChange") && !Target.BaseDisplayName.Contains("clone of"))
		{
			if (Target.HasProperName)
			{
				gameObject.pRender.DisplayName = "clone of " + Target.a + Target.BaseDisplayName;
			}
			else
			{
				string text = gameObject.GetBlueprint().DisplayName();
				if (!string.IsNullOrEmpty(text) && !text.Contains("["))
				{
					gameObject.pRender.DisplayName = "clone of " + text;
				}
				else
				{
					gameObject.pRender.DisplayName = "clone of " + Target.a + Target.BaseDisplayName;
				}
			}
		}
		if (gameObject.pBrain != null)
		{
			gameObject.pBrain.Mindwipe();
		}
		if (Target.IsPlayer() && !AchievementManager.GetAchievement("ACH_30_CLONES"))
		{
			Cloning.QueueAchievementCheck();
		}
		randomElement.AddObject(gameObject);
		gameObject.MakeActive();
		WasReplicatedEvent.Send(Target, ParentObject, gameObject, "Cloneling");
		ReplicaCreatedEvent.Send(gameObject, ParentObject, Target, "Cloneling");
		gameObject.Bloodsplatter(bSelfsplatter: false);
		CooldownMyActivatedAbility(ActivatedAbilityID, "2d10".Roll());
		ClonesLeft--;
		SyncAbility();
		ParentObject.UseEnergy(1000, "Ability Clone");
		Messaging.XDidYToZ(ParentObject, "produce", "a clone of", Target, "in a flurry of {{C|flashing chrome}} and {{cloning|spurting liquid}}", null, null, ParentObject);
		return true;
	}

	public bool AttemptCloning()
	{
		if (ClonesLeft <= 0)
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("Your onboard systems are out of " + LiquidVolume.getLiquid("cloning").GetName() + ".");
			}
			return false;
		}
		if (!IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID))
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("Your onboard cloning systems are offline.");
			}
			return false;
		}
		Cell cell = PickDirection();
		if (cell == null)
		{
			return false;
		}
		GameObject combatTarget = cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
		if (combatTarget == null)
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("There is nothing there for you to clone.");
			}
			return false;
		}
		if (!CanBeCloned(combatTarget))
		{
			if (ParentObject.IsPlayer())
			{
				if (!combatTarget.FlightMatches(ParentObject))
				{
					Popup.ShowFail("You cannot reach " + combatTarget.the + combatTarget.ShortDisplayName + ".");
				}
				else if (!combatTarget.PhaseMatches(ParentObject))
				{
					Popup.ShowFail("You cannot make contact with " + combatTarget.the + combatTarget.ShortDisplayName + ".");
				}
				else if (Cloning.CanBeCloned(combatTarget))
				{
					Popup.ShowFail("You cannot clone " + combatTarget.the + combatTarget.ShortDisplayName + ".");
				}
				else
				{
					Popup.ShowFail(combatTarget.The + combatTarget.ShortDisplayName + " cannot be cloned.");
				}
			}
			return false;
		}
		if (!PerformCloning(combatTarget))
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("You fail to clone " + combatTarget.the + combatTarget.ShortDisplayName + ".");
			}
			return false;
		}
		return false;
	}
}
