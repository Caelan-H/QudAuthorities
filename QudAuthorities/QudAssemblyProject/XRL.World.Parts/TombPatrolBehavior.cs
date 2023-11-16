using System;
using Genkit;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class TombPatrolBehavior : IPart
{
	public Point2D GetNextWaypoint()
	{
		return new Point2D(0, 0);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIBoredEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return base.HandleEvent(E);
		}
		if (ParentObject.IsPlayerControlled())
		{
			return base.HandleEvent(E);
		}
		Zone parentZone = ParentObject.pPhysics.CurrentCell.ParentZone;
		if (parentZone != null)
		{
			int wX = parentZone.wX;
			int wY = parentZone.wY;
			int z = parentZone.Z;
			int num = parentZone.X;
			int num2 = parentZone.Y;
			if (num == 0 && num2 == 0)
			{
				num = 1;
			}
			else if (num == 1 && num2 == 0)
			{
				num = 2;
			}
			else if (num == 2 && num2 == 0)
			{
				num2 = 1;
			}
			else if (num == 2 && num2 == 1)
			{
				num2 = 2;
			}
			else if (num == 2 && num2 == 2)
			{
				num = 1;
			}
			else if (num == 1 && num2 == 2)
			{
				num = 0;
			}
			else if (num == 0 && num2 == 2)
			{
				num2 = 1;
			}
			else if (num == 0 && num2 == 1)
			{
				num2 = 0;
			}
			ParentObject.pBrain.PushGoal(new TombPatrolGoal("JoppaWorld." + wX + "." + wY + "." + num + "." + num2 + "." + z));
		}
		return false;
	}
}
