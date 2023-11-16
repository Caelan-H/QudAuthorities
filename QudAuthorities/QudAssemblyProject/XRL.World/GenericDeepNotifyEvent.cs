using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GenericDeepNotifyEvent : MinEvent
{
	public GameObject Object;

	public GameObject Subject;

	public GameObject Source;

	public string Notify;

	public int Level;

	public new static readonly int ID;

	private static List<GenericDeepNotifyEvent> Pool;

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

	static GenericDeepNotifyEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GenericDeepNotifyEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GenericDeepNotifyEvent()
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
		Notify = null;
		Level = 0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static GenericDeepNotifyEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(GameObject Object, string Notify, GameObject Subject = null, GameObject Source = null, int Level = 0)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("GenericDeepNotify"))
		{
			Event @event = Event.New("GenericDeepNotify");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Subject", Subject);
			@event.SetParameter("Source", Source);
			@event.SetParameter("Notify", Notify);
			@event.SetParameter("Level", Level);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			GenericDeepNotifyEvent genericDeepNotifyEvent = FromPool();
			genericDeepNotifyEvent.Object = Object;
			genericDeepNotifyEvent.Subject = Subject;
			genericDeepNotifyEvent.Source = Source;
			genericDeepNotifyEvent.Notify = Notify;
			genericDeepNotifyEvent.Level = Level;
			flag = Object.HandleEvent(genericDeepNotifyEvent);
		}
	}
}
