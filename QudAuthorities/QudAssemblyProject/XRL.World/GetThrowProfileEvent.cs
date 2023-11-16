using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.Rules;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetThrowProfileEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Object;

	public GameObject ApparentTarget;

	public Cell TargetCell;

	public int Distance;

	public int Range;

	public int Strength;

	public int AimVariance;

	public bool Telekinetic;

	public new static readonly int ID;

	private static List<GetThrowProfileEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 1;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetThrowProfileEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetThrowProfileEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetThrowProfileEvent()
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

	public static GetThrowProfileEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetThrowProfileEvent FromPool(GameObject Actor, GameObject Object, GameObject ApparentTarget = null, Cell TargetCell = null, int Distance = 0, int Range = 0, int Strength = 0, int AimVariance = 0, bool Telekinetic = false)
	{
		GetThrowProfileEvent getThrowProfileEvent = FromPool();
		getThrowProfileEvent.Actor = Actor;
		getThrowProfileEvent.Object = Object;
		getThrowProfileEvent.ApparentTarget = ApparentTarget;
		getThrowProfileEvent.TargetCell = TargetCell;
		getThrowProfileEvent.Distance = Distance;
		getThrowProfileEvent.Range = Range;
		getThrowProfileEvent.Strength = Strength;
		getThrowProfileEvent.AimVariance = AimVariance;
		getThrowProfileEvent.Telekinetic = Telekinetic;
		return getThrowProfileEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Actor = null;
		Object = null;
		ApparentTarget = null;
		TargetCell = null;
		Distance = 0;
		Range = 0;
		Strength = 0;
		AimVariance = 0;
		Telekinetic = false;
		base.Reset();
	}

	public static bool Process(out int Range, out int Strength, out int AimVariance, out bool Telekinetic, GameObject Actor, GameObject Object, GameObject ApparentTarget = null, Cell TargetCell = null, int Distance = 0)
	{
		GameObject.validate(ref Object);
		GameObject.validate(ref Actor);
		Range = 4;
		Strength = 0;
		AimVariance = 0;
		Telekinetic = false;
		if (Actor != null)
		{
			Range += Actor.GetIntProperty("ThrowRangeBonus") + Object.GetIntProperty("ThrowRangeSkillBonus");
			Strength = Actor.Stat("Strength");
		}
		if (Object != null)
		{
			Range += Object.GetIntProperty("ThrowRangeBonus");
		}
		AimVariance = Stat.Random(0, 20) - 10;
		if (Actor != null)
		{
			int num = Actor.StatMod("Agility");
			if (num != 0)
			{
				if (AimVariance == 0)
				{
					if (num < 0)
					{
						AimVariance += (50.in100() ? num : (-num));
					}
				}
				else if (AimVariance > 0)
				{
					AimVariance -= num;
					if (AimVariance < 0)
					{
						AimVariance = 0;
					}
				}
				else
				{
					AimVariance += num;
					if (AimVariance > 0)
					{
						AimVariance = 0;
					}
				}
			}
		}
		GetThrowProfileEvent getThrowProfileEvent = FromPool(Actor, Object, ApparentTarget, TargetCell, Distance, Range, Strength, AimVariance, Telekinetic);
		try
		{
			if (Actor != null && Actor.WantEvent(ID, CascadeLevel) && !Actor.HandleEvent(getThrowProfileEvent))
			{
				return false;
			}
			if (Object != null && Actor.WantEvent(ID, CascadeLevel) && !Object.HandleEvent(getThrowProfileEvent))
			{
				return false;
			}
			return true;
		}
		finally
		{
			Strength = getThrowProfileEvent.Strength;
			Range = getThrowProfileEvent.Range + Stat.GetScoreModifier(Strength);
			AimVariance = getThrowProfileEvent.AimVariance;
			Telekinetic = getThrowProfileEvent.Telekinetic;
		}
	}
}
