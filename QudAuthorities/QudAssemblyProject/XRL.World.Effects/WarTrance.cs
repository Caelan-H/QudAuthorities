using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class WarTrance : Effect
{
	public WarTrance()
	{
		base.Duration = 2;
		base.DisplayName = "{{r|war trance}}";
	}

	public WarTrance(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 83886082;
	}

	public override string GetDetails()
	{
		return "+5 Quickness\n+6 Willpower";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyWarTrance", "Duration", base.Duration)))
		{
			return false;
		}
		ApplyChanges();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyChanges();
		base.Remove(Object);
	}

	private void ApplyChanges()
	{
		base.StatShifter.SetStatShift("Speed", 5);
		base.StatShifter.SetStatShift("Willpower", 6);
	}

	private void UnapplyChanges()
	{
		base.StatShifter.RemoveStatShifts();
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 10)
			{
				E.RenderString = "!";
				E.ColorString = "&R";
			}
		}
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeginTakeAction");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			base.Duration--;
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyChanges();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChanges();
		}
		return base.FireEvent(E);
	}
}
