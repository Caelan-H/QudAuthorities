using System;
using Genkit;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class LayMineGoal : GoalHandler
{
	public Location2D target;

	public string mine;

	public string mineName;

	public string timer = "-1";

	public int hideDifficulty;

	public LayMineGoal(Location2D target, string mine, string mineName = "", string timer = "-1", int hideDifficulty = 0)
	{
		this.target = target;
		this.mine = mine;
		this.mineName = mineName;
		this.timer = timer;
		this.hideDifficulty = hideDifficulty;
	}

	public override void Create()
	{
		Think("I'm trying to lay mines!");
	}

	public override bool CanFight()
	{
		return false;
	}

	public override bool Finished()
	{
		return false;
	}

	public override void TakeAction()
	{
		if (target == null)
		{
			Think("I don't have a target anymore!");
			FailToParent();
		}
		else if (base.ParentObject.DistanceTo(target) == 1)
		{
			int num = timer.RollCached();
			Think((num > 0) ? "I'm going to set a bomb!" : "I'm going to lay a mine!");
			GameObject gameObject = ((num > 0) ? Tinkering_LayMine.CreateBomb(mine, base.ParentObject, num) : Tinkering_LayMine.CreateMine(mine, base.ParentObject));
			if (hideDifficulty > 0)
			{
				gameObject.RequirePart<Hidden>().Difficulty = hideDifficulty;
			}
			base.ParentObject.CurrentCell.AddObject(gameObject);
			base.ParentObject.UseEnergy(1000);
			FailToParent();
		}
		else if (!MoveTowards(base.ParentObject.CurrentZone.GetCell(target)))
		{
			Think("I can't get to my target!");
			FailToParent();
		}
	}
}
