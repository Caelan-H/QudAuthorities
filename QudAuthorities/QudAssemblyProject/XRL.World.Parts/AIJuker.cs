using System;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AIJuker : IPart
{
	public float Trigger = 0.35f;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AITakingAction");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AITakingAction" && 25.in100())
		{
			ParentObject.pBrain.PushGoal(new Step(Directions.GetRandomDirection()));
			if (ParentObject.pBrain.Goals.Count > 0)
			{
				ParentObject.pBrain.Goals.Peek().TakeAction();
			}
		}
		return base.FireEvent(E);
	}
}
