using System;

namespace XRL.World.Effects;

[Serializable]
public class TimeDilatedIndependent : ITimeDilated
{
	public double? SpeedPenaltyMultiplier;

	public TimeDilatedIndependent()
	{
	}

	public TimeDilatedIndependent(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public TimeDilatedIndependent(int Duration, double SpeedPenaltyMultiplier)
		: this(Duration)
	{
		this.SpeedPenaltyMultiplier = SpeedPenaltyMultiplier;
	}

	public override bool DoTimeDilationVisualEffects()
	{
		return base.Duration > 0;
	}

	public override bool Apply(GameObject Object)
	{
		bool num = base.Apply(Object);
		if (num)
		{
			Sync();
		}
		return num;
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

	public void Sync()
	{
		if (SpeedPenaltyMultiplier.HasValue)
		{
			int num = Math.Max((int)((double)base.Object.BaseStat("Speed") * SpeedPenaltyMultiplier).Value, 1);
			if (num != SpeedPenalty)
			{
				UnapplyChanges();
				SpeedPenalty = num;
				ApplyChanges();
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && --base.Duration > 0)
		{
			Sync();
		}
		return base.FireEvent(E);
	}
}
