using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CheckAnythingToCleanEvent : MinEvent
{
	public GameObject CascadeFrom;

	public GameObject Using;

	public new static readonly int ID;

	private static List<CheckAnythingToCleanEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CheckAnythingToCleanEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CheckAnythingToCleanEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CheckAnythingToCleanEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		CascadeFrom = null;
		Using = null;
		base.Reset();
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static CheckAnythingToCleanEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CheckAnythingToCleanEvent FromPool(GameObject CascadeFrom, GameObject Using = null)
	{
		CheckAnythingToCleanEvent checkAnythingToCleanEvent = FromPool();
		checkAnythingToCleanEvent.CascadeFrom = CascadeFrom;
		checkAnythingToCleanEvent.Using = Using;
		return checkAnythingToCleanEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static bool Check(GameObject CascadeFrom, GameObject Using = null)
	{
		if (GameObject.validate(ref CascadeFrom) && CascadeFrom.WantEvent(ID, CascadeLevel) && !CascadeFrom.HandleEvent(FromPool(CascadeFrom, Using)))
		{
			return true;
		}
		return false;
	}
}
