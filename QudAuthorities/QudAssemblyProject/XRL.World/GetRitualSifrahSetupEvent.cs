using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetRitualSifrahSetupEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Subject;

	public string Type;

	public bool Interruptable;

	public int Difficulty;

	public int Rating;

	public int Turns;

	public bool PsychometryApplied;

	public new static readonly int ID;

	private static List<GetRitualSifrahSetupEvent> Pool;

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

	static GetRitualSifrahSetupEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetRitualSifrahSetupEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetRitualSifrahSetupEvent()
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

	public static GetRitualSifrahSetupEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Actor = null;
		Subject = null;
		Type = null;
		Interruptable = false;
		Difficulty = 0;
		Rating = 0;
		Turns = 0;
		PsychometryApplied = false;
		base.Reset();
	}

	public static bool GetFor(GameObject Actor, GameObject Subject, string Type, bool Interruptable, ref int Difficulty, ref int Rating, ref int Turns, ref bool Interrupt, ref bool PsychometryApplied)
	{
		bool flag = true;
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			GetRitualSifrahSetupEvent getRitualSifrahSetupEvent = FromPool();
			getRitualSifrahSetupEvent.Actor = Actor;
			getRitualSifrahSetupEvent.Subject = Subject;
			getRitualSifrahSetupEvent.Type = Type;
			getRitualSifrahSetupEvent.Difficulty = Difficulty;
			getRitualSifrahSetupEvent.Rating = Rating;
			getRitualSifrahSetupEvent.Turns = Turns;
			getRitualSifrahSetupEvent.Interruptable = Interruptable;
			getRitualSifrahSetupEvent.PsychometryApplied = PsychometryApplied;
			flag = Actor.HandleEvent(getRitualSifrahSetupEvent);
			if (Interruptable && !flag)
			{
				Interrupt = true;
			}
			Difficulty = getRitualSifrahSetupEvent.Difficulty;
			Rating = getRitualSifrahSetupEvent.Rating;
			Turns = getRitualSifrahSetupEvent.Turns;
			PsychometryApplied = getRitualSifrahSetupEvent.PsychometryApplied;
		}
		return flag;
	}
}
