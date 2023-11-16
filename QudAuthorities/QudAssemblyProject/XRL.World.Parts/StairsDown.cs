using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class StairsDown : IPart
{
	public bool Connected = true;

	public string ConnectionObject = "StairsUp";

	public bool PullDown;

	public bool GenericFall;

	[FieldSaveVersion(254)]
	public bool ConnectLanding = true;

	public string PullMessage = "You fall down a deep shaft!";

	public string JumpPrompt = "It looks like an awfully long fall. Are you sure you want to jump into the shaft?";

	public int Levels = 1;

	public override bool SameAs(IPart p)
	{
		StairsDown stairsDown = p as StairsDown;
		if (stairsDown.Connected != Connected)
		{
			return false;
		}
		if (stairsDown.PullDown != PullDown)
		{
			return false;
		}
		if (stairsDown.PullMessage != PullMessage)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CheckAttackableEvent.ID && ID != CommandSmartUseEvent.ID && ID != EnteredCellEvent.ID && (ID != GetInventoryActionsEvent.ID || PullDown) && (ID != GetNavigationWeightEvent.ID || !PullDown) && (ID != GetAdjacentNavigationWeightEvent.ID || !PullDown) && ID != GravitationEvent.ID && ID != IdleQueryEvent.ID && (ID != InterruptAutowalkEvent.ID || !PullDown) && (ID != InventoryActionEvent.ID || PullDown) && (ID != ObjectEnteredCellEvent.ID || !PullDown) && (ID != ObjectEnteringCellEvent.ID || !PullDown))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!PullDown && LegacyKeyMapping.GetCommandFromKey((Keys)65726) == "CmdMoveD")
		{
			E.AddAction("Descend", "descend", "Descend", null, 'd');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Descend" && !PullDown && E.Actor.IsPlayer() && LegacyKeyMapping.GetCommandFromKey((Keys)65726) == "CmdMoveD")
		{
			Popup.ShowFail("Use {{W|>}} to descend.");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (PullDown)
		{
			E.MinWeight(99);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (PullDown)
		{
			E.MinWeight(2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckAttackableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		if (PullDown)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (E.Cell.ParentZone.ZoneWorld == "Tzimtzlum")
		{
			E.Cell.RemoveObject(ParentObject);
			ParentObject.Obliterate();
			E.Cell.AddObject("Space-Time Rift");
			return false;
		}
		int i = 0;
		for (int count = E.Cell.Objects.Count; i < count; i++)
		{
			GameObject gameObject = E.Cell.Objects[i];
			if (gameObject != ParentObject && gameObject.HasPart("StairsDown"))
			{
				E.Cell.RemoveObject(ParentObject);
				ParentObject.Obliterate();
				return false;
			}
		}
		if (Connected)
		{
			E.Cell.ParentZone.AddZoneConnection("d", E.Cell.X, E.Cell.Y, "StairsUp", ConnectionObject);
		}
		else if (ConnectLanding)
		{
			E.Cell.ParentZone.AddZoneConnection("d", E.Cell.X, E.Cell.Y, PullDown ? "PullDownEnd" : "DownEnd", null);
		}
		if (PullDown && !E.IgnoreGravity)
		{
			List<GameObject> list = Event.NewGameObjectList();
			list.AddRange(E.Cell.Objects);
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				CheckPullDown(list[j]);
			}
		}
		E.Cell?.ClearWalls();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		if (PullDown)
		{
			Cell cell = base.currentCell;
			if (cell != null)
			{
				List<GameObject> list = Event.NewGameObjectList();
				list.AddRange(cell.Objects);
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					CheckPullDown(list[i]);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		if (PullDown && !E.Forced && !E.System && IsValidForPullDown(E.Object) && E.Object.IsPlayer() && E.Object.GetConfusion() <= 0 && IsLongFall() && Popup.ShowYesNoCancel(JumpPrompt) != 0)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (PullDown && !E.IgnoreGravity)
		{
			CheckPullDown(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GravitationEvent E)
	{
		if (PullDown)
		{
			CheckPullDown(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.CurrentCell == ParentObject.CurrentCell)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.CurrentCell == ParentObject.CurrentCell)
		{
			if (E.Actor.IsPlayer())
			{
				Keyboard.PushMouseEvent("Command:CmdMoveD");
			}
			else
			{
				for (int i = 0; i < Levels; i++)
				{
					E.Actor.Move("D", Forced: false, Levels > 1);
				}
			}
		}
		return false;
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (ParentObject.HasTagOrProperty("IdleStairs") && E.Actor.HasPart("Brain") && Stat.Random(1, 2000) == 2000)
		{
			GameObject who = E.Actor;
			who.pBrain.PushGoal(new DelegateGoal(delegate(GoalHandler h)
			{
				if (who.CurrentCell == ParentObject.CurrentCell)
				{
					for (int i = 0; i < Levels; i++)
					{
						who.Move("D", Forced: false, Levels > 1);
					}
				}
				h.FailToParent();
			}));
			who.pBrain.PushGoal(new MoveTo(ParentObject));
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ClimbDown");
		base.Register(Object);
	}

	public bool IsValidForPullDown(GameObject go)
	{
		if (go == ParentObject)
		{
			return false;
		}
		if (go.HasPart("StairsUp"))
		{
			return false;
		}
		if (go.HasPart("StairsDown"))
		{
			return false;
		}
		if (go.HasPropertyOrTag("ElevatorPlatform"))
		{
			return false;
		}
		if (go.HasPropertyOrTag("NoFall"))
		{
			return false;
		}
		if (go.IsFlying)
		{
			return false;
		}
		if (!go.PhaseMatches(1))
		{
			return false;
		}
		if (go.HasTagOrProperty("IgnoresGravity"))
		{
			return false;
		}
		if (go.GetWeight() < 0.0)
		{
			return false;
		}
		if (go.pPhysics == null || !go.pPhysics.IsReal)
		{
			return false;
		}
		if (go.GetPart("Hangable") is Hangable hangable && hangable.Hanging)
		{
			return false;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ClimbDown")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("GO");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				string tag = ParentObject.GetTag("KeyObject");
				if (!string.IsNullOrEmpty(tag) && !gameObjectParameter.IsCarryingObject(tag))
				{
					DidX("are", "locked, and you don't have the key", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
					return false;
				}
			}
			if (!PullDown || gameObjectParameter.IsFlying)
			{
				for (int i = 0; i < Levels; i++)
				{
					gameObjectParameter.Move("D", Forced: false, Levels > 1);
				}
			}
			return false;
		}
		return base.FireEvent(E);
	}

	public bool CheckPullDown(GameObject obj)
	{
		if (!PullDown)
		{
			return false;
		}
		if (!IsValidForPullDown(obj))
		{
			return false;
		}
		Cell cell = ParentObject.CurrentCell;
		Cell cell2 = GetPullDownCell(cell, out var Distance);
		if (cell2 == null)
		{
			return false;
		}
		if (obj.IsPlayer())
		{
			ZoneManager.ZoneTransitionCount -= Distance;
		}
		if (!cell2.IsPassable(obj))
		{
			Cell closestPassableCellFor = cell2.getClosestPassableCellFor(obj);
			if (closestPassableCellFor != null && closestPassableCellFor.RealDistanceTo(cell2) <= 2.0)
			{
				cell2 = closestPassableCellFor;
			}
		}
		IComponent<GameObject>.XDidYToZ(obj, "fall", "down", ParentObject, null, null, null, null, obj, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
		obj.SystemMoveTo(cell2, 0, forced: true);
		if (!obj.IsPlayer())
		{
			IComponent<GameObject>.XDidY(obj, "fall", "down from above", null, null, null, obj, UseFullNames: false, IndefiniteSubject: true);
		}
		cell2 = obj.CurrentCell ?? cell2;
		List<GameObject> list = null;
		if (obj.GetMatterPhase() <= 2)
		{
			int phase = obj.GetPhase();
			int i = 0;
			for (int count = cell2.Objects.Count; i < count; i++)
			{
				GameObject gameObject = cell2.Objects[i];
				if (gameObject != obj && gameObject.HasPart("Combat") && gameObject.GetMatterPhase() <= 2 && gameObject.PhaseMatches(phase))
				{
					if (list == null)
					{
						list = Event.NewGameObjectList();
					}
					list.Add(gameObject);
				}
			}
		}
		if (list != null)
		{
			if (Distance <= 1)
			{
				list.Add(obj);
			}
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				list[j].TakeDamage(Stat.Random(1, 4), Owner: obj, Message: "from " + obj.the + obj.ShortDisplayName + " falling on " + list[j].them, Attributes: "Crushing", DeathReason: obj.A + obj.ShortDisplayName + " fell on " + list[j].them + ".", ThirdPersonDeathReason: obj.A + obj.ShortDisplayName + " fell on " + list[j].them + ".", Attacker: null, Source: null, Perspective: null, Accidental: true);
				if (obj.IsPlayer() && obj == list[j] && obj.hitpoints <= 0)
				{
					AchievementManager.SetAchievement("ACH_DIE_BY_FALLING");
				}
			}
		}
		if (Distance > 1)
		{
			string deathReason = (GenericFall ? "You fell from a great height." : ("You fell down " + ParentObject.a + ParentObject.ShortDisplayName + "."));
			string thirdPersonDeathReason = (GenericFall ? (obj.It + " @@fell from a great height.") : (obj.It + " @@fell down " + ParentObject.a + ParentObject.ShortDisplayName + "."));
			int amount2 = Stat.Roll(Distance + "d20+" + (100 + Distance * 25));
			GameObject owner2 = obj;
			obj.TakeDamage(amount2, "from " + obj.its + " fall.", "Crushing Falling", deathReason, thirdPersonDeathReason, owner2, null, null, null, Accidental: true);
			if (obj.IsPlayer() && obj.hitpoints <= 0)
			{
				AchievementManager.SetAchievement("ACH_DIE_BY_FALLING");
			}
		}
		FellDownEvent.Send(obj, cell2, cell, Distance);
		if (!obj.IsInGraveyard() && obj.PartyLeader != null && !obj.InSameZone(obj.PartyLeader) && !obj.HasEffect("Incommunicado"))
		{
			obj.ApplyEffect(new Incommunicado());
		}
		if (obj.IsPlayer())
		{
			The.ZoneManager.SetActiveZone(cell2.ParentZone.ZoneID);
			The.ZoneManager.ProcessGoToPartyLeader();
			if (Distance > 1)
			{
				IComponent<GameObject>.AddPlayerMessage("You fall down!");
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage(PullMessage);
			}
		}
		return true;
	}

	public bool IsLongFall()
	{
		GetPullDownCell(out var Distance, 2);
		return Distance > 1;
	}

	public Cell GetPullDownCell(Cell CC, out int Distance, int MaxDistance = int.MaxValue)
	{
		Distance = 0;
		if (CC == null || CC.ParentZone == null || !CC.ParentZone.Built || CC.HasObjectWithIntProperty("ElevatorPlatform"))
		{
			return null;
		}
		Cell cell = CC.GetCellFromDirection("D", BuiltOnly: false);
		bool flag = cell.HasObjectWithIntProperty("ElevatorPlatform");
		if (cell != null && !flag)
		{
			Distance++;
			if (Distance >= MaxDistance)
			{
				return null;
			}
			for (int i = 1; i < Levels; i++)
			{
				Cell cellFromDirection = cell.GetCellFromDirection("D", BuiltOnly: false);
				if (cellFromDirection == null)
				{
					break;
				}
				cell = cellFromDirection;
				Distance++;
				if (Distance >= MaxDistance)
				{
					return null;
				}
				if (cell.HasObjectWithIntProperty("ElevatorPlatform"))
				{
					break;
				}
			}
		}
		if (cell != null && !flag)
		{
			GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("StairsDown");
			if (firstObjectWithPart != null && firstObjectWithPart.GetPart("StairsDown") is StairsDown stairsDown && stairsDown.PullDown)
			{
				int Distance2;
				Cell pullDownCell = stairsDown.GetPullDownCell(out Distance2, MaxDistance - Distance);
				if (pullDownCell != null)
				{
					cell = pullDownCell;
				}
				Distance += Distance2;
				if (Distance >= MaxDistance)
				{
					return null;
				}
			}
		}
		return cell;
	}

	public Cell GetPullDownCell(out int Distance, int MaxDistance = int.MaxValue)
	{
		return GetPullDownCell(base.currentCell, out Distance, MaxDistance);
	}

	public Cell GetPullDownCell(int MaxDistance = int.MaxValue)
	{
		int Distance;
		return GetPullDownCell(out Distance, MaxDistance);
	}
}
