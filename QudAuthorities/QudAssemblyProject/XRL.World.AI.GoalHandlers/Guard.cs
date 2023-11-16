using System;
using XRL.Rules;
using XRL.World.AI.Pathfinding;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Guard : GoalHandler
{
	public override void Create()
	{
		Think("I'm guarding this place.");
	}

	public override bool Finished()
	{
		return false;
	}

	public override void TakeAction()
	{
		Cell currentCell = base.ParentObject.CurrentCell;
		if (currentCell == null || currentCell.IsGraveyard() || The.Player?.CurrentCell == null)
		{
			return;
		}
		Think("I'm guarding this place.");
		if (ParentBrain.PartyLeader != null)
		{
			if (ParentBrain.GoToPartyLeader())
			{
				return;
			}
			if (ParentBrain.Target == null && currentCell.ParentZone.IsActive())
			{
				GameObject gameObject = ParentBrain.FindProspectiveTarget(currentCell);
				if (gameObject != null)
				{
					ParentBrain.WantToKill(gameObject, "to guard this place for my leader");
					if (ParentBrain.Target == null)
					{
						return;
					}
				}
			}
			if (ParentBrain.isMobile())
			{
				if (ParentBrain.Staying)
				{
					bool flag = false;
					if (!ParentBrain.Wanders && !ParentBrain.WandersRandomly && ParentBrain.StartingCell != null && ParentBrain.StartingCell.Equals(ParentBrain.pPhysics.CurrentCell) && ParentBrain.pPhysics.CurrentCell != null)
					{
						PushChildGoal(new MoveTo(ParentBrain.StartingCell.ZoneID, ParentBrain.StartingCell.CellX, ParentBrain.StartingCell.CellX, careful: true));
						flag = true;
					}
					if (!flag)
					{
						ParentBrain.ParentObject.UseEnergy(1000);
					}
				}
				else
				{
					int num = 5;
					if (ParentBrain.PartyLeader.IsPlayer())
					{
						num = 1;
					}
					if (ParentBrain.ParentObject.DistanceTo(ParentBrain.PartyLeader) > num)
					{
						Cell currentCell2 = base.ParentObject.CurrentCell;
						Cell currentCell3 = ParentBrain.PartyLeader.CurrentCell;
						FindPath findPath = new FindPath(currentCell2.ParentZone.ZoneID, currentCell2.X, currentCell2.Y, currentCell3.ParentZone.ZoneID, currentCell3.X, currentCell3.Y, PathGlobal: false, PathUnlimited: false, ParentBrain.ParentObject);
						if (findPath.bFound)
						{
							int num2 = Stat.Random(1, 5);
							foreach (string direction in findPath.Directions)
							{
								PushChildGoal(new Step(direction));
								num2--;
								if (num2 <= 0)
								{
									return;
								}
							}
						}
					}
				}
			}
			ParentBrain.ParentObject.UseEnergy(1000);
			return;
		}
		if (currentCell.ParentZone.IsActive() && ParentBrain.Target == null)
		{
			GameObject gameObject2 = ParentBrain.FindProspectiveTarget(currentCell);
			if (gameObject2 != null)
			{
				ParentBrain.WantToKill(gameObject2, "to guard this place");
				if (ParentBrain.Target != null)
				{
					return;
				}
			}
		}
		ParentBrain.ParentObject.UseEnergy(1000);
	}
}
