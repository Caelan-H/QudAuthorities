using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetPsionicSifrahSetupEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Subject;

	public string Type;

	public string Subtype;

	public bool Interruptable;

	public int Difficulty;

	public int Rating;

	public int Turns;

	public bool PsychometryApplied;

	public new static readonly int ID;

	private static List<GetPsionicSifrahSetupEvent> Pool;

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

	static GetPsionicSifrahSetupEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetPsionicSifrahSetupEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetPsionicSifrahSetupEvent()
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

	public static GetPsionicSifrahSetupEvent FromPool()
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
		Subtype = null;
		Interruptable = false;
		Difficulty = 0;
		Rating = 0;
		Turns = 0;
		PsychometryApplied = false;
		base.Reset();
	}

	public static bool GetFor(GameObject Actor, GameObject Subject, string Type, string Subtype, bool Interruptable, ref int Difficulty, ref int Rating, ref int Turns, ref bool Interrupt, ref bool PsychometryApplied)
	{
		bool flag = true;
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			GetPsionicSifrahSetupEvent getPsionicSifrahSetupEvent = FromPool();
			getPsionicSifrahSetupEvent.Actor = Actor;
			getPsionicSifrahSetupEvent.Subject = Subject;
			getPsionicSifrahSetupEvent.Type = Type;
			getPsionicSifrahSetupEvent.Subtype = Subtype;
			getPsionicSifrahSetupEvent.Difficulty = Difficulty;
			getPsionicSifrahSetupEvent.Rating = Rating;
			getPsionicSifrahSetupEvent.Turns = Turns;
			getPsionicSifrahSetupEvent.Interruptable = Interruptable;
			getPsionicSifrahSetupEvent.PsychometryApplied = PsychometryApplied;
			flag = Actor.HandleEvent(getPsionicSifrahSetupEvent);
			if (Interruptable && !flag)
			{
				Interrupt = true;
			}
			Difficulty = getPsionicSifrahSetupEvent.Difficulty;
			Rating = getPsionicSifrahSetupEvent.Rating;
			Turns = getPsionicSifrahSetupEvent.Turns;
			PsychometryApplied = getPsionicSifrahSetupEvent.PsychometryApplied;
		}
		return flag;
	}
}
