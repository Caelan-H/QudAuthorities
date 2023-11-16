using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Dazed : Effect
{
	private int Penalty = 4;

	private int SpeedPenalty = 10;

	private bool DontStunIfPlayer;

	public Dazed()
	{
		base.DisplayName = "{{C|dazed}}";
	}

	public override string GetDescription()
	{
		return base.DisplayName;
	}

	public Dazed(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public Dazed(int Duration, bool DontStunIfPlayer)
		: this(Duration)
	{
		this.DontStunIfPlayer = DontStunIfPlayer;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool SameAs(Effect e)
	{
		Dazed dazed = e as Dazed;
		if (dazed.Penalty != Penalty)
		{
			return false;
		}
		if (dazed.SpeedPenalty != SpeedPenalty)
		{
			return false;
		}
		if (dazed.DontStunIfPlayer != DontStunIfPlayer)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return "-4 Agility\n-4 Intelligence\n-10 Move Speed";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.pBrain == null)
		{
			return false;
		}
		if (Object.HasEffect("Dazed"))
		{
			if (!DontStunIfPlayer || !Object.IsPlayer() || !Object.HasEffect("Stun"))
			{
				Object.ApplyEffect(new Stun(1, 30, DontStunIfPlayer));
			}
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyDazed", "Duration", base.Duration)))
		{
			return false;
		}
		DidX("are", "dazed", null, null, null, Object);
		Object.ParticleText("*dazed*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		ApplyStats();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	private void ApplyStats()
	{
		Penalty = 4;
		SpeedPenalty = 10;
		base.StatShifter.SetStatShift(base.Object, "Intelligence", -Penalty);
		base.StatShifter.SetStatShift(base.Object, "Agility", -Penalty);
		base.StatShifter.SetStatShift(base.Object, "MoveSpeed", SpeedPenalty);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
		Penalty = 0;
		SpeedPenalty = 0;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeginTakeActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0 && base.Duration != 9999 && !base.Object.HasEffect("Stunned"))
		{
			base.Duration--;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "ApplyDazed");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "ApplyDazed");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 10)
			{
				E.RenderString = "?";
				E.ColorString = "&c^b";
				return false;
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyDazed")
		{
			if (base.Duration > 0)
			{
				return false;
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
