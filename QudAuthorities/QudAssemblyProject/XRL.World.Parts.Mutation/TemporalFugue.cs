using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class TemporalFugue : BaseMutation
{
	public new Guid ActivatedAbilityID;

	public TemporalFugue()
	{
		DisplayName = "Temporal Fugue";
		Type = "Mental";
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("time", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandTemporalFugue");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You quickly pass back and forth through time creating multiple copies of yourself.";
	}

	public static int GetTemporalFugueDuration(int Level)
	{
		return 20 + 2 * (Level / 2);
	}

	public int GetTemporalFugueDuration()
	{
		return GetTemporalFugueDuration(base.Level);
	}

	public static int GetTemporalFugueCopies(int Level)
	{
		return (Level - 1) / 2 + 1;
	}

	public int GetTemporalFugueCopies()
	{
		return GetTemporalFugueCopies(base.Level);
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("" + "Duration: {{rules|" + GetTemporalFugueDuration(Level) + "}} rounds\n", "Copies: {{rules|", GetTemporalFugueCopies(Level).ToString(), "}}\n"), "Cooldown: 200 rounds");
	}

	public static bool PerformTemporalFugue(GameObject who, TemporalFugue Mutation = null, Event TriggeringEvent = null, GameObject Source = null, bool Involuntary = false, int? Duration = null, int? Copies = null, int HostileCopyChance = 0, string FriendlyCopyColorString = null, string HostileCopyColorString = null, string FriendlyCopyPrefix = null, string HostileCopyPrefix = null)
	{
		if (who.HasStringProperty("FugueCopy"))
		{
			if (!Involuntary && who.IsPlayer())
			{
				Popup.ShowFail("You are too tenuously anchored in this time to perform temporal fugue in it.");
			}
			return false;
		}
		Cell cell = who.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		if (cell.ParentZone != null && cell.ParentZone.IsWorldMap())
		{
			if (!Involuntary && who.IsPlayer())
			{
				Popup.ShowFail("You may not activate temporal fugue on the world map.");
			}
			return false;
		}
		string context = "Temporal Fugue";
		if (!CanBeReplicatedEvent.Check(who, Source, context))
		{
			if (!Involuntary && who.IsPlayer())
			{
				Popup.ShowFail("It is impossible to duplicate you.");
			}
			return false;
		}
		Event e = ((Mutation != null) ? Event.New("InitiateRealityDistortionLocal", "Object", who, "Mutation", Mutation) : ((Source == null) ? Event.New("InitiateRealityDistortionLocal", "Object", who) : Event.New("InitiateRealityDistortionLocal", "Object", who, "Source", Source)));
		if (!who.FireEvent(e, TriggeringEvent))
		{
			return false;
		}
		int num = 0;
		int duration = (Duration.HasValue ? Duration.Value : ((Mutation == null) ? Stat.Random(20, 40) : (Mutation.GetTemporalFugueDuration() + 1)));
		int num2 = (Copies.HasValue ? Copies.Value : (Mutation?.GetTemporalFugueCopies() ?? Stat.Random(2, 4)));
		Mutation?.CooldownMyActivatedAbility(Mutation.ActivatedAbilityID, 200);
		List<Cell> adjacentCells = cell.GetAdjacentCells(2);
		List<Cell> list = null;
		List<Cell> list2 = null;
		foreach (Cell item in adjacentCells.ShuffleInPlace())
		{
			if (!item.IsSpawnable())
			{
				continue;
			}
			int navigationWeightFor = item.GetNavigationWeightFor(who);
			if (navigationWeightFor >= 95)
			{
				continue;
			}
			if (navigationWeightFor >= 70)
			{
				if (list2 == null)
				{
					list2 = new List<Cell>();
				}
				list2.Add(item);
			}
			else if (navigationWeightFor >= 30 || !item.IsEmpty())
			{
				if (list == null)
				{
					list = new List<Cell>();
				}
				list.Add(item);
			}
			else if (CreateFugueCopyOf(who, item, Source, duration, HostileCopyChance, context, FriendlyCopyColorString, HostileCopyColorString, FriendlyCopyPrefix, HostileCopyPrefix) != null && ++num >= num2)
			{
				list = null;
				list2 = null;
				break;
			}
		}
		if (list != null)
		{
			foreach (Cell item2 in list)
			{
				if (CreateFugueCopyOf(who, item2, Source, duration, HostileCopyChance, context, FriendlyCopyColorString, HostileCopyColorString, FriendlyCopyPrefix, HostileCopyPrefix) != null && ++num >= num2)
				{
					list2 = null;
					break;
				}
			}
		}
		if (list2 != null)
		{
			foreach (Cell item3 in list2)
			{
				if (CreateFugueCopyOf(who, item3, Source, duration, HostileCopyChance, context, FriendlyCopyColorString, HostileCopyColorString, FriendlyCopyPrefix, HostileCopyPrefix) != null && ++num >= num2)
				{
					break;
				}
			}
		}
		return true;
	}

	public static GameObject CreateFugueCopyOf(GameObject Object, Cell TargetCell, GameObject Source = null, int Duration = 20, int HostileCopyChance = 0, string Context = "Temporal Fugue", string FriendlyCopyColorString = null, string HostileCopyColorString = null, string FriendlyCopyPrefix = null, string HostileCopyPrefix = null)
	{
		if (Object.HasStringProperty("FugueCopy"))
		{
			return null;
		}
		if (!TargetCell.FireEvent("CheckRealityDistortionAccessibility"))
		{
			return null;
		}
		if (!CanBeReplicatedEvent.Check(Object, Source, Context))
		{
			return null;
		}
		GameObject gameObject = Object.DeepCopy();
		if (gameObject.HasStat("XPValue"))
		{
			gameObject.GetStat("XPValue").BaseValue = 0;
		}
		if (Object.IsPlayer())
		{
			gameObject.SetStringProperty("PlayerCopy", "true");
			if (Object.IsOriginalPlayerBody())
			{
				if (Object.HasPart("ConversationScript"))
				{
					gameObject.RemovePart("Chat");
					gameObject.AddPart(new Chat("*You make small talk with =player.reflexive=."));
				}
				gameObject.SetStringProperty("OriginalPlayerCopy", "true");
			}
		}
		gameObject.SetStringProperty("FugueCopy", Object.id);
		gameObject.SetStringProperty("CloneOf", Object.id);
		if (HostileCopyChance.in100())
		{
			gameObject.SetIntProperty("CopyAllowFeelingAdjust", 1);
			if (gameObject.pBrain != null)
			{
				gameObject.pBrain.PartyLeader = null;
				gameObject.pBrain.Staying = false;
				gameObject.pBrain.Passive = false;
				gameObject.pBrain.Goals.Clear();
				gameObject.pBrain.Target = Object;
			}
			if (!string.IsNullOrEmpty(HostileCopyColorString))
			{
				gameObject.pRender.ColorString = HostileCopyColorString;
			}
			if (!string.IsNullOrEmpty(HostileCopyPrefix))
			{
				gameObject.pRender.DisplayName = HostileCopyPrefix + " " + gameObject.pRender.DisplayName;
			}
		}
		else
		{
			if (gameObject.pBrain != null)
			{
				gameObject.pBrain.Goals.Clear();
				gameObject.pBrain.PartyLeader = Object;
				gameObject.IsTrifling = true;
			}
			if (!string.IsNullOrEmpty(FriendlyCopyColorString))
			{
				gameObject.pRender.ColorString = FriendlyCopyColorString;
			}
			if (!string.IsNullOrEmpty(FriendlyCopyPrefix))
			{
				gameObject.pRender.DisplayName = FriendlyCopyPrefix + " " + gameObject.pRender.DisplayName;
			}
		}
		Temporary.AddHierarchically(gameObject, Duration, "*fugue");
		gameObject.FireEvent("TemporalFugueCopied");
		TargetCell.AddObject(gameObject);
		gameObject.MakeActive();
		WasReplicatedEvent.Send(Object, Source, gameObject, Context);
		ReplicaCreatedEvent.Send(gameObject, Source, Object, Context);
		if (Object.IsPlayer() && !AchievementManager.GetAchievement("ACH_30_CLONES"))
		{
			Cloning.QueueAchievementCheck();
		}
		return gameObject;
	}

	public bool PerformTemporalFugue(Event TriggeringEvent = null)
	{
		return PerformTemporalFugue(ParentObject, this, TriggeringEvent);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (!ParentObject.HasStringProperty("FugueCopy") && E.GetIntParameter("Distance") <= 2 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)) && CanBeReplicatedEvent.Check(ParentObject, ParentObject))
			{
				E.AddAICommand("CommandTemporalFugue");
			}
		}
		else if (E.ID == "CommandTemporalFugue")
		{
			if (!PerformTemporalFugue(E))
			{
				return false;
			}
			UseEnergy(1000, "Mental Mutation");
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Temporal Fugue", "CommandTemporalFugue", "Mental Mutation", null, "\u0013", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
