using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ShouldDescribeStatBonusEvent : MinEvent
{
	public GameObject Object;

	public IComponent<GameObject> Component;

	public string Stat;

	public int Amount;

	public new static readonly int ID;

	private static List<ShouldDescribeStatBonusEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static ShouldDescribeStatBonusEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ShouldDescribeStatBonusEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ShouldDescribeStatBonusEvent()
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
		Object = null;
		base.Reset();
	}

	public static ShouldDescribeStatBonusEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static bool Check(GameObject Object, IComponent<GameObject> Component, string Stat, int Amount)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("ShouldDescribeStatBonus"))
		{
			Event @event = Event.New("ShouldDescribeStatBonus");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Component", Component);
			@event.SetParameter("Stat", Stat);
			@event.SetParameter("Amount", Amount);
			flag = Object.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			ShouldDescribeStatBonusEvent shouldDescribeStatBonusEvent = FromPool();
			shouldDescribeStatBonusEvent.Object = Object;
			shouldDescribeStatBonusEvent.Component = Component;
			shouldDescribeStatBonusEvent.Stat = Stat;
			shouldDescribeStatBonusEvent.Amount = Amount;
			flag = Object.HandleEvent(shouldDescribeStatBonusEvent);
		}
		return flag;
	}
}
