using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeDieEvent : IDeathEvent
{
	public new static readonly int ID;

	private static List<BeforeDieEvent> Pool;

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

	static BeforeDieEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(BeforeDieEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public BeforeDieEvent()
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

	public static BeforeDieEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static BeforeDieEvent FromPool(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		BeforeDieEvent beforeDieEvent = FromPool();
		beforeDieEvent.Dying = Dying;
		beforeDieEvent.Killer = Killer;
		beforeDieEvent.Weapon = Weapon;
		beforeDieEvent.Projectile = Projectile;
		beforeDieEvent.Accidental = Accidental;
		beforeDieEvent.KillerText = KillerText;
		beforeDieEvent.Reason = Reason;
		beforeDieEvent.ThirdPersonReason = ThirdPersonReason;
		return beforeDieEvent;
	}

	public static bool Check(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.validate(ref Dying) && Dying.HasRegisteredEvent("BeforeDie"))
			{
				Event @event = Event.New("BeforeDie");
				@event.SetParameter("Dying", Dying);
				@event.SetParameter("Killer", Killer);
				@event.SetParameter("Weapon", Weapon);
				@event.SetParameter("Projectile", Projectile);
				@event.SetParameter("KillerText", KillerText);
				@event.SetParameter("Reason", Reason);
				@event.SetParameter("ThirdPersonReason", ThirdPersonReason);
				@event.SetFlag("Accidental", Accidental);
				flag = Dying.FireEvent(@event);
				
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("BeforeDie registered event handling", x);
		}
		try
		{
			if (flag)
			{
				if (GameObject.validate(ref Dying))
				{
					if (Dying.WantEvent(ID, MinEvent.CascadeLevel))
					{
						BeforeDieEvent e = FromPool(Dying, Killer, Weapon, Projectile, Accidental, KillerText, Reason, ThirdPersonReason);
						flag = Dying.HandleEvent(e);
						return flag;
					}
					return flag;
				}
				return flag;
			}
			return flag;
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("BeforeDie MinEvent handling", x2);
			return flag;
		}
	}
}
