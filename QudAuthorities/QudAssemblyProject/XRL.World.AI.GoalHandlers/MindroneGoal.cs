using System;
using System.Collections.Generic;
using XRL.World.AI.Pathfinding;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class MindroneGoal : GoalHandler
{
	public GameObject Target;

	public Physics pTargetPhysics;

	public Physics pPhysics;

	private int LastSeen;

	public MindroneGoal(GameObject _Target)
	{
		Target = _Target;
	}

	public override void Create()
	{
		Think("I'm trying to heal " + Target.Blueprint + "!");
	}

	public override bool CanFight()
	{
		return false;
	}

	public override bool Finished()
	{
		return false;
	}

	public override void Push(Brain pBrain)
	{
		base.Push(pBrain);
	}

	public override void TakeAction()
	{
		if (Target == null)
		{
			Think("I don't have a target anymore!");
			FailToParent();
			return;
		}
		if (Target.IsInvalid())
		{
			Target = null;
			Think("My target has been destroyed!");
			FailToParent();
			return;
		}
		pTargetPhysics = Target.GetPart("Physics") as Physics;
		pPhysics = ParentBrain.ParentObject.GetPart("Physics") as Physics;
		if (pTargetPhysics.CurrentCell == null || pTargetPhysics.CurrentCell.ParentZone.ZoneID == null || pTargetPhysics.CurrentCell.PathDistanceTo(pPhysics.CurrentCell) > 80)
		{
			LastSeen++;
		}
		if (pTargetPhysics.ParentObject.HasPart("GraftekGraft"))
		{
			Think("My target is already grafted...");
			Target = null;
			FailToParent();
		}
		else if (LastSeen > 5)
		{
			Think("I can't find my target...");
			Target = null;
			FailToParent();
		}
		else if (pTargetPhysics.CurrentCell == null || pTargetPhysics.CurrentCell.ParentZone.ZoneID == null)
		{
			Think("My target is dead!");
			Target = null;
			FailToParent();
		}
		else if (pTargetPhysics.CurrentCell.PathDistanceTo(pPhysics.CurrentCell) == 1)
		{
			Think("I'm going to graft my target!");
			Target.Sparksplatter();
			Target.Statistics["Hitpoints"].Penalty -= 3;
			Target.ParticleText("&G+3", 0f, -0.2f);
		}
		else if (ParentBrain.isMobile())
		{
			Think("I'm going to move towards my target.");
			bool pathGlobal = false;
			if (Target.IsPlayer())
			{
				pathGlobal = true;
			}
			if (pTargetPhysics.CurrentCell.ParentZone.IsWorldMap())
			{
				Think("Target's on the world map, can't follow!");
				Target = null;
				FailToParent();
				return;
			}
			FindPath findPath = new FindPath(ParentBrain.pPhysics.CurrentCell.ParentZone.ZoneID, ParentBrain.pPhysics.CurrentCell.X, ParentBrain.pPhysics.CurrentCell.Y, pTargetPhysics.CurrentCell.ParentZone.ZoneID, pTargetPhysics.CurrentCell.X, pTargetPhysics.CurrentCell.Y, pathGlobal, PathUnlimited: false, ParentBrain.ParentObject);
			if (findPath.bFound)
			{
				using (List<string>.Enumerator enumerator = findPath.Directions.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						string current = enumerator.Current;
						PushChildGoal(new Step(current));
					}
					return;
				}
			}
			FailToParent();
		}
		else
		{
			ParentBrain.ParentObject.UseEnergy(1000);
			Think("My target is too far and I'm immobile.");
			FailToParent();
		}
	}
}
