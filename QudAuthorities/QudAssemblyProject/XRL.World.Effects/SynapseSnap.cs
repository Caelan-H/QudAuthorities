using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class SynapseSnap : Effect
{
	public SynapseSnap()
	{
	}

	public SynapseSnap(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 67108866;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		Object.ParticleText("*synapse snap*", 'W');
		ApplyChanges();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Object.ParticleText("*synapse snap wore off*", 'r');
		UnapplyChanges();
	}

	private void ApplyChanges()
	{
		base.StatShifter.SetStatShift("Agility", 4);
		base.StatShifter.SetStatShift("Intelligence", 4);
	}

	private void UnapplyChanges()
	{
		base.StatShifter.RemoveStatShifts();
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
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
		int num = XRLCore.CurrentFrame % 60;
		if (num > 25 && num < 35)
		{
			E.Tile = null;
			E.RenderString = "\u0018";
			E.ColorString = "&W";
		}
		return true;
	}
}
