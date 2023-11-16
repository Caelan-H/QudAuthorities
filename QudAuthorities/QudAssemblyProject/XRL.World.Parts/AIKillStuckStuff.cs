using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class AIKillStuckStuff : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeginTakeActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && ParentObject.Target == null && !ParentObject.IsPlayerControlled() && ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			List<GameObject> list = cell.ParentZone.FastFloodVisibility(cell.X, cell.Y, 15, "Combat", ParentObject);
			if (list.Count > 1)
			{
				list.ShuffleInPlace();
			}
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				GameObject gameObject = list[i];
				if (gameObject.HasEffect("Stuck") && !ParentObject.IsAlliedTowards(gameObject))
				{
					ParentObject.Target = gameObject;
					break;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
