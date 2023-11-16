using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetRandomBuyMutationCountEvent : MinEvent
{
	public GameObject Actor;

	public int BaseAmount;

	public int Amount;

	public new static readonly int ID;

	private static List<GetRandomBuyMutationCountEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetRandomBuyMutationCountEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetRandomBuyMutationCountEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetRandomBuyMutationCountEvent()
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
		BaseAmount = 0;
		Amount = 0;
		base.Reset();
	}

	public static GetRandomBuyMutationCountEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetRandomBuyMutationCountEvent FromPool(GameObject Actor, int BaseAmount, int Amount)
	{
		GetRandomBuyMutationCountEvent getRandomBuyMutationCountEvent = FromPool();
		getRandomBuyMutationCountEvent.Actor = Actor;
		getRandomBuyMutationCountEvent.BaseAmount = BaseAmount;
		getRandomBuyMutationCountEvent.Amount = Amount;
		return getRandomBuyMutationCountEvent;
	}

	public static int GetFor(GameObject Actor, int BaseAmount)
	{
		if (GameObject.validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetRandomBuyMutationCountEvent getRandomBuyMutationCountEvent = FromPool(Actor, BaseAmount, BaseAmount);
			Actor.HandleEvent(getRandomBuyMutationCountEvent);
			return getRandomBuyMutationCountEvent.Amount;
		}
		return BaseAmount;
	}
}
