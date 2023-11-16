using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class CryptFerretBehavior : IPart
{
	public bool Fleeing;

	public string behaviorState = "hunting";

	public Location2D startingLocation;

	private List<Cell> hiddenCells = new List<Cell>();

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "AICantAttackRange");
		base.Register(Object);
	}

	public void scurry()
	{
		Cell cell = ParentObject.pBrain.pPhysics.CurrentCell;
		if (cell == null || !cell.IsVisible())
		{
			return;
		}
		if (hiddenCells.Count == 0)
		{
			hiddenCells.Clear();
			hiddenCells.AddRange(from c in cell.ParentZone.GetCells()
				where !c.IsVisible() && c.IsPassable()
				select c);
			hiddenCells.Sort((Cell a, Cell b) => a.DistanceTo(ParentObject).CompareTo(b.DistanceTo(ParentObject)));
		}
		List<Cell> list = null;
		int num = 4;
		foreach (Cell hiddenCell in hiddenCells)
		{
			if (!hiddenCell.IsVisible())
			{
				if (ParentObject.canPathTo(hiddenCell))
				{
					ParentObject.pBrain.Goals.Clear();
					ParentObject.pBrain.FleeTo(hiddenCell, 1);
					ParentObject.pBrain.MoveTo(hiddenCell, clearFirst: false);
					if (list == null)
					{
						return;
					}
					{
						foreach (Cell item in list)
						{
							hiddenCells.Remove(item);
						}
						return;
					}
				}
				if (list == null)
				{
					list = new List<Cell>();
				}
				list.Add(hiddenCell);
			}
			num--;
			if (num < 0)
			{
				break;
			}
		}
		hiddenCells.Clear();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AICantAttackRange")
		{
			if (Fleeing)
			{
				GameObject target = ParentObject.Target;
				if (target != null && (target.IsPlayer() || target.IsPlayerLed()))
				{
					scurry();
					return false;
				}
				if (ParentObject.pPhysics.currentCell.FastFloodVisibilityFirstBlueprint("Reliquary", ParentObject) != null)
				{
					behaviorState = "looting";
				}
			}
			return true;
		}
		if (E.ID == "BeginTakeAction")
		{
			if (behaviorState == "fleeing")
			{
				behaviorState = "hunting";
				ParentObject.UseEnergy(1000);
				ParentObject.TeleportSwirl();
				ParentObject.TeleportTo(ParentObject.CurrentZone.GetEmptyCells().GetRandomElement(), 0);
				ParentObject.TeleportSwirl();
			}
			else if (behaviorState == "looting")
			{
				GameObject gameObject = ParentObject.pPhysics.currentCell.FastFloodVisibilityFirstBlueprint("Reliquary", ParentObject);
				if (gameObject != null)
				{
					if (ParentObject.DistanceTo(gameObject) <= 1)
					{
						GameObject gameObject2 = gameObject.Inventory.Objects.RemoveRandomElement();
						if (gameObject2 != null)
						{
							gameObject.GetPart<AICryptHelpBroadcaster>().BroadcastHelp();
							if (ParentObject.IsVisible())
							{
								IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.DisplayNameOnlyDirect + " filches " + gameObject2.a + gameObject2.DisplayName + "!");
							}
							ParentObject.TakeObject(gameObject2, Silent: false, 0);
						}
						behaviorState = "fleeing";
					}
					else
					{
						if (ParentObject.pBrain.Goals.Items.Any((GoalHandler g) => g.GetType() == typeof(Step) || g.GetType() == typeof(MoveTo)))
						{
							return true;
						}
						ParentObject.pBrain.PushGoal(new NoFightGoal());
						ParentObject.pBrain.MoveTo(gameObject, clearFirst: false);
					}
				}
			}
			else if (behaviorState == "hunting")
			{
				int num = 200;
				if (ParentObject.CurrentZone != null)
				{
					if (ParentObject.CurrentZone.Z == 14)
					{
						num = The.Game.RequireSystem(() => new CatacombsAnchorSystem()).nextAnchorCall;
					}
					if (ParentObject.CurrentZone.Z == 13)
					{
						num = The.Game.RequireSystem(() => new CryptOfLandlordsAnchorSystem()).nextAnchorCall;
					}
					if (ParentObject.CurrentZone.Z == 12)
					{
						num = The.Game.RequireSystem(() => new CryptOfWarriorsAnchorSystem()).nextAnchorCall;
					}
					if (ParentObject.CurrentZone.Z == 11)
					{
						num = The.Game.RequireSystem(() => new CryptOfPriestsAnchorSystem()).nextAnchorCall;
					}
				}
				if (num <= 20)
				{
					if (Fleeing)
					{
						ParentObject.pBrain.Goals.Clear();
						Fleeing = false;
					}
				}
				else if (!Fleeing)
				{
					ParentObject.pBrain.Goals.Clear();
					Fleeing = true;
				}
			}
		}
		return base.FireEvent(E);
	}
}
