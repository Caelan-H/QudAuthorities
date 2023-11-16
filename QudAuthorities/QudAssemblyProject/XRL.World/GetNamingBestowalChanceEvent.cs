using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetNamingBestowalChanceEvent : IActOnItemEvent
{
	public int Base;

	public int PercentageBonus;

	public int LinearBonus;

	public new static readonly int ID;

	private static List<GetNamingBestowalChanceEvent> Pool;

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

	static GetNamingBestowalChanceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetNamingBestowalChanceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetNamingBestowalChanceEvent()
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

	public static GetNamingBestowalChanceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetNamingBestowalChanceEvent FromPool(GameObject Actor, GameObject Item, int Base, int PercentageBonus = 0, int LinearBonus = 0)
	{
		GetNamingBestowalChanceEvent getNamingBestowalChanceEvent = FromPool();
		getNamingBestowalChanceEvent.Actor = Actor;
		getNamingBestowalChanceEvent.Item = Item;
		getNamingBestowalChanceEvent.Base = Base;
		getNamingBestowalChanceEvent.PercentageBonus = PercentageBonus;
		getNamingBestowalChanceEvent.LinearBonus = LinearBonus;
		return getNamingBestowalChanceEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Base = 0;
		LinearBonus = 0;
		PercentageBonus = 0;
		base.Reset();
	}

	public static int GetFor(GameObject Actor, GameObject Item, int Base, int PercentageBonus = 0, int LinearBonus = 0)
	{
		bool flag = Actor?.WantEvent(ID, CascadeLevel) ?? false;
		bool flag2 = Item?.WantEvent(ID, CascadeLevel) ?? false;
		if (flag || flag2)
		{
			GetNamingBestowalChanceEvent getNamingBestowalChanceEvent = FromPool(Actor, Item, Base, PercentageBonus, LinearBonus);
			bool flag3 = true;
			if (flag)
			{
				flag3 = Actor.HandleEvent(getNamingBestowalChanceEvent);
			}
			if (flag3 && flag2)
			{
				flag3 = Item.HandleEvent(getNamingBestowalChanceEvent);
			}
			return Base * (100 + getNamingBestowalChanceEvent.PercentageBonus) / 100 + getNamingBestowalChanceEvent.LinearBonus;
		}
		return Base * (100 + PercentageBonus) / 100 + LinearBonus;
	}
}
