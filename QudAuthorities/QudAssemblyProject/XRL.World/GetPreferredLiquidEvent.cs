using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetPreferredLiquidEvent : ILiquidEvent
{
	public GameObject Object;

	public new static readonly int ID;

	private static List<GetPreferredLiquidEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 0;

	public override bool handlePartDispatch(IPart Part)
	{
		if (!base.handlePartDispatch(Part))
		{
			return false;
		}
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		if (!base.handleEffectDispatch(Effect))
		{
			return false;
		}
		return Effect.HandleEvent(this);
	}

	static GetPreferredLiquidEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetPreferredLiquidEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetPreferredLiquidEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public new void Reset()
	{
		Object = null;
		base.Reset();
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static GetPreferredLiquidEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static string GetFor(GameObject Object, GameObject Actor)
	{
		if (!Object.Understood())
		{
			return null;
		}
		if (Object.WantEvent(ID, CascadeLevel))
		{
			GetPreferredLiquidEvent getPreferredLiquidEvent = FromPool();
			getPreferredLiquidEvent.Object = Object;
			getPreferredLiquidEvent.Actor = Actor;
			getPreferredLiquidEvent.Liquid = null;
			getPreferredLiquidEvent.LiquidVolume = null;
			getPreferredLiquidEvent.Drams = 0;
			getPreferredLiquidEvent.Skip = null;
			getPreferredLiquidEvent.SkipList = null;
			getPreferredLiquidEvent.Filter = null;
			getPreferredLiquidEvent.Auto = false;
			getPreferredLiquidEvent.ImpureOkay = false;
			getPreferredLiquidEvent.SafeOnly = false;
			if (Object.HandleEvent(getPreferredLiquidEvent))
			{
				return getPreferredLiquidEvent.Liquid;
			}
		}
		return null;
	}
}
