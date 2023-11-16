using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CheckExistenceSupportEvent : MinEvent
{
	public GameObject SupportedBy;

	public GameObject Object;

	public new static readonly int ID;

	private static List<CheckExistenceSupportEvent> Pool;

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

	static CheckExistenceSupportEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CheckExistenceSupportEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CheckExistenceSupportEvent()
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

	public static CheckExistenceSupportEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CheckExistenceSupportEvent FromPool(GameObject SupportedBy, GameObject Object)
	{
		CheckExistenceSupportEvent checkExistenceSupportEvent = FromPool();
		checkExistenceSupportEvent.SupportedBy = SupportedBy;
		checkExistenceSupportEvent.Object = Object;
		return checkExistenceSupportEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		SupportedBy = null;
		Object = null;
		base.Reset();
	}

	public static bool Check(GameObject SupportedBy, GameObject Object)
	{
		if (GameObject.validate(ref SupportedBy) && !SupportedBy.IsInGraveyard() && SupportedBy.WantEvent(ID, CascadeLevel) && !SupportedBy.HandleEvent(FromPool(SupportedBy, Object)))
		{
			return true;
		}
		return false;
	}
}
