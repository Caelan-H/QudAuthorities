using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AwardXPEvent : IXPEvent
{
	public new static readonly int ID;

	private static List<AwardXPEvent> Pool;

	private static int PoolCounter;

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

	static AwardXPEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(AwardXPEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public AwardXPEvent()
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

	public static AwardXPEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static AwardXPEvent FromPool(GameObject Actor, int Amount, int AmountBefore, int Tier = -1, int Minimum = 0, int Maximum = int.MaxValue, GameObject Kill = null, GameObject InfluencedBy = null, GameObject PassedUpFrom = null, GameObject PassedDownFrom = null, string Deed = null)
	{
		AwardXPEvent awardXPEvent = FromPool();
		awardXPEvent.Actor = Actor;
		awardXPEvent.Amount = Amount;
		awardXPEvent.Tier = Tier;
		awardXPEvent.Minimum = Minimum;
		awardXPEvent.Maximum = Maximum;
		awardXPEvent.Kill = Kill;
		awardXPEvent.InfluencedBy = InfluencedBy;
		awardXPEvent.PassedUpFrom = PassedUpFrom;
		awardXPEvent.PassedDownFrom = PassedDownFrom;
		awardXPEvent.Deed = Deed;
		return awardXPEvent;
	}

	public static int Send(GameObject Actor, int Amount, int Tier = -1, int Minimum = 0, int Maximum = int.MaxValue, GameObject Kill = null, GameObject InfluencedBy = null, GameObject PassedUpFrom = null, GameObject PassedDownFrom = null, string Deed = null)
	{
		for (int i = 0; i < (The.Game?.Systems?.Count).GetValueOrDefault(); i++)
		{
			if (!The.Game.Systems[i].AwardingXP(ref Actor, ref Amount, ref Tier, ref Minimum, ref Maximum, ref Kill, ref InfluencedBy, ref PassedUpFrom, ref PassedDownFrom, ref Deed))
			{
				return 0;
			}
		}
		if (GameObject.validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AwardXPEvent awardXPEvent = FromPool(Actor, Amount, Actor.Stat("XP"), Tier, Minimum, Maximum, Kill, InfluencedBy, PassedUpFrom, PassedDownFrom, Deed);
			Actor.HandleEvent(awardXPEvent);
			Amount = awardXPEvent.Amount;
		}
		return Amount;
	}
}
