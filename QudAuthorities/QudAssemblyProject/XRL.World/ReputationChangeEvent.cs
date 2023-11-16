using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ReputationChangeEvent : MinEvent
{
	public Faction Faction;

	public int BaseAmount;

	public int Amount;

	public bool Silent;

	public bool Transient;

	public new static readonly int ID;

	private static List<ReputationChangeEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static ReputationChangeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ReputationChangeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ReputationChangeEvent()
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
		Faction = null;
		BaseAmount = 0;
		Amount = 0;
		Silent = false;
		Transient = false;
		base.Reset();
	}

	public static ReputationChangeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ReputationChangeEvent FromPool(Faction Faction, int BaseAmount, int Amount, bool Silent, bool Transient)
	{
		ReputationChangeEvent reputationChangeEvent = FromPool();
		reputationChangeEvent.Faction = Faction;
		reputationChangeEvent.BaseAmount = BaseAmount;
		reputationChangeEvent.Amount = Amount;
		reputationChangeEvent.Silent = Silent;
		reputationChangeEvent.Transient = Transient;
		return reputationChangeEvent;
	}

	public static int GetFor(Faction Faction, int BaseAmount, bool Silent = false, bool Transient = false)
	{
		if (The.Player != null && The.Player.WantEvent(ID, MinEvent.CascadeLevel))
		{
			ReputationChangeEvent reputationChangeEvent = FromPool(Faction, BaseAmount, BaseAmount, Silent, Transient);
			The.Player.HandleEvent(reputationChangeEvent);
			return reputationChangeEvent.Amount;
		}
		return BaseAmount;
	}
}
