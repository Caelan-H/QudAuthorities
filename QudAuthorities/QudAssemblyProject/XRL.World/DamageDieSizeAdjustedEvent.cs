using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class DamageDieSizeAdjustedEvent : MinEvent
{
	public GameObject Object;

	public int Amount;

	public new static readonly int ID;

	private static List<DamageDieSizeAdjustedEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static DamageDieSizeAdjustedEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public DamageDieSizeAdjustedEvent()
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

	public static DamageDieSizeAdjustedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static DamageDieSizeAdjustedEvent FromPool(GameObject Object, int Amount)
	{
		DamageDieSizeAdjustedEvent damageDieSizeAdjustedEvent = FromPool();
		damageDieSizeAdjustedEvent.Object = Object;
		damageDieSizeAdjustedEvent.Amount = Amount;
		return damageDieSizeAdjustedEvent;
	}

	public override void Reset()
	{
		Object = null;
		Amount = 0;
		base.Reset();
	}

	public static void Send(GameObject Object, int Amount)
	{
		if (GameObject.validate(ref Object) && Object.HasRegisteredEvent("DamageDieSizeAdjusted"))
		{
			Event @event = Event.New("DamageDieSizeAdjusted");
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
