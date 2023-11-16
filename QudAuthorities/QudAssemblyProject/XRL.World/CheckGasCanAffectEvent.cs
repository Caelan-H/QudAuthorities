using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CheckGasCanAffectEvent : MinEvent
{
	public GameObject Object;

	public GameObject GasObject;

	public Gas Gas;

	public new static readonly int ID;

	private static List<CheckGasCanAffectEvent> Pool;

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

	static CheckGasCanAffectEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(CheckGasCanAffectEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public CheckGasCanAffectEvent()
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

	public static CheckGasCanAffectEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static CheckGasCanAffectEvent FromPool(GameObject Object, GameObject GasObject, Gas Gas = null)
	{
		CheckGasCanAffectEvent checkGasCanAffectEvent = FromPool();
		checkGasCanAffectEvent.Object = Object;
		checkGasCanAffectEvent.GasObject = GasObject;
		checkGasCanAffectEvent.Gas = Gas ?? (checkGasCanAffectEvent.GasObject.GetPart("Gas") as Gas);
		return checkGasCanAffectEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Object = null;
		GasObject = null;
		Gas = null;
		base.Reset();
	}

	public static bool Check(GameObject Object, GameObject GasObject, Gas Gas = null)
	{
		if (GameObject.validate(ref Object) && GameObject.validate(ref GasObject) && !Object.HandleEvent(FromPool(Object, GasObject, Gas)))
		{
			return false;
		}
		return true;
	}
}
