using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetLevelUpDiceEvent : MinEvent
{
	public GameObject Actor;

	public int Level;

	public string BaseHPGain;

	public string BaseSPGain;

	public string BaseMPGain;

	public new static readonly int ID;

	private static List<GetLevelUpDiceEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetLevelUpDiceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount("GetLevelUpDiceEvent", () => (Pool != null) ? Pool.Count : 0);
	}

	public GetLevelUpDiceEvent()
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

	public static GetLevelUpDiceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Actor = null;
		Level = 0;
		BaseHPGain = null;
		BaseSPGain = null;
		BaseMPGain = null;
		base.Reset();
	}

	public static void GetFor(GameObject Actor, int Level, ref string BaseHPGain, ref string BaseSPGain, ref string BaseMPGain)
	{
		if (Actor.HasRegisteredEvent("GetLevelUpDice"))
		{
			Event @event = Event.New("GetLevelUpDice");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Level", Level);
			@event.SetParameter("BaseHPGain", BaseHPGain);
			@event.SetParameter("BaseSPGain", BaseSPGain);
			@event.SetParameter("BaseMPGain", BaseMPGain);
			bool num = Actor.FireEvent(@event);
			BaseHPGain = @event.GetStringParameter("BaseHPGain");
			BaseSPGain = @event.GetStringParameter("BaseSPGain");
			BaseMPGain = @event.GetStringParameter("BaseMPGain");
			if (!num)
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetLevelUpDiceEvent getLevelUpDiceEvent = FromPool();
			getLevelUpDiceEvent.Actor = Actor;
			getLevelUpDiceEvent.Level = Level;
			getLevelUpDiceEvent.BaseHPGain = BaseHPGain;
			getLevelUpDiceEvent.BaseSPGain = BaseSPGain;
			getLevelUpDiceEvent.BaseMPGain = BaseMPGain;
			Actor.HandleEvent(getLevelUpDiceEvent);
			BaseHPGain = getLevelUpDiceEvent.BaseHPGain;
			BaseSPGain = getLevelUpDiceEvent.BaseSPGain;
			BaseMPGain = getLevelUpDiceEvent.BaseMPGain;
		}
	}
}
