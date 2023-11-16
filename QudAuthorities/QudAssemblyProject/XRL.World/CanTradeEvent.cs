using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanTradeEvent : MinEvent
{
	public GameObject Actor;

	public GameObject SpeakingWith;

	public Conversation Conversation;

	public bool Base;

	public bool Physical;

	public bool Mental;

	public bool CanTrade;

	public new static readonly int ID;

	private static List<CanTradeEvent> Pool;

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

	static CanTradeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanTradeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanTradeEvent()
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
		Base = false;
		Physical = false;
		Mental = false;
		CanTrade = false;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static CanTradeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanTradeEvent FromPool(GameObject Actor, GameObject SpeakingWith, Conversation Conversation, bool Base, bool Physical, bool Mental, bool CanTrade)
	{
		CanTradeEvent canTradeEvent = FromPool();
		canTradeEvent.Actor = Actor;
		canTradeEvent.SpeakingWith = SpeakingWith;
		canTradeEvent.Conversation = Conversation;
		canTradeEvent.Base = Base;
		canTradeEvent.Physical = Physical;
		canTradeEvent.Mental = Mental;
		canTradeEvent.CanTrade = CanTrade;
		return canTradeEvent;
	}

	public static bool Check(GameObject Actor, GameObject SpeakingWith, Conversation Conversation, bool Base, bool Physical = false, bool Mental = false)
	{
		bool flag = true;
		bool flag2 = Base;
		if (flag)
		{
			bool flag3 = GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("CanTrade");
			bool flag4 = GameObject.validate(ref SpeakingWith) && SpeakingWith.HasRegisteredEvent("CanTrade");
			if (flag3 || flag4)
			{
				Event @event = Event.New("CanTrade");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("SpeakingWith", SpeakingWith);
				@event.SetParameter("Conversation", Conversation);
				@event.SetFlag("Base", Base);
				@event.SetFlag("Physical", Physical);
				@event.SetFlag("Mental", Mental);
				@event.SetFlag("CanTrade", flag2);
				if (flag && flag3)
				{
					flag = Actor.FireEvent(@event);
				}
				if (flag && flag4)
				{
					flag = SpeakingWith.FireEvent(@event);
				}
				flag2 = @event.HasFlag("CanTrade");
			}
		}
		if (flag)
		{
			bool flag5 = GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel);
			bool flag6 = GameObject.validate(ref SpeakingWith) && SpeakingWith.WantEvent(ID, CascadeLevel);
			if (flag5 || flag6)
			{
				CanTradeEvent canTradeEvent = FromPool(Actor, SpeakingWith, Conversation, Base, Physical, Mental, flag2);
				if (flag && flag5)
				{
					flag = Actor.HandleEvent(canTradeEvent);
				}
				if (flag && flag6)
				{
					flag = SpeakingWith.HandleEvent(canTradeEvent);
				}
				flag2 = canTradeEvent.CanTrade;
			}
		}
		return flag2;
	}
}
