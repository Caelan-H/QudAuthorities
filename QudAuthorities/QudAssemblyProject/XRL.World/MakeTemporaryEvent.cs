using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class MakeTemporaryEvent : MinEvent
{
	public GameObject RootObject;

	public int Duration;

	public string TurnInto;

	public GameObject DependsOn;

	public new static readonly int ID;

	private static List<MakeTemporaryEvent> Pool;

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

	static MakeTemporaryEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(MakeTemporaryEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public MakeTemporaryEvent()
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

	public static MakeTemporaryEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static MakeTemporaryEvent FromPool(GameObject RootObject, int Duration = -1, string TurnInto = null, GameObject DependsOn = null)
	{
		MakeTemporaryEvent makeTemporaryEvent = FromPool();
		makeTemporaryEvent.RootObject = RootObject;
		makeTemporaryEvent.Duration = Duration;
		makeTemporaryEvent.TurnInto = TurnInto;
		makeTemporaryEvent.DependsOn = DependsOn;
		return makeTemporaryEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		RootObject = null;
		Duration = -1;
		TurnInto = null;
		DependsOn = null;
		base.Reset();
	}

	public static bool Send(GameObject RootObject, int Duration = -1, string TurnInto = null, GameObject DependsOn = null)
	{
		if (RootObject.WantEvent(ID, CascadeLevel) && !RootObject.HandleEvent(FromPool(RootObject, Duration, TurnInto, DependsOn)))
		{
			return false;
		}
		return true;
	}
}
