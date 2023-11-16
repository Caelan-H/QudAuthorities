using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetTinkeringBonusEvent : IActOnItemEvent
{
	public string Type;

	public int BaseRating;

	public int Bonus;

	public int ToolboxBonus;

	public bool PsychometryApplied;

	public bool Interruptable;

	public new static readonly int ID;

	private static List<GetTinkeringBonusEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 3;

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

	static GetTinkeringBonusEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetTinkeringBonusEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetTinkeringBonusEvent()
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

	public static GetTinkeringBonusEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetTinkeringBonusEvent FromPool(GameObject Actor, GameObject Item, string Type, int BaseRating, int Bonus = 0, int ToolboxBonus = 0, bool PsychometryApplied = false, bool Interruptable = true)
	{
		GetTinkeringBonusEvent getTinkeringBonusEvent = FromPool();
		getTinkeringBonusEvent.Actor = Actor;
		getTinkeringBonusEvent.Item = Item;
		getTinkeringBonusEvent.Type = Type;
		getTinkeringBonusEvent.BaseRating = BaseRating;
		getTinkeringBonusEvent.Bonus = Bonus;
		getTinkeringBonusEvent.ToolboxBonus = ToolboxBonus;
		getTinkeringBonusEvent.PsychometryApplied = PsychometryApplied;
		getTinkeringBonusEvent.Interruptable = Interruptable;
		return getTinkeringBonusEvent;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Type = null;
		BaseRating = 0;
		Bonus = 0;
		ToolboxBonus = 0;
		PsychometryApplied = false;
		Interruptable = true;
		base.Reset();
	}

	public static int GetFor(GameObject Actor, GameObject Item, string Type, int BaseRating, int Bonus, ref bool Interrupt, ref bool PsychometryApplied, bool Interruptable = true)
	{
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			GetTinkeringBonusEvent getTinkeringBonusEvent = FromPool(Actor, Item, Type, BaseRating, Bonus, 0, PsychometryApplied, Interruptable);
			if (!Actor.HandleEvent(getTinkeringBonusEvent))
			{
				Interrupt = true;
			}
			Bonus = getTinkeringBonusEvent.Bonus;
			PsychometryApplied = getTinkeringBonusEvent.PsychometryApplied;
		}
		return Bonus;
	}

	public static int GetFor(GameObject Actor, GameObject Item, string Type, int BaseRating, int Bonus, ref bool Interrupt, bool Interruptable = true)
	{
		bool PsychometryApplied = false;
		return GetFor(Actor, Item, Type, BaseRating, Bonus, ref Interrupt, ref PsychometryApplied, Interruptable);
	}

	public static int GetFor(GameObject Actor, GameObject Item, string Type, int BaseRating, int Bonus = 0, bool Interruptable = true)
	{
		bool Interrupt = false;
		return GetFor(Actor, Item, Type, BaseRating, Bonus, ref Interrupt, Interruptable);
	}
}
