using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ModifyAttackingSaveEvent : ISaveEvent
{
	public new static readonly int ID;

	private static List<ModifyAttackingSaveEvent> Pool;

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

	static ModifyAttackingSaveEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ModifyAttackingSaveEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ModifyAttackingSaveEvent()
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

	public static ModifyAttackingSaveEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ModifyAttackingSaveEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, int Roll, int BaseDifficulty, int Difficulty, bool IgnoreNatural1, bool IgnoreNatural20, bool Actual)
	{
		ModifyAttackingSaveEvent modifyAttackingSaveEvent = FromPool();
		modifyAttackingSaveEvent.Attacker = Attacker;
		modifyAttackingSaveEvent.Defender = Defender;
		modifyAttackingSaveEvent.Source = Source;
		modifyAttackingSaveEvent.Stat = Stat;
		modifyAttackingSaveEvent.AttackerStat = AttackerStat;
		modifyAttackingSaveEvent.Vs = Vs;
		modifyAttackingSaveEvent.NaturalRoll = NaturalRoll;
		modifyAttackingSaveEvent.Roll = Roll;
		modifyAttackingSaveEvent.BaseDifficulty = BaseDifficulty;
		modifyAttackingSaveEvent.Difficulty = Difficulty;
		modifyAttackingSaveEvent.Actual = Actual;
		return modifyAttackingSaveEvent;
	}

	public static bool Process(GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, ref int Roll, int BaseDifficulty, ref int Difficulty, ref bool IgnoreNatural1, ref bool IgnoreNatural20, bool Actual)
	{
		return ISaveEvent.Process(Attacker, "ModifyAttackingSave", ID, ISaveEvent.CascadeLevel, FromPool, Attacker, Defender, Source, Stat, AttackerStat, Vs, NaturalRoll, ref Roll, BaseDifficulty, ref Difficulty, ref IgnoreNatural1, ref IgnoreNatural20, Actual);
	}
}
