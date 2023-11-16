using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GenericDeepQueryEvent : MinEvent
{
	public GameObject Object;

	public GameObject Subject;

	public GameObject Source;

	public string Query;

	public int Level;

	public bool Result;

	public new static readonly int ID;

	private static List<GenericDeepQueryEvent> Pool;

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

	static GenericDeepQueryEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GenericDeepQueryEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GenericDeepQueryEvent()
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

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static GenericDeepQueryEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object, string Query, GameObject Subject = null, GameObject Source = null, int Level = 0)
	{
		bool flag = true;
		bool flag2 = false;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("GenericDeepQuery"))
		{
			Event @event = Event.New("GenericDeepQuery");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Subject", Subject);
			@event.SetParameter("Source", Source);
			@event.SetParameter("Query", Query);
			@event.SetParameter("Level", Level);
			@event.SetFlag("Result", flag2);
			flag = Object.FireEvent(@event);
			flag2 = @event.HasFlag("Result");
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			GenericDeepQueryEvent genericDeepQueryEvent = FromPool();
			genericDeepQueryEvent.Object = Object;
			genericDeepQueryEvent.Subject = Subject;
			genericDeepQueryEvent.Source = Source;
			genericDeepQueryEvent.Query = Query;
			genericDeepQueryEvent.Level = Level;
			genericDeepQueryEvent.Result = flag2;
			flag = Object.HandleEvent(genericDeepQueryEvent);
			flag2 = genericDeepQueryEvent.Result;
		}
		return flag2;
	}
}
