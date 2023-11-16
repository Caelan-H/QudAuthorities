using System;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IHitDiceEvent : MinEvent
{
	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Weapon;

	public int PenetrationBonus;

	public int AV;

	public bool ShieldBlocked;

	public new static int CascadeLevel => 1;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Attacker = null;
		Defender = null;
		Weapon = null;
		PenetrationBonus = 0;
		AV = 0;
		ShieldBlocked = false;
		base.Reset();
	}

	public static bool Process(ref int PenetrationBonus, ref int AV, ref bool ShieldBlocked, GameObject Attacker, GameObject Defender, GameObject Weapon, string RegisteredEvent, GameObject Target, int ID, int CascadeLevel, Func<GameObject, GameObject, GameObject, int, int, bool, IHitDiceEvent> Generator)
	{
		if (GameObject.validate(ref Target))
		{
			if (Target.HasRegisteredEvent(RegisteredEvent))
			{
				Event @event = Event.New(RegisteredEvent);
				@event.SetParameter("PenetrationBonus", PenetrationBonus);
				@event.SetParameter("Attacker", Attacker);
				@event.SetParameter("Defender", Defender);
				@event.SetParameter("Weapon", Weapon);
				@event.SetParameter("AV", AV);
				@event.SetFlag("ShieldBlocked", ShieldBlocked);
				try
				{
					if (!Target.FireEvent(@event))
					{
						return false;
					}
				}
				finally
				{
					PenetrationBonus = @event.GetIntParameter("PenetrationBonus");
					AV = @event.GetIntParameter("AV");
					ShieldBlocked = @event.HasFlag("ShieldBlocked");
				}
			}
			if (Target.WantEvent(ID, CascadeLevel))
			{
				IHitDiceEvent hitDiceEvent = Generator(Attacker, Defender, Weapon, PenetrationBonus, AV, ShieldBlocked);
				try
				{
					if (!Target.HandleEvent(hitDiceEvent))
					{
						return false;
					}
				}
				finally
				{
					PenetrationBonus = hitDiceEvent.PenetrationBonus;
					AV = hitDiceEvent.AV;
					ShieldBlocked = hitDiceEvent.ShieldBlocked;
				}
			}
		}
		return true;
	}
}
