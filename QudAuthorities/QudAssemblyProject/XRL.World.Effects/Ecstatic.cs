using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Ecstatic : Effect
{
	public Ecstatic()
	{
		base.DisplayName = "{{G|ecstatic}}";
		base.Duration = 1;
	}

	public Ecstatic(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 16777218;
	}

	public override string GetDetails()
	{
		return "+10 Quickness";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyEcstatic", "Duration", base.Duration)))
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
		base.StatShifter.SetStatShift("Speed", 10);
	}

	private void UnapplyChanges()
	{
		base.StatShifter.RemoveStatShifts();
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

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 10)
			{
				E.RenderString = "!";
				E.ColorString = "&G";
				return false;
			}
		}
		return true;
	}
}
