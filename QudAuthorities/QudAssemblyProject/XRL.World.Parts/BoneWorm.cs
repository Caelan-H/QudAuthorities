using System;
using System.Linq;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class BoneWorm : IPart
{
	public string Duration = "1d3";

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIBored");
		Object.RegisterPartEvent(this, "AIBeginKill");
		Object.RegisterPartEvent(this, "BeforeAITakingAction");
		base.Register(Object);
	}

	public void enclose()
	{
		int r;
		for (r = 2; r < 4; r++)
		{
			Cell cell = (from c in ParentObject.pBrain.Target.CurrentCell.GetAdjacentCells(r)
				where c.DistanceTo(ParentObject.pBrain.Target) == r
				where c.IsPassable()
				orderby c.DistanceTo(ParentObject)
				select c).FirstOrDefault();
			if (cell != null)
			{
				ParentObject.pBrain.PushGoal(new MoveTo(cell));
				break;
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeAITakingAction" || E.ID == "AIBored")
		{
			if (ParentObject.HasEffect("Burrowed") && ParentObject.pBrain.Target != null)
			{
				if (ParentObject.pBrain.Goals.Count == 0 || (ParentObject.pBrain.Goals.Peek().GetType() != typeof(MoveTo) && ParentObject.pBrain.Goals.Peek().GetType() != typeof(Step)))
				{
					GameObject target = ParentObject.pBrain.Target;
					ParentObject.pBrain.Goals.Clear();
					ParentObject.pBrain.Target = target;
					enclose();
				}
				if (E.ID == "AIBored")
				{
					return false;
				}
			}
			return true;
		}
		if (E.ID == "AIBeginKill")
		{
			if (ParentObject.HasEffect("Burrowed") && ParentObject.pBrain.Target != null)
			{
				enclose();
				return false;
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
