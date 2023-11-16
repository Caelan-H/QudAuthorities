using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class LiquidMixedEvent : MinEvent
{
	public LiquidVolume Liquid;

	public new static readonly int ID;

	private static List<LiquidMixedEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static LiquidMixedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(LiquidMixedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public LiquidMixedEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Liquid = null;
		base.Reset();
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static LiquidMixedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static LiquidMixedEvent FromPool(LiquidVolume Liquid)
	{
		LiquidMixedEvent liquidMixedEvent = FromPool();
		liquidMixedEvent.Liquid = Liquid;
		return liquidMixedEvent;
	}

	public static void Send(LiquidVolume Liquid)
	{
		if (Liquid.ParentObject != null)
		{
			if (Liquid.ParentObject.HasRegisteredEvent("LiquidMixed"))
			{
				Liquid.ParentObject.FireEvent(Event.New("LiquidMixed", "Volume", Liquid));
			}
			if (Liquid.ParentObject.WantEvent(ID, MinEvent.CascadeLevel))
			{
				Liquid.ParentObject.HandleEvent(FromPool(Liquid));
			}
		}
	}
}
