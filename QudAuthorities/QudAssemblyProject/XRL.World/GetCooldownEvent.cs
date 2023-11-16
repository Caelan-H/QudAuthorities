using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetCooldownEvent : MinEvent
{
	public GameObject Actor;

	public ActivatedAbilityEntry Ability;

	public int Base;

	public int PercentageReduction;

	public int LinearReduction;

	public new static readonly int ID;

	private static List<GetCooldownEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 1;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetCooldownEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetCooldownEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetCooldownEvent()
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

	public static GetCooldownEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetCooldownEvent FromPool(GameObject Actor, ActivatedAbilityEntry Ability, int Base, int PercentageReduction = 0, int LinearReduction = 0)
	{
		GetCooldownEvent getCooldownEvent = FromPool();
		getCooldownEvent.Actor = Actor;
		getCooldownEvent.Ability = Ability;
		getCooldownEvent.Base = Base;
		getCooldownEvent.PercentageReduction = PercentageReduction;
		getCooldownEvent.LinearReduction = LinearReduction;
		return getCooldownEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Actor = null;
		Ability = null;
		Base = 0;
		LinearReduction = 0;
		PercentageReduction = 0;
		base.Reset();
	}

	public static int GetFor(GameObject Actor, ActivatedAbilityEntry Ability, int Base, int PercentageReduction = 0, int LinearReduction = 0)
	{
		int val;
		if (Actor != null && Actor.WantEvent(ID, CascadeLevel))
		{
			GetCooldownEvent getCooldownEvent = FromPool(Actor, Ability, Base, PercentageReduction, LinearReduction);
			Actor.HandleEvent(getCooldownEvent);
			val = Base * (100 - getCooldownEvent.PercentageReduction) / 100 - getCooldownEvent.LinearReduction;
		}
		else
		{
			val = Base * (100 - PercentageReduction) / 100 - LinearReduction;
		}
		return Math.Max(val, ActivatedAbilities.MinimumValueForCooldown(Base));
	}
}
