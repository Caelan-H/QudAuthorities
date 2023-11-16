using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetRandomBuyChimericBodyPartRollsEvent : MinEvent
{
	public GameObject Actor;

	public int BaseAmount;

	public int Amount;

	public new static readonly int ID;

	private static List<GetRandomBuyChimericBodyPartRollsEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetRandomBuyChimericBodyPartRollsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetRandomBuyChimericBodyPartRollsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetRandomBuyChimericBodyPartRollsEvent()
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

	public static GetRandomBuyChimericBodyPartRollsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetRandomBuyChimericBodyPartRollsEvent FromPool(GameObject Actor, int BaseAmount, int Amount)
	{
		GetRandomBuyChimericBodyPartRollsEvent getRandomBuyChimericBodyPartRollsEvent = FromPool();
		getRandomBuyChimericBodyPartRollsEvent.Actor = Actor;
		getRandomBuyChimericBodyPartRollsEvent.BaseAmount = BaseAmount;
		getRandomBuyChimericBodyPartRollsEvent.Amount = Amount;
		return getRandomBuyChimericBodyPartRollsEvent;
	}

	public static int GetFor(GameObject Actor, int BaseAmount)
	{
		if (GameObject.validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetRandomBuyChimericBodyPartRollsEvent getRandomBuyChimericBodyPartRollsEvent = FromPool(Actor, BaseAmount, BaseAmount);
			Actor.HandleEvent(getRandomBuyChimericBodyPartRollsEvent);
			return getRandomBuyChimericBodyPartRollsEvent.Amount;
		}
		return BaseAmount;
	}
}
