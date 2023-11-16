using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class JoinPartyLeaderPossibleEvent : MinEvent
{
	public GameObject Companion;

	public GameObject Leader;

	public Cell CurrentCell;

	public Cell TargetCell;

	public bool IsMobile;

	public bool Result;

	public new static readonly int ID;

	private static List<JoinPartyLeaderPossibleEvent> Pool;

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

	static JoinPartyLeaderPossibleEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount("JoinPartyLeaderPossibleEvent", () => (Pool != null) ? Pool.Count : 0);
	}

	public JoinPartyLeaderPossibleEvent()
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
		IsMobile = false;
		Result = false;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static JoinPartyLeaderPossibleEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Companion, GameObject Leader, Cell CurrentCell, ref Cell TargetCell, bool IsMobile)
	{
		if (!GameObject.validate(ref Companion))
		{
			return false;
		}
		bool flag = IsMobile;
		if (Companion.HasRegisteredEvent("JoinPartyLeaderPossible"))
		{
			Event @event = Event.New("JoinPartyLeaderPossible");
			@event.SetParameter("Companion", Companion);
			@event.SetParameter("Leader", Leader);
			@event.SetParameter("CurrentCell", CurrentCell);
			@event.SetParameter("TargetCell", TargetCell);
			@event.SetFlag("IsMobile", IsMobile);
			@event.SetFlag("Result", flag);
			bool num = Companion.FireEvent(@event);
			TargetCell = @event.GetParameter("TargetCell") as Cell;
			flag = @event.HasFlag("Result");
			if (!num)
			{
				return flag;
			}
		}
		if (Companion.WantEvent(ID, CascadeLevel))
		{
			JoinPartyLeaderPossibleEvent joinPartyLeaderPossibleEvent = FromPool();
			joinPartyLeaderPossibleEvent.Companion = Companion;
			joinPartyLeaderPossibleEvent.Leader = Leader;
			joinPartyLeaderPossibleEvent.CurrentCell = CurrentCell;
			joinPartyLeaderPossibleEvent.TargetCell = TargetCell;
			joinPartyLeaderPossibleEvent.IsMobile = IsMobile;
			joinPartyLeaderPossibleEvent.Result = flag;
			Companion.HandleEvent(joinPartyLeaderPossibleEvent);
			TargetCell = joinPartyLeaderPossibleEvent.TargetCell;
			flag = joinPartyLeaderPossibleEvent.Result;
		}
		return flag;
	}
}
