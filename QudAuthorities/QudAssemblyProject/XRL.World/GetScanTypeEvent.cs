using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Capabilities;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetScanTypeEvent : MinEvent
{
	public GameObject Object;

	public Scanning.Scan ScanType;

	public new static readonly int ID;

	private static List<GetScanTypeEvent> Pool;

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

	static GetScanTypeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetScanTypeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetScanTypeEvent()
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

	public static GetScanTypeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetScanTypeEvent FromPool(GameObject Object, Scanning.Scan ScanType)
	{
		GetScanTypeEvent getScanTypeEvent = FromPool();
		getScanTypeEvent.Object = Object;
		getScanTypeEvent.ScanType = ScanType;
		return getScanTypeEvent;
	}

	public override void Reset()
	{
		Object = null;
		ScanType = Scanning.Scan.Structure;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static Scanning.Scan GetFor(GameObject Object)
	{
		Scanning.Scan scan = ((!Object.IsAlive) ? Scanning.Scan.Structure : Scanning.Scan.Bio);
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("GetScanType"))
		{
			Event @event = Event.New("GetScanType");
			@event.SetParameter("Object", Object);
			@event.SetParameter("ScanType", scan);
			flag = Object.FireEvent(@event);
			try
			{
				scan = (Scanning.Scan)@event.GetParameter("ScanType");
			}
			catch (Exception)
			{
			}
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			GetScanTypeEvent getScanTypeEvent = FromPool(Object, scan);
			Object.HandleEvent(getScanTypeEvent);
			scan = getScanTypeEvent.ScanType;
		}
		return scan;
	}
}
