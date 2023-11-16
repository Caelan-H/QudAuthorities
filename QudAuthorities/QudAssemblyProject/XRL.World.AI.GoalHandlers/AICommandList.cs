using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.AI.GoalHandlers;

public class AICommandList
{
	public int Priority;

	public string Command;

	public GameObject Object;

	public bool Inv;

	public string DebugName
	{
		get
		{
			string text = Command;
			if (Priority != 0)
			{
				text = text + "(" + Priority + ")";
			}
			if (Inv)
			{
				text = "inv:" + text;
			}
			return text;
		}
	}

	public AICommandList(string Command, int Priority)
	{
		this.Priority = Priority;
		this.Command = Command;
	}

	public AICommandList(string Command, int Priority, GameObject Object = null, bool Inv = false)
		: this(Command, Priority)
	{
		this.Object = Object;
		this.Inv = Inv;
	}

	private bool ProcessCommand(GameObject Handler, GameObject Owner, GameObject Target)
	{
		if (Command == "ApplyTonic")
		{
			return Handler.FireEvent(Event.New(Command, "Owner", Owner, "Target", Owner));
		}
		return CommandEvent.Send(Owner, Command, Target, null, Handler);
	}

	private bool ProcessCommand(GameObject Handler, GameObject Owner, Cell TargetCell)
	{
		if (Command == "ApplyTonic")
		{
			return Handler.FireEvent(Event.New(Command, "Owner", Owner, "Target", Owner));
		}
		return CommandEvent.Send(Owner, Command, null, TargetCell, Handler);
	}

	public bool HandleCommand(GameObject Owner, GameObject Target)
	{
		if (Command == "MetaCommandMoveAway")
		{
			if (Owner.Move(Target.CurrentCell.GetDirectionFromCell(Owner.CurrentCell)))
			{
				return true;
			}
		}
		else
		{
			GameObject gameObject = Object ?? Owner;
			if (Inv)
			{
				if (InventoryActionEvent.Check(gameObject, Owner, gameObject, Command, Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, 0, 0, Target))
				{
					return true;
				}
			}
			else if (ProcessCommand(gameObject, Owner, Target))
			{
				return true;
			}
		}
		return false;
	}

	public bool HandleCommand(GameObject Owner, Cell Target)
	{
		if (Command == "MetaCommandMoveAway")
		{
			if (Owner.Move(Target.GetDirectionFromCell(Owner.CurrentCell)))
			{
				return true;
			}
		}
		else
		{
			GameObject gameObject = Object ?? Owner;
			if (Inv)
			{
				if (InventoryActionEvent.Check(gameObject, Owner, gameObject, Command, Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, 0, 0, null, Target))
				{
					return true;
				}
			}
			else if (ProcessCommand(gameObject, Owner, Target))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HandleCommandList(List<AICommandList> List, GameObject Owner, GameObject Target)
	{
		if (List != null && List.Count > 0)
		{
			int num = Stat.Random(0, List.Count - 1);
			if (List[num].HandleCommand(Owner, Target))
			{
				return true;
			}
			if (List.Count > 1)
			{
				int num2 = Stat.Random(0, List.Count - 1);
				if (num2 != num && List[num2].HandleCommand(Owner, Target))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HandleCommandList(List<AICommandList> List, GameObject Owner, Cell Target)
	{
		if (List != null && List.Count > 0)
		{
			int num = Stat.Random(0, List.Count - 1);
			if (List[num].HandleCommand(Owner, Target))
			{
				return true;
			}
			if (List.Count > 1)
			{
				int num2 = Stat.Random(0, List.Count - 1);
				if (num2 != num && List[num2].HandleCommand(Owner, Target))
				{
					return true;
				}
			}
		}
		return false;
	}
}
