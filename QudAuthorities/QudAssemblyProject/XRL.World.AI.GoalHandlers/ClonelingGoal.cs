using System;
using XRL.Rules;
using XRL.World.AI.Pathfinding;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class ClonelingGoal : GoalHandler
{
	public GameObject Target;

	public bool Done;

	private int LastSeen;

	public ClonelingGoal(GameObject Target)
	{
		this.Target = Target;
	}

	public override void Create()
	{
		Think("I'm trying to clone someone!");
	}

	public override bool CanFight()
	{
		return false;
	}

	public override bool Finished()
	{
		return Done;
	}

	public override void TakeAction()
	{
		if (!(base.ParentObject.GetPart("Cloneling") is Cloneling cloneling))
		{
			Think("I'm not a cloneling any more!");
			FailToParent();
			return;
		}
		if (cloneling.ClonesLeft <= 0)
		{
			Think("I'm out of cloning draught!");
			FailToParent();
			return;
		}
		if (Target == null)
		{
			Think("I don't have a target anymore!");
			FailToParent();
			return;
		}
		if (Target.IsInvalid() || Target.IsInGraveyard())
		{
			Target = null;
			Think("My target has been destroyed!");
			FailToParent();
			return;
		}
		Cell currentCell = base.ParentObject.CurrentCell;
		if (currentCell == null)
		{
			Target = null;
			Think("I no longer have a location!");
			FailToParent();
			return;
		}
		Cell currentCell2 = Target.CurrentCell;
		if (currentCell2 == null || currentCell2.ParentZone.ZoneID == null)
		{
			Target = null;
			Think("My target no longer exists!");
			FailToParent();
			return;
		}
		int num = currentCell2.PathDistanceTo(currentCell);
		if (num > 80 || Target.OnWorldMap())
		{
			LastSeen++;
			if (LastSeen > 5)
			{
				Think("I can't find my target...");
				Target = null;
				FailToParent();
				return;
			}
		}
		if (base.ParentObject.pBrain.Staying && num > 1)
		{
			Target = null;
			Think("I'm staying put and I'd have to move to reach my target.");
			FailToParent();
		}
		else if (!cloneling.CanBeCloned(Target))
		{
			Target = null;
			Think("I can no longer clone my target!");
			FailToParent();
		}
		else if (num == 1)
		{
			Think("I'm going to clone my target!");
			if (cloneling.PerformCloning(Target))
			{
				Done = true;
			}
			else
			{
				FailToParent();
			}
		}
		else if (ParentBrain.isMobile())
		{
			Think("I'm going to move towards my target.");
			FindPath findPath = new FindPath(currentCell.ParentZone.ZoneID, currentCell.X, currentCell.Y, currentCell2.ParentZone.ZoneID, currentCell2.X, currentCell2.Y, Target.IsPlayer(), PathUnlimited: false, base.ParentObject);
			if (findPath.bFound)
			{
				for (int num2 = Math.Min(Stat.Random(0, 8), findPath.Directions.Count - 2); num2 >= 0; num2--)
				{
					PushChildGoal(new Step(findPath.Directions[num2]));
				}
			}
			else
			{
				FailToParent();
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
