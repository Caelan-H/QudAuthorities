using System;
using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class Mindrone : IPart
{
	public int GraftCooldown;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction" && GraftCooldown <= 0)
		{
			Brain brain = ParentObject.GetPart("Brain") as Brain;
			Physics physics = ParentObject.GetPart("Physics") as Physics;
			for (int i = 0; i < brain.Goals.Items.Count; i++)
			{
				if (brain.Goals.Items[i].GetType().FullName.Contains("Graftek"))
				{
					return true;
				}
			}
			List<GameObject> list = physics.CurrentCell.ParentZone.FastSquareVisibility(physics.CurrentCell.X, physics.CurrentCell.Y, 12, "Combat", ParentObject);
			List<GameObject> list2 = new List<GameObject>();
			foreach (GameObject item in list)
			{
				if (!item.IsPlayer() && item.HasPart("Metal") && item.Statistics.ContainsKey("Hitpoints") && item.Statistics["Hitpoints"].Penalty > 0)
				{
					list2.Add(item);
				}
			}
			if (list2.Count == 0)
			{
				if (IComponent<GameObject>.ThePlayer != null)
				{
					brain.PushGoal(new Flee(IComponent<GameObject>.ThePlayer, 1));
				}
			}
			else
			{
				brain.Goals.Clear();
				brain.PushGoal(new MindroneGoal(list2.GetRandomElement()));
			}
		}
		return true;
	}
}
