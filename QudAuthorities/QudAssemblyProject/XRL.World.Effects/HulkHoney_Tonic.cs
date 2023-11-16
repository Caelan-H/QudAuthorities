using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class HulkHoney_Tonic : ITonicEffect
{
	private bool bOverdosed;

	private int BonusHP;

	private float HPLoss;

	[NonSerialized]
	private int TempHPPenalty;

	public int x;

	public int y;

	public string zone;

	public HulkHoney_Tonic()
	{
	}

	public HulkHoney_Tonic(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{G|hulk}} {{w|honey}} tonic";
	}

	public override string GetDetails()
	{
		if (base.Object.IsTrueKin())
		{
			return "+9 Strength\n+3 temporary hit points per level.\n-25 Move Speed\nCan't feel pain.\nImmune to fear.\nSuffers 1% of max hit points in damage each turn (can't be reduced below one hit point by this effect).";
		}
		return "+6 Strength\n+2 temporary hit points per level.\n-25 Move Speed\nCan't feel pain.\nImmune to fear.\nSuffers 1% of max hit points in damage each turn (can't be reduced below one hit point by this effect).";
	}

	public override bool Apply(GameObject Object)
	{
		bool result = true;
		if (Object.GetEffect("HulkHoney_Tonic") is HulkHoney_Tonic hulkHoney_Tonic)
		{
			if (hulkHoney_Tonic.Duration < base.Duration)
			{
				hulkHoney_Tonic.Duration = base.Duration;
			}
			result = false;
		}
		else
		{
			if (Object.IsPlayer())
			{
				Popup.Show("Your muscles bulge grotesquely.");
			}
			ApplyChanges();
		}
		int num = 0;
		while (Object.HasEffect("Terrified") && ++num < 10)
		{
			Object.RemoveEffect("Terrified");
		}
		if (Object.GetLongProperty("Overdosing") == 1)
		{
			FireEvent(Event.New("Overdose"));
		}
		return result;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("Your muscles deflate to their usual size.");
		}
		UnapplyChanges();
	}

	private void ApplyChanges()
	{
		BonusHP = base.Object.Stat("Level") * (base.Object.IsTrueKin() ? 3 : 2);
		base.StatShifter.SetStatShift(base.Object, "MoveSpeed", 25);
		base.StatShifter.SetStatShift(base.Object, "Strength", base.Object.IsTrueKin() ? 9 : 6);
		base.StatShifter.SetStatShift(base.Object, "Hitpoints", BonusHP, baseValue: true);
		if (base.Object.HasStat("Hitpoints"))
		{
			Statistic statistic = base.Object.Statistics["Hitpoints"];
			if (TempHPPenalty > 0)
			{
				statistic.Penalty += TempHPPenalty;
				TempHPPenalty = 0;
			}
		}
		base.Object.ModIntProperty("Analgesia", 1);
	}

	private void UnapplyChanges()
	{
		if (base.Object.HasStat("Hitpoints"))
		{
			Statistic statistic = base.Object.Statistics["Hitpoints"];
			if (statistic.Penalty >= statistic.BaseValue - BonusHP)
			{
				int num = statistic.BaseValue - BonusHP - 1;
				TempHPPenalty = statistic.Penalty - num;
				statistic.Penalty = num;
			}
		}
		base.StatShifter.RemoveStatShifts(base.Object);
		base.Object.ModIntProperty("Analgesia", -1, RemoveIfZero: true);
		if (bOverdosed)
		{
			base.Object.ModIntProperty("ConfusionLevel", -1);
			base.Object.ModIntProperty("FuriousConfusionLevel", -1);
			bOverdosed = false;
		}
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
		if (base.Duration > 0 && base.Object.HasStat("Hitpoints"))
		{
			HPLoss += (float)base.Object.BaseStat("Hitpoints") * 0.01f;
			int num = Math.Min(base.Object.hitpoints - 1, (int)Math.Floor(HPLoss));
			if (num > 0)
			{
				base.Object.TakeDamage(num, "from the {{G|hulk}} {{w|honey}}!", "Metabolic Expected Unavoidable");
				HPLoss -= num;
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "ApplyFear");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "CanApplyFear");
		Object.RegisterEffectEvent(this, "Overdose");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "ApplyFear");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "CanApplyFear");
		Object.UnregisterEffectEvent(this, "Overdose");
		base.Unregister(Object);
	}

	public override void ApplyAllergy(GameObject Object)
	{
		if (Object.GetLongProperty("Overdosing") == 1)
		{
			Popup.Show("Your mutant physiology reacts adversely to the tonic. Aaaaaaaaargh!");
		}
		else
		{
			Popup.Show("The tonics you ingested react adversely to each other. Aaaaaaaaargh!");
		}
		Object.ApplyEffect(new HulkHoney_Tonic_Allergy(base.Duration));
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyFear" || E.ID == "ApplyFear")
		{
			return false;
		}
		if (E.ID == "Overdose")
		{
			if (base.Duration > 0 && base.Object.IsPlayer())
			{
				AchievementManager.SetAchievement("ACH_OVERDOSE_HULKHONEY");
				if (base.Object.GetLongProperty("Overdosing") == 1)
				{
					Popup.Show("Your mutant physiology reacts adversely to the tonic. Aaaaaaaaargh!");
				}
				else
				{
					Popup.Show("The tonics you ingested react adversely to each other. Aaaaaaaaargh!");
				}
				bOverdosed = true;
				base.Object.ModIntProperty("ConfusionLevel", 1);
				base.Object.ModIntProperty("FuriousConfusionLevel", 1);
			}
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
		int num = XRLCore.CurrentFrame % 60;
		if (base.Duration > 0 && num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "!";
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				E.ColorString = "&G";
			}
		}
		return true;
	}
}
