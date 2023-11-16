using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI.Pathfinding;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Bored : GoalHandler
{
	public int ExtinguishSelfTries;

	public long NextLeaderPathFind;

	public long NextLeaderAltPathFind;

	[NonSerialized]
	private static List<AICommandList> CommandList = new List<AICommandList>();

	public override bool Finished()
	{
		return false;
	}

	public override bool IsBusy()
	{
		return false;
	}

	public bool TryMutations()
	{
		if (base.ParentObject.IsValid())
		{
			CommandList.Clear();
			base.ParentObject.FireEvent(Event.New("AIGetPassiveMutationList", "List", CommandList));
			if (CommandList.Count > 0 && base.ParentObject.FireEvent(Event.New(CommandList.GetRandomElement().Command, "Owner", base.ParentObject)))
			{
				Think("Did a passive mutation");
				return true;
			}
		}
		return false;
	}

	public bool TryMovementMutations(Cell target)
	{
		if (base.ParentObject.IsValid())
		{
			CommandList.Clear();
			base.ParentObject.FireEvent(Event.New("AIGetMovementMutationList", "List", CommandList, "TargetCell", target));
			if (CommandList.Count > 0 && base.ParentObject.FireEvent(Event.New(CommandList.GetRandomElement().Command, "Owner", base.ParentObject)))
			{
				Think("Did a movement mutation");
				return true;
			}
		}
		return false;
	}

	public bool TryMovementMutations()
	{
		if (base.ParentObject.IsValid())
		{
			CommandList.Clear();
			base.ParentObject.FireEvent(Event.New("AIGetMovementMutationList", "List", CommandList));
			if (CommandList.Count > 0 && ParentBrain.ParentObject.FireEvent(Event.New(CommandList.GetRandomElement().Command, "Owner", base.ParentObject)))
			{
				Think("Did a movement mutation");
				return true;
			}
		}
		return false;
	}

	public void TakeActionWithPartyLeader()
	{
		if (base.ParentObject == null || ParentBrain == null)
		{
			return;
		}
		GameObject partyLeader = ParentBrain.PartyLeader;
		Cell currentCell = base.ParentObject.CurrentCell;
		if (currentCell == null || currentCell.IsGraveyard() || currentCell.ParentZone == null || ParentBrain.GoToPartyLeader())
		{
			return;
		}
		Zone parentZone = currentCell.ParentZone;
		if (parentZone == null || !parentZone.IsActive())
		{
			return;
		}
		if (ParentBrain.Target == null && ParentBrain.CanAcquireTarget())
		{
			if (partyLeader.IsPlayerControlled())
			{
				GameObject target = partyLeader.Target;
				if (target != null && ParentBrain.CheckPerceptionOf(target))
				{
					ParentBrain.WantToKill(target, "to boredly aid my leader");
					if (ParentBrain.Target != null)
					{
						return;
					}
				}
			}
			GameObject gameObject = ParentBrain.FindProspectiveTarget(currentCell);
			if (gameObject != null)
			{
				ParentBrain.WantToKill(gameObject, "out of bored hostility while having a party leader");
				if (ParentBrain.Target != null)
				{
					return;
				}
			}
			else
			{
				Think("I boredly looked for a target while having a party leader, but didn't find one.");
			}
		}
		if (TryMovementMutations())
		{
			return;
		}
		if (!ParentBrain.isMobile())
		{
			base.ParentObject.UseEnergy(1000);
		}
		else if (ParentBrain.Staying)
		{
			bool flag = false;
			if (!ParentBrain.Wanders && !ParentBrain.WandersRandomly)
			{
				Cell cell = ParentBrain?.StartingCell?.ResolveCell();
				if (cell != null && currentCell != cell && cell.ParentZone == parentZone && base.ParentObject.FireEvent("CanAIDoIndependentBehavior"))
				{
					PushChildGoal(new MoveTo(parentZone.ZoneID, cell.X, cell.Y, careful: true));
					flag = true;
				}
			}
			if (!flag)
			{
				base.ParentObject.UseEnergy(1000);
			}
		}
		else if (The.Game.TimeTicks > NextLeaderPathFind && base.ParentObject.DistanceTo(partyLeader) > (partyLeader.IsPlayer() ? 1 : 5))
		{
			Cell currentCell2 = partyLeader.CurrentCell;
			if (currentCell2 == null || currentCell2.ParentZone == null || TryMovementMutations(currentCell2))
			{
				return;
			}
			FindPath findPath = new FindPath(currentCell, currentCell2, PathGlobal: false, PathUnlimited: true, base.ParentObject);
			if (findPath.bFound)
			{
				NextLeaderPathFind = The.Game.TimeTicks;
				PathTowardLeader(findPath);
				return;
			}
			if (base.ParentObject.IsPlayerLed())
			{
				NextLeaderPathFind = The.Game.TimeTicks + 2;
			}
			else
			{
				NextLeaderPathFind = The.Game.TimeTicks + Stat.Random(50, 100);
			}
			if (The.Game.TimeTicks <= NextLeaderAltPathFind)
			{
				return;
			}
			Cell closestPassableCellFor = currentCell2.getClosestPassableCellFor(base.ParentObject);
			if (closestPassableCellFor != null && closestPassableCellFor != currentCell2)
			{
				FindPath findPath2 = new FindPath(currentCell, closestPassableCellFor, PathGlobal: false, PathUnlimited: true, base.ParentObject);
				if (findPath2.bFound)
				{
					NextLeaderPathFind = The.Game.TimeTicks + 2;
					PathTowardLeader(findPath2);
				}
				else if (base.ParentObject.IsPlayerLed())
				{
					NextLeaderAltPathFind = The.Game.TimeTicks + 10;
				}
				else
				{
					NextLeaderAltPathFind = The.Game.TimeTicks + Stat.Random(100, 200);
				}
			}
		}
		else
		{
			Think("Waiting for party leader");
			base.ParentObject.UseEnergy(1000);
		}
	}

	private void PathTowardLeader(FindPath PathFinder)
	{
		for (int num = PathFinder.Steps.Count - 2; num >= 1; num--)
		{
			if (PathFinder.Steps[num].IsEmpty())
			{
				if (!TryMovementMutations(PathFinder.Steps[num]))
				{
					break;
				}
				return;
			}
		}
		for (int num2 = Math.Min(Stat.Random(0, 4), PathFinder.Directions.Count - 2); num2 >= 0; num2--)
		{
			PushChildGoal(new Step(PathFinder.Directions[num2]));
		}
	}

	public override void TakeAction()
	{
		Cell currentCell = base.ParentObject.CurrentCell;
		if (currentCell == null || currentCell.IsGraveyard() || GoalHandler.ThePlayer == null || GoalHandler.ThePlayer.CurrentCell == null)
		{
			return;
		}
		if (base.ParentObject.IsAflame() && ++ExtinguishSelfTries < 5)
		{
			PushChildGoal(new ExtinguishSelf());
			return;
		}
		Think("I'm bored.");
		string stringProperty = base.ParentObject.GetStringProperty("WhenBoredReturnToOnce");
		if (!string.IsNullOrEmpty(stringProperty))
		{
			string[] array = stringProperty.Split(',');
			int num = Convert.ToInt32(array[0]);
			int num2 = Convert.ToInt32(array[1]);
			if (currentCell.X == num && currentCell.Y == num2)
			{
				base.ParentObject.DeleteStringProperty("WhenBoredReturnToOnce");
			}
			else
			{
				Cell cell = currentCell.ParentZone.GetCell(num, num2);
				ParentBrain.PushGoal(new MoveTo(cell));
			}
			base.ParentObject.UseEnergy(1000);
		}
		else
		{
			if (!AIBoredEvent.Check(base.ParentObject) || base.ParentObject.Energy.Value < 1000)
			{
				return;
			}
			if (ParentBrain.PartyLeader != null)
			{
				TakeActionWithPartyLeader();
				return;
			}
			if (ParentBrain.Target == null)
			{
				GameObject gameObject = ParentBrain.FindProspectiveTarget(currentCell);
				if (gameObject != null)
				{
					ParentBrain.WantToKill(gameObject, "out of bored hostility");
					if (ParentBrain.Target != null)
					{
						return;
					}
				}
				else
				{
					Think("I boredly looked for a target, but didn't find one.");
				}
			}
			if (TryMutations())
			{
				return;
			}
			Cell cell2 = ParentBrain?.StartingCell?.ResolveCell();
			if (cell2 != null && !ParentBrain.Wanders && !ParentBrain.WandersRandomly && currentCell != cell2 && base.ParentObject.FireEvent("CanAIDoIndependentBehavior"))
			{
				PushChildGoal(new MoveTo(cell2.ParentZone.ZoneID, cell2.X, cell2.Y, careful: true));
			}
			if (!base.ParentObject.InSameZone(GoalHandler.ThePlayer))
			{
				PushChildGoal(new Wait(Stat.Random(1, 20), "I'm not in the same zone as the player"));
			}
			else if (base.ParentObject.FireEvent("CanAIDoIndependentBehavior"))
			{
				if (ParentBrain.Wanders)
				{
					if ((base.ParentObject.HasTagOrProperty("Restless") && !base.ParentObject.HasTagOrProperty("Social")) || 10.in100())
					{
						PushChildGoal(new Wander());
						return;
					}
				}
				else if (ParentBrain.WandersRandomly && ((base.ParentObject.HasTagOrProperty("Restless") && !base.ParentObject.HasTagOrProperty("Social")) || 20.in100()))
				{
					PushChildGoal(new WanderRandomly(5));
					return;
				}
				if (base.ParentObject.HasTagOrProperty("AllowIdleBehavior") && currentCell.ParentZone.WantEvent(IdleQueryEvent.ID, MinEvent.CascadeLevel))
				{
					List<GameObject> list = null;
					Zone parentZone = currentCell.ParentZone;
					int i = 0;
					for (int width = parentZone.Width; i < width; i++)
					{
						int j = 0;
						for (int height = parentZone.Height; j < height; j++)
						{
							Cell cell3 = parentZone.GetCell(i, j);
							int k = 0;
							for (int count = cell3.Objects.Count; k < count; k++)
							{
								GameObject gameObject2 = cell3.Objects[k];
								if (gameObject2.WantEvent(IdleQueryEvent.ID, MinEvent.CascadeLevel) || gameObject2.HasRegisteredEvent("IdleQuery"))
								{
									if (list == null)
									{
										list = Event.NewGameObjectList();
									}
									list.Add(gameObject2);
								}
							}
						}
					}
					if (list != null)
					{
						list.ShuffleInPlace();
						IdleQueryEvent e = IdleQueryEvent.FromPool(ParentBrain.ParentObject);
						Event @event = Event.New("IdleQuery", "Object", ParentBrain.ParentObject);
						int l = 0;
						for (int count2 = list.Count; l < count2; l++)
						{
							GameObject gameObject3 = list[l];
							if (!gameObject3.HandleEvent(e))
							{
								base.ParentObject.UseEnergy(1000);
								return;
							}
							if (gameObject3.HasRegisteredEvent(@event.ID) && gameObject3.FireEvent(@event))
							{
								base.ParentObject.UseEnergy(1000);
								return;
							}
						}
					}
				}
				PushChildGoal(new Wait(Stat.Random(1, 10), "I couldn't find anything to do"));
			}
			else
			{
				PushChildGoal(new Wait(Stat.Random(1, 10), "I couldn't perform independent behavior"));
			}
		}
	}
}
