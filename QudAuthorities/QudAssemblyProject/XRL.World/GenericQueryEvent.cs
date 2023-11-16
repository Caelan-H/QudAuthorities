using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GenericQueryEvent : MinEvent
{
	public GameObject Object;

	public GameObject Subject;

	public GameObject Source;

	public string Query;

	public int Level;

	public bool Result;

	public new static readonly int ID;

	private static List<GenericQueryEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GenericQueryEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GenericQueryEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GenericQueryEvent()
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

	public override void Reset()
	{
		Object = null;
		Subject = null;
		Source = null;
		Query = null;
		Level = 0;
		Result = false;
		base.Reset();
	}

	public static GenericQueryEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object, string Query, GameObject Subject = null, GameObject Source = null, int Level = 0)
	{
		bool flag = true;
		bool flag2 = false;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("GenericQuery"))
		{
			Event @event = Event.New("GenericQuery");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Subject", Subject);
			@event.SetParameter("Source", Source);
			@event.SetParameter("Query", Query);
			@event.SetParameter("Level", Level);
			@event.SetFlag("Result", flag2);
			flag = Object.FireEvent(@event);
			flag2 = @event.HasFlag("Result");
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GenericQueryEvent genericQueryEvent = FromPool();
			genericQueryEvent.Object = Object;
			genericQueryEvent.Subject = Subject;
			genericQueryEvent.Source = Source;
			genericQueryEvent.Query = Query;
			genericQueryEvent.Level = Level;
			genericQueryEvent.Result = flag2;
			flag = Object.HandleEvent(genericQueryEvent);
			flag2 = genericQueryEvent.Result;
		}
		return flag2;
	}
}
