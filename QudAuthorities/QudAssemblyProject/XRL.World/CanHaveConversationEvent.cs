using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanHaveConversationEvent : MinEvent
{
	public GameObject Actor;

	public GameObject SpeakingWith;

	public Conversation Conversation;

	public bool CanTrade;

	public bool Physical;

	public bool Mental;

	public bool Silent;

	public new static readonly int ID;

	private static List<CanHaveConversationEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CanHaveConversationEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanHaveConversationEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanHaveConversationEvent()
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
		SpeakingWith = null;
		Conversation = null;
		CanTrade = false;
		Physical = false;
		Mental = false;
		Silent = false;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static CanHaveConversationEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanHaveConversationEvent FromPool(GameObject Actor, GameObject SpeakingWith, Conversation Conversation, bool CanTrade, bool Physical, bool Mental, bool Silent)
	{
		CanHaveConversationEvent canHaveConversationEvent = FromPool();
		canHaveConversationEvent.Actor = Actor;
		canHaveConversationEvent.SpeakingWith = SpeakingWith;
		canHaveConversationEvent.Conversation = Conversation;
		canHaveConversationEvent.CanTrade = CanTrade;
		canHaveConversationEvent.Physical = Physical;
		canHaveConversationEvent.Mental = Mental;
		canHaveConversationEvent.Silent = Silent;
		return canHaveConversationEvent;
	}

	public static bool Check(GameObject Actor, GameObject SpeakingWith, Conversation Conversation, bool CanTrade = false, bool Physical = false, bool Mental = false, bool Silent = false)
	{
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("CanHaveConversation");
			bool flag3 = GameObject.validate(ref SpeakingWith) && SpeakingWith.HasRegisteredEvent("CanHaveConversation");
			if (flag2 || flag3)
			{
				Event @event = Event.New("CanHaveConversation");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("SpeakingWith", SpeakingWith);
				@event.SetParameter("Conversation", Conversation);
				@event.SetFlag("CanTrade", CanTrade);
				@event.SetFlag("Physical", Physical);
				@event.SetFlag("Mental", Mental);
				@event.SetSilent(Silent);
				if (flag && flag2)
				{
					flag = Actor.FireEvent(@event);
				}
				if (flag && flag3)
				{
					flag = SpeakingWith.FireEvent(@event);
				}
			}
		}
		if (flag)
		{
			bool flag4 = GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel);
			bool flag5 = GameObject.validate(ref SpeakingWith) && SpeakingWith.WantEvent(ID, CascadeLevel);
			if (flag4 || flag5)
			{
				CanHaveConversationEvent e = FromPool(Actor, SpeakingWith, Conversation, CanTrade, Physical, Mental, Silent);
				if (flag && flag4)
				{
					flag = Actor.HandleEvent(e);
				}
				if (flag && flag5)
				{
					flag = SpeakingWith.HandleEvent(e);
				}
			}
		}
		return flag;
	}
}
