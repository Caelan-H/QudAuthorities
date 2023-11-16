using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetContextEvent : MinEvent
{
	public GameObject Object;

	public GameObject ObjectContext;

	public Cell CellContext;

	public int Relation;

	public IPart RelationManager;

	public new static readonly int ID;

	private static List<GetContextEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetContextEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetContextEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetContextEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Object = null;
		ObjectContext = null;
		CellContext = null;
		Relation = 0;
		RelationManager = null;
		base.Reset();
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static GetContextEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetContextEvent FromPool(GameObject Object)
	{
		GetContextEvent getContextEvent = FromPool();
		getContextEvent.Object = Object;
		getContextEvent.ObjectContext = null;
		getContextEvent.CellContext = null;
		getContextEvent.Relation = 0;
		getContextEvent.RelationManager = null;
		return getContextEvent;
	}

	public static void Get(GameObject Object, out GameObject ObjectContext, out Cell CellContext, out int Relation, out IPart RelationManager)
	{
		if (GameObject.validate(ref Object))
		{
			GetContextEvent getContextEvent = FromPool(Object);
			Object.HandleEvent(getContextEvent);
			ObjectContext = getContextEvent.ObjectContext;
			CellContext = getContextEvent.CellContext;
			Relation = getContextEvent.Relation;
			RelationManager = getContextEvent.RelationManager;
		}
		else
		{
			ObjectContext = null;
			CellContext = null;
			Relation = 0;
			RelationManager = null;
		}
	}

	public static void Get(GameObject Object, out GameObject ObjectContext, out Cell CellContext, out int Relation)
	{
		Get(Object, out ObjectContext, out CellContext, out Relation, out var _);
	}

	public static void Get(GameObject Object, out GameObject ObjectContext, out Cell CellContext)
	{
		Get(Object, out ObjectContext, out CellContext, out var _);
	}

	public static bool HasAny(GameObject Object)
	{
		Get(Object, out var ObjectContext, out var CellContext);
		if (ObjectContext == null)
		{
			return CellContext != null;
		}
		return true;
	}
}
