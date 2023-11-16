using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetLevelUpSkillPointsEvent : MinEvent
{
	public GameObject Actor;

	public int BaseAmount;

	public int Amount;

	public new static readonly int ID;

	private static List<GetLevelUpSkillPointsEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetLevelUpSkillPointsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetLevelUpSkillPointsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetLevelUpSkillPointsEvent()
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
		BaseAmount = 0;
		Amount = 0;
		base.Reset();
	}

	public static GetLevelUpSkillPointsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetLevelUpSkillPointsEvent FromPool(GameObject Actor, int BaseAmount, int Amount)
	{
		GetLevelUpSkillPointsEvent getLevelUpSkillPointsEvent = FromPool();
		getLevelUpSkillPointsEvent.Actor = Actor;
		getLevelUpSkillPointsEvent.BaseAmount = BaseAmount;
		getLevelUpSkillPointsEvent.Amount = Amount;
		return getLevelUpSkillPointsEvent;
	}

	public static int GetFor(GameObject Actor, int BaseAmount)
	{
		if (GameObject.validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetLevelUpSkillPointsEvent getLevelUpSkillPointsEvent = FromPool(Actor, BaseAmount, BaseAmount);
			Actor.HandleEvent(getLevelUpSkillPointsEvent);
			return getLevelUpSkillPointsEvent.Amount;
		}
		return BaseAmount;
	}
}
