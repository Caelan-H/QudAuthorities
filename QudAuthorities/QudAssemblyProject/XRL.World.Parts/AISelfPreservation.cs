using System;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AISelfPreservation : IPart
{
	public int Threshold = 35;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AITakingAction");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AITakingAction" && ParentObject.HasStat("Hitpoints"))
		{
			Statistic stat = ParentObject.GetStat("Hitpoints");
			if (stat.Penalty >= stat.BaseValue * (100 - Threshold) / 100 && !ParentObject.pBrain.HasGoal("Retreat"))
			{
				ParentObject.pBrain.Goals.Clear();
				ParentObject.pBrain.PushGoal(new Retreat(Stat.Random(30, 50)));
				if (ParentObject.pBrain.Goals.Count > 0)
				{
					ParentObject.pBrain.Goals.Peek().TakeAction();
					return false;
				}
			}
		}
		return base.FireEvent(E);
	}
}
