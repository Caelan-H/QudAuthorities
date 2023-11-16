using System;
using System.Collections.Generic;
using XRL.Messages;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Flee : IMovementGoal
{
	public GameObject Target;

	public int Duration = -1;

	public bool Panicked;

	private List<Cell> Entered = new List<Cell>();

	public Flee()
	{
	}

	public Flee(GameObject Target)
		: this()
	{
		this.Target = Target;
	}

	public Flee(GameObject Target, int Duration)
		: this(Target)
	{
		this.Duration = Duration;
	}

	public Flee(GameObject Target, int Duration, bool Panicked)
		: this(Target, Duration)
	{
		this.Panicked = Panicked;
	}

	public Flee(GameObject Target, bool Panicked)
		: this(Target)
	{
		this.Panicked = Panicked;
	}

	public override bool CanFight()
	{
		return false;
	}

	public override bool IsNonAggressive()
	{
		return true;
	}

	public override bool IsFleeing()
	{
		return true;
	}

	public override void Create()
	{
		Think("I'm trying to flee from " + ((Target == null) ? "nothing" : (Target.a + Target.ShortDisplayName)) + "!");
	}

	public override bool Finished()
	{
		return Duration == 0;
	}

	public bool TryAbilities()
	{
		return false;
	}

	public override void TakeAction()
	{
		if (Duration > 0)
		{
			Duration--;
		}
		else if (Duration == 0)
		{
			return;
		}
		if (!GameObject.validate(ref Target))
		{
			Think("I don't have a target any more!");
			FailToParent();
			return;
		}
		if (base.ParentObject.IsPlayer())
		{
			MessageQueue.AddPlayerMessage("You are fleeing from " + Target.a + Target.ShortDisplayName + "!", 'r');
		}
		Cell currentCell = base.ParentObject.CurrentCell;
		if (currentCell == null)
		{
			Think("I don't have a location!");
			Target = null;
			FailToParent();
			return;
		}
		Cell currentCell2 = Target.GetCurrentCell();
		if (currentCell2 == null || currentCell2.ParentZone?.ZoneID == null)
		{
			Think("My target has no location!");
			Target = null;
			FailToParent();
			return;
		}
		if (currentCell2.IsGraveyard())
		{
			Think("My target is dead!");
			Target = null;
			FailToParent();
			return;
		}
		if (currentCell.ParentZone != currentCell2.ParentZone)
		{
			Think("My target is in another zone!");
			Target = null;
			FailToParent();
			return;
		}
		string directionFromCell = currentCell2.GetDirectionFromCell(currentCell);
		Cell cell = null;
		int Nav = 268435456;
		int num;
		if (Panicked)
		{
			num = 50;
			cell = ((!(directionFromCell == ".")) ? currentCell.GetCellFromDirectionGlobal(directionFromCell) : currentCell.GetRandomLocalAdjacentCell());
		}
		else
		{
			num = 10;
			List<Cell> directionAndAdjacentCells = currentCell.GetDirectionAndAdjacentCells(directionFromCell);
			if (directionAndAdjacentCells.Count > 3)
			{
				directionAndAdjacentCells.ShuffleInPlace();
			}
			int num2 = int.MaxValue;
			foreach (Cell item in directionAndAdjacentCells)
			{
				if (item == currentCell || Entered.Contains(item) || item.NavigationWeight(base.ParentObject, ref Nav) >= num)
				{
					continue;
				}
				int totalAdjacentHostileDifficultyLevel = item.GetTotalAdjacentHostileDifficultyLevel(base.ParentObject);
				if (totalAdjacentHostileDifficultyLevel < num2)
				{
					cell = item;
					num2 = totalAdjacentHostileDifficultyLevel;
					if (num2 <= 0)
					{
						break;
					}
				}
			}
		}
		if (cell != null && !Entered.Contains(cell) && cell.NavigationWeight(base.ParentObject, ref Nav) < num)
		{
			ProcessDestination(cell, currentCell);
			return;
		}
		int num3 = currentCell2.PathDistanceTo(currentCell);
		foreach (Cell item2 in currentCell.GetLocalAdjacentCells(1).ShuffleInPlace())
		{
			if (!Entered.Contains(item2) && currentCell2.PathDistanceTo(item2) >= num3 && item2.NavigationWeight(base.ParentObject, ref Nav) < num)
			{
				ProcessDestination(item2, currentCell);
				return;
			}
		}
		Think("I can't find anyplace to flee!");
		FailToParent();
	}

	private void ProcessDestination(Cell C, Cell From)
	{
		PushChildGoal(new Step(From.GetDirectionFromCell(C), careful: false, Panicked));
		Entered.Add(C);
	}
}
