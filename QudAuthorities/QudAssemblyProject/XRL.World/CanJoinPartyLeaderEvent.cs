using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CanJoinPartyLeaderEvent : MinEvent
{
	public GameObject Companion;

	public GameObject Leader;

	public Cell CurrentCell;

	public Cell TargetCell;

	public int DistanceFromCurrentCell;

	public int DistanceFromLeader;

	public new static readonly int ID;

	private static List<CanJoinPartyLeaderEvent> Pool;

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

	static CanJoinPartyLeaderEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CanJoinPartyLeaderEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CanJoinPartyLeaderEvent()
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
		Companion = null;
		Leader = null;
		CurrentCell = null;
		TargetCell = null;
		DistanceFromCurrentCell = 0;
		DistanceFromLeader = 0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static CanJoinPartyLeaderEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Companion, GameObject Leader, Cell CurrentCell, Cell TargetCell, int DistanceFromCurrentCell, int DistanceFromLeader)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Companion) && Companion.HasRegisteredEvent("CanJoinPartyLeader"))
		{
			Event @event = Event.New("CanJoinPartyLeader");
			@event.SetParameter("Companion", Companion);
			@event.SetParameter("Leader", Leader);
			@event.SetParameter("CurrentCell", CurrentCell);
			@event.SetParameter("TargetCell", TargetCell);
			@event.SetParameter("DistanceFromCurrentCell", DistanceFromCurrentCell);
			@event.SetParameter("DistanceFromLeader", DistanceFromLeader);
			flag = Companion.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Companion) && Companion.WantEvent(ID, CascadeLevel))
		{
			CanJoinPartyLeaderEvent canJoinPartyLeaderEvent = FromPool();
			canJoinPartyLeaderEvent.Companion = Companion;
			canJoinPartyLeaderEvent.Leader = Leader;
			canJoinPartyLeaderEvent.CurrentCell = CurrentCell;
			canJoinPartyLeaderEvent.TargetCell = TargetCell;
			canJoinPartyLeaderEvent.DistanceFromCurrentCell = DistanceFromCurrentCell;
			canJoinPartyLeaderEvent.DistanceFromLeader = DistanceFromLeader;
			flag = Companion.HandleEvent(canJoinPartyLeaderEvent);
		}
		return flag;
	}
}
