using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class PowerCordFinishChargeAvailableEvent : IPowerCordEvent
{
	public new static readonly int ID;

	private static List<PowerCordFinishChargeAvailableEvent> Pool;

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

	static PowerCordFinishChargeAvailableEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(PowerCordFinishChargeAvailableEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public PowerCordFinishChargeAvailableEvent()
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

	public static PowerCordFinishChargeAvailableEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static PowerCordFinishChargeAvailableEvent FromPool(GameObject Source, IChargeEvent Event)
	{
		PowerCordFinishChargeAvailableEvent powerCordFinishChargeAvailableEvent = FromPool();
		powerCordFinishChargeAvailableEvent.Source = Source;
		powerCordFinishChargeAvailableEvent.Event = Event;
		return powerCordFinishChargeAvailableEvent;
	}

	public static bool Send(GameObject Object, GameObject Source, IChargeEvent Event)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			PowerCordFinishChargeAvailableEvent e = FromPool(Source, Event);
			return Object.HandleEvent(e);
		}
		return true;
	}

	public static bool Send(List<Cell> Cells, GameObject Source, IChargeEvent Event)
	{
		PowerCordFinishChargeAvailableEvent powerCordFinishChargeAvailableEvent = null;
		int i = 0;
		for (int count = Cells.Count; i < count; i++)
		{
			Cell cell = Cells[i];
			int j = 0;
			for (int count2 = cell.Objects.Count; j < count2; j++)
			{
				if (cell.Objects[j].WantEvent(ID, MinEvent.CascadeLevel))
				{
					if (powerCordFinishChargeAvailableEvent == null)
					{
						powerCordFinishChargeAvailableEvent = FromPool(Source, Event);
					}
					if (!cell.Objects[j].HandleEvent(powerCordFinishChargeAvailableEvent))
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
