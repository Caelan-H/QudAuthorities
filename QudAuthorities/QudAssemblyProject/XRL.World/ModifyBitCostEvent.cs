using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Tinkering;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ModifyBitCostEvent : MinEvent
{
	public GameObject Actor;

	public BitCost Bits;

	public string Context;

	public new static readonly int ID;

	private static List<ModifyBitCostEvent> Pool;

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

	static ModifyBitCostEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ModifyBitCostEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ModifyBitCostEvent()
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
		Bits = null;
		Context = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static ModifyBitCostEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ModifyBitCostEvent FromPool(GameObject Actor, BitCost Bits, string Context)
	{
		ModifyBitCostEvent modifyBitCostEvent = FromPool();
		modifyBitCostEvent.Actor = Actor;
		modifyBitCostEvent.Bits = Bits;
		modifyBitCostEvent.Context = Context;
		return modifyBitCostEvent;
	}

	public static bool Process(GameObject Actor, BitCost Bits, string Context)
	{
		if (GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("ModifyBitCost"))
		{
			Event @event = Event.New("ModifyBitCost");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Bits", Bits);
			@event.SetParameter("Context", Context);
			if (!Actor.FireEvent(@event))
			{
				return false;
			}
		}
		if (GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			ModifyBitCostEvent e = FromPool(Actor, Bits, Context);
			if (!Actor.HandleEvent(e))
			{
				return false;
			}
		}
		return true;
	}
}
