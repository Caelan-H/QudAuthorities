using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ModificationAppliedEvent : MinEvent
{
	public GameObject Object;

	public IModification Modification;

	public new static readonly int ID;

	private static List<ModificationAppliedEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static ModificationAppliedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ModificationAppliedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ModificationAppliedEvent()
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

	public static ModificationAppliedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ModificationAppliedEvent FromPool(GameObject Object, IModification Modification)
	{
		ModificationAppliedEvent modificationAppliedEvent = FromPool();
		modificationAppliedEvent.Object = Object;
		modificationAppliedEvent.Modification = Modification;
		return modificationAppliedEvent;
	}

	public static void Send(GameObject Object, IModification Modification)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object, Modification));
		}
		if (Object.HasRegisteredEvent("ModificationApplied"))
		{
			Object.FireEvent(Event.New("ModificationApplied", "Object", Object, "Modification", Modification));
		}
	}

	public override void Reset()
	{
		Object = null;
		Modification = null;
		base.Reset();
	}
}
