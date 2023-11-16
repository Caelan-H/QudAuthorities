using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetUtilityScoreEvent : IActOnItemEvent
{
	public Damage Damage;

	public bool ForPermission;

	public int Score;

	public new static readonly int ID;

	private static List<GetUtilityScoreEvent> Pool;

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

	static GetUtilityScoreEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetUtilityScoreEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetUtilityScoreEvent()
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

	public static GetUtilityScoreEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetUtilityScoreEvent FromPool(GameObject Actor, GameObject Item, Damage Damage = null, bool ForPermission = false)
	{
		GetUtilityScoreEvent getUtilityScoreEvent = FromPool();
		getUtilityScoreEvent.Actor = Actor;
		getUtilityScoreEvent.Item = Item;
		getUtilityScoreEvent.Damage = Damage;
		getUtilityScoreEvent.ForPermission = ForPermission;
		getUtilityScoreEvent.Score = 0;
		return getUtilityScoreEvent;
	}

	public override void Reset()
	{
		Damage = null;
		Score = 0;
		base.Reset();
	}

	public void ApplyScore(int Score)
	{
		if (this.Score < Score)
		{
			this.Score = Score;
		}
	}

	public static int GetFor(GameObject Actor, GameObject Item, Damage Damage = null, bool ForPermission = false)
	{
		if (Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetUtilityScoreEvent getUtilityScoreEvent = FromPool(Actor, Item, Damage, ForPermission);
			Item.HandleEvent(getUtilityScoreEvent);
			return getUtilityScoreEvent.Score;
		}
		return 0;
	}
}
