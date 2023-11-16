using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class TimeDilated : ITimeDilated
{
	public GameObject Mutant;

	public TimeDilated()
	{
	}

	public TimeDilated(GameObject Mutant)
		: this()
	{
		this.Mutant = Mutant;
	}

	public override bool DoTimeDilationVisualEffects()
	{
		if (Mutant.GetPart("TimeDilation") is TimeDilation timeDilation)
		{
			return timeDilation.Duration > 0;
		}
		return false;
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
		Object.RegisterEffectEvent(this, "EnteredCell");
		Object.RegisterEffectEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "EnteredCell");
		Object.UnregisterEffectEvent(this, "EndTurn");
		base.Unregister(Object);
	}

	private bool SyncInner()
	{
		if (!GameObject.validate(ref Mutant))
		{
			return false;
		}
		if (Mutant.IsInGraveyard() || Mutant.OnWorldMap() || base.Object.IsInGraveyard() || base.Object.OnWorldMap())
		{
			return false;
		}
		if (!(Mutant.GetPart("TimeDilation") is TimeDilation timeDilation) || timeDilation.Duration <= 0)
		{
			return false;
		}
		if (base.Duration > 0)
		{
			double num = timeDilation.ParentObject.RealDistanceTo(base.Object);
			if (num > (double)timeDilation.Range)
			{
				return false;
			}
			double num2 = TimeDilation.CalculateQuicknessPenaltyMultiplier(num, timeDilation.Range, timeDilation.Level);
			UnapplyChanges();
			SpeedPenalty = Math.Max((int)((double)base.Object.BaseStat("Speed") * num2), 1);
			ApplyChanges();
		}
		return true;
	}

	public void Sync()
	{
		if (!SyncInner())
		{
			base.Duration = 0;
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" || E.ID == "EnteredCell")
		{
			Sync();
		}
		return base.FireEvent(E);
	}
}
