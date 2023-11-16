using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanStartConversationEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Object;

	public bool Physical;

	public bool Mental;

	public string FailureMessage;

	public new static readonly int ID;

	private static List<CanStartConversationEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CanStartConversationEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanStartConversationEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanStartConversationEvent()
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

	public static CanStartConversationEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanStartConversationEvent FromPool(GameObject Actor, GameObject Object, bool Physical = false, bool Mental = false)
	{
		CanStartConversationEvent canStartConversationEvent = FromPool();
		canStartConversationEvent.Actor = Actor;
		canStartConversationEvent.Object = Object;
		canStartConversationEvent.Physical = Physical;
		canStartConversationEvent.Mental = Mental;
		canStartConversationEvent.FailureMessage = null;
		return canStartConversationEvent;
	}

	public override void Reset()
	{
		Actor = null;
		Object = null;
		Physical = false;
		Mental = false;
		FailureMessage = null;
		base.Reset();
	}

	public static bool Check(GameObject Actor, GameObject Object, out string FailureMessage, bool Physical = false, bool Mental = false)
	{
		FailureMessage = null;
		if (Actor.HasRegisteredEvent("CanStartConversation") || Object.HasRegisteredEvent("CanStartConversation"))
		{
			Event @event = Event.New("CanStartConversation");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Object", Object);
			@event.SetFlag("Physical", Physical);
			@event.SetFlag("Mental", Mental);
			try
			{
				if (!Actor.FireEvent(@event))
				{
					return false;
				}
				if (!Object.FireEvent(@event))
				{
					return false;
				}
			}
			finally
			{
				FailureMessage = @event.GetStringParameter("FailureMessage");
			}
		}
		bool flag = Actor.WantEvent(ID, MinEvent.CascadeLevel);
		bool flag2 = Object.WantEvent(ID, MinEvent.CascadeLevel);
		if (flag || flag2)
		{
			CanStartConversationEvent canStartConversationEvent = FromPool(Actor, Object, Physical, Mental);
			try
			{
				if (flag && !Actor.HandleEvent(canStartConversationEvent))
				{
					return false;
				}
				if (flag2 && !Object.HandleEvent(canStartConversationEvent))
				{
					return false;
				}
			}
			finally
			{
				FailureMessage = canStartConversationEvent.FailureMessage;
			}
		}
		return true;
	}
}
