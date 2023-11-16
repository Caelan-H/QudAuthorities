using System;

namespace XRL.World.Effects;

[Serializable]
public class PhasedWhileStuck : Effect
{
	public Phased PhasedEffect;

	public PhasedWhileStuck()
	{
	}

	public PhasedWhileStuck(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 33554464;
	}

	public override string GetDescription()
	{
		return null;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		PhasedEffect = new Phased(9999);
		return Object.ApplyEffect(PhasedEffect);
	}

	public override void Remove(GameObject Object)
	{
		foreach (Effect effect in Object.GetEffects("Phased"))
		{
			effect.Duration = 1;
		}
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "EndTurn");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && !base.Object.HasEffect("Stuck"))
		{
			base.Duration = 0;
		}
		return base.FireEvent(E);
	}
}
