using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class DelegateGoal : GoalHandler
{
	[NonSerialized]
	public Action<GoalHandler> onTakeAction;

	[NonSerialized]
	public Func<GoalHandler, bool> onFinished;

	[NonSerialized]
	public bool? SetCanFight;

	[NonSerialized]
	public bool? SetNonAggressive;

	public DelegateGoal()
	{
	}

	public DelegateGoal(Action<GoalHandler> takeAction, Func<GoalHandler, bool> finished = null)
	{
		onTakeAction = takeAction;
		onFinished = finished;
	}

	public override bool Finished()
	{
		if (onTakeAction == null && onFinished == null)
		{
			return true;
		}
		if (onFinished != null)
		{
			return onFinished(this);
		}
		return false;
	}

	public override void TakeAction()
	{
		if (onTakeAction != null)
		{
			onTakeAction(this);
		}
	}

	public override bool CanFight()
	{
		return SetCanFight ?? base.CanFight();
	}

	public override bool IsNonAggressive()
	{
		return SetNonAggressive ?? base.IsNonAggressive();
	}
}
