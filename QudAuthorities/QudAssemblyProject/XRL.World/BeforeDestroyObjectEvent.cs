using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeDestroyObjectEvent : IDestroyObjectEvent
{
	public new static readonly int ID;

	private static List<BeforeDestroyObjectEvent> Pool;

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

	static BeforeDestroyObjectEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeforeDestroyObjectEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeDestroyObjectEvent()
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

	public static BeforeDestroyObjectEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BeforeDestroyObjectEvent FromPool(GameObject Object, bool Obliterate = false, bool Silent = false, string Reason = null, string ThirdPersonReason = null)
	{
		BeforeDestroyObjectEvent beforeDestroyObjectEvent = FromPool();
		beforeDestroyObjectEvent.Object = Object;
		beforeDestroyObjectEvent.Obliterate = Obliterate;
		beforeDestroyObjectEvent.Silent = Silent;
		beforeDestroyObjectEvent.Reason = Reason;
		beforeDestroyObjectEvent.ThirdPersonReason = ThirdPersonReason;
		return beforeDestroyObjectEvent;
	}

	public static bool Check(GameObject Object, bool Obliterate = false, bool Silent = false, string Reason = null, string ThirdPersonReason = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("BeforeDestroyObject"))
			{
				Event @event = Event.New("BeforeDestroyObject");
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
			MetricsManager.LogError("BeforeDestroyObject registered event handling", x);
		}
		try
		{
			if (flag)
			{
				if (GameObject.validate(ref Object))
				{
					if (Object.WantEvent(ID, MinEvent.CascadeLevel))
					{
						BeforeDestroyObjectEvent e = FromPool(Object, Obliterate, Silent, Reason, ThirdPersonReason);
						flag = Object.HandleEvent(e);
						return flag;
					}
					return flag;
				}
				return flag;
			}
			return flag;
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("BeforeDestroyObject MinEvent handling", x2);
			return flag;
		}
	}
}
