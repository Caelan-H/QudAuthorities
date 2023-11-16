using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetSpecialEffectChanceEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Object;

	public GameObject Subject;

	public GameObject Projectile;

	public string Type;

	public int BaseChance;

	public int Chance;

	public new static readonly int ID;

	private static List<GetSpecialEffectChanceEvent> Pool;

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

	static GetSpecialEffectChanceEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetSpecialEffectChanceEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetSpecialEffectChanceEvent()
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
		Actor = null;
		Object = null;
		Subject = null;
		Projectile = null;
		Type = null;
		BaseChance = 0;
		Chance = 0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static GetSpecialEffectChanceEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static int GetFor(GameObject Actor, GameObject Object, string Type = null, int Chance = 0, GameObject Subject = null, GameObject Projectile = null, bool ConstrainToPercentage = true, bool ConstrainToPermillage = false)
	{
		int num = Chance;
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.validate(ref Actor) && Actor.HasRegisteredEvent("GetSpecialEffectChance");
			bool flag3 = GameObject.validate(ref Object) && Object.HasRegisteredEvent("GetSpecialEffectChance");
			bool flag4 = GameObject.validate(ref Subject) && Subject.HasRegisteredEvent("GetSpecialEffectChance");
			bool flag5 = GameObject.validate(ref Projectile) && Projectile.HasRegisteredEvent("GetSpecialEffectChance");
			if (flag2 || flag3 || flag4 || flag5)
			{
				Event @event = Event.New("GetSpecialEffectChance");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Object", Object);
				@event.SetParameter("Subject", Subject);
				@event.SetParameter("Projectile", Projectile);
				@event.SetParameter("Type", Type);
				@event.SetParameter("BaseChance", num);
				@event.SetParameter("Chance", Chance);
				if (flag && flag2)
				{
					flag = Actor.FireEvent(@event);
				}
				if (flag && flag3)
				{
					flag = Object.FireEvent(@event);
				}
				if (flag && flag4)
				{
					flag = Subject.FireEvent(@event);
				}
				if (flag && flag5)
				{
					flag = Projectile.FireEvent(@event);
				}
				Chance = @event.GetIntParameter("Chance");
			}
		}
		if (flag)
		{
			bool flag6 = GameObject.validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel);
			bool flag7 = GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel);
			bool flag8 = GameObject.validate(ref Subject) && Subject.WantEvent(ID, CascadeLevel);
			bool flag9 = GameObject.validate(ref Projectile) && Projectile.WantEvent(ID, CascadeLevel);
			if (flag6 || flag7 || flag8 || flag9)
			{
				GetSpecialEffectChanceEvent getSpecialEffectChanceEvent = FromPool();
				getSpecialEffectChanceEvent.Actor = Actor;
				getSpecialEffectChanceEvent.Object = Object;
				getSpecialEffectChanceEvent.Subject = Subject;
				getSpecialEffectChanceEvent.Projectile = Projectile;
				getSpecialEffectChanceEvent.Type = Type;
				getSpecialEffectChanceEvent.BaseChance = num;
				getSpecialEffectChanceEvent.Chance = Chance;
				if (flag && flag6)
				{
					flag = Actor.HandleEvent(getSpecialEffectChanceEvent);
				}
				if (flag && flag7)
				{
					flag = Object.HandleEvent(getSpecialEffectChanceEvent);
				}
				if (flag && flag8)
				{
					flag = Subject.HandleEvent(getSpecialEffectChanceEvent);
				}
				if (flag && flag9)
				{
					flag = Projectile.HandleEvent(getSpecialEffectChanceEvent);
				}
				Chance = getSpecialEffectChanceEvent.Chance;
			}
		}
		if (ConstrainToPercentage)
		{
			if (Chance > 100)
			{
				Chance = 100;
			}
			else if (Chance < 0)
			{
				Chance = 0;
			}
		}
		else if (ConstrainToPermillage)
		{
			if (Chance > 1000)
			{
				Chance = 1000;
			}
			else if (Chance < 0)
			{
				Chance = 0;
			}
		}
		return Chance;
	}
}
