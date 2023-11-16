using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetOverloadChargeEvent : MinEvent
{
	public GameObject Object;

	public int Amount;

	public new static readonly int ID;

	private static List<GetOverloadChargeEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetOverloadChargeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetOverloadChargeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetOverloadChargeEvent()
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

	public static GetOverloadChargeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Object = null;
		Amount = 0;
		base.Reset();
	}

	public static int GetFor(GameObject Object, int Amount)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("GetOverloadCharge"))
		{
			Event @event = Event.New("GetOverloadCharge");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Amount", Amount);
			flag = Object.FireEvent(@event);
			Amount = @event.GetIntParameter("Amount");
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetOverloadChargeEvent getOverloadChargeEvent = FromPool();
			getOverloadChargeEvent.Object = Object;
			getOverloadChargeEvent.Amount = Amount;
			flag = Object.HandleEvent(getOverloadChargeEvent);
			Amount = getOverloadChargeEvent.Amount;
		}
		return Amount;
	}
}
