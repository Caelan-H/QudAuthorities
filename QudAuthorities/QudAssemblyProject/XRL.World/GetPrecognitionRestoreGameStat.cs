using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetPrecognitionRestoreGameStateEvent : MinEvent
{
	public GameObject Object;

	public Dictionary<string, object> GameState;

	public new static readonly int ID;

	private static List<GetPrecognitionRestoreGameStateEvent> Pool;

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

	static GetPrecognitionRestoreGameStateEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetPrecognitionRestoreGameStateEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetPrecognitionRestoreGameStateEvent()
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
		GameState = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static GetPrecognitionRestoreGameStateEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public void Set(string Key, object Value)
	{
		if (GameState == null)
		{
			GameState = new Dictionary<string, object>();
		}
		GameState[Key] = Value;
	}

	public static Dictionary<string, object> GetFor(GameObject Object)
	{
		bool flag = true;
		Dictionary<string, object> dictionary = null;
		if (flag && GameObject.validate(ref Object) && Object.HasRegisteredEvent("GetPrecognitionRestoreGameState"))
		{
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, object>();
			}
			Event @event = Event.New("GetPrecognitionRestoreGameState");
			@event.SetParameter("Object", Object);
			@event.SetParameter("GameState", dictionary);
			flag = Object.FireEvent(@event);
			dictionary = @event.GetParameter("GameState") as Dictionary<string, object>;
		}
		if (flag && GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			GetPrecognitionRestoreGameStateEvent getPrecognitionRestoreGameStateEvent = FromPool();
			getPrecognitionRestoreGameStateEvent.Object = Object;
			getPrecognitionRestoreGameStateEvent.GameState = dictionary;
			flag = Object.HandleEvent(getPrecognitionRestoreGameStateEvent);
			dictionary = getPrecognitionRestoreGameStateEvent.GameState;
		}
		return dictionary;
	}
}
