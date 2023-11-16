using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class OnDestroyObjectEvent : IDestroyObjectEvent
{
	public new static readonly int ID;

	private static List<OnDestroyObjectEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		if (!base.handlePartDispatch(Part))
		{
			return false;
		}
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		if (!base.handleEffectDispatch(Effect))
		{
			return false;
		}
		return Effect.HandleEvent(this);
	}

	static OnDestroyObjectEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(OnDestroyObjectEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public OnDestroyObjectEvent()
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

	public static OnDestroyObjectEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static OnDestroyObjectEvent FromPool(GameObject Object, bool Obliterate = false, bool Silent = false, string Reason = null, string ThirdPersonReason = null)
	{
		OnDestroyObjectEvent onDestroyObjectEvent = FromPool();
		onDestroyObjectEvent.Object = Object;
		onDestroyObjectEvent.Obliterate = Obliterate;
		onDestroyObjectEvent.Silent = Silent;
		onDestroyObjectEvent.Reason = Reason;
		onDestroyObjectEvent.ThirdPersonReason = ThirdPersonReason;
		return onDestroyObjectEvent;
	}

	public static void Send(GameObject Object, bool Obliterate = false, bool Silent = false, string Reason = null, string ThirdPersonReason = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.validate(Object) && Object.HasRegisteredEvent("OnDestroyObject"))
			{
				Event @event = Event.New("OnDestroyObject");
				@event.SetParameter("Object", Object);
				@event.SetFlag("Obliterate", Obliterate);
				@event.SetSilent(Silent);
				@event.SetParameter("Reason", Reason);
				@event.SetParameter("ThirdPersonReason", ThirdPersonReason);
				flag = Object.FireEvent(@event);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("OnDestroyObject registered event handling", x);
		}
		try
		{
			if (flag && GameObject.validate(Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
			{
				OnDestroyObjectEvent e = FromPool(Object, Obliterate, Silent, Reason, ThirdPersonReason);
				flag = Object.HandleEvent(e);
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("OnDestroyObject MinEvent handling", x2);
		}
	}
}
