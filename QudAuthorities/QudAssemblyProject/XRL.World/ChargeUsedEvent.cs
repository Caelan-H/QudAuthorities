using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ChargeUsedEvent : IChargeConsumptionEvent
{
	public int DesiredAmount;

	public new static readonly int ID;

	private static List<ChargeUsedEvent> Pool;

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

	static ChargeUsedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(ChargeUsedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public ChargeUsedEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		DesiredAmount = 0;
		base.Reset();
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static ChargeUsedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(GameObject Object, GameObject Source, int Amount, int DesiredAmount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeTransient = true, bool IncludeBiological = true, int PowerLoadLevel = 100)
	{
		if (Wanted(Object))
		{
			ChargeUsedEvent chargeUsedEvent = FromPool();
			chargeUsedEvent.Source = Source;
			chargeUsedEvent.Amount = Amount;
			chargeUsedEvent.DesiredAmount = DesiredAmount;
			chargeUsedEvent.Multiple = Multiple;
			chargeUsedEvent.GridMask = GridMask;
			chargeUsedEvent.Forced = Forced;
			chargeUsedEvent.LiveOnly = LiveOnly;
			chargeUsedEvent.IncludeTransient = IncludeTransient;
			chargeUsedEvent.IncludeBiological = IncludeBiological;
			chargeUsedEvent.PowerLoadLevel = PowerLoadLevel;
			Process(Object, chargeUsedEvent);
		}
		if (PowerLoadLevel <= 100)
		{
			return;
		}
		int @for = GetOverloadChargeEvent.GetFor(Object, Amount);
		if (@for > 0)
		{
			GameObject gameObject = Object.Equipped ?? Object.Implantee ?? Object.InInventory;
			Object.TemperatureChange(1 + @for / 100, gameObject);
			gameObject?.TemperatureChange(1 + @for / 100, gameObject);
			if ((1 + @for / 10).in10000() && Object.ApplyEffect(new Broken(FromDamage: false, FromExamine: false, FromOverload: true)))
			{
				Messaging.XDidY(Object, "overheat", null, "!", null, null, null, UseFullNames: false, IndefiniteSubject: false, gameObject, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup: false, gameObject);
			}
		}
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("ChargeUsed");
		}
		return true;
	}

	public static bool Process(GameObject Object, ChargeUsedEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "ChargeUsed"))
		{
			return false;
		}
		if (!Object.HandleEvent(E))
		{
			return false;
		}
		return true;
	}
}
