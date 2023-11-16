using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CheckAttackableEvent : MinEvent
{
	public GameObject Object;

	public GameObject Attacker;

	public new static readonly int ID;

	private static List<CheckAttackableEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CheckAttackableEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CheckAttackableEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CheckAttackableEvent()
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

	public static CheckAttackableEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CheckAttackableEvent FromPool(GameObject Object, GameObject Attacker)
	{
		CheckAttackableEvent checkAttackableEvent = FromPool();
		checkAttackableEvent.Object = Object;
		checkAttackableEvent.Attacker = Attacker;
		return checkAttackableEvent;
	}

	public override void Reset()
	{
		Object = null;
		Attacker = null;
		base.Reset();
	}

	public static bool Check(GameObject Object, GameObject Attacker)
	{
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object, Attacker)))
		{
			return false;
		}
		return true;
	}
}
