using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class DisposeOfCorpse : GoalHandler
{
	public GameObject Corpse;

	public GameObject Container;

	public Cell Destination;

	public bool Done;

	public int GoToContainerTries;

	public int GoToCorpseTries;

	public DisposeOfCorpse(GameObject Corpse, GameObject Container)
	{
		this.Corpse = Corpse;
		this.Container = Container;
	}

	public override bool CanFight()
	{
		return true;
	}

	public override bool Finished()
	{
		return Done;
	}

	public override void TakeAction()
	{
		if (!GameObject.validate(ref Corpse) || !GameObject.validate(ref Container) || !base.ParentObject.InSameZone(Container))
		{
			FailToParent();
			return;
		}
		if (Corpse.InInventory == base.ParentObject)
		{
			if (base.ParentObject.InSameOrAdjacentCellTo(Container))
			{
				if (!Container.FireEvent(Event.New("PerformTake", "Object", Corpse, "PutBy", base.ParentObject, "Context", "DisposeOfCorpse")))
				{
					base.ParentObject.FireEvent(Event.New("PerformDrop", "Object", Corpse));
				}
				Done = true;
			}
			else if (++GoToContainerTries <= 10)
			{
				PushChildGoal(new MoveTo(Container, careful: false, overridesCombat: false, 1));
			}
			else
			{
				base.ParentObject.FireEvent(Event.New("PerformDrop", "Object", Corpse));
				Done = true;
			}
			return;
		}
		Cell currentCell = Corpse.CurrentCell;
		if (currentCell == null || currentCell.ParentZone != base.ParentObject.CurrentZone)
		{
			FailToParent();
		}
		else if (base.ParentObject.InSameOrAdjacentCellTo(Corpse))
		{
			if (!base.ParentObject.TakeObject(Corpse, Silent: false, 0, "DisposeOfCorpse"))
			{
				FailToParent();
			}
		}
		else if (++GoToCorpseTries <= 10)
		{
			PushChildGoal(new MoveTo(Corpse, careful: false, overridesCombat: false, 1));
		}
		else
		{
			Done = true;
		}
	}
}
