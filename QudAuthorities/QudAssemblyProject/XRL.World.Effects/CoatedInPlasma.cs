using System;
using System.Text;

namespace XRL.World.Effects;

[Serializable]
public class CoatedInPlasma : Effect
{
	public GameObject Owner;

	public CoatedInPlasma()
	{
		base.DisplayName = "{{coated in plasma|coated in plasma}}";
	}

	public CoatedInPlasma(int Duration = 1, GameObject Owner = null)
		: this()
	{
		base.Duration = Duration;
		this.Owner = Owner;
	}

	public override int GetEffectType()
	{
		return 100663328;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("-100 heat resistance\n").Append("-100 cold resistance\n").Append("-100 electric resistance\n")
			.Append("Temperature does not passively return to ambient temperature\n")
			.Append("Patting or rolling firefighting actions are 25% as effective\n")
			.Append("Removes liquid coatings\n");
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("CanApplyCoatedInPlasma"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyCoatedInPlasma"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "CoatedInPlasma", this))
		{
			return false;
		}
		if (Object.GetEffect("CoatedInPlasma") is CoatedInPlasma coatedInPlasma)
		{
			if (base.Duration > coatedInPlasma.Duration)
			{
				coatedInPlasma.Duration = base.Duration;
			}
			if (!GameObject.validate(ref coatedInPlasma.Owner) && GameObject.validate(ref Owner))
			{
				coatedInPlasma.Owner = Owner;
			}
			return false;
		}
		Object.RemoveAllEffects("LiquidCovered");
		ApplyStats();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != CanTemperatureReturnToAmbientEvent.ID && ID != GetFirefightingPerformanceEvent.ID)
		{
			return ID == GeneralAmnestyEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		GameObject.validate(ref Owner);
		base.Object.RemoveAllEffects("LiquidCovered");
		if (base.Object.IsPlayer() && base.Object.IsAflame())
		{
			AchievementManager.SetAchievement("ACH_AURORAL");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanTemperatureReturnToAmbientEvent E)
	{
		if (E.Object == base.Object)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetFirefightingPerformanceEvent E)
	{
		if (E.Object == base.Object)
		{
			E.Result /= 4;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Owner = null;
		return base.HandleEvent(E);
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

	public void ApplyStats()
	{
		base.StatShifter.SetStatShift("HeatResistance", -100);
		base.StatShifter.SetStatShift("ColdResistance", -100);
		base.StatShifter.SetStatShift("ElectricResistance", -100);
	}

	public void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts();
	}
}
