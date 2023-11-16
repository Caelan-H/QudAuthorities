using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IsConversationallyResponsiveEvent : MinEvent
{
	public GameObject Speaker;

	public GameObject Actor;

	public bool Physical;

	public bool Mental;

	public string Message;

	public new static readonly int ID;

	private static List<IsConversationallyResponsiveEvent> Pool;

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

	static IsConversationallyResponsiveEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(IsConversationallyResponsiveEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public IsConversationallyResponsiveEvent()
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
		Speaker = null;
		Actor = null;
		Physical = false;
		Mental = false;
		Message = null;
		base.Reset();
	}

	public static IsConversationallyResponsiveEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static bool Check(GameObject Speaker, GameObject Actor, out string Message, bool Physical = false, bool Mental = false)
	{
		Message = null;
		bool flag = true;
		if (flag && GameObject.validate(ref Speaker) && Speaker.HasRegisteredEvent("IsConversationallyResponsive"))
		{
			Event @event = Event.New("IsConversationallyResponsive");
			@event.SetParameter("Speaker", Speaker);
			@event.SetParameter("Actor", Actor);
			@event.SetFlag("Physical", Physical);
			@event.SetFlag("Mental", Mental);
			@event.SetParameter("Message", Message);
			flag = Speaker.FireEvent(@event);
			Message = @event.GetStringParameter("Message");
		}
		if (flag && GameObject.validate(ref Speaker) && Speaker.WantEvent(ID, CascadeLevel))
		{
			IsConversationallyResponsiveEvent isConversationallyResponsiveEvent = FromPool();
			isConversationallyResponsiveEvent.Speaker = Speaker;
			isConversationallyResponsiveEvent.Actor = Actor;
			isConversationallyResponsiveEvent.Physical = Physical;
			isConversationallyResponsiveEvent.Mental = Mental;
			isConversationallyResponsiveEvent.Message = Message;
			flag = Speaker.HandleEvent(isConversationallyResponsiveEvent);
			Message = isConversationallyResponsiveEvent.Message;
		}
		return flag;
	}

	public static bool Check(GameObject Speaker, GameObject Actor, bool Physical = false, bool Mental = false)
	{
		string Message;
		return Check(Speaker, Actor, out Message, Physical, Mental);
	}
}
