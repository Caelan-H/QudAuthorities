using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Skittish : BaseMutation
{
	public Skittish()
	{
		DisplayName = "Skittish ({{r|D}})";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You startle easily and engage your defense mechanisms.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public bool TryMutations()
	{
		Physics pPhysics = ParentObject.pPhysics;
		if (pPhysics != null && Sidebar.CurrentTarget != null)
		{
			int value = pPhysics.CurrentCell.PathDistanceTo(Sidebar.CurrentTarget.CurrentCell);
			Event @event = Event.New("AIGetOffensiveMutationList", "Distance", value);
			List<AICommandList> list = new List<AICommandList>();
			@event.AddParameter("List", list);
			ParentObject.FireEvent(@event);
			@event.ID = "AIGetPassiveMutationList";
			ParentObject.FireEvent(@event);
			@event.ID = "AIGetMovementMutationList";
			ParentObject.FireEvent(@event);
			if (list.Count > 0)
			{
				int index = Stat.Random(0, list.Count - 1);
				ParentObject.SetStringProperty("Skittishing", "1");
				if (ParentObject.FireEvent(Event.New(list[index].Command, "ForceAI", true)) && ParentObject.IsPlayer())
				{
					Popup.Show("You are startled!");
				}
				ParentObject.RemoveStringProperty("Skittishing");
				return true;
			}
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction" && 3.in1000() && TryMutations())
		{
			return false;
		}
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}
