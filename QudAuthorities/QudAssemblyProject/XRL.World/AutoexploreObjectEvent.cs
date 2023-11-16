using System;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AutoexploreObjectEvent : IActOnItemEvent
{
	[NonSerialized]
	private static AutoexploreObjectEvent instance;

	public new static readonly int ID;

	public bool Want;

	public string FromAdjacent;

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

	static AutoexploreObjectEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public AutoexploreObjectEvent()
	{
		base.ID = ID;
	}

	public static bool Check(GameObject who, GameObject obj)
	{
		if (instance == null)
		{
			instance = new AutoexploreObjectEvent();
		}
		instance.Actor = who;
		instance.Item = obj;
		instance.Want = false;
		instance.FromAdjacent = null;
		if (who.HandleEvent(instance) && obj.WantEvent(ID, MinEvent.CascadeLevel))
		{
			obj.HandleEvent(instance);
		}
		return instance.Want;
	}

	public static bool CheckForAdjacent(GameObject who, GameObject obj)
	{
		if (instance == null)
		{
			instance = new AutoexploreObjectEvent();
		}
		instance.Actor = who;
		instance.Item = obj;
		instance.Want = false;
		instance.FromAdjacent = null;
		if (who.HandleEvent(instance) && obj.WantEvent(ID, MinEvent.CascadeLevel))
		{
			obj.HandleEvent(instance);
		}
		if (instance.Want)
		{
			return instance.FromAdjacent != null;
		}
		return false;
	}

	public static string GetAdjacentAction(GameObject who, GameObject obj)
	{
		if (instance == null)
		{
			instance = new AutoexploreObjectEvent();
		}
		instance.Actor = who;
		instance.Item = obj;
		instance.Want = false;
		instance.FromAdjacent = null;
		if (who.HandleEvent(instance) && obj.WantEvent(ID, MinEvent.CascadeLevel))
		{
			obj.HandleEvent(instance);
		}
		if (!instance.Want)
		{
			return null;
		}
		return instance.FromAdjacent;
	}
}
