using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ModifyOriginatingSaveEvent : ISaveEvent
{
	public new static readonly int ID;

	private static List<ModifyOriginatingSaveEvent> Pool;

	private static int PoolCounter;

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

	static ModifyOriginatingSaveEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ModifyOriginatingSaveEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ModifyOriginatingSaveEvent()
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

	public static ModifyOriginatingSaveEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ModifyOriginatingSaveEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, int Roll, int BaseDifficulty, int Difficulty, bool IgnoreNatural1, bool IgnoreNatural20, bool Actual)
	{
		ModifyOriginatingSaveEvent modifyOriginatingSaveEvent = FromPool();
		modifyOriginatingSaveEvent.Attacker = Attacker;
		modifyOriginatingSaveEvent.Defender = Defender;
		modifyOriginatingSaveEvent.Source = Source;
		modifyOriginatingSaveEvent.Stat = Stat;
		modifyOriginatingSaveEvent.AttackerStat = AttackerStat;
		modifyOriginatingSaveEvent.Vs = Vs;
		modifyOriginatingSaveEvent.NaturalRoll = NaturalRoll;
		modifyOriginatingSaveEvent.Roll = Roll;
		modifyOriginatingSaveEvent.BaseDifficulty = BaseDifficulty;
		modifyOriginatingSaveEvent.Difficulty = Difficulty;
		modifyOriginatingSaveEvent.Actual = Actual;
		return modifyOriginatingSaveEvent;
	}

	public static bool Process(GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, ref int Roll, int BaseDifficulty, ref int Difficulty, ref bool IgnoreNatural1, ref bool IgnoreNatural20, bool Actual)
	{
		return ISaveEvent.Process(Attacker, "ModifyOriginatingSave", ID, ISaveEvent.CascadeLevel, FromPool, Attacker, Defender, Source, Stat, AttackerStat, Vs, NaturalRoll, ref Roll, BaseDifficulty, ref Difficulty, ref IgnoreNatural1, ref IgnoreNatural20, Actual);
	}
}
