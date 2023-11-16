using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class StatChangeEvent : MinEvent
{
	public GameObject Object;

	public string Name;

	public string Type;

	public int OldValue;

	public int NewValue;

	public int OldBaseValue;

	public int NewBaseValue;

	public Statistic Stat;

	public new static readonly int ID;

	private static List<StatChangeEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static StatChangeEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(StatChangeEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public StatChangeEvent()
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

	public static StatChangeEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static StatChangeEvent FromPool(GameObject Object, string Name = null, string Type = null, int OldValue = 0, int NewValue = 0, int OldBaseValue = 0, int NewBaseValue = 0, Statistic Stat = null)
	{
		StatChangeEvent statChangeEvent = FromPool();
		statChangeEvent.Object = Object;
		statChangeEvent.Name = Name;
		statChangeEvent.Type = Type;
		statChangeEvent.OldValue = OldValue;
		statChangeEvent.NewValue = NewValue;
		statChangeEvent.OldBaseValue = OldBaseValue;
		statChangeEvent.NewBaseValue = NewBaseValue;
		statChangeEvent.Stat = Stat;
		return statChangeEvent;
	}

	public override void Reset()
	{
		Object = null;
		Name = null;
		Type = null;
		OldValue = 0;
		NewValue = 0;
		OldBaseValue = 0;
		NewBaseValue = 0;
		Stat = null;
		base.Reset();
	}
}
