using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetSocialSifrahSetupEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Interlocutor;

	public string Type;

	public int Difficulty;

	public int Rating;

	public int Turns;

	public new static readonly int ID;

	private static List<GetSocialSifrahSetupEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 3;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetSocialSifrahSetupEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetSocialSifrahSetupEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetSocialSifrahSetupEvent()
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

	public static GetSocialSifrahSetupEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetSocialSifrahSetupEvent FromPool(GameObject Actor, GameObject Interlocutor, string Type, int Difficulty, int Rating, int Turns)
	{
		GetSocialSifrahSetupEvent getSocialSifrahSetupEvent = FromPool();
		getSocialSifrahSetupEvent.Actor = Actor;
		getSocialSifrahSetupEvent.Interlocutor = Interlocutor;
		getSocialSifrahSetupEvent.Type = Type;
		getSocialSifrahSetupEvent.Difficulty = Difficulty;
		getSocialSifrahSetupEvent.Rating = Rating;
		getSocialSifrahSetupEvent.Turns = Turns;
		return getSocialSifrahSetupEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Actor = null;
		Interlocutor = null;
		Type = null;
		Difficulty = 0;
		Rating = 0;
		Turns = 0;
		base.Reset();
	}

	public static bool GetFor(GameObject Actor, GameObject Interlocutor, string Type, ref int Difficulty, ref int Rating, ref int Turns)
	{
		bool result = true;
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			GetSocialSifrahSetupEvent getSocialSifrahSetupEvent = FromPool(Actor, Interlocutor, Type, Difficulty, Rating, Turns);
			result = Actor.HandleEvent(getSocialSifrahSetupEvent);
			Difficulty = getSocialSifrahSetupEvent.Difficulty;
			Rating = getSocialSifrahSetupEvent.Rating;
			Turns = getSocialSifrahSetupEvent.Turns;
		}
		return result;
	}
}
