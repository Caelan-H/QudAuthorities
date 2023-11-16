using System;

namespace XRL.World.Effects;

[Serializable]
public class FungalVisionary : Effect
{
	public static int VisionLevel;

	public FungalVisionary()
	{
		base.DisplayName = "{{O|shimmering}}";
		base.Duration = 1;
	}

	public FungalVisionary(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 8194;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "-60 Quickness\nCan see into dimensions half a step over.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyFungalVisionary", "Duration", base.Duration)))
		{
			return false;
		}
		ApplyStats();
		if (Object.IsPlayer())
		{
			VisionLevel = 1;
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		if (Object.IsPlayer())
		{
			VisionLevel = 0;
		}
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift("Speed", -60);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
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
		if (base.Object.IsPlayer())
		{
			VisionLevel = 1;
		}
		else if (!The.Player.HasEffect("FungalVisionary"))
		{
			VisionLevel = 0;
		}
		return base.HandleEvent(E);
	}
}
