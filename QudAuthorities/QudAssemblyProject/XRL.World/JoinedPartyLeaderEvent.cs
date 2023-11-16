using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class JoinedPartyLeaderEvent : MinEvent
{
	public GameObject Companion;

	public GameObject Leader;

	public Cell PreviousCell;

	public Cell TargetCell;

	public int DistanceFromPreviousCell;

	public int DistanceFromLeader;

	public new static readonly int ID;

	private static List<JoinedPartyLeaderEvent> Pool;

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

	static JoinedPartyLeaderEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(JoinedPartyLeaderEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public JoinedPartyLeaderEvent()
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
		PreviousCell = null;
		TargetCell = null;
		DistanceFromPreviousCell = 0;
		DistanceFromLeader = 0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static JoinedPartyLeaderEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(GameObject Companion, GameObject Leader, Cell PreviousCell, Cell TargetCell, int DistanceFromPreviousCell, int DistanceFromLeader)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Companion) && Companion.HasRegisteredEvent("JoinedPartyLeader"))
		{
			Event @event = Event.New("JoinedPartyLeader");
			@event.SetParameter("Companion", Companion);
			@event.SetParameter("Leader", Leader);
			@event.SetParameter("PreviousCell", PreviousCell);
			@event.SetParameter("TargetCell", TargetCell);
			@event.SetParameter("DistanceFromPreviousCell", DistanceFromPreviousCell);
			@event.SetParameter("DistanceFromLeader", DistanceFromLeader);
			flag = Companion.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Companion) && Companion.WantEvent(ID, CascadeLevel))
		{
			JoinedPartyLeaderEvent joinedPartyLeaderEvent = FromPool();
			joinedPartyLeaderEvent.Companion = Companion;
			joinedPartyLeaderEvent.Leader = Leader;
			joinedPartyLeaderEvent.PreviousCell = PreviousCell;
			joinedPartyLeaderEvent.TargetCell = TargetCell;
			joinedPartyLeaderEvent.DistanceFromPreviousCell = DistanceFromPreviousCell;
			joinedPartyLeaderEvent.DistanceFromLeader = DistanceFromLeader;
			flag = Companion.HandleEvent(joinedPartyLeaderEvent);
		}
	}
}
