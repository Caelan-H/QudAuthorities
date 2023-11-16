using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class ShatterMentalArmor : IShatterEffect
{
	public int MAPenalty = 1;

	public GameObject Owner;

	public ShatterMentalArmor()
	{
		base.DisplayName = "{{psionic|psionically cleaved}}";
	}

	public ShatterMentalArmor(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public ShatterMentalArmor(int Duration, GameObject Owner = null, int MAPenalty = 1)
		: this(Duration)
	{
		this.Owner = Owner;
		this.MAPenalty = MAPenalty;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool SameAs(Effect e)
	{
		ShatterMentalArmor shatterMentalArmor = e as ShatterMentalArmor;
		if (shatterMentalArmor.MAPenalty != MAPenalty)
		{
			return false;
		}
		if (shatterMentalArmor.Owner != Owner)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDescription()
	{
		return "{{psionic|psionically cleaved (-" + MAPenalty + " MA)}}";
	}

	public override string GetDetails()
	{
		return "-" + MAPenalty + " MA";
	}

	public override int GetPenalty()
	{
		return MAPenalty;
	}

	public override void IncrementPenalty()
	{
		MAPenalty++;
	}

	public override GameObject GetOwner()
	{
		return Owner;
	}

	public override void SetOwner(GameObject Owner)
	{
		this.Owner = Owner;
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasStat("MA"))
		{
			return false;
		}
		if (Object.FireEvent(Event.New("ApplyShatterMentalArmor", "Owner", Owner, "Effect", this)) && ApplyEffectEvent.Check(Object, "ShatterMentalArmor", this))
		{
			if (Object.pPhysics != null)
			{
				Object.pPhysics.PlayWorldSound("breakage", 0.5f, 0f, combat: true);
			}
			if (!Object.HasEffect("ShatterMentalArmor"))
			{
				ApplyStats();
				Object.ParticleText("*psionic cleave (-" + MAPenalty + " MA)*", 'b');
				return true;
			}
			ShatterMentalArmor shatterMentalArmor = Object.GetEffect("ShatterMentalArmor") as ShatterMentalArmor;
			if (base.Duration > shatterMentalArmor.Duration)
			{
				shatterMentalArmor.Duration = base.Duration;
			}
			shatterMentalArmor.UnapplyStats();
			shatterMentalArmor.MAPenalty += MAPenalty;
			shatterMentalArmor.ApplyStats();
			Object.ParticleText("*psionic cleave (-" + shatterMentalArmor.MAPenalty + " MA)*", 'b');
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift("MA", -MAPenalty);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts();
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

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 40 && num < 55)
			{
				E.Tile = null;
				E.RenderString = "X";
				E.ColorString = "&M^k";
			}
		}
		return true;
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
}
