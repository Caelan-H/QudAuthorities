using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class PowerCordUseChargeEvent : IPowerCordEvent
{
	public new static readonly int ID;

	private static List<PowerCordUseChargeEvent> Pool;

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

	static PowerCordUseChargeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(PowerCordUseChargeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public PowerCordUseChargeEvent()
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

	public static PowerCordUseChargeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static PowerCordUseChargeEvent FromPool(GameObject Source, IChargeEvent Event)
	{
		PowerCordUseChargeEvent powerCordUseChargeEvent = FromPool();
		powerCordUseChargeEvent.Source = Source;
		powerCordUseChargeEvent.Event = Event;
		return powerCordUseChargeEvent;
	}

	public static bool Send(GameObject Object, GameObject Source, IChargeEvent Event)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			PowerCordUseChargeEvent e = FromPool(Source, Event);
			return Object.HandleEvent(e);
		}
		return true;
	}

	public static bool Send(List<Cell> Cells, GameObject Source, IChargeEvent Event)
	{
		PowerCordUseChargeEvent powerCordUseChargeEvent = null;
		int i = 0;
		for (int count = Cells.Count; i < count; i++)
		{
			Cell cell = Cells[i];
			int j = 0;
			for (int count2 = cell.Objects.Count; j < count2; j++)
			{
				if (cell.Objects[j].WantEvent(ID, MinEvent.CascadeLevel))
				{
					if (powerCordUseChargeEvent == null)
					{
						powerCordUseChargeEvent = FromPool(Source, Event);
					}
					if (!cell.Objects[j].HandleEvent(powerCordUseChargeEvent))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public static bool Send(IActivePart Part, GameObject Source, IChargeEvent Event)
	{
		if (Part.AnyActivePartSubjectWantsEvent(ID, MinEvent.CascadeLevel))
		{
			return Part.ActivePartSubjectsHandleEvent(FromPool(Source, Event));
		}
		return true;
	}
}
