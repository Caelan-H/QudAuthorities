using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetNamingChanceEvent : IActOnItemEvent
{
	public double Base;

	public double PercentageBonus;

	public double LinearBonus;

	public new static readonly int ID;

	private static List<GetNamingChanceEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 1;

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

	static GetNamingChanceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetNamingChanceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetNamingChanceEvent()
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

	public static GetNamingChanceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetNamingChanceEvent FromPool(GameObject Actor, GameObject Item, double Base, double PercentageBonus = 0.0, double LinearBonus = 0.0)
	{
		GetNamingChanceEvent getNamingChanceEvent = FromPool();
		getNamingChanceEvent.Actor = Actor;
		getNamingChanceEvent.Item = Item;
		getNamingChanceEvent.Base = Base;
		getNamingChanceEvent.PercentageBonus = PercentageBonus;
		getNamingChanceEvent.LinearBonus = LinearBonus;
		return getNamingChanceEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Base = 0.0;
		LinearBonus = 0.0;
		PercentageBonus = 0.0;
		base.Reset();
	}

	public static double GetFor(GameObject Actor, GameObject Item, double Base, double PercentageBonus = 0.0, double LinearBonus = 0.0)
	{
		bool flag = Actor?.WantEvent(ID, CascadeLevel) ?? false;
		bool flag2 = Item?.WantEvent(ID, CascadeLevel) ?? false;
		if (flag || flag2)
		{
			GetNamingChanceEvent getNamingChanceEvent = FromPool(Actor, Item, Base, PercentageBonus, LinearBonus);
			bool flag3 = true;
			if (flag)
			{
				flag3 = Actor.HandleEvent(getNamingChanceEvent);
			}
			if (flag3 && flag2)
			{
				flag3 = Item.HandleEvent(getNamingChanceEvent);
			}
			return Base * (100.0 + getNamingChanceEvent.PercentageBonus) / 100.0 + getNamingChanceEvent.LinearBonus;
		}
		return Base * (100.0 + PercentageBonus) / 100.0 + LinearBonus;
	}
}
