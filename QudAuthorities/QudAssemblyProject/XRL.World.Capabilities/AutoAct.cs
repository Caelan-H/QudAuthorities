using XRL.Messages;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Capabilities;

public static class AutoAct
{
	public const string OngoingActionSetting = "o";

	public static string ResumeSetting = "";

	public static int Digging = 0;

	public static OngoingAction _Action = null;

	public static OngoingAction _ResumeAction = null;

	public static string Setting
	{
		get
		{
			return The.Core.PlayerWalking;
		}
		set
		{
			The.Core.PlayerWalking = value;
			Digging = 0;
		}
	}

	public static OngoingAction Action
	{
		get
		{
			return _Action;
		}
		set
		{
			if (value != null && IsActive())
			{
				ResumeAction = _Action;
				ResumeSetting = Setting;
			}
			_Action = value;
			if (_Action != null)
			{
				Setting = "o";
			}
		}
	}

	public static OngoingAction ResumeAction
	{
		get
		{
			return _ResumeAction;
		}
		set
		{
			_ResumeAction = value;
			if (_ResumeAction != null)
			{
				ResumeSetting = "o";
			}
		}
	}

	public static bool IsActive(string what)
	{
		if (what != null && what != "")
		{
			return what != "ReopenMissileUI";
		}
		return false;
	}

	public static bool IsActive()
	{
		return IsActive(Setting);
	}

	public static bool IsInterruptable(string what)
	{
		return IsActive(what);
	}

	public static bool IsInterruptable()
	{
		return IsInterruptable(Setting);
	}

	public static bool ShouldHostilesInterrupt(string what, OngoingAction Action = null, bool logSpot = false, bool popSpot = false, bool CheckingPrior = true)
	{
		if (!IsActive(what))
		{
			return false;
		}
		if (what == "?")
		{
			if (Options.AutogetIfHostiles)
			{
				return false;
			}
		}
		else if (what == "o" && Action != null && !Action.ShouldHostilesInterrupt())
		{
			return false;
		}
		return The.Player.ArePerceptibleHostilesNearby(logSpot, popSpot, null, Action, what, CheckingPrior: CheckingPrior, IgnoreEasierThan: IsResting(what) ? int.MinValue : Options.AutoexploreIgnoreEasyEnemies, IgnoreFartherThan: Options.AutoexploreIgnoreDistantEnemies, IgnorePlayerTarget: IsCombat(what));
	}

	public static bool ShouldHostilesInterrupt(bool logSpot = false, bool popSpot = false, bool CheckingPrior = true)
	{
		return ShouldHostilesInterrupt(Setting, Action, logSpot, popSpot, CheckingPrior);
	}

	public static bool CheckHostileInterrupt(bool logSpot)
	{
		if (ShouldHostilesInterrupt(logSpot, popSpot: false, CheckingPrior: false))
		{
			Interrupt();
			return true;
		}
		return false;
	}

	public static bool CheckHostileInterrupt()
	{
		return CheckHostileInterrupt(!IsOnlyGathering() || Sidebar.AnyAutogotItems());
	}

	public static bool ShouldHostilesPreventAutoget()
	{
		return ShouldHostilesInterrupt("g");
	}

	public static bool IsMovement(string what, OngoingAction Action = null)
	{
		if (!IsActive(what))
		{
			return false;
		}
		switch (what[0])
		{
		case '.':
			return false;
		case 'g':
			return false;
		case 'o':
			return Action?.IsMovement() ?? false;
		case 'r':
		case 'z':
			return false;
		default:
			return true;
		}
	}

	public static bool IsMovement()
	{
		return IsMovement(Setting, Action);
	}

	public static bool IsAnyMovement()
	{
		if (!IsMovement(Setting, Action))
		{
			return IsMovement(ResumeSetting, ResumeAction);
		}
		return true;
	}

	public static bool IsCombat(string what, OngoingAction Action = null)
	{
		return what switch
		{
			"ReopenMissileUI" => true, 
			"a" => true, 
			"o" => Action?.IsCombat() ?? false, 
			_ => false, 
		};
	}

	public static bool IsCombat()
	{
		return IsCombat(Setting, Action);
	}

	public static bool IsAnyCombat()
	{
		if (!IsCombat(Setting, Action))
		{
			return IsCombat(ResumeSetting, ResumeAction);
		}
		return true;
	}

	public static bool IsExploration(string what, OngoingAction Action = null)
	{
		if (what == "?")
		{
			return true;
		}
		if (what == "o")
		{
			return Action?.IsExploration() ?? false;
		}
		return false;
	}

	public static bool IsExploration()
	{
		return IsExploration(Setting, Action);
	}

