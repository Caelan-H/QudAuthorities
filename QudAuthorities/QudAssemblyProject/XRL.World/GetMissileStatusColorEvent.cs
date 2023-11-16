using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetMissileStatusColorEvent : MinEvent
{
	public GameObject Object;

	public string Color;

	public new static readonly int ID;

	private static List<GetMissileStatusColorEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetMissileStatusColorEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetMissileStatusColorEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetMissileStatusColorEvent()
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
		Color = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return MinEvent.CascadeLevel;
	}

	public static GetMissileStatusColorEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetMissileStatusColorEvent FromPool(GameObject Object, string Color = null)
	{
		GetMissileStatusColorEvent getMissileStatusColorEvent = FromPool();
		getMissileStatusColorEvent.Object = Object;
		getMissileStatusColorEvent.Color = Color;
		return getMissileStatusColorEvent;
	}

	public static string GetFor(GameObject Object, string Color = null)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("GetMissileStatusColor"))
		{
			Event @event = Event.New("GetMissileStatusColor");
			@event.SetParameter("Object", Object);
			@event.SetParameter("Color", Color);
			flag = Object.FireEvent(@event);
			Color = @event.GetStringParameter("Color");
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetMissileStatusColorEvent getMissileStatusColorEvent = FromPool(Object, Color);
			flag = Object.HandleEvent(getMissileStatusColorEvent);
			Color = getMissileStatusColorEvent.Color;
		}
		return Color ?? Object.DisplayNameColor;
	}
}
