using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CleanItemsEvent : MinEvent
{
	public GameObject Actor;

	public GameObject CascadeFrom;

	public GameObject Using;

	public List<GameObject> Objects = new List<GameObject>();

	public List<string> Types = new List<string>();

	public new static readonly int ID;

	private static List<CleanItemsEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CleanItemsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CleanItemsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CleanItemsEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Actor = null;
		CascadeFrom = null;
		Using = null;
		Objects.Clear();
		Types.Clear();
		base.Reset();
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static CleanItemsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CleanItemsEvent FromPool(GameObject Actor, GameObject CascadeFrom, GameObject Using)
	{
		CleanItemsEvent cleanItemsEvent = FromPool();
		cleanItemsEvent.Actor = Actor;
		cleanItemsEvent.CascadeFrom = CascadeFrom;
		cleanItemsEvent.Using = Using;
		cleanItemsEvent.Objects.Clear();
		cleanItemsEvent.Types.Clear();
		return cleanItemsEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public void RegisterObject(GameObject obj)
	{
		if (obj != null && !Objects.Contains(obj))
		{
			Objects.Add(obj);
		}
	}

	public void RegisterType(string type)
	{
		if (!string.IsNullOrEmpty(type) && !Types.Contains(type))
		{
			Types.Add(type);
		}
	}

	public static bool PerformFor(GameObject Actor, GameObject CascadeFrom, GameObject Using, out List<GameObject> Objects, out List<string> Types)
	{
		if (GameObject.validate(ref CascadeFrom) && CascadeFrom.WantEvent(ID, CascadeLevel))
		{
			CleanItemsEvent cleanItemsEvent = FromPool(Actor, CascadeFrom, Using);
			CascadeFrom.HandleEvent(cleanItemsEvent);
			Objects = cleanItemsEvent.Objects;
			Types = cleanItemsEvent.Types;
			return true;
		}
		Objects = null;
		Types = null;
		return false;
	}
}
