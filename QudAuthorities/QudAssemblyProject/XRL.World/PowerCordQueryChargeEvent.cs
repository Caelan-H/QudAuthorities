using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class PowerCordQueryChargeEvent : IPowerCordEvent
{
	public new static readonly int ID;

	private static List<PowerCordQueryChargeEvent> Pool;

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

	static PowerCordQueryChargeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(PowerCordQueryChargeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public PowerCordQueryChargeEvent()
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

	public static PowerCordQueryChargeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static PowerCordQueryChargeEvent FromPool(GameObject Source, IChargeEvent Event)
	{
		PowerCordQueryChargeEvent powerCordQueryChargeEvent = FromPool();
		powerCordQueryChargeEvent.Source = Source;
		powerCordQueryChargeEvent.Event = Event;
		return powerCordQueryChargeEvent;
	}

	public static bool Send(GameObject Object, GameObject Source, IChargeEvent Event)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			PowerCordQueryChargeEvent e = FromPool(Source, Event);
			return Object.HandleEvent(e);
		}
		return true;
	}

	public static bool Send(List<Cell> Cells, GameObject Source, IChargeEvent Event)
	{
		PowerCordQueryChargeEvent powerCordQueryChargeEvent = null;
		int i = 0;
		for (int count = Cells.Count; i < count; i++)
		{
			Cell cell = Cells[i];
			int j = 0;
			for (int count2 = cell.Objects.Count; j < count2; j++)
			{
				if (cell.Objects[j].WantEvent(ID, MinEvent.CascadeLevel))
				{
					if (powerCordQueryChargeEvent == null)
					{
						powerCordQueryChargeEvent = FromPool(Source, Event);
					}
					if (!cell.Objects[j].HandleEvent(powerCordQueryChargeEvent))
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
