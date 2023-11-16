using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class DamageConstantAdjustedEvent : MinEvent
{
	public GameObject Object;

	public int Amount;

	public new static readonly int ID;

	private static List<DamageConstantAdjustedEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static DamageConstantAdjustedEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public DamageConstantAdjustedEvent()
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

	public static DamageConstantAdjustedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static DamageConstantAdjustedEvent FromPool(GameObject Object, int Amount)
	{
		DamageConstantAdjustedEvent damageConstantAdjustedEvent = FromPool();
		damageConstantAdjustedEvent.Object = Object;
		damageConstantAdjustedEvent.Amount = Amount;
		return damageConstantAdjustedEvent;
	}

	public override void Reset()
	{
		Object = null;
		Amount = 0;
		base.Reset();
	}

	public static void Send(GameObject Object, int Amount)
	{
		if (GameObject.validate(ref Object) && Object.HasRegisteredEvent("DamageConstantAdjusted"))
		{
			Event @event = Event.New("DamageConstantAdjusted");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Amount", Amount);
			Object.FireEvent(@event);
		}
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object, Amount));
		}
	}
}
