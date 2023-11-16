using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeConversationEvent : MinEvent
{
	public GameObject Actor;

	public GameObject SpeakingWith;

	public Conversation Conversation;

	public bool CanTrade;

	public bool Physical;

	public bool Mental;

	public bool Silent;

	public new static readonly int ID;

	private static List<BeforeConversationEvent> Pool;

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

	static BeforeConversationEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeforeConversationEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeConversationEvent()
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

	public static BeforeConversationEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BeforeConversationEvent FromPool(GameObject Actor, GameObject SpeakingWith, Conversation Conversation, bool CanTrade, bool Physical, bool Mental, bool Silent)
	{
		BeforeConversationEvent beforeConversationEvent = FromPool();
		beforeConversationEvent.Actor = Actor;
		beforeConversationEvent.SpeakingWith = SpeakingWith;
		beforeConversationEvent.Conversation = Conversation;
		beforeConversationEvent.CanTrade = CanTrade;
		beforeConversationEvent.Physical = Physical;
		beforeConversationEvent.Mental = Mental;
		beforeConversationEvent.Silent = Silent;
		return beforeConversationEvent;
	}

	public static bool Check(GameObject Actor, GameObject SpeakingWith, Conversation Conversation, bool CanTrade = false, bool Physical = false, bool Mental = false, bool Silent = false)
	{
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("BeforeConversation");
			bool flag3 = GameObject.validate(ref SpeakingWith) && SpeakingWith.HasRegisteredEvent("BeforeConversation");
			if (flag2 || flag3)
			{
				Event @event = Event.New("BeforeConversation");
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
				BeforeConversationEvent e = FromPool(Actor, SpeakingWith, Conversation, CanTrade, Physical, Mental, Silent);
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
