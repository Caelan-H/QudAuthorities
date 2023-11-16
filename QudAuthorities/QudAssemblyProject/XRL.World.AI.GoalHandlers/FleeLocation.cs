using System;
using System.Collections.Generic;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class FleeLocation : IMovementGoal
{
	public Cell Target;

	public int Duration = -1;

	public bool Panicked;

	public List<Cell> Entered = new List<Cell>();

	public FleeLocation()
	{
	}

	public FleeLocation(Cell Target)
		: this()
	{
		this.Target = Target;
	}

	public FleeLocation(Cell Target, int Duration)
		: this(Target)
	{
		this.Duration = Duration;
	}

	public FleeLocation(Cell Target, int Duration, bool Panicked)
		: this(Target, Duration)
	{
		this.Panicked = Panicked;
	}

	public FleeLocation(Cell Target, bool Panicked)
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
		Think("I'm trying to flee from a location!");
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
		Cell currentCell = base.ParentObject.CurrentCell;
		if (currentCell == null)
		{
			Think("I don't have a location!");
			FailToParent();
			return;
		}
		string directionFromCell = Target.GetDirectionFromCell(currentCell);
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
		int num3 = Target.PathDistanceTo(currentCell);
		foreach (Cell item2 in currentCell.GetLocalAdjacentCells(1).ShuffleInPlace())
		{
			if (!Entered.Contains(item2) && Target.PathDistanceTo(item2) >= num3 && item2.NavigationWeight(base.ParentObject, ref Nav) < num)
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
		if (ParentBrain.StartingCell != null && ParentBrain.StartingCell.Equals(Target))
		{
			ParentBrain.StartingCell = new GlobalLocation(C);
		}
	}
}