	public static bool IsAnyExploration()
	{
		if (!IsExploration(Setting, Action))
		{
			return IsExploration(ResumeSetting, ResumeAction);
		}
		return true;
	}

	public static bool IsGathering(string what, OngoingAction Action = null)
	{
		if (what == "g")
		{
			return true;
		}
		if (what == "o")
		{
			return Action?.IsGathering() ?? false;
		}
		return false;
	}

	public static bool IsGathering()
	{
		return IsGathering(Setting, Action);
	}

	public static bool IsAnyGathering()
	{
		if (!IsGathering(Setting, Action))
		{
			return IsGathering(ResumeSetting, ResumeAction);
		}
		return true;
	}

	public static bool IsOnlyGathering()
	{
		if (IsGathering(Setting, Action))
		{
			if (!string.IsNullOrEmpty(ResumeSetting))
			{
				return IsGathering(ResumeSetting, ResumeAction);
			}
			return true;
		}
		return false;
	}

	public static bool IsResting(string what, OngoingAction Action = null)
	{
		if (!IsActive(what))
		{
			return false;
		}
		switch (what[0])
		{
		case '.':
		case 'r':
		case 'z':
			return true;
		case 'o':
			return Action?.IsResting() ?? false;
		default:
			return false;
		}
	}

	public static bool IsResting()
	{
		return IsResting(Setting, Action);
	}

	public static bool IsAnyResting()
	{
		if (!IsResting(Setting, Action))
		{
			return IsResting(ResumeSetting, ResumeAction);
		}
		return true;
	}

	public static bool IsRateLimited(string what, OngoingAction Action = null)
	{
		if (what == "o")
		{
			return Action?.IsRateLimited() ?? false;
		}
		if (IsGathering(what, Action))
		{
			return true;
		}
		if (IsCombat(what, Action))
		{
			return true;
		}
		if (IsMovement(what, Action) && Digging <= 0)
		{
			return true;
		}
		return false;
	}

	public static bool IsRateLimited()
	{
		return IsRateLimited(Setting, Action);
	}

	public static bool IsAnyRateLimited()
	{
		if (!IsRateLimited(Setting, Action))
		{
			return IsRateLimited(ResumeSetting, ResumeAction);
		}
		return true;
	}

	public static string GetDescription(string what, OngoingAction action)
	{
		if (!IsActive(what))
		{
			return "acting";
		}
		switch (what[0])
		{
		case '?':
			return "exploring";
		case '.':
			return "waiting";
		case 'd':
			if (Digging > 0)
			{
				return "digging";
			}
			break;
		case 'g':
			return "gathering";
		case 'o':
			return action?.GetDescription() ?? "acting";
		case 'r':
		case 'z':
			return "resting";
		case 'a':
			return "attacking";
		}
		return "moving";
	}

	public static string GetDescription()
	{
		if (!string.IsNullOrEmpty(ResumeSetting))
		{
			return GetDescription(ResumeSetting, ResumeAction);
		}
		return GetDescription(Setting, Action);
	}

	public static void Interrupt(string Because = null, Cell IndicateCell = null, GameObject IndicateObject = null)
	{
		if (IsActive())
		{
			if (Because == null && Action != null)
			{
				Because = Action.GetInterruptBecause();
			}
			if (!string.IsNullOrEmpty(Because) && (!IsGathering() || Sidebar.AnyAutogotItems()))
			{
				MessageQueue.AddPlayerMessage(Event.NewStringBuilder().Append("{{r|You stop ").Append(GetDescription())
					.Append(" because ")
					.Append(Because)
					.Append(".}}")
					.ToString());
			}
			if (IndicateObject != null && (!IsGathering() || Sidebar.AnyAutogotItems()))
			{
				IndicateObject.Indicate();
			}
			if (IndicateCell != null && (!IsGathering() || Sidebar.AnyAutogotItems()))
			{
				IndicateCell.Indicate();
			}
		}
		ResumeAction?.Interrupt();
		ResumeAction?.End();
		Action?.Interrupt();
		Action?.End();
		Setting = "";
		Action = null;
		ResumeSetting = "";
		ResumeAction = null;
		The.Core.PlayerAvoid.Clear();
	}

	public static void Interrupt(GameObject BecauseOf, bool ShowIndicator = true)
	{
		if (BecauseOf != null && IsActive())
		{
			if (!IsGathering() || Sidebar.AnyAutogotItems())
			{
				MessageQueue.AddPlayerMessage(The.Player.GenerateSpotMessage(BecauseOf));
			}
			if (ShowIndicator)
			{
				BecauseOf.Indicate();
			}
		}
		ResumeAction?.Interrupt();
		ResumeAction?.End();
		Action?.Interrupt();
		Action?.End();
		Setting = "";
		Action = null;
		ResumeSetting = "";
		ResumeAction = null;
		The.Core.PlayerAvoid.Clear();
	}

