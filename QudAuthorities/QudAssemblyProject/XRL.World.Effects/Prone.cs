using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Prone : Effect
{
	public bool Voluntary;

	public GameObject LyingOn;

	public Prone()
	{
		base.Duration = 1;
		base.DisplayName = "{{C|prone}}";
	}

	public Prone(bool Voluntary)
		: this()
	{
		this.Voluntary = Voluntary;
	}

	public Prone(GameObject LyingOn)
	{
		this.LyingOn = LyingOn;
		base.Duration = 1;
		if (LyingOn != null)
		{
			base.DisplayName = "{{C|lying on " + LyingOn.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + "}}";
		}
	}

	public Prone(GameObject LyingOn, bool Voluntary)
		: this(LyingOn)
	{
		this.Voluntary = Voluntary;
	}

	public override int GetEffectType()
	{
		int num = 117440640;
		if (Voluntary)
		{
			num |= 0x8000000;
		}
		return num;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		CheckLyingOn();
		if (LyingOn != null)
		{
			return "{{C|lying on " + LyingOn.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + "}}";
		}
		return "{{C|prone}}";
	}

	private string EffectSummary()
	{
		return "-6 Agility.\n-5 DV.\n-80 move speed.\nMust spend a turn to stand up.";
	}

	public override string GetDetails()
	{
		if (LyingOn != null)
		{
			return LyingOn.DisplayName + ":\n" + EffectSummary();
		}
		return EffectSummary();
	}

	public bool IsLyingOnValid()
	{
		if (!GameObject.validate(ref LyingOn))
		{
			return false;
		}
		if (LyingOn.CurrentCell == null)
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
		if (LyingOn.CurrentCell != base.Object.CurrentCell)
		{
			return false;
		}
		return true;
	}

	public bool CheckLyingOn()
	{
		bool num = IsLyingOnValid();
		if (!num || LyingOn == null)
		{
			if (LyingOn != null)
			{
				LyingOn = null;
			}
			base.DisplayName = "{{C|prone}}";
			return num;
		}
		base.DisplayName = "{{C|lying on " + LyingOn.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + "}}";
		return num;
	}

	public static bool LimbSupportsProneness(BodyPart Part)
	{
		if (Part.Type != "Feet" && Part.Type != "Roots")
		{
			return false;
		}
		if (Part.Mobility <= 0)
		{
			return false;
		}
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Prone"))
		{
			return false;
		}
		if (!Object.HasBodyPart(LimbSupportsProneness))
		{
			return false;
		}
		if (!Object.CanChangeBodyPosition("Prone", ShowMessage: false, !Voluntary))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyProne"))
		{
			return false;
		}
		Object.RemoveEffect("Sitting");
		if (Voluntary)
		{
			DidX("lie", "down");
		}
		else
		{
			DidX("are", "knocked prone", "!", null, null, Object);
			Object.ParticleText("*knocked prone*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		}
		Object.BodyPositionChanged(null, !Voluntary);
		Cell cell = Object.CurrentCell;
		if (cell != null)
		{
			ObjectGoingProneEvent.Send(Object, cell, Voluntary);
		}
		ApplyStats();
		Object.ForfeitTurn(EnergyNeutral: true);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		foreach (Immobilized effect in Object.GetEffects("Immobilized"))
		{
			if (effect != null && effect.LinkedToProne)
			{
				effect.EndImmobilization();
			}
		}
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandTakeActionEvent.ID)
		{
			return ID == GetDisplayNameEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		if (base.Duration > 0 && !base.Object.HasEffect("Asleep") && base.Object.FireEvent("CanStandUp") && base.Object.CanChangeBodyPosition("Standing"))
		{
			base.Object.UseEnergy(1000);
			base.Duration--;
			if (base.Duration <= 0)
			{
				DidX("stand", "up");
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		CheckLyingOn();
		if (LyingOn != null)
		{
			E.AddTag("[{{B|lying on " + LyingOn.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + "}}]");
		}
		else
		{
			E.AddTag("[{{B|prone}}]");
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BodyPositionChanged");
		Object.RegisterEffectEvent(this, "CanChangeBodyPosition");
		Object.RegisterEffectEvent(this, "MovementModeChanged");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BodyPositionChanged");
		Object.UnregisterEffectEvent(this, "CanChangeBodyPosition");
		Object.UnregisterEffectEvent(this, "MovementModeChanged");
		base.Unregister(Object);
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift(base.Object, "Agility", -6);
		base.StatShifter.SetStatShift(base.Object, "DV", -5);
		base.StatShifter.SetStatShift(base.Object, "MoveSpeed", 80);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "_";
			E.ColorString = "&R";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanChangeBodyPosition")
		{
			if (!E.HasFlag("Involuntary") && E.GetStringParameter("To") != "Asleep" && E.GetStringParameter("To") != "Standing" && (!base.Object.FireEvent("CanStandUp") || !base.Object.FireEvent("CanStandUpFromProne")))
			{
				return false;
			}
		}
		else if (E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			if (E.GetStringParameter("To") != "Asleep" && E.GetStringParameter("To") != "Standing")
			{
				base.Object.RemoveEffect(this);
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
