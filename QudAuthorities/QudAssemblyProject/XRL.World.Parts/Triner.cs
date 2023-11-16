using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class Triner : IPart
{
	public GameObject Trin1;

	public GameObject Trin2;

	public int DesiredDistance = 3;

	public int KeepAdjacentToTarget = 2;

	public int RealityStabilizationPenetration = 80;

	public override bool SameAs(IPart p)
	{
		Triner triner = p as Triner;
		if (triner.Trin1 != null || Trin1 != null)
		{
			return false;
		}
		if (triner.Trin2 != null || Trin2 != null)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != IsSensableAsPsychicEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == OnDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		E.Sensable = true;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDeathRemovalEvent E)
	{
		Unlink();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		Unlink();
		return base.HandleEvent(E);
	}

	private bool Link(GameObject who)
	{
		if (who == null)
		{
			return false;
		}
		if (!GameObject.validate(ref Trin1) && Trin2 != who && who != ParentObject)
		{
			Trin1 = who;
			Trin2?.GetPart<Triner>()?.Link(who);
			who.GetPart<Triner>()?.Link(Trin2);
			return true;
		}
		if (!GameObject.validate(ref Trin2) && Trin1 != who && who != ParentObject)
		{
			Trin2 = who;
			Trin1?.GetPart<Triner>()?.Link(who);
			who.GetPart<Triner>()?.Link(Trin1);
			return true;
		}
		return false;
	}

	private void Unlink(GameObject who)
	{
		if (who == Trin1)
		{
			Trin1 = null;
		}
		if (who == Trin2)
		{
			Trin2 = null;
		}
	}

	private void Unlink()
	{
		Trin1?.GetPart<Triner>()?.Unlink(ParentObject);
		Trin2?.GetPart<Triner>()?.Unlink(ParentObject);
		Trin1 = null;
		Trin2 = null;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AITakingAction");
		base.Register(Object);
	}

	public int Spawn()
	{
		if (GameObject.validate(ref Trin1) && GameObject.validate(ref Trin2))
		{
			return 0;
		}
		if (!ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this, "RealityStabilizationPenetration", RealityStabilizationPenetration)))
		{
			return 0;
		}
		int num = 0;
		if (SpawnOne())
		{
			num++;
			if ((!GameObject.validate(ref Trin1) || !GameObject.validate(ref Trin2)) && SpawnOne())
			{
				num++;
			}
		}
		return num;
	}

	public bool SpawnOne()
	{
		GameObject gameObject = null;
		foreach (Cell item in new List<Cell>(ParentObject.CurrentCell.GetLocalAdjacentCells()).ShuffleInPlace())
		{
			if (!item.IsEmpty())
			{
				continue;
			}
			if (!item.FireEvent(Event.New("InitiateRealityDistortionRemote", "Object", ParentObject, "Mutation", this, "RealityStabilizationPenetration", RealityStabilizationPenetration)))
			{
				break;
			}
			gameObject = GameObject.createUnmodified(ParentObject.Blueprint);
			gameObject.RemovePart("RandomLoot");
			gameObject.GetStat("XPValue").BaseValue = 0;
			gameObject.MakeActive();
			item.AddObject(gameObject);
			Link(gameObject);
			gameObject.RequirePart<Triner>().Link(ParentObject);
			gameObject.TakeOnAttitudesOf(ParentObject, CopyLeader: true, CopyTarget: true);
			if (IComponent<GameObject>.Visible(gameObject))
			{
				for (int i = 0; i < 10; i++)
				{
					The.ParticleManager.AddRadial("&Mù", item.X, item.Y, Stat.Random(0, 5), Stat.Random(5, 10), 0.01f * (float)Stat.Random(4, 6), -0.05f * (float)Stat.Random(3, 7));
				}
			}
			return true;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AITakingAction")
		{
			CheckSpawn();
			CheckStationKeeping();
		}
		return base.FireEvent(E);
	}

	public void CheckSpawn()
	{
		if ((!GameObject.validate(ref Trin1) || !GameObject.validate(ref Trin2)) && ParentObject.IsValid() && ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)))
		{
			Spawn();
		}
	}

	public void CheckStationKeeping()
	{
		if (ParentObject.pBrain == null)
		{
			return;
		}
		int num = (GameObject.validate(ref Trin1) ? ParentObject.DistanceTo(Trin1) : DesiredDistance);
		int num2 = (GameObject.validate(ref Trin2) ? ParentObject.DistanceTo(Trin2) : DesiredDistance);
		if (num == DesiredDistance && num2 == DesiredDistance)
		{
			return;
		}
		GameObject target = ParentObject.Target;
		if (target != null && ParentObject.InAdjacentCellTo(target))
		{
			GameObject gameObject = Trin1?.Target;
			GameObject gameObject2 = Trin2?.Target;
			if (gameObject == null || gameObject2 == null || !Trin1.InAdjacentCellTo(gameObject) || !Trin2.InAdjacentCellTo(gameObject2))
			{
				return;
			}
		}
		List<Cell> passableConnectedAdjacentCellsFor = ParentObject.CurrentCell.GetPassableConnectedAdjacentCellsFor(ParentObject, 1);
		if (passableConnectedAdjacentCellsFor != null && passableConnectedAdjacentCellsFor.Count != 0)
		{
			passableConnectedAdjacentCellsFor.Sort(CellSort);
			Cell target2 = passableConnectedAdjacentCellsFor[0];
			if (ParentObject.pBrain.Goals.Peek() is Step)
			{
				ParentObject.pBrain.Goals.Pop();
			}
			ParentObject.pBrain.Think("I'm going to move to try to keep station with my trins.");
			ParentObject.pBrain.PushGoal(new Step(ParentObject.CurrentCell.GetDirectionFromCell(target2)));
		}
	}

	private int CellSort(Cell a, Cell b)
	{
		int num = CellAvoidance(a).CompareTo(CellAvoidance(b));
		if (num != 0)
		{
			return num;
		}
		GameObject target = ParentObject.Target;
		if (target != null)
		{
			return a.DistanceTo(target).CompareTo(b.DistanceTo(target));
		}
		return 0;
	}

	private int CellAvoidance(Cell C)
	{
		int num = (GameObject.validate(ref Trin1) ? C.DistanceTo(Trin1) : DesiredDistance);
		int num2 = (GameObject.validate(ref Trin2) ? C.DistanceTo(Trin2) : DesiredDistance);
		int num3 = Math.Abs(DesiredDistance - num);
		int num4 = Math.Abs(DesiredDistance - num2);
		return num3 + num4;
	}
}