	public static void Resume()
	{
		Setting = ResumeSetting;
		Action = ResumeAction;
		ResumeSetting = "";
		ResumeAction = null;
	}

	public static bool TryToMove(GameObject Actor, Cell FromCell, ref GameObject LastDoor, Cell ToCell = null, string Direction = null, bool AllowDigging = true, bool OpenDoors = true, bool Peaceful = true, bool PostMoveHostileCheck = true, bool PostMoveSidebarCheck = true)
	{
		GameObject gameObject = LastDoor;
		LastDoor = null;
		if (FromCell == null)
		{
			Interrupt();
			return false;
		}
		if (ToCell == null && !string.IsNullOrEmpty(Direction))
		{
			ToCell = FromCell.GetCellFromDirection(Direction);
		}
		if (ToCell == null)
		{
			Interrupt();
			return false;
		}
		if (!ToCell.IsAdjacentTo(FromCell))
		{
			Interrupt();
			return false;
		}
		if (string.IsNullOrEmpty(Direction))
		{
			Direction = FromCell.GetDirectionFromCell(ToCell);
		}
		if (string.IsNullOrEmpty(Direction) || Direction == "." || Direction == "?")
		{
			Interrupt();
			return false;
		}
		GameObject gameObject2;
		Door door;
		if (OpenDoors)
		{
			int num = 0;
			int count = ToCell.Objects.Count;
			while (num < count)
			{
				gameObject2 = ToCell.Objects[num];
				door = gameObject2.GetPart("Door") as Door;
				if (door == null || door.bOpen || !Actor.PhaseMatches(gameObject2))
				{
					num++;
					continue;
				}
				goto IL_00db;
			}
		}
		if (AllowDigging && ToCell.IsSolidFor(Actor) && Actor.IsBurrower)
		{
			if (Digging > 1000)
			{
				if (Actor.IsPlayer())
				{
					Popup.Show("You cannot seem to make any progress digging.");
				}
				Interrupt(null, ToCell);
				return false;
			}
			int num2 = 0;
			int i = 0;
			for (int count2 = ToCell.Objects.Count; i < count2; i++)
			{
				num2 += ToCell.Objects[i].hitpoints;
			}
			if (!Actor.AttackDirection(Direction))
			{
				Interrupt(null, ToCell);
				return false;
			}
			int num3 = 0;
			int j = 0;
			for (int count3 = ToCell.Objects.Count; j < count3; j++)
			{
				num3 += ToCell.Objects[j].hitpoints;
			}
			if (num3 < num2)
			{
				Digging = 1;
			}
			else
			{
				Digging++;
			}
		}
		else
		{
			if (!Actor.Move(Direction, Forced: false, System: false, IgnoreGravity: false, NoStack: false, null, NearestAvailable: false, null, null, null, Peaceful))
			{
				Interrupt(null, ToCell);
				return false;
			}
			if (Actor.CurrentCell == FromCell)
			{
				Interrupt();
				return false;
			}
			Digging = 0;
		}
		goto IL_026a;
		IL_00db:
		if (gameObject2 == gameObject)
		{
			Interrupt(null, null, gameObject2);
			return false;
		}
		LastDoor = gameObject2;
		if (door.FireEvent(Event.New("Open", "Opener", Actor, "UsePopups", Actor.IsPlayer())))
		{
			Digging = 0;
			goto IL_026a;
		}
		Interrupt(null, null, gameObject2);
		return false;
		IL_026a:
		if (PostMoveHostileCheck)
		{
			CheckHostileInterrupt();
		}
		if (PostMoveSidebarCheck && Actor.IsPlayer())
		{
			Cell currentCell = Actor.CurrentCell;
			if (currentCell != null && currentCell != FromCell)
			{
				if (currentCell.X > 42 && Sidebar.State == "right")
				{
					Sidebar.SetSidebarState("left");
				}
				else if (currentCell.X < 38 && Sidebar.State == "left")
				{
					Sidebar.SetSidebarState("right");
				}
			}
		}
		return true;
	}

	public static bool TryToMove(GameObject Actor, Cell FromCell, Cell ToCell = null, string Direction = null, bool AllowDigging = true, bool OpenDoors = true, bool Peaceful = true, bool PostMoveHostileCheck = true, bool PostMoveSidebarCheck = true)
	{
		GameObject LastDoor = null;
		return TryToMove(Actor, FromCell, ref LastDoor, ToCell, Direction, AllowDigging, OpenDoors, Peaceful, PostMoveHostileCheck, PostMoveSidebarCheck);
	}
}
