using System;
using System.Collections.Generic;
using System.Text;
using XRL.Messages;
using XRL.World.AI.GoalHandlers;
using XRL.World.AI.Pathfinding;
using XRL.World.Parts;

namespace XRL.World.AI;

[Serializable]
public class GoalHandler
{
	public int Age;

	public Brain ParentBrain;

	public GoalHandler ParentHandler;

	[NonSerialized]
	private static StringBuilder descBuilder = new StringBuilder();

	public GameObject ParentObject => ParentBrain.ParentObject;

	public Zone CurrentZone => ParentObject.CurrentZone;

	public Cell CurrentCell => ParentObject.CurrentCell;

	public static GameObject ThePlayer => The.Player;

	public List<string> AdjacentObjects
	{
		get
		{
			List<string> list = new List<string>();
			foreach (Cell localAdjacentCell in ParentObject.pPhysics.CurrentCell.GetLocalAdjacentCells())
			{
				foreach (GameObject @object in localAdjacentCell.Objects)
				{
					list.Add(@object.Blueprint);
				}
			}
			return list;
		}
	}

	public virtual bool CanFight()
	{
		return true;
	}

	public virtual bool IsBusy()
	{
		return true;
	}

	public virtual bool IsNonAggressive()
	{
		return false;
	}

	public virtual bool IsFleeing()
	{
		return false;
	}

	public virtual void Failed()
	{
	}

	public virtual string GetDetails()
	{
		return null;
	}

	public virtual string GetDescription()
	{
		descBuilder.Length = 0;
		descBuilder.Append(GetType().Name);
		string details = GetDetails();
		if (!string.IsNullOrEmpty(details))
		{
			descBuilder.Append(": ").Append(details);
		}
		return descBuilder.ToString();
	}

	public void FailToParent()
	{
		while (ParentBrain.Goals.Count > 0 && ParentBrain.Goals.Peek() != ParentHandler)
		{
			ParentBrain.Goals.Pop();
		}
		if (ParentBrain.Goals.Count > 0)
		{
			ParentBrain.Goals.Peek().Failed();
		}
	}

	public virtual void PushGoal(GoalHandler Goal)
	{
		Goal.Push(ParentBrain);
	}

	public virtual void ForcePushGoal(GoalHandler Goal)
	{
		Goal.ParentBrain = ParentBrain;
		ParentBrain.Goals.Push(this);
		Goal.Create();
	}

	public virtual void PushChildGoal(GoalHandler Child)
	{
		Child.ParentHandler = this;
		Child.Push(ParentBrain);
	}

	public void PushChildGoal(GoalHandler Child, GoalHandler Parent)
	{
		Child.ParentHandler = Parent;
		Child.Push(ParentBrain);
	}

	public virtual void Push(Brain pBrain)
	{
		ParentBrain = pBrain;
		pBrain.Goals.Push(this);
		Create();
	}

	public void Pop()
	{
		if (ParentBrain.Goals.Count > 0)
		{
			ParentBrain.Goals.Pop();
		}
	}

	public virtual void Create()
	{
	}

	public virtual void TakeAction()
	{
	}

	public virtual bool Finished()
	{
		return true;
	}

	public void Think(string Hrm)
	{
		ParentBrain.Think(Hrm);
	}

	public bool MoveTowards(Cell targetCell, bool Global = false, bool MoveAwayIfAt = true)
	{
		Think("I'm going to move towards my target.");
		if (targetCell.ParentZone.IsWorldMap())
		{
			Think("Target's on the world map, can't follow!");
			return false;
		}
		if (MoveAwayIfAt && targetCell == ParentObject.CurrentCell)
		{
			Cell randomLocalAdjacentCell = ParentObject.CurrentCell.GetRandomLocalAdjacentCell();
			if (randomLocalAdjacentCell != null)
			{
				PushChildGoal(new Step(ParentObject.CurrentCell.GetDirectionFromCell(randomLocalAdjacentCell)));
				return true;
			}
		}
		FindPath findPath = new FindPath(ParentObject.CurrentZone.ZoneID, ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, targetCell.ParentZone.ZoneID, targetCell.X, targetCell.Y, Global, PathUnlimited: false, ParentObject);
		if (findPath.bFound)
		{
			using (List<string>.Enumerator enumerator = findPath.Directions.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					PushChildGoal(new Step(current));
					return true;
				}
			}
			return true;
		}
		return false;
	}

	public static void AddPlayerMessage(string Message, string Color = null, bool Capitalize = true)
	{
		MessageQueue.AddPlayerMessage(Message, Color, Capitalize);
	}

	public static void AddPlayerMessage(string Message, char Color, bool Capitalize = true)
	{
		MessageQueue.AddPlayerMessage(Message, Color, Capitalize);
	}

	public static bool Visible(GameObject obj)
	{
		return obj?.IsVisible() ?? false;
	}

	public bool Visible()
	{
		return Visible(ParentObject);
	}
}
