using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetProjectileBlueprintEvent : MinEvent
{
	public GameObject Object;

	public string Blueprint;

	public new static readonly int ID;

	private static List<GetProjectileBlueprintEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetProjectileBlueprintEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetProjectileBlueprintEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetProjectileBlueprintEvent()
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
		Blueprint = null;
		base.Reset();
	}

	public static GetProjectileBlueprintEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static string GetFor(GameObject Object)
	{
		bool flag = true;
		string text = null;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("GetProjectileBlueprint"))
		{
			Event @event = Event.New("GetProjectileBlueprint");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Blueprint", text);
			flag = Object.FireEvent(@event);
			text = @event.GetStringParameter("Blueprint");
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetProjectileBlueprintEvent getProjectileBlueprintEvent = FromPool();
			getProjectileBlueprintEvent.Object = Object;
			getProjectileBlueprintEvent.Blueprint = text;
			flag = Object.HandleEvent(getProjectileBlueprintEvent);
			text = getProjectileBlueprintEvent.Blueprint;
		}
		return text;
	}
}
