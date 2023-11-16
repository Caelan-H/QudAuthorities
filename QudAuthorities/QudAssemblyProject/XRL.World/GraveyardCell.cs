using System;
using System.Collections.Generic;

namespace XRL.World;

[Serializable]
public class GraveyardCell : Cell
{
	public static List<GameObject> EmptyObjects;

	public GraveyardCell(Zone ParentZone)
		: base(ParentZone)
	{
	}

	public override GameObject AddObject(GameObject GO, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, bool Repaint = true, string Direction = null, string Type = null, GameObject Dragging = null, List<GameObject> Tracking = null, IEvent ParentEvent = null)
	{
		OccludeCache = -1;
		Objects.Add(GO);
		Tracking?.Add(GO);
		if (GO.pPhysics != null)
		{
			GO.pPhysics.CurrentCell = this;
		}
		return GO;
	}

	public override List<GameObject> GetObjectsInCell()
	{
		if (EmptyObjects == null)
		{
			EmptyObjects = new List<GameObject>();
		}
		else
		{
			EmptyObjects.Clear();
		}
		return EmptyObjects;
	}

	public override List<GameObject> GetObjects(string Blueprint)
	{
		if (EmptyObjects == null)
		{
			EmptyObjects = new List<GameObject>();
		}
		else
		{
			EmptyObjects.Clear();
		}
		return EmptyObjects;
	}

	public override void GetObjects(List<GameObject> List, string Blueprint)
	{
	}

	public override List<GameObject> GetObjectsWithTagOrProperty(string Name)
	{
		if (EmptyObjects == null)
		{
			EmptyObjects = new List<GameObject>();
		}
		else
		{
			EmptyObjects.Clear();
		}
		return EmptyObjects;
	}

	public override void GetObjectsWithTagOrProperty(List<GameObject> List, string Name)
	{
	}

	public override List<GameObject> GetObjectsWithPart(string PartName)
	{
		if (EmptyObjects == null)
		{
			EmptyObjects = new List<GameObject>();
		}
		else
		{
			EmptyObjects.Clear();
		}
		return EmptyObjects;
	}

	public override List<GameObject> GetObjectsWithProperty(string PartName)
	{
		if (EmptyObjects == null)
		{
			EmptyObjects = new List<GameObject>();
		}
		else
		{
			EmptyObjects.Clear();
		}
		return EmptyObjects;
	}

	public override void GetObjectsWithPart(string PartName, List<GameObject> Return)
	{
	}

	public override void GetObjectsWithProperty(string PartName, List<GameObject> Return)
	{
	}

	public override bool HasObjectWithPart(string PartName)
	{
		return false;
	}

	public override GameObject GetFirstObjectWithPart(string PartName)
	{
		return null;
	}

	public new virtual GameObject GetCombatObject()
	{
		return null;
	}

	public virtual GameObject GetCombatTarget(GameObject Attacker = null, bool IgnoreFlight = false, bool IgnorePhase = false, bool IgnoreAttackable = false, GameObject CheckPhaseAgainst = null, bool AllowInanimate = true, bool InanimateSolidOnly = false, bool ForMissile = false, GameObject MissileFrom = null)
	{
		return null;
	}

	public override IEnumerable<GameObject> LoopObjectsWithPart(string PartName)
	{
		yield break;
	}

	public override bool IsEmpty()
	{
		return true;
	}

	public override bool IsEmptyFor(GameObject who)
	{
		return true;
	}

	public override bool IsExplored()
	{
		return false;
	}

	public override bool IsPassable(GameObject obj, bool includeCombatObjects)
	{
		return true;
	}

	public override bool IsOccluding()
	{
		return false;
	}

	public override bool IsOccludingFor(GameObject obj)
	{
		return false;
	}

	public override bool IsOccludingOtherThan(GameObject obj)
	{
		return false;
	}
}
