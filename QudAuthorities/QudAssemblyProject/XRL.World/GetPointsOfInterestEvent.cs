using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using Occult.Engine.CodeGeneration;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetPointsOfInterestEvent : MinEvent
{
	public GameObject Actor;

	public Zone Zone;

	public List<PointOfInterest> List = new List<PointOfInterest>();

	public new static readonly int ID;

	private static List<GetPointsOfInterestEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetPointsOfInterestEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetPointsOfInterestEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetPointsOfInterestEvent()
	{
		base.ID = ID;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static GetPointsOfInterestEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetPointsOfInterestEvent FromPool(GameObject Actor, Zone Zone)
	{
		GetPointsOfInterestEvent getPointsOfInterestEvent = FromPool();
		getPointsOfInterestEvent.Actor = Actor;
		getPointsOfInterestEvent.Zone = Zone;
		getPointsOfInterestEvent.List.Clear();
		return getPointsOfInterestEvent;
	}

	public override void Reset()
	{
		Actor = null;
		Zone = null;
		List.Clear();
		base.Reset();
	}

	public PointOfInterest Find(GameObject Object)
	{
		foreach (PointOfInterest item in List)
		{
			if (item.Object == Object)
			{
				return item;
			}
		}
		return null;
	}

	public PointOfInterest Find(string Key)
	{
		foreach (PointOfInterest item in List)
		{
			if (item.Key == Key)
			{
				return item;
			}
		}
		return null;
	}

	public void Remove(PointOfInterest p)
	{
		List.Remove(p);
	}

	public PointOfInterest Add(GameObject Object = null, string DisplayName = null, string Explanation = null, string Key = null, string Preposition = null, Location2D Location = null, Cell Cell = null, int Radius = -1, IRenderable Icon = null, int Order = 0)
	{
		PointOfInterest pointOfInterest = new PointOfInterest();
		pointOfInterest.Object = Object;
		pointOfInterest.DisplayName = DisplayName;
		pointOfInterest.Explanation = Explanation;
		pointOfInterest.Key = Key;
		if (!string.IsNullOrEmpty(Preposition))
		{
			pointOfInterest.Preposition = Preposition;
		}
		pointOfInterest.Location = Location ?? Cell?.location;
		pointOfInterest.Radius = Radius;
		pointOfInterest.Icon = Icon;
		pointOfInterest.Order = Order;
		List.Add(pointOfInterest);
		return pointOfInterest;
	}

	public bool StandardChecks(IPart Part, GameObject Actor)
	{
		GameObject obj = Part.ParentObject;
		if (!GameObject.validate(ref obj))
		{
			return false;
		}
		if (obj == Actor)
		{
			return false;
		}
		if (Part.Name != "Interesting" && obj.HasPart("Interesting"))
		{
			return false;
		}
		if (obj.HasPart("FungalVision") && FungalVisionary.VisionLevel <= 0)
		{
			return false;
		}
		Render pRender = obj.pRender;
		if (pRender == null || !pRender.Visible)
		{
			return false;
		}
		Cell currentCell = obj.CurrentCell;
		if (currentCell == null || !currentCell.IsExplored())
		{
			return false;
		}
		if (Find(obj) != null)
		{
			return false;
		}
		if (obj.IsHostileTowards(Actor))
		{
			return false;
		}
		if (obj.IsLedBy(Actor))
		{
			return false;
		}
		return true;
	}

	public static List<PointOfInterest> GetFor(GameObject Actor, Zone Z = null)
	{
		if (Z == null)
		{
			Z = Actor?.CurrentZone;
		}
		List<PointOfInterest> result = null;
		if (Z != null && Z.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetPointsOfInterestEvent getPointsOfInterestEvent = FromPool(Actor, Z);
			Z.HandleEvent(getPointsOfInterestEvent);
			if (getPointsOfInterestEvent.List.Count > 0)
			{
				result = new List<PointOfInterest>(getPointsOfInterestEvent.List);
			}
		}
		return result;
	}

	public static PointOfInterest GetOne(GameObject Actor, string Key, Zone Z = null)
	{
		if (Z == null)
		{
			Z = Actor?.CurrentZone;
		}
		if (Z != null && Z.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetPointsOfInterestEvent getPointsOfInterestEvent = FromPool(Actor, Z);
			Z.HandleEvent(getPointsOfInterestEvent);
			return getPointsOfInterestEvent.Find(Key);
		}
		return null;
	}
}
