using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetWaterRitualSellSecretBehaviorEvent : MinEvent
{
	public GameObject Actor;

	public GameObject SpeakingWith;

	public string Message;

	public int ReputationProvided;

	public int BonusReputationProvided;

	public bool IsSecret;

	public bool IsGossip;

	public new static readonly int ID;

	private static List<GetWaterRitualSellSecretBehaviorEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetWaterRitualSellSecretBehaviorEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetWaterRitualSellSecretBehaviorEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetWaterRitualSellSecretBehaviorEvent()
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

	public static GetWaterRitualSellSecretBehaviorEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetWaterRitualSellSecretBehaviorEvent FromPool(GameObject Actor, GameObject SpeakingWith, string Message, int ReputationProvided, int BonusReputationProvided, bool IsSecret = false, bool IsGossip = false)
	{
		GetWaterRitualSellSecretBehaviorEvent getWaterRitualSellSecretBehaviorEvent = FromPool();
		getWaterRitualSellSecretBehaviorEvent.Actor = Actor;
		getWaterRitualSellSecretBehaviorEvent.SpeakingWith = SpeakingWith;
		getWaterRitualSellSecretBehaviorEvent.Message = Message;
		getWaterRitualSellSecretBehaviorEvent.ReputationProvided = ReputationProvided;
		getWaterRitualSellSecretBehaviorEvent.BonusReputationProvided = BonusReputationProvided;
		getWaterRitualSellSecretBehaviorEvent.IsSecret = IsSecret;
		getWaterRitualSellSecretBehaviorEvent.IsGossip = IsGossip;
		return getWaterRitualSellSecretBehaviorEvent;
	}

	public override void Reset()
	{
		Actor = null;
		SpeakingWith = null;
		Message = null;
		ReputationProvided = 0;
		BonusReputationProvided = 0;
		IsSecret = false;
		IsGossip = false;
		base.Reset();
	}

	public static void Send(GameObject Actor, GameObject SpeakingWith, ref string Message, ref int ReputationProvided, ref int BonusReputationProvided, bool IsSecret = false, bool IsGossip = false)
	{
		if (GameObject.validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetWaterRitualSellSecretBehaviorEvent getWaterRitualSellSecretBehaviorEvent = FromPool(Actor, SpeakingWith, Message, ReputationProvided, BonusReputationProvided, IsSecret, IsGossip);
			bool num = Actor.HandleEvent(getWaterRitualSellSecretBehaviorEvent);
			Message = getWaterRitualSellSecretBehaviorEvent.Message;
			ReputationProvided = getWaterRitualSellSecretBehaviorEvent.ReputationProvided;
			BonusReputationProvided = getWaterRitualSellSecretBehaviorEvent.BonusReputationProvided;
			if (!num)
			{
				return;
			}
		}
		if (GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("GetWaterRitualSellSecretBehavior"))
		{
			Event @event = Event.New("GetWaterRitualSellSecretBehavior");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("SpeakingWith", SpeakingWith);
			@event.SetParameter("Message", Message);
			@event.SetParameter("ReputationProvided", ReputationProvided);
			@event.SetParameter("BonusReputationProvided", BonusReputationProvided);
			@event.SetFlag("IsSecret", IsSecret);
			@event.SetFlag("IsGossip", IsGossip);
			Actor.FireEvent(@event);
			Message = @event.GetStringParameter("Message");
			ReputationProvided = @event.GetIntParameter("ReputationProvided");
			BonusReputationProvided = @event.GetIntParameter("BonusReputationProvided");
		}
	}
}
