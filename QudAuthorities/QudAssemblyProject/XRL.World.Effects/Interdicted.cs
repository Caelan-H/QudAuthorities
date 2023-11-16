using System;
using System.Linq;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Interdicted : Effect
{
	private int PenaltyToApply = 10;

	private int CurrentSpeedPenalty;

	public string interdictorId;

	public Interdicted()
	{
		base.DisplayName = "{{C|interdicted}}";
		base.Duration = 1;
	}

	public Interdicted(string interdictorId, int penalty)
		: this()
	{
		PenaltyToApply = penalty;
		this.interdictorId = interdictorId;
	}

	public override int GetEffectType()
	{
		return 33554560;
	}

	public override string GetDetails()
	{
		return "-10 Move Speed";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		foreach (Effect effect in Object.Effects)
		{
			if (effect is Interdicted interdicted && interdicted.interdictorId == interdictorId)
			{
				return false;
			}
		}
		if (Object.FireEvent(Event.New("ApplyInterdiction", "Duration", base.Duration)))
		{
			CurrentSpeedPenalty = PenaltyToApply;
			ApplyStats();
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		CurrentSpeedPenalty = 0;
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift(base.Object, "MoveSpeed", CurrentSpeedPenalty);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		Object.RegisterEffectEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "CommandTakeAction");
		Object.UnregisterEffectEvent(this, "EndTurn");
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
		if (E.ID == "CommandTakeAction" || E.ID == "EndTurn")
		{
			GameObject gameObject = base.Object.GetVisibleCombatObjects().FirstOrDefault((GameObject o) => o.id == interdictorId);
			if (gameObject == null || !base.Object.HasLOSTo(gameObject))
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
