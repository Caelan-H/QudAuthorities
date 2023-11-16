using System;
using System.Collections.Generic;
using System.Linq;
using Wintellect.PowerCollections;
using XRL.Rules;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class DropOffStolenGoods : GoalHandler
{
	public string TargetObject = "OpenShaft";

	public override bool Finished()
	{
		return ParentBrain.ParentObject.Inventory.Objects.Count == 0;
	}

	public override void Create()
	{
	}

	public void MoveToDropoff()
	{
		List<GameObject> list = base.CurrentZone.FindObjects(TargetObject);
		list.Sort((GameObject a, GameObject b) => a.DistanceTo(base.ParentObject).CompareTo(b.DistanceTo(base.ParentObject)));
		GameObject gameObject = list.FirstOrDefault();
		if (gameObject != null)
		{
			if (gameObject.DistanceTo(ParentBrain.ParentObject) <= 1 && ParentBrain.ParentObject.IsVisible())
			{
				GameObject randomElement = ParentBrain.ParentObject.Inventory.Objects.GetRandomElement();
				if (randomElement != null)
				{
					base.ParentObject.Inventory.RemoveObject(randomElement);
					gameObject.CurrentCell.AddObject(randomElement);
					GoalHandler.AddPlayerMessage(base.ParentObject.The + base.ParentObject.ShortDisplayName + base.ParentObject.GetVerb("drop") + " " + randomElement.a + randomElement.ShortDisplayName + " down " + gameObject.the + gameObject.ShortDisplayName + ".", 'W');
				}
			}
			List<Cell> localAdjacentCells = gameObject.CurrentCell.GetLocalAdjacentCells(1);
			Algorithms.RandomShuffleInPlace(localAdjacentCells, Stat.Rand);
			for (int i = 0; i < localAdjacentCells.Count; i++)
			{
				if (localAdjacentCells[i].IsEmpty())
				{
					ParentBrain.PushGoal(new MoveTo(base.CurrentZone.ZoneID, localAdjacentCells[i].X, localAdjacentCells[i].Y));
					return;
				}
			}
			ParentBrain.PushGoal(new WanderRandomly(6));
		}
		else
		{
			FailToParent();
		}
	}

	public override void TakeAction()
	{
		MoveToDropoff();
	}
}
