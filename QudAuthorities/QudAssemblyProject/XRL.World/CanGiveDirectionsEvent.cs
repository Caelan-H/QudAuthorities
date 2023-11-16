using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanGiveDirectionsEvent : MinEvent
{
	public GameObject Actor;

	public GameObject SpeakingWith;

	public Conversation Conversation;

	public bool CanTrade;

	public bool Physical;

	public bool Mental;

	public bool Silent;

	public bool PlayerCompanion;

	public bool CanGiveDirections;

	public new static readonly int ID;

	private static List<CanGiveDirectionsEvent> Pool;

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

	static CanGiveDirectionsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanGiveDirectionsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanGiveDirectionsEvent()
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
		PlayerCompanion = false;
		CanGiveDirections = false;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static CanGiveDirectionsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CanGiveDirectionsEvent FromPool(GameObject Actor, GameObject SpeakingWith, Conversation Conversation, bool CanTrade, bool Physical, bool Mental, bool Silent, bool PlayerCompanion, bool CanGiveDirections)
	{
		CanGiveDirectionsEvent canGiveDirectionsEvent = FromPool();
		canGiveDirectionsEvent.Actor = Actor;
		canGiveDirectionsEvent.SpeakingWith = SpeakingWith;
		canGiveDirectionsEvent.Conversation = Conversation;
		canGiveDirectionsEvent.CanTrade = CanTrade;
		canGiveDirectionsEvent.Physical = Physical;
		canGiveDirectionsEvent.Mental = Mental;
		canGiveDirectionsEvent.Silent = Silent;
		canGiveDirectionsEvent.PlayerCompanion = PlayerCompanion;
		canGiveDirectionsEvent.CanGiveDirections = CanGiveDirections;
		return canGiveDirectionsEvent;
	}

	public static bool Check(GameObject Actor, GameObject SpeakingWith, Conversation Conversation, bool CanTrade = false, bool Physical = false, bool Mental = false, bool Silent = false)
	{
		bool flag = SpeakingWith.GetIntProperty("TurnsAsPlayerMinion") > 0 || SpeakingWith.IsPlayerLed();
		bool flag2 = false;
		if (!flag && SpeakingWith.HasTagOrProperty("ClearLost"))
		{
			flag2 = true;
		}
		bool flag3 = true;
		if (flag3)
		{
			bool flag4 = GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("CanGiveDirections");
			bool flag5 = GameObject.validate(ref SpeakingWith) && SpeakingWith.HasRegisteredEvent("CanGiveDirections");
			if (flag4 || flag5)
			{
				Event @event = Event.New("CanGiveDirections");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("SpeakingWith", SpeakingWith);
				@event.SetParameter("Conversation", Conversation);
				@event.SetFlag("CanTrade", CanTrade);
				@event.SetFlag("Physical", Physical);
				@event.SetFlag("Mental", Mental);
				@event.SetFlag("PlayerCompanion", flag);
				@event.SetFlag("CanGiveDirections", flag2);
				@event.SetSilent(Silent);
				if (flag3 && flag4)
				{
					flag3 = Actor.FireEvent(@event);
				}
				if (flag3 && flag5)
				{
					flag3 = SpeakingWith.FireEvent(@event);
				}
				flag2 = @event.HasFlag("CanGiveDirections");
			}
		}
		if (flag3)
		{
			bool flag6 = GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel);
			bool flag7 = GameObject.validate(ref SpeakingWith) && SpeakingWith.WantEvent(ID, CascadeLevel);
			if (flag6 || flag7)
			{
				CanGiveDirectionsEvent canGiveDirectionsEvent = FromPool(Actor, SpeakingWith, Conversation, CanTrade, Physical, Mental, Silent, flag, flag2);
				if (flag3 && flag6)
				{
					flag3 = Actor.HandleEvent(canGiveDirectionsEvent);
				}
				if (flag3 && flag7)
				{
					flag3 = SpeakingWith.HandleEvent(canGiveDirectionsEvent);
				}
				flag2 = canGiveDirectionsEvent.CanGiveDirections;
			}
		}
		return flag2;
	}
}
