using System;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Sitting : Effect
{
	public int HealCounter;

	public string DamageAttributes;

	public GameObject SittingOn;

	public int Level;

	public Sitting()
	{
		base.Duration = 1;
		base.DisplayName = "{{C|sitting}}";
	}

	public Sitting(int Level)
		: this()
	{
		this.Level = Level;
	}

	public Sitting(int Level, string DamageAttributes)
		: this(Level)
	{
		this.DamageAttributes = DamageAttributes;
	}

	public Sitting(GameObject SittingOn)
	{
		this.SittingOn = SittingOn;
		base.Duration = 1;
		base.DisplayName = "{{C|sitting on " + SittingOn.a + SittingOn.ShortDisplayName + "}}";
	}

	public Sitting(GameObject SittingOn, int Level)
		: this(SittingOn)
	{
		this.Level = Level;
	}

	public Sitting(GameObject SittingOn, int Level, string DamageAttributes)
		: this(SittingOn, Level)
	{
		this.DamageAttributes = DamageAttributes;
	}

	public override int GetEffectType()
	{
		return 83886208;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		if (Level >= 5)
		{
			return "Improves natural healing rate.\nAids in examining and disassembling artifacts.\n-6 DV.\nMust spend a turn to stand up before moving.";
		}
		if (Level > -2)
		{
			return "Slightly improves natural healing rate.\nAids in examining and disassembling artifacts.\n-6 DV.\nMust spend a turn to stand up before moving.";
		}
		if (Level == -2)
		{
			return "Slightly improves natural healing rate.\n-6 DV.\nMust spend a turn to stand up before moving.";
		}
		if (Level > -10)
		{
			return "Slightly improves natural healing rate.\nDistracts from examining and disassembling artifacts.\n-6 DV.\nMust spend a turn to stand up before moving.";
		}
		return "Inflicts ongoing damage.\nDistracts from examining and disassembling artifacts.\n-6 DV.\nMust spend a turn to stand up before moving.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.CanChangeBodyPosition("Sitting"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplySitting"))
		{
			return false;
		}
		if (Object.CurrentCell != null)
		{
			Object.CurrentCell.FireEvent(Event.New("ObjectSitting", "Object", Object, "SittingOn", SittingOn));
		}
		Object.BodyPositionChanged("Sitting");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		base.Duration = 0;
		Object.BodyPositionChanged("NotSitting");
		base.Remove(Object);
	}

	public bool IsSittingOnValid()
	{
		if (!GameObject.validate(ref SittingOn))
		{
			return false;
		}
		if (SittingOn.CurrentCell == null)
		{
			return false;
		}
		if (!GameObject.validate(base.Object))
		{
			return false;
		}
		if (base.Object.CurrentCell == null)
		{
			return false;
		}
		if (SittingOn.CurrentCell != base.Object.CurrentCell)
		{
			return false;
		}
		if (!SittingOn.PhaseAndFlightMatches(base.Object))
		{
			return false;
		}
		return true;
	}

	public bool CheckSittingOn()
	{
		if (!IsSittingOnValid())
		{
			SittingOn = null;
			Level = 0;
			DamageAttributes = null;
			return false;
		}
		if (SittingOn != null)
		{
			base.DisplayName = "{{C|sitting on " + SittingOn.a + SittingOn.ShortDisplayName + "}}";
		}
		return true;
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift("DV", -6);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID && ID != GetDisplayNameEvent.ID)
		{
			return ID == GetTinkeringBonusEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTinkeringBonusEvent E)
	{
		if (E.Type == "Inspect" || E.Type == "Disassemble")
		{
			int num = Math.Min(2 + Level, 4);
			if (num != 0)
			{
				E.Bonus += num;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (CheckSittingOn())
		{
			if (Level > -10)
			{
				if (++HealCounter >= 10 - Level)
				{
					base.Object.Heal(1);
					HealCounter = 0;
				}
			}
			else if (base.Object.TakeDamage(-Level - 9, Attributes: DamageAttributes, Attacker: SittingOn, Message: (SittingOn == null) ? "from sitting!" : "from %O!") && 50.in100())
			{
				base.Object.Bloodsplatter();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		CheckSittingOn();
		if (SittingOn != null)
		{
			E.AddTag("[{{B|sitting on " + SittingOn.a + SittingOn.ShortDisplayName + "}}]", 20);
		}
		else
		{
			E.AddTag("[{{B|sitting}}]", 20);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BodyPositionChanged");
		Object.RegisterEffectEvent(this, "LeaveCell");
		Object.RegisterEffectEvent(this, "MovementModeChanged");
		ApplyStats();
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BodyPositionChanged");
		Object.UnregisterEffectEvent(this, "LeaveCell");
		Object.UnregisterEffectEvent(this, "MovementModeChanged");
		UnapplyStats();
		base.Unregister(Object);
	}

	private void StandUp(Event E = null)
	{
		CheckSittingOn();
		if (SittingOn != null)
		{
			SittingOn.GetPart<Chair>().StandUp(base.Object, E, this);
			return;
		}
		DidX("stand", "up", null, null, base.Object);
		base.Object.UseEnergy(1000, "Position");
		base.Object.RemoveEffect(this);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LeaveCell" || E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			if (base.Duration > 0)
			{
				StandUp(E);
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}
}
