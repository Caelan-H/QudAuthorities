using System;
using System.Text;

namespace XRL.World.Effects;

[Serializable]
public class CyberneticRejectionSyndrome : Effect
{
	public int Level = 1;

	public CyberneticRejectionSyndrome()
	{
		base.DisplayName = "{{c|cybernetic rejection syndrome}}";
		base.Duration = 1;
	}

	public CyberneticRejectionSyndrome(int Level)
		: this()
	{
		this.Level = Level;
	}

	public override int GetEffectType()
	{
		return 50331652;
	}

	public override string GetDescription()
	{
		return "{{c|cybernetic rejection syndrome}}";
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		int moveSpeedPenalty = GetMoveSpeedPenalty();
		if (moveSpeedPenalty != 0)
		{
			stringBuilder.Append(-moveSpeedPenalty).Append(" move speed.\n");
		}
		int regenerationReductionPercentage = GetRegenerationReductionPercentage();
		if (regenerationReductionPercentage != 0)
		{
			stringBuilder.Append("Natural healing reduced by ").Append(regenerationReductionPercentage).Append("%.\n");
		}
		int healingReductionPercentage = GetHealingReductionPercentage();
		if (healingReductionPercentage != 0)
		{
			stringBuilder.Append("External healing reduced by ").Append(healingReductionPercentage).Append("%.\n");
		}
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("ApplyCyberneticRejectionSyndrome"))
		{
			return false;
		}
		if (Object.GetEffect("CyberneticRejectionSyndrome") is CyberneticRejectionSyndrome cyberneticRejectionSyndrome)
		{
			if (Level > 0)
			{
				cyberneticRejectionSyndrome.Level += Level;
				if (Object.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Your feverish feeling is getting worse.", 'r');
				}
				cyberneticRejectionSyndrome.ApplyChanges();
			}
			return false;
		}
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You feel feverish.", 'r');
		}
		ApplyChanges();
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You feel less feverish.", 'g');
		}
		base.Remove(Object);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "Healing");
		Object.RegisterEffectEvent(this, "Regenerating");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "Healing");
		Object.UnregisterEffectEvent(this, "Regenerating");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Healing")
		{
			E.SetParameter("Amount", E.GetIntParameter("Amount") * (100 - GetHealingReductionPercentage()) / 100);
		}
		else if (E.ID == "Regenerating")
		{
			E.SetParameter("Amount", E.GetIntParameter("Amount") * (100 - GetRegenerationReductionPercentage()) / 100);
		}
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

	private void ApplyChanges()
	{
		base.StatShifter.SetStatShift(base.Object, "MoveSpeed", GetMoveSpeedPenalty());
	}

	private void UnapplyChanges()
	{
		base.StatShifter.RemoveStatShifts();
	}

	public int GetHealingReductionPercentage()
	{
		return Math.Min(Level * 5, 50);
	}

	public int GetRegenerationReductionPercentage()
	{
		return Math.Min(Level * 7, 60);
	}

	public int GetMoveSpeedPenalty()
	{
		return Math.Min(Level, 10);
	}

	public void Reduce(int By)
	{
		if (By >= Level)
		{
			base.Object.RemoveEffect(this);
			return;
		}
		if (base.Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Your feverish feeling eases up a bit.", 'g');
		}
		Level -= By;
	}
}
