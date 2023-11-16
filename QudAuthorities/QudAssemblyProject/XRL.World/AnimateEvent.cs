using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AnimateEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Object;

	public GameObject Using;

	public List<IPart> PartsToRemove;

	public new static readonly int ID;

	private static List<AnimateEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static AnimateEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AnimateEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AnimateEvent()
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
		Actor = null;
		Object = null;
		Using = null;
		PartsToRemove = null;
		base.Reset();
	}

	public static AnimateEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public void WantToRemove(IPart Part)
	{
		if (PartsToRemove != null)
		{
			if (!PartsToRemove.Contains(Part))
			{
				PartsToRemove.Add(Part);
			}
		}
		else
		{
			Object.RemovePart(Part);
		}
	}

	public static void Send(GameObject Actor, GameObject Object, GameObject Using)
	{
		bool flag = true;
		List<IPart> list = null;
		try
		{
			if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("Animate"))
			{
				if (list == null)
				{
					list = new List<IPart>();
				}
				Event @event = Event.New("Animate");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Object", Object);
				@event.SetParameter("Using", Using);
				@event.SetParameter("PartsToRemove", list);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
			{
				if (list == null)
				{
					list = new List<IPart>();
				}
				AnimateEvent animateEvent = FromPool();
				animateEvent.Actor = Actor;
				animateEvent.Object = Object;
				animateEvent.Using = Using;
				animateEvent.PartsToRemove = list;
				flag = Object.HandleEvent(animateEvent);
			}
		}
		finally
		{
			if (list != null)
			{
				foreach (IPart item in list)
				{
					Object.RemovePart(item);
				}
			}
		}
	}
}
