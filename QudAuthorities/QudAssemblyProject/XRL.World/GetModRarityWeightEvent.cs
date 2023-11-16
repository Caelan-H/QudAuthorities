using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetModRarityWeightEvent : MinEvent
{
	public GameObject Object;

	public ModEntry Mod;

	public int BaseWeight;

	public int LinearAdjustment;

	public double FactorAdjustment = 1.0;

	public new static readonly int ID;

	private static List<GetModRarityWeightEvent> Pool;

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

	static GetModRarityWeightEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetModRarityWeightEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetModRarityWeightEvent()
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
		Object = null;
		Mod = null;
		BaseWeight = 0;
		LinearAdjustment = 0;
		FactorAdjustment = 1.0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static GetModRarityWeightEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static int GetFor(GameObject Object, ModEntry Mod, int BaseWeight)
	{
		bool flag = true;
		int num = 0;
		double num2 = 1.0;
		if (flag)
		{
			bool flag2 = GameObject.validate(ref Object) && Object.HasRegisteredEvent("GetModRarityWeight");
			bool flag3 = The.Player != null && The.Player.HasRegisteredEvent("GetModRarityWeight");
			if (flag2 || flag3)
			{
				Event @event = Event.New("GetModRarityWeight");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Mod", Mod);
				@event.SetParameter("BaseWeight", BaseWeight);
				@event.SetParameter("LinearAdjustment", num);
				@event.SetParameter("FactorAdjustment", num2);
				if (flag && flag2 && GameObject.validate(ref Object))
				{
					flag = Object.FireEvent(@event);
				}
				if (flag && flag3 && The.Player != null)
				{
					flag = The.Player.FireEvent(@event);
				}
				num = @event.GetIntParameter("LinearAdjustment");
				num2 = (double)@event.GetParameter("FactorAdjustment");
			}
		}
		if (flag)
		{
			bool flag4 = GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel);
			bool flag5 = The.Player != null && The.Player.WantEvent(ID, CascadeLevel);
			if (flag4 || flag5)
			{
				GetModRarityWeightEvent getModRarityWeightEvent = FromPool();
				getModRarityWeightEvent.Object = Object;
				getModRarityWeightEvent.Mod = Mod;
				getModRarityWeightEvent.BaseWeight = BaseWeight;
				getModRarityWeightEvent.LinearAdjustment = num;
				getModRarityWeightEvent.FactorAdjustment = num2;
				if (flag && flag4 && GameObject.validate(ref Object))
				{
					flag = Object.HandleEvent(getModRarityWeightEvent);
				}
				if (flag && flag5 && The.Player != null)
				{
					flag = The.Player.HandleEvent(getModRarityWeightEvent);
				}
				num = getModRarityWeightEvent.LinearAdjustment;
				num2 = getModRarityWeightEvent.FactorAdjustment;
			}
		}
		return (int)Math.Round((double)(BaseWeight + num) * num2);
	}
}
